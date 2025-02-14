using ClosedXML.Excel;
using Login_WebAPI.Data;
using Login_WebAPI.Interface;
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
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        try
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users.");
            return StatusCode(500, $"Error retrieving users: {ex.Message}");
        }
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet("employees")]
    public IActionResult GetAllEmployees()
    {
        try
        {
            var employees = _userService.GetAllEmployees();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees.");
            return StatusCode(500, $"Error retrieving employees: {ex.Message}");
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpGet("my-profile")]
    public IActionResult GetEmployeeProfile()
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = _userService.GetEmployeeProfile(userEmail);

            if (profile == null)
            {
                return NotFound("Employee profile not found.");
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee profile.");
            return StatusCode(500, $"Error retrieving profile: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("update-user/{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UserEmployeeUpdateDto updatedUser)
    {
        try
        {
            var user = _userService.UpdateUser(id, updatedUser);
            return user != null ? Ok(user) : NotFound("User not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user.");
            return StatusCode(500, $"Error updating user: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-user/{id}")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            return _userService.DeleteUser(id) ? Ok("User deleted successfully.") : NotFound("User not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user.");
            return StatusCode(500, $"Error deleting user: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("export-employees")]
    public IActionResult ExportEmployeesToExcel()
    {
        var content = _userService.ExportEmployeesToExcel();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
    }
}

