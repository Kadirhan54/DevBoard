using DevBoard.Domain.Common;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace DevBoard.Infrastructure.Contexts.Application
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantProvider _tenantProvider;

        // Property used in global query filters
        // Fix for CS0266 and CS8629: Ensure TenantId is never null by using GetValueOrDefault()
        // This assumes ITenantProvider.GetTenantId() returns Guid? (nullable Guid).
        public Guid TenantId => _tenantProvider?.GetTenantId() ?? Guid.Empty;
        //public Guid TenantId => _tenantProvider?.GetTenantId() ?? throw new InvalidOperationException("TenantId not available for current request");

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Multi-tenant query filters
            builder.Entity<Project>().HasQueryFilter(p => TenantId == Guid.Empty || p.TenantId == TenantId);
            builder.Entity<Board>().HasQueryFilter(b => TenantId == Guid.Empty || b.TenantId == TenantId);
            builder.Entity<TaskItem>().HasQueryFilter(t => TenantId == Guid.Empty || t.TenantId == TenantId);


            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.EnableNotifications).HasDefaultValue(true);
            });

            builder.HasDefaultSchema("identity");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantAndAudit();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically sets TenantId and audit fields for added entities.
        /// Propagates TenantId from parent entities if available.
        /// </summary>
        private void SetTenantAndAudit()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    // Set CreatedOn if supported
                    if (entry.Entity is ICreatedByEntity createdEntity)
                    {
                        createdEntity.CreatedOn = DateTime.UtcNow;
                    }

                    // Set TenantId if entity implements ITenantEntity
                    if (entry.Entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty)
                    {
                        switch (entry.Entity)
                        {
                            case Board board:
                                tenantEntity.TenantId = board.Project?.TenantId ?? TenantId;
                                break;

                            case TaskItem task:
                                tenantEntity.TenantId = task.Board?.TenantId ?? TenantId;
                                break;

                            default:
                                tenantEntity.TenantId = TenantId;
                                break;
                        }
                    }
                }

                // Optional: handle modified/deleted timestamps
                // if (entry.State == EntityState.Modified && entry.Entity is IModifiedByEntity modifiedEntity)
                // {
                //     modifiedEntity.ModifiedOn = DateTime.UtcNow;
                // }
                // else if (entry.State == EntityState.Deleted && entry.Entity is IDeletedByEntity deletedEntity)
                // {
                //     deletedEntity.DeletedOn = DateTime.UtcNow;
                // }
            }
        }
    }

}

