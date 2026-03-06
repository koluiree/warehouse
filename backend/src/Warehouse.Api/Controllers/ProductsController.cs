using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Models;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Sku.Contains(search) || x.Name.Contains(search));
        }

        return Ok(await query.OrderBy(x => x.Name).ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Storekeeper")]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var exists = await db.Products.AnyAsync(x => x.Sku == request.Sku);
        if (exists)
        {
            return Conflict("Product with this SKU already exists.");
        }

        if (request.MinQty <= 0)
        {
            return BadRequest("MinQty must be positive.");
        }

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Unit = request.Unit,
            MinQty = request.MinQty,
            IsInteger = request.IsInteger,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Storekeeper")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (request.MinQty <= 0)
            return BadRequest("MinQty must be positive.");

        product.Name = request.Name;
        product.Unit = request.Unit;
        product.MinQty = request.MinQty;
        product.IsInteger = request.IsInteger;
        product.Description = request.Description;

        await db.SaveChangesAsync();
        return Ok(product);
    }
}
