using ClosedXML.Excel;
using Login_WebAPI.Data;
using Login_WebAPI.Interface;
using Login_WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public List<User> GetAllUsers()
    {
        _logger.LogInformation("Fetching all users...");
        return _dbContext.Users.Include(u => u.Employee).ToList();
    }

    public List<object> GetAllEmployees()
    {
        _logger.LogInformation("Fetching all employees with roles...");

        return (from emp in _dbContext.Employees
                join user in _dbContext.Users on emp.Email equals user.Email into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new
                {
                    emp.Id,
                    emp.Name,
                    emp.Email,
                    emp.Department,
                    emp.ManagerId,
                    emp.JoiningDate,
                    Role = user != null ? user.Role : "Employee"
                }).ToList<object>();
    }

    public Employee GetEmployeeProfile(string email)
    {
        _logger.LogInformation("Fetching employee profile for {Email}...", email);
        var user = _dbContext.Users.Include(u => u.Employee).FirstOrDefault(u => u.Email == email);
        return user?.Employee;
    }

    public User UpdateUser(int id, UserEmployeeUpdateDto updatedUser)
    {
        _logger.LogInformation("Updating user with ID {UserId}...", id);

        var user = _dbContext.Users.Find(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", id);
            return null;
        }

        user.Email = updatedUser.Email;
        user.Role = updatedUser.Role;

        if (user.EmployeeId.HasValue)
        {
            var employee = _dbContext.Employees.Find(user.EmployeeId.Value);
            if (employee != null)
            {
                employee.Name = updatedUser.Name;
                employee.Department = updatedUser.Department;
            }
        }

        _dbContext.SaveChanges();
        return user;
    }

    public bool DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID {UserId}...", id);

        var user = _dbContext.Users.Find(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", id);
            return false;
        }

        _dbContext.Users.Remove(user);
        _dbContext.SaveChanges();
        return true;
    }

    public byte[] ExportEmployeesToExcel()
    {
        var employeesWithRoles = (from emp in _dbContext.Employees
                                  join user in _dbContext.Users on emp.Email equals user.Email into empUserGroup
                                  from user in empUserGroup.DefaultIfEmpty()
                                  select new
                                  {
                                      emp.Id,
                                      emp.Name,
                                      emp.Email,
                                      emp.Department,
                                      emp.JoiningDate,
                                      Role = user != null ? user.Role : "N/A"
                                  }).ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employees");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "ID";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Email";
            worksheet.Cell(currentRow, 4).Value = "Department";
            worksheet.Cell(currentRow, 5).Value = "Joining Date";
            worksheet.Cell(currentRow, 6).Value = "Role";

            foreach (var emp in employeesWithRoles)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = emp.Id;
                worksheet.Cell(currentRow, 2).Value = emp.Name;
                worksheet.Cell(currentRow, 3).Value = emp.Email;
                worksheet.Cell(currentRow, 4).Value = emp.Department;
                worksheet.Cell(currentRow, 5).Value = emp.JoiningDate.ToString("yyyy-MM-dd");
                worksheet.Cell(currentRow, 6).Value = emp.Role;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}
