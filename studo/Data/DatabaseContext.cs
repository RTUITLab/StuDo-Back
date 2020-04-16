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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DatabaseContext(DbContextOptions options) : base (options) {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureAds(builder);
            ConfigureOrganizations(builder);
            ConfigureResumes(builder);
            ConfigureOrganizationRights(builder);
            ConfigureUserOrganization(builder);
            ConfigureUserAd(builder);
            ConfigureComments(builder);

            ConfigureRefreshToken(builder);
        }

        private void ConfigureComments(ModelBuilder builder)
        {
            builder.Entity<Comment>()
                .HasKey(c => new { c.Id });
        }

        private void ConfigureUserAd(ModelBuilder builder)
        {
            builder.Entity<UserAd>()
                .HasKey(ua => new { ua.UserId, ua.AdId });

            builder.Entity<UserAd>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(ua => ua.UserId);

            builder.Entity<UserAd>()
                .HasOne(ua => ua.Ad)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(ua => ua.AdId);
        }

        private void ConfigureUserOrganization(ModelBuilder builder)
        {
            builder.Entity<UserRightsInOrganization>()
                .HasKey(uo => new { uo.UserId, uo.OrganizationId, uo.OrganizationRightId });

            builder.Entity<UserRightsInOrganization>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.Organizations)
                .HasForeignKey(uo => uo.UserId);

            builder.Entity<UserRightsInOrganization>()
                .HasOne(uo => uo.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(uo => uo.OrganizationId);

            builder.Entity<UserRightsInOrganization>()
                .HasOne(uo => uo.UserOrganizationRight)
                .WithMany(uor => uor.UserRightsInOrganizations)
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

            builder.Entity<Organization>()
                .HasMany(org => org.Users);

            builder.Entity<Organization>()
                .HasIndex(org => org.Name)
                .IsUnique();
        }

        private void ConfigureAds(ModelBuilder builder)
        {
            builder.Entity<Ad>()
                .HasKey(ad => new { ad.Id });
        }

        private void ConfigureRefreshToken(ModelBuilder builder)
        {
            builder.Entity<RefreshToken>(cfg =>
            {
                cfg.HasKey(rt => new { rt.Token, rt.UserId });

                cfg.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId);
            });
        }
    }
}
