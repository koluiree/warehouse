using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Api.Data;
using Warehouse.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:7173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<WarehouseDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Port=3306;Database=warehouse_db;User=root;Password=qweqwe2;GuidFormat=None;";
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

var jwt = configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? "dev-super-secret-key-change-me";
var issuer = jwt["Issuer"] ?? "warehouse-api";
var audience = jwt["Audience"] ?? "warehouse-client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

    if (!await db.Departments.AnyAsync())
    {
        db.Departments.Add(new Department
        {
            Name = "Администрация",
            Code = "ADM",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == "Admin");
    if (adminRole is null)
    {
        adminRole = new Role { Name = "Admin" };
        db.Roles.Add(adminRole);
        await db.SaveChangesAsync();
    }

    var adminUser = await db.Users.FirstOrDefaultAsync(x => x.Username == "admin");
    if (adminUser is null)
    {
        adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin",
            Email = "admin@warehouse.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin12345"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();
    }

    var hasAdminRole = await db.UserRoles.AnyAsync(x => x.UserId == adminUser.Id && x.RoleId == adminRole.Id);
    if (!hasAdminRole)
    {
        db.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "Warehouse.Api" }));

app.Run();
