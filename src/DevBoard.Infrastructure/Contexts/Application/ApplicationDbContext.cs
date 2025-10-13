using DevBoard.Domain.Common;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace DevBoard.Infrastructure.Contexts.Application
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.EnableNotifications).HasDefaultValue(true);
            });

            builder.HasDefaultSchema("identity");

            //// User → Task (assigned)
            //builder.Entity<TaskItem>()
            //    .HasOne(t => t.AssignedUser)
            //    .WithMany(u => u.AssignedTasks)
            //    .HasForeignKey(t => t.AssignedUserId)
            //    .OnDelete(DeleteBehavior.SetNull); // Keep task even if user is deleted
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((ICreatedByEntity)entry.Entity).CreatedOn = DateTime.UtcNow;
                }
                //else if (entry.State == EntityState.Modified)
                //{
                //    ((IModifiedByEntity)entry.Entity).ModifiedOn = DateTime.UtcNow;
                //}
                //else if (entry.State == EntityState.Deleted)
                //{
                //    ((IDeletedByEntity)entry.Entity).DeletedOn = DateTime.UtcNow;
                //}
            }

            return base.SaveChanges();
        }
    }
}

