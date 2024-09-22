using System.Diagnostics;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<PhotoAlbum> PhotoAlbums { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Memoryland> Memorylands { get; set; }
    public DbSet<MemorylandType> MemorylandTypes { get; set; }
    public DbSet<MemorylandConfiguration> MemorylandConfigurations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<ApplicationDbContext>()
                .Build();
        
            optionsBuilder
                .LogTo(
                    msg => Debug.WriteLine(msg),
                    LogLevel.Debug,
                    DbContextLoggerOptions.SingleLine | DbContextLoggerOptions.UtcTime)
                .UseNpgsql(configuration["ConnectionStrings:Default"]); 
        }
    }
}