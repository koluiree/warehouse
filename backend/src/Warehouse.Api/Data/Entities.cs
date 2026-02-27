namespace Warehouse.Api.Data;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserRole
{
    public string UserId { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class Employee
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockBalance
{
    public long Id { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class IssueRequest
{
    public long Id { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Status { get; set; } = RequestStatuses.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedById { get; set; }
    public string? Comment { get; set; }
}

public class IssueRequestItem
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    public int ProductId { get; set; }
    public decimal RequestedQty { get; set; }
    public decimal IssuedQty { get; set; }
}

public class StockMovement
{
    public long Id { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public string Type { get; set; } = MovementTypes.Adjustment;
    public decimal Quantity { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Comment { get; set; }
    public string? PerformedById { get; set; }
    public long? RequestId { get; set; }
}

public class RefreshToken
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class RequestStatuses
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string InProgress = "InProgress";
    public const string PartiallyIssued = "PartiallyIssued";
    public const string Issued = "Issued";
    public const string Cancelled = "Cancelled";
}

public static class MovementTypes
{
    public const string Receipt = "Receipt";
    public const string WriteOff = "WriteOff";
    public const string IssueByRequest = "IssueByRequest";
    public const string TransferIn = "TransferIn";
    public const string TransferOut = "TransferOut";
    public const string Adjustment = "Adjustment";
}
