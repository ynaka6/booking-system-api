using app.Domain.Entities;
using app.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Scriban;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResetPasswordController : ApiControllerBase
{

    public class ResetasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
    }

    private const int TokenExpireMinutes = 60;

    private readonly IWebHostEnvironment _env;

    private readonly ApplicationDbContext _db;

    private readonly IEmailSender _mailSender;

    private readonly ILogger<EmailSender> _logger;

    public ResetPasswordController(IWebHostEnvironment env, ApplicationDbContext db, IEmailSender mailSender, ILogger<EmailSender> logger)
    {
        _env = env;
        _db = db;
        _mailSender = mailSender;
        _logger = logger;
    }

    [HttpPost]
    [Route("")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(ResetasswordRequest request)
    {
        DateTime fromDate = DateTime.Now.AddMinutes(-1 * TokenExpireMinutes);
        _logger.LogInformation("fromDate: " + fromDate);
        _logger.LogInformation("request.Token: " + request.Token);
        PasswordReset passwordReset = _db.PasswordResets
            .Where(p => p.Token == request.Token)
            .Where(p => p.CreatedDate >= fromDate)
            .FirstOrDefault<PasswordReset>();
        if (passwordReset == null)
        {
            return NotFound();
        }

        User user = _db.Users.Where(u => u.Email == passwordReset.Email).FirstOrDefault<User>();
        if (user == null)
        {
            return NotFound();
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        _db.Users.Update(user);
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        _db.PasswordResets.Remove(passwordReset);
        _db.SaveChanges();

        return Ok();
    }
}