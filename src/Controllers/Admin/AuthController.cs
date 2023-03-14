using app.Application.Services;
using app.Domain.Entities;
using app.Infrastructure;
using app.Controllers;
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

namespace app.Controllers.Admin;

[ApiController]
[Area("admin")]
[Route("api/[area]/[controller]")]
public class AuthController : ApiControllerBase
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

    private readonly IWebHostEnvironment _env;

    private readonly IAuthenticationService _authenticationService;

    private readonly ApplicationDbContext _db;

    public AuthController(IWebHostEnvironment env, IAuthenticationService authenticationService, ApplicationDbContext db)
    {
        _env = env;
        _authenticationService = authenticationService;
        _db = db;
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        AdminUser user = _db.AdminUsers.Where(u => u.Email == request.Email).FirstOrDefault<AdminUser>();
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
        var user = _db.AdminUsers.Find(refreshToken.AdminUserId);
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

    [HttpDelete]
    [Route("logout")]
    public async Task<IActionResult> Logout(ClaimsPrincipal user)
    {
        // TODO: Remove JWT Token
        return Ok(user.Identity?.Name);
    }

    private void removeOldRefreshTokens(AdminUser user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.CreatedDate?.AddDays(2) <= DateTime.Now);
    }
}