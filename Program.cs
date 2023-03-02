using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using app.Models;
using app.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SqlConnectionString");

// TODO: Seeder
using (var context = new SeederContext(connectionString))
{
    context.Database.EnsureCreated();

    var testUser = context.Users.FirstOrDefault(b => b.Email == "test1@example.com");
    if (testUser == null)
    {
        context.Users.Add(new User { Email = "test1@example.com", Password = BCrypt.Net.BCrypt.HashPassword("Pa$$w0rd"), CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now });
    }
    context.SaveChanges();

    for (int index = 0; index < 5; index++)
    {
        var testBlog = context.Blogs.FirstOrDefault(b => b.Name == "Blog post #" + index);
        if (testBlog == null)
        {
            context.Blogs.Add(new Blog { Name = "Blog post #" + index, CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now });
        }
        context.SaveChanges();
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-7.0#log-automatic-400-responses
// builder.Services.AddSingleton<IConfiguration>(builder);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        var builtInFactory = options.InvalidModelStateResponseFactory;
        options.InvalidModelStateResponseFactory = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            return builtInFactory(context);
        };
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT:ValidIssuer"],
            ValidAudience = configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT:Secret"])
            ),
        };
    });

builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/secret", (ClaimsPrincipal user) => $"Hello {user.Identity?.Name}. My secret")
    .RequireAuthorization();
app.MapControllers().RequireAuthorization();

app.Run();
