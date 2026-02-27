using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances([FromQuery] int? warehouseId, [FromQuery] string? search)
    {
        var query =
            from b in db.StockBalances
            join w in db.Warehouses on b.WarehouseId equals w.Id
            join p in db.Products on b.ProductId equals p.Id
            select new
            {
                b.Id,
                Warehouse = new { w.Id, w.Name },
                Product = new { p.Id, p.Sku, p.Name, p.Unit },
                b.Quantity,
                b.UpdatedAt
            };

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.Warehouse.Id == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Product.Sku.Contains(search) || x.Product.Name.Contains(search));
        }

        return Ok(await query.OrderBy(x => x.Product.Name).ToListAsync());
    }

    [HttpPost("receipt")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<IActionResult> Receipt(InventoryOperationRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be positive.");
        }

        await ApplyInventoryOperation(request, request.Quantity, MovementTypes.Receipt);
        return Ok();
    }

    [HttpPost("writeoff")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<IActionResult> WriteOff(InventoryOperationRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be positive.");
        }

        var balance = await db.StockBalances.FirstOrDefaultAsync(x =>
            x.WarehouseId == request.WarehouseId && x.ProductId == request.ProductId);
        if (balance is null || balance.Quantity < request.Quantity)
        {
            return BadRequest("Insufficient stock.");
        }

        await ApplyInventoryOperation(request, -request.Quantity, MovementTypes.WriteOff);
        return Ok();
    }

    private async Task ApplyInventoryOperation(InventoryOperationRequest request, decimal delta, string movementType)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var balance = await db.StockBalances.FirstOrDefaultAsync(x =>
            x.WarehouseId == request.WarehouseId && x.ProductId == request.ProductId);
        if (balance is null)
        {
            balance = new StockBalance
            {
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                Quantity = 0,
                UpdatedAt = DateTime.UtcNow
            };
            db.StockBalances.Add(balance);
        }

        balance.Quantity += delta;
        balance.UpdatedAt = DateTime.UtcNow;

        db.StockMovements.Add(new StockMovement
        {
            WarehouseId = request.WarehouseId,
            ProductId = request.ProductId,
            Type = movementType,
            Quantity = delta,
            OccurredAt = DateTime.UtcNow,
            DocumentNumber = request.DocumentNumber,
            Comment = request.Comment,
            PerformedById = userId
        });

        await db.SaveChangesAsync();
    }
}
