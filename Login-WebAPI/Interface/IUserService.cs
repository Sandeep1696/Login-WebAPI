using Login_WebAPI.Models;

namespace Login_WebAPI.Interface
{
    public interface  IUserService
    {
        List<User> GetAllUsers();
        List<object> GetAllEmployees();
        Employee GetEmployeeProfile(string email);
        User UpdateUser(int id, UserEmployeeUpdateDto updatedUser);
        bool DeleteUser(int id);
        byte[] ExportEmployeesToExcel();
    }
}
