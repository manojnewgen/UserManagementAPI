using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Models;

namespace UserManagementAPI.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Password = "password123" },
                new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Password = "password456" }
            );
        }
    }
}
