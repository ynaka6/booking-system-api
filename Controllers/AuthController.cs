using app.Models;
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
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Scriban;

namespace app.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{

    public class LoginRequest
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class RegistrationRequest
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }

    private readonly IWebHostEnvironment _env;

    private readonly IConfiguration _configuration;

    private readonly ApplicationDbContext _db;

    private readonly IEmailSender _mailSender;

    public AuthController(IWebHostEnvironment env, IConfiguration configuration, ApplicationDbContext db, IEmailSender mailSender)
    {
        _env = env;
        _configuration = configuration;
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

        return Ok(this.GenerateToken(request.Email)); // 認証トークンをレスポンスする
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

        _mailSender.SendEmailAsync(user.Email, "Plsase confirm register info.", (string) result);

        return Ok();
    }

    [HttpDelete]
    [Route("logout")]
    public async Task<IActionResult> Logout(ClaimsPrincipal user)
    {
        // TODO: Remove JWT Token
        return Ok(user.Identity?.Name);
    }


    private string GenerateToken(String userId)
    {
        var claims = new[] {
            // 必要な認証情報を追加する
            new Claim(ClaimTypes.Name, userId)
        };

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"], // issuer
            _configuration["JWT:ValidAudience"], // audience
            claims,
            expires: DateTime.Now.AddSeconds(10000), // 有効期限
            signingCredentials: CreateSigningCredentials()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}