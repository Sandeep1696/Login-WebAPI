using Login_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Login_WebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<TokenRequest> TokenRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define TokenRequest as a keyless entity
            modelBuilder.Entity<TokenRequest>().HasNoKey();

            // Seed Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    Department = "HR",
                    ManagerId = null, // Alice doesn't have a manager
                    JoiningDate = DateTime.Now.AddYears(-2)
                },
                new Employee
                {
                    Id = 2,
                    Name = "Bob Smith",
                    Email = "bob.smith@example.com",
                    Department = "IT",
                    ManagerId = 1, // Alice is Bob's manager
                    JoiningDate = DateTime.Now.AddYears(-1)
                }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), // Ensure it's hashed
                    Role = "Admin",
                    EmployeeId = 1,
                    RefreshToken = "sample-refresh-token-1"
                },
                new User
                {
                    Id = 2,
                    Name = "Bob Smith",
                    Email = "bob.smith@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Employee",
                    EmployeeId = 2,
                    RefreshToken = "sample-refresh-token-2"
                }
            );

            // TokenRequest does not have seed data since it's keyless
        }

    }
}
