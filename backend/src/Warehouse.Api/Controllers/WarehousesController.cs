using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await db.Warehouses.OrderBy(x => x.Name).ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Storekeeper")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Name))
        {
            return BadRequest("Name is required.");
        }

        var exists = await db.Warehouses.AnyAsync(x => x.Name == payload.Name);
        if (exists)
        {
            return Conflict("Warehouse with this name already exists.");
        }

        var entity = new Warehouse.Api.Data.Warehouse
        {
            Name = payload.Name,
            Address = payload.Address,
            CreatedAt = DateTime.UtcNow
        };
        db.Warehouses.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }
}
