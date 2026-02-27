using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MovementsController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? productId,
        [FromQuery] int? warehouseId,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = db.StockMovements.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.OccurredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.OccurredAt <= to.Value);
        }

        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(500)
            .ToListAsync();

        return Ok(items);
    }
}
