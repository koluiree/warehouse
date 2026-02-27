namespace Warehouse.Web.Models;

public record LoginRequest(string Username, string Password);
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
public record LoginResponse(string Token, LoginUser User);
public record LoginUser(string Id, string Username, string Email, List<string> Roles);

public record DepartmentDto(int Id, string Name, string Code);
public record EmployeeDto(string Id, string FullName, string Position, string? Phone, bool IsActive, DepartmentDto Department, EmployeeUserDto User);
public record EmployeeUserDto(string Id, string Username, string Email);

public record WarehouseDto(int Id, string Name, string? Address);
public record CreateWarehouseRequest(string Name, string? Address);
public record ProductDto(int Id, string Sku, string Name, string Unit, string? Description);
public record CreateProductRequest(string Sku, string Name, string Unit, string? Description);

public record BalanceDto(long Id, WarehouseRef Warehouse, ProductRef Product, decimal Quantity, DateTime UpdatedAt);
public record WarehouseRef(int Id, string Name);
public record ProductRef(int Id, string Sku, string Name, string Unit);
public record InventoryOperationRequest(int WarehouseId, int ProductId, decimal Quantity, string? DocumentNumber, string? Comment);

public record CreateDepartmentRequest(string Name, string Code);

public record IssueRequestDto(long Id, string RequesterId, int DepartmentId, string Status, DateTime CreatedAt, DateTime? ApprovedAt, string? ApprovedById, string? Comment);
public record IssueRequestItemDto(long Id, long RequestId, int ProductId, decimal RequestedQty, decimal IssuedQty);
public record CreateIssueRequestItemRequest(int ProductId, decimal RequestedQty);
public record CreateIssueRequestRequest(int DepartmentId, string? Comment, List<CreateIssueRequestItemRequest> Items);
public record ProcessIssueRequestRequest(string? Comment);
public record IssueItemRequest(int WarehouseId, int ProductId, decimal Quantity, string? Comment);

public record StockMovementDto(long Id, int WarehouseId, int ProductId, string Type, decimal Quantity, DateTime OccurredAt, string? DocumentNumber, string? Comment, string? PerformedById, long? RequestId);

public record AdminUserDto(string Id, string Username, string Email, bool IsActive, List<string> Roles);
public record AssignRoleRequest(string UserId, string RoleName);
