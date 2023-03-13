using app.Application.Services;
using app.Domain.Entities;
using app.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Scriban;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    public class LoginRequest
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class RefreshRequest
    {
        [Required]
        public string? RefreshToken { get; set; }
    }

    public class RegistrationRequest
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }

    private readonly IWebHostEnvironment _env;

    private readonly IAuthenticationService _authenticationService;

    private readonly ApplicationDbContext _db;

    private readonly IEmailSender _mailSender;

    public AuthController(IWebHostEnvironment env, IAuthenticationService authenticationService, ApplicationDbContext db, IEmailSender mailSender)
    {
        _env = env;
        _authenticationService = authenticationService;
        _db = db;
        _mailSender = mailSender;
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        User user = _db.Users.Where(u => u.Email == request.Email).FirstOrDefault<User>();
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return BadRequest("ユーザー名またはパスワードが違います。");
        }

        var jwtToken = _authenticationService.GenerateToken(user);
        var refreshToken = _authenticationService.GenerateRefreshToken(ipAddress());
        user.RefreshTokens.Add(refreshToken);

        removeOldRefreshTokens(user);

        _db.Update(user);
        _db.SaveChanges();

        return Ok(new
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token
        });
    }

    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var refreshToken = _db.RefreshTokens.Where(r => r.Token == request.RefreshToken).FirstOrDefault<RefreshToken>();
        if (refreshToken == null || refreshToken.IsExpired)
        {
            return NotFound();
        }
        var user = _db.Users.Find(refreshToken.UserId);
        var jwtToken = _authenticationService.GenerateToken(user);
        var newRefreshToken = _authenticationService.GenerateRefreshToken(ipAddress());
        user.RefreshTokens.Add(newRefreshToken);

        removeOldRefreshTokens(user);

        _db.Update(user);
        _db.RefreshTokens.Remove(refreshToken);
        _db.SaveChanges();

        return Ok(new
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token
        });
    }


    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        var user = _db.Users.Where(u => u.Email == request.Email).FirstOrDefault<User>();
        if (user != null)
        {
            return BadRequest("Exists user");
        }

        user = new User { Email = request.Email, Password = BCrypt.Net.BCrypt.HashPassword(request.Password) };
        _db.Users.Add(user);
        _db.SaveChanges();

        // TODO: Should a separate class
        string contentRootPath = _env.ContentRootPath;
        string text = System.IO.File.ReadAllText(@contentRootPath + "/templates/emails/register.txt");
        var template = Template.Parse(text);
        var result = template.Render(new { Name = "World" });

        _mailSender.SendEmailAsync(user.Email, "Plsase confirm register info.", (string)result);

        return Ok();
    }

    [HttpDelete]
    [Route("logout")]
    public async Task<IActionResult> Logout(ClaimsPrincipal user)
    {
        // TODO: Remove JWT Token
        return Ok(user.Identity?.Name);
    }

    private string ipAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }

    private void removeOldRefreshTokens(User user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.CreatedDate?.AddDays(2) <= DateTime.Now);
    }
}