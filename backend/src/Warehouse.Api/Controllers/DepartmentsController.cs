using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await db.Departments.OrderBy(x => x.Name).ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateDepartmentRequest request)
    {
        var exists = await db.Departments.AnyAsync(x => x.Code == request.Code || x.Name == request.Name);
        if (exists)
        {
            return Conflict("Department with same code or name already exists.");
        }

        var department = new Department
        {
            Name = request.Name,
            Code = request.Code,
            CreatedAt = DateTime.UtcNow
        };

        db.Departments.Add(department);
        await db.SaveChangesAsync();
        return Ok(department);
    }
}
