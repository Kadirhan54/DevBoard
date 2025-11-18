// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Infrastructure/Data/ProjectDbContext.cs
// ============================================================================
using DevBoard.Services.Projects.Core.Entities;
using DevBoard.Shared.Common;
using Microsoft.EntityFrameworkCore;


namespace DevBoard.Services.Projects.Infrastructure.Data;

public class ProjectDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ProjectDbContext(
        DbContextOptions<ProjectDbContext> options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Board> Boards => Set<Board>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TODO : Solve this error
        //builder.HasDefaultSchema("public");

        // Multi-tenant query filters
        builder.Entity<Project>()
            .HasQueryFilter(p => _tenantProvider.GetTenantId() == Guid.Empty
                || p.TenantId == _tenantProvider.GetTenantId());

        builder.Entity<Board>()
            .HasQueryFilter(b => _tenantProvider.GetTenantId() == Guid.Empty
                || b.TenantId == _tenantProvider.GetTenantId());

        // Relationships
        builder.Entity<Board>()
            .HasOne(b => b.Project)
            .WithMany(p => p.Boards)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.Entity<Project>()
            .HasIndex(p => p.TenantId);

        builder.Entity<Board>()
            .HasIndex(b => b.TenantId);

        builder.Entity<Board>()
            .HasIndex(b => b.ProjectId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetTenantId();

        if (tenantId != Guid.Empty)
        {
            foreach (var entry in ChangeTracker.Entries<Project>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                    entry.Entity.TenantId = tenantId;
            }

            foreach (var entry in ChangeTracker.Entries<Board>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                    entry.Entity.TenantId = tenantId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}