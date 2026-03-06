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

        var userIds = items.Where(x => x.PerformedById != null).Select(x => x.PerformedById!).Distinct().ToList();
        var users = await db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username);

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => new { p.Sku, p.Name });

        var warehouseIds = items.Select(x => x.WarehouseId).Distinct().ToList();
        var warehouses = await db.Warehouses.Where(w => warehouseIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w.Name);

        var result = items.Select(m => new
        {
            m.Id,
            m.WarehouseId,
            WarehouseName = warehouses.GetValueOrDefault(m.WarehouseId, "—"),
            m.ProductId,
            ProductSku = products.TryGetValue(m.ProductId, out var p) ? p.Sku : "—",
            ProductName = p?.Name ?? "—",
            m.Type,
            m.Quantity,
            m.OccurredAt,
            m.DocumentNumber,
            m.Comment,
            m.PerformedById,
            PerformedByName = m.PerformedById != null ? users.GetValueOrDefault(m.PerformedById, "—") : "—",
            m.RequestId
        });

        return Ok(result);
    }
}
