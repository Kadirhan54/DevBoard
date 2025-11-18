using DevBoard.Services.Tasks.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Tasks.Infrastructure.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Attachment> Attachments => Set<Attachment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Attachment entity
            builder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachments");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
                entity.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(a => a.StoragePath).IsRequired().HasMaxLength(500);
                entity.Property(a => a.Description).HasMaxLength(500);

                // Relationships
                entity.HasOne(a => a.TaskItem)
                    .WithMany(t => t.Attachments)
                    .HasForeignKey(a => a.TaskItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                //entity.HasOne(a => a.Comment)
                //    .WithMany(c => c.Attachments)
                //    .HasForeignKey(a => a.CommentId)
                //    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(a => a.TaskItemId);
                entity.HasIndex(a => a.TenantId);
                entity.HasIndex(a => a.UploadedAt);
                //entity.HasIndex(a => a.CommentId);
            });

            // Business tables in "public" schema (PostgreSQL default)
            builder.Entity<TaskItem>().ToTable("Tasks", "public");
            builder.Entity<Comment>().ToTable("Comments", "public");

            // Apply all entity configurations automatically
            builder.ApplyConfigurationsFromAssembly(typeof(TaskDbContext).Assembly);
        }
    }
}
