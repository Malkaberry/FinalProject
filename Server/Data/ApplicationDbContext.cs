using Microsoft.EntityFrameworkCore;
using Programmin2_classroom.GoogleAuth.Data.Entities;
using Programmin2_classroom.Shared.Models;
using Programmin2_classroom.Server.Controllers;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Define DbSets for your entities
    public DbSet<User> Users { get; set; }
    // Add other DbSets for other entity models
}