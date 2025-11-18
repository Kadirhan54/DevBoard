
// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Infrastructure/Data/IdentityDbContext.cs
// ============================================================================
using DevBoard.Services.Identity.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DevBoard.Services.Identity.Infrastructure.Data;

public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity tables in "identity" schema
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers", "identity");
        builder.Entity<IdentityRole>().ToTable("AspNetRoles", "identity");
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "identity");
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "identity");
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "identity");
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "identity");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "identity");

        // Business tables in "public" schema
        builder.Entity<Tenant>().ToTable("Tenants", "public");
        builder.Entity<TenantInvitation>().ToTable("TenantInvitations", "public");

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.EnableNotifications).HasDefaultValue(true);
            entity.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
