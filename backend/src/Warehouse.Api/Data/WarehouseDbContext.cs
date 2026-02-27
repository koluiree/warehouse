using Microsoft.EntityFrameworkCore;

namespace Warehouse.Api.Data;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<IssueRequest> IssueRequests => Set<IssueRequest>();
    public DbSet<IssueRequestItem> IssueRequestItems => Set<IssueRequestItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Username).HasColumnName("username");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasColumnType("tinyint(1)");
            entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.AssignedAt).HasColumnName("assigned_at");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.Position).HasColumnName("position");
            entity.Property(x => x.Phone).HasColumnName("phone");
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasColumnType("tinyint(1)");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Sku).HasColumnName("sku");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Unit).HasColumnName("unit");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.ToTable("stock_balances");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<IssueRequest>(entity =>
        {
            entity.ToTable("issue_requests");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequesterId).HasColumnName("requester_id");
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.ApprovedById).HasColumnName("approved_by_id");
            entity.Property(x => x.Comment).HasColumnName("comment");
        });

        modelBuilder.Entity<IssueRequestItem>(entity =>
        {
            entity.ToTable("issue_request_items");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequestId).HasColumnName("request_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.RequestedQty).HasColumnName("requested_qty");
            entity.Property(x => x.IssuedQty).HasColumnName("issued_qty");
            entity.HasIndex(x => new { x.RequestId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Type).HasColumnName("type");
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.DocumentNumber).HasColumnName("document_number");
            entity.Property(x => x.Comment).HasColumnName("comment");
            entity.Property(x => x.PerformedById).HasColumnName("performed_by_id");
            entity.Property(x => x.RequestId).HasColumnName("request_id");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.TokenHash).HasColumnName("token_hash");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
