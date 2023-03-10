using app.Application.Services;
using app.Domain.Entities;
using app.Domain.Enums;
using app.Infrastructure;
using app.Infrastructure.Services;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

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

    var defaultAdminUser = context.AdminUsers.FirstOrDefault(b => b.Email == "admin@example.com");
    if (defaultAdminUser == null)
    {
        context.AdminUsers.Add(new AdminUser { Email = "admin@example.com", Password = BCrypt.Net.BCrypt.HashPassword("Pa$$w0rd"), AdminUserRole = AdminUserRole.Administrator, CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now });
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

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
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
app.MapControllers().RequireAuthorization();

app.Run();
