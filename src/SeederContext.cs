using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using app.Domain.Entities;

public class SeederContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<AdminUser> AdminUsers { get; set; }

    private string _connectionString;

    public SeederContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
    }
}