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
                ).ToList()
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
}
