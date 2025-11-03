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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TODO : Solve this error
            //// Business tables in "public" schema (PostgreSQL default)
            //builder.Entity<TaskItem>().ToTable("Tasks", "public");
            //builder.Entity<Comment>().ToTable("Comments", "public");

            // Apply all entity configurations automatically
            builder.ApplyConfigurationsFromAssembly(typeof(TaskDbContext).Assembly);
        }
    }
}
