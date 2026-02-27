using BCrypt.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;
using Warehouse.Api.Services;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(WarehouseDbContext db, JwtTokenService jwtTokenService) : ControllerBase
{
    private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9_]{4,32}$", RegexOptions.Compiled);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!IsValidUsername(request.Username))
        {
            return BadRequest("Username must be 4-32 chars and contain only letters, digits, underscore.");
        }

        if (!IsValidPassword(request.Password))
        {
            return BadRequest("Password must be at least 8 chars and include at least one letter and one digit.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest("Email is invalid.");
        }

        var exists = await db.Users.AnyAsync(x => x.Username == request.Username || x.Email == request.Email);
        if (exists)
        {
            return Conflict("User with the same username or email already exists.");
        }

        var department = await db.Departments.FindAsync(request.DepartmentId);
        if (department is null)
        {
            department = await db.Departments.OrderBy(x => x.Id).FirstOrDefaultAsync();
        }

        if (department is null)
        {
            department = new Department
            {
                Name = "Общий отдел",
                Code = "GEN",
                CreatedAt = DateTime.UtcNow
            };
            db.Departments.Add(department);
            await db.SaveChangesAsync();
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var employee = new Employee
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            DepartmentId = department.Id,
            FullName = request.FullName,
            Position = request.Position,
            Phone = request.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "Storekeeper" : request.Role.Trim();
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
        if (role is not null)
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        return Ok(new { user.Id, user.Username, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        var roles = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name
        ).ToListAsync();

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var token = jwtTokenService.CreateToken(user.Id, user.Username, roles);
        return Ok(new
        {
            token,
            user = new { user.Id, user.Username, user.Email, roles }
        });
    }

    private static bool IsValidUsername(string value)
        => !string.IsNullOrWhiteSpace(value) && UsernameRegex.IsMatch(value);

    private static bool IsValidPassword(string value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length >= 8
           && value.Any(char.IsLetter)
           && value.Any(char.IsDigit);
}
