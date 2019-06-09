using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using studo.Models;
using System;

namespace studo.Data
{
    public class DatabaseContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Resume> Resumes { get; set; }

        public DatabaseContext(DbContextOptions options)
            : base (options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureAds(builder);
            ConfigureOrganizations(builder);
            ConfigureResumes(builder);
        }

        private void ConfigureResumes(ModelBuilder builder)
        {
            builder.Entity<Resume>()
                .HasKey(r => new { r.Id });
        }

        private void ConfigureOrganizations(ModelBuilder builder)
        {
            builder.Entity<Organization>()
                .HasKey(org => new { org.Id });

            builder.Entity<Organization>()
                .HasMany(org => org.Ads);
        }

        private void ConfigureAds(ModelBuilder builder)
        {
            builder.Entity<Ad>()
                .HasKey(ad => new { ad.Id });
        }
    }
}
