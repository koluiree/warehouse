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
public class RequestsController(WarehouseDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = db.IssueRequests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:long}/items")]
    public async Task<IActionResult> GetItems(long id)
    {
        var items = await db.IssueRequestItems
            .Where(x => x.RequestId == id)
            .OrderBy(x => x.Id)
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIssueRequestRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (request.Items.Count == 0)
        {
            return BadRequest("Request must have at least one item.");
        }

        var departmentId = request.DepartmentId;
        if (departmentId <= 0)
        {
            departmentId = await db.Employees
                .Where(x => x.UserId == userId)
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync();
        }

        if (departmentId <= 0)
        {
            departmentId = await db.Departments
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        if (departmentId <= 0)
        {
            return BadRequest("Department is required to create request.");
        }

        var entity = new IssueRequest
        {
            RequesterId = userId,
            DepartmentId = departmentId,
            Status = RequestStatuses.Submitted,
            CreatedAt = DateTime.UtcNow,
            Comment = request.Comment
        };

        db.IssueRequests.Add(entity);
        await db.SaveChangesAsync();

        var items = request.Items.Select(i => new IssueRequestItem
        {
            RequestId = entity.Id,
            ProductId = i.ProductId,
            RequestedQty = i.RequestedQty,
            IssuedQty = 0
        });
        db.IssueRequestItems.AddRange(items);
        await db.SaveChangesAsync();

        return Ok(entity);
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Roles = "DepartmentHead,Admin")]
    public async Task<IActionResult> Approve(long id, ProcessIssueRequestRequest request)
    {
        var issueRequest = await db.IssueRequests.FindAsync(id);
        if (issueRequest is null)
        {
            return NotFound();
        }

        if (issueRequest.Status != RequestStatuses.Submitted)
        {
            return BadRequest("Only submitted request can be approved.");
        }

        issueRequest.Status = RequestStatuses.Approved;
        issueRequest.ApprovedAt = DateTime.UtcNow;
        issueRequest.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
        issueRequest.Comment = request.Comment ?? issueRequest.Comment;
        await db.SaveChangesAsync();
        return Ok(issueRequest);
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Roles = "DepartmentHead,Admin")]
    public async Task<IActionResult> Reject(long id, ProcessIssueRequestRequest request)
    {
        var issueRequest = await db.IssueRequests.FindAsync(id);
        if (issueRequest is null)
        {
            return NotFound();
        }

        if (issueRequest.Status != RequestStatuses.Submitted)
        {
            return BadRequest("Only submitted request can be rejected.");
        }

        issueRequest.Status = RequestStatuses.Rejected;
        issueRequest.ApprovedAt = DateTime.UtcNow;
        issueRequest.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
        issueRequest.Comment = request.Comment ?? issueRequest.Comment;
        await db.SaveChangesAsync();
        return Ok(issueRequest);
    }

    [HttpPost("{id:long}/start-issue")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<IActionResult> StartIssue(long id)
    {
        var issueRequest = await db.IssueRequests.FindAsync(id);
        if (issueRequest is null)
        {
            return NotFound();
        }

        if (issueRequest.Status != RequestStatuses.Approved && issueRequest.Status != RequestStatuses.PartiallyIssued)
        {
            return BadRequest("Only approved or partially issued request can be started.");
        }

        issueRequest.Status = RequestStatuses.InProgress;
        await db.SaveChangesAsync();
        return Ok(issueRequest);
    }

    [HttpPost("{id:long}/issue-item")]
    [Authorize(Roles = "Storekeeper,Admin")]
    public async Task<IActionResult> IssueItem(long id, IssueItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be positive.");
        }

        var issueRequest = await db.IssueRequests.FindAsync(id);
        if (issueRequest is null)
        {
            return NotFound();
        }

        if (issueRequest.Status != RequestStatuses.InProgress && issueRequest.Status != RequestStatuses.PartiallyIssued)
        {
            return BadRequest("Request is not in issue state.");
        }

        var item = await db.IssueRequestItems.FirstOrDefaultAsync(x => x.RequestId == id && x.ProductId == request.ProductId);
        if (item is null)
        {
            return BadRequest("Item is not part of request.");
        }

        var allowed = item.RequestedQty - item.IssuedQty;
        if (request.Quantity > allowed)
        {
            return BadRequest("Issuing quantity is greater than requested remainder.");
        }

        var balance = await db.StockBalances.FirstOrDefaultAsync(x =>
            x.WarehouseId == request.WarehouseId && x.ProductId == request.ProductId);
        if (balance is null || balance.Quantity < request.Quantity)
        {
            return BadRequest("Insufficient stock.");
        }

        balance.Quantity -= request.Quantity;
        balance.UpdatedAt = DateTime.UtcNow;
        item.IssuedQty += request.Quantity;

        db.StockMovements.Add(new StockMovement
        {
            WarehouseId = request.WarehouseId,
            ProductId = request.ProductId,
            Type = MovementTypes.IssueByRequest,
            Quantity = -request.Quantity,
            OccurredAt = DateTime.UtcNow,
            DocumentNumber = $"REQ-{id}",
            Comment = request.Comment,
            PerformedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
            RequestId = id
        });

        var allItems = await db.IssueRequestItems.Where(x => x.RequestId == id).ToListAsync();
        var fullyIssued = allItems.All(x => x.IssuedQty >= x.RequestedQty);
        var partiallyIssued = allItems.Any(x => x.IssuedQty > 0) && !fullyIssued;

        issueRequest.Status = fullyIssued
            ? RequestStatuses.Issued
            : partiallyIssued ? RequestStatuses.PartiallyIssued : RequestStatuses.InProgress;

        await db.SaveChangesAsync();
        return Ok(issueRequest);
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, ProcessIssueRequestRequest request)
    {
        var issueRequest = await db.IssueRequests.FindAsync(id);
        if (issueRequest is null)
        {
            return NotFound();
        }

        if (issueRequest.Status is RequestStatuses.Issued or RequestStatuses.Rejected)
        {
            return BadRequest("Request in final state cannot be cancelled.");
        }

        issueRequest.Status = RequestStatuses.Cancelled;
        issueRequest.Comment = request.Comment ?? issueRequest.Comment;
        await db.SaveChangesAsync();
        return Ok(issueRequest);
    }
}
