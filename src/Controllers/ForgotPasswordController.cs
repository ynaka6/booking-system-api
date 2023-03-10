using app.Domain.Entities;
using app.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Scriban;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForgotPasswordController : ControllerBase
{

    public class ForgotPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
    }



    private readonly IWebHostEnvironment _env;

    private readonly ApplicationDbContext _db;

    private readonly IEmailSender _mailSender;

    public ForgotPasswordController(IWebHostEnvironment env, ApplicationDbContext db, IEmailSender mailSender)
    {
        _env = env;
        _db = db;
        _mailSender = mailSender;
    }

    [HttpPost]
    [Route("")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(ForgotPasswordRequest request)
    {
        User user = _db.Users.Where(u => u.Email == request.Email).FirstOrDefault<User>();
        if (user == null)
        {
            return BadRequest("Invalid E-Mail");
        }

        // TODO: Utilクラスに切り出す
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var token = new string(Enumerable.Repeat(characters, 60)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        PasswordReset passwordReset = new PasswordReset { Email = request.Email, Token = token };
        _db.PasswordResets.Add(passwordReset);
        _db.SaveChanges();


        string contentRootPath = _env.ContentRootPath;
        string text = System.IO.File.ReadAllText(@contentRootPath + "/templates/emails/passowrd_reset.txt");
        var template = Template.Parse(text);
        var result = template.Render(new { Token = passwordReset.Token });

        _mailSender.SendEmailAsync(user.Email, "Plsase confirm password reset.", (string)result);

        return Ok();
    }
}