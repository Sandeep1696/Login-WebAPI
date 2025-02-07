namespace Login_WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }  // Admin, Manager, Employee

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public string RefreshToken { get; internal set; }
    }

    public class UserEmployeeUpdateDto
    {
        public int Id { get; set; }  // User ID
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }  // Admin, Manager, Employee
    }
}
