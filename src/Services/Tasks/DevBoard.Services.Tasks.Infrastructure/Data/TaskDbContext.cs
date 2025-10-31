using DevBoard.Services.Tasks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Add this using directive

namespace DevBoard.Services.Tasks.Infrastructure.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Business tables in "public" schema
            builder.Entity<TaskItem>().ToTable("Tasks", "public");
            builder.Entity<Comment>().ToTable("Comments", "public");

            // TODO : Configure TaskItem entity if needed
            //builder.Entity<TaskItem>(entity =>
            //{
            //    entity.HasOne(u => u.Tenant)
            //        .WithMany(t => t.Users)
            //        .HasForeignKey(u => u.TenantId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});
        }

    }
}
