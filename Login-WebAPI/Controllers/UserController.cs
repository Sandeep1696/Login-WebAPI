using ClosedXML.Excel;
using Login_WebAPI.Data;
using Login_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationDbContext dbContext, ILogger<UserController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Get All Users (Admin Only)
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        _logger.LogInformation("Fetching all users for Admin...");

        try
        {
            var users = _dbContext.Users.Include(u => u.Employee).ToList();
            _logger.LogInformation("Successfully retrieved {UserCount} users.", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users.");
            return StatusCode(500, $"Error retrieving users: {ex.Message}");
        }
    }


    [Authorize(Roles = "Manager,Admin")]
    [HttpGet("employees")]
    public IActionResult GetAllEmployees()
    {
        _logger.LogInformation("Fetching all employees with roles...");

        try
        {
            var employees = (from emp in _dbContext.Employees
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
                                 Role = user != null ? user.Role : "Employee" // ✅ Fetch Role
                             }).ToList();

            _logger.LogInformation("Successfully retrieved employees.");
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees.");
            return StatusCode(500, $"Error retrieving employees: {ex.Message}");
        }
    }

    // Get Employee Profile (Employee Only)
    [Authorize(Roles = "Employee")]
    [HttpGet("my-profile")]
    public IActionResult GetEmployeeProfile()
    {
        _logger.LogInformation("Fetching employee profile...");

        try
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.Include(u => u.Employee).FirstOrDefault(u => u.Email == userEmail);

            if (user == null || user.Employee == null)
            {
                _logger.LogWarning("Employee profile not found for user {Email}.", userEmail);
                return NotFound("Employee profile not found.");
            }

            _logger.LogInformation("Successfully retrieved profile for user {Email}.", userEmail);
            return Ok(user.Employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employee profile.");
            return StatusCode(500, $"Error retrieving profile: {ex.Message}");
        }
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("update-user/{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UserEmployeeUpdateDto updatedUser)
    {
        _logger.LogInformation("Updating user and employee with ID {UserId}...", id);

        try
        {
            // Find User
            var user = _dbContext.Users.Find(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound("User not found.");
            }

            // Update User table fields
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            // Find and update Employee (if linked to User)
            if (user.EmployeeId.HasValue)
            {
                var employee = _dbContext.Employees.Find(user.EmployeeId.Value);
                if (employee != null)
                {
                    employee.Name = updatedUser.Name;
                    employee.Department = updatedUser.Department;
                }
            }

            // Save changes to DB
            _dbContext.SaveChanges();

            _logger.LogInformation("User and Employee with ID {UserId} updated successfully.", id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user and employee with ID {UserId}.", id);
            return StatusCode(500, $"Error updating user: {ex.Message}");
        }
    }

    // Delete a User (Admin Only)
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-user/{id}")]
    public IActionResult DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID {UserId}...", id);

        try
        {
            var user = _dbContext.Users.Find(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound("User not found.");
            }

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
            _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
            return Ok("User deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}.", id);
            return StatusCode(500, $"Error deleting user: {ex.Message}");
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("export-employees")]
    public IActionResult ExportEmployeesToExcel()
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
                                      Role = user != null ? user.Role : "N/A" // ✅ Fetch Role from User table
                                  }).ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Employees");
            var currentRow = 1;

            // Add Headers
            worksheet.Cell(currentRow, 1).Value = "ID";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Email";
            worksheet.Cell(currentRow, 4).Value = "Department";
            worksheet.Cell(currentRow, 5).Value = "Joining Date";
            worksheet.Cell(currentRow, 6).Value = "Role"; // ✅ Include Role

            // Add Data Rows
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
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
            }
        }
    }
}

