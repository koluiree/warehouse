using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = await db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.IsActive,
                Roles = (
                    from ur in db.UserRoles
                    join r in db.Roles on ur.RoleId equals r.Id
                    where ur.UserId == u.Id
                    select r.Name
                ).ToList(),
                Department = (
                    from e in db.Employees
                    join d in db.Departments on e.DepartmentId equals d.Id
                    where e.UserId == u.Id
                    select new { d.Id, d.Name }
                ).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(AssignRoleRequest request)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Name == request.RoleName);
        if (role is null)
        {
            return BadRequest("Role not found.");
        }

        var user = await db.Users.FindAsync(request.UserId);
        if (user is null)
        {
            return BadRequest("User not found.");
        }

        var hasRole = await db.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.RoleId == role.Id);
        if (!hasRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = request.UserId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(RemoveRoleRequest request)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Name == request.RoleName);
        if (role is null) return BadRequest("Роль не найдена.");

        var userRole = await db.UserRoles.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.RoleId == role.Id);
        if (userRole is null) return BadRequest("У пользователя нет этой роли.");

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("change-department")]
    public async Task<IActionResult> ChangeDepartment(ChangeDepartmentRequest request)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.UserId == request.UserId);
        if (employee is null) return BadRequest("Сотрудник не найден.");

        var department = await db.Departments.FindAsync(request.DepartmentId);
        if (department is null) return BadRequest("Отдел не найден.");

        employee.DepartmentId = request.DepartmentId;
        await db.SaveChangesAsync();
        return Ok();
    }
}
