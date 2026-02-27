namespace Warehouse.Api.Models;

public record RegisterRequest(
    string Username,
    string Password,
    string Email,
    string FullName,
    string Position,
    string? Phone,
    int DepartmentId,
    string? Role
);

public record LoginRequest(string Username, string Password);

public record CreateDepartmentRequest(string Name, string Code);

public record CreateProductRequest(string Sku, string Name, string Unit, string? Description);
public record CreateWarehouseRequest(string Name, string? Address);

public record InventoryOperationRequest(
    int WarehouseId,
    int ProductId,
    decimal Quantity,
    string? DocumentNumber,
    string? Comment
);

public record CreateIssueRequestItemRequest(int ProductId, decimal RequestedQty);

public record CreateIssueRequestRequest(
    int DepartmentId,
    string? Comment,
    List<CreateIssueRequestItemRequest> Items
);

public record ProcessIssueRequestRequest(string? Comment);

public record IssueItemRequest(int WarehouseId, int ProductId, decimal Quantity, string? Comment);

public record AssignRoleRequest(string UserId, string RoleName);
