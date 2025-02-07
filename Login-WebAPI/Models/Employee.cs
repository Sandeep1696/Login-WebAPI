namespace Login_WebAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public int? ManagerId { get; set; }
        public DateTime JoiningDate { get; set; }


    }
}
