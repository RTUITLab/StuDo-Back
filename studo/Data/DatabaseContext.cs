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
        public DbSet<OrganizationRight> OrganizationRights { get; set; }
        public DbSet<UserRightsInOrganiaztion> UserRightsInOrganiaztions { get; set; }

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
            ConfigureOrganizationRights(builder);
            ConfigureUserOrganization(builder);
        }

        private void ConfigureUserOrganization(ModelBuilder builder)
        {
            builder.Entity<UserRightsInOrganiaztion>()
                .HasKey(uo => new { uo.UserId, uo.OrganizationId });

            builder.Entity<UserRightsInOrganiaztion>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserRightsInOrganiaztions)
                .HasForeignKey(uo => uo.UserId);

            builder.Entity<UserRightsInOrganiaztion>()
                .HasOne(uo => uo.Organization)
                .WithMany(o => o.UserRightsInOrganiaztions)
                .HasForeignKey(uo => uo.OrganizationId);

            builder.Entity<UserRightsInOrganiaztion>()
                .HasOne(uo => uo.UserOrganizationRight)
                .WithMany(uor => uor.UserRightsInOrganiaztions)
                .HasForeignKey(uo => uo.OrganizationRightId);

        }

        private void ConfigureOrganizationRights(ModelBuilder builder)
        {
            builder.Entity<OrganizationRight>()
                .HasKey(or => new { or.Id });
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
