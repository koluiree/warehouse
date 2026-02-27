using System.Net.Http.Headers;
using System.Net.Http.Json;
using Warehouse.Web.Models;

namespace Warehouse.Web.Services;

public class ApiClient(HttpClient httpClient, AppSession session)
{
    private void ApplyAuthHeader()
    {
        httpClient.DefaultRequestHeaders.Authorization = session.IsAuthenticated
            ? new AuthenticationHeaderValue("Bearer", session.Token)
            : null;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest payload)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", payload);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<bool> RegisterAsync(RegisterRequest payload)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/register", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<DepartmentDto>> GetDepartmentsAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<DepartmentDto>>("api/departments") ?? [];
    }

    public async Task<bool> CreateDepartmentAsync(CreateDepartmentRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/departments", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<EmployeeDto>>("api/employees") ?? [];
    }

    public async Task<List<ProductDto>> GetProductsAsync(string? search = null)
    {
        ApplyAuthHeader();
        var url = string.IsNullOrWhiteSpace(search) ? "api/products" : $"api/products?search={Uri.EscapeDataString(search)}";
        return await httpClient.GetFromJsonAsync<List<ProductDto>>(url) ?? [];
    }

    public async Task<bool> CreateProductAsync(CreateProductRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/products", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<WarehouseDto>> GetWarehousesAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<WarehouseDto>>("api/warehouses") ?? [];
    }

    public async Task<bool> CreateWarehouseAsync(CreateWarehouseRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/warehouses", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<BalanceDto>> GetBalancesAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<BalanceDto>>("api/inventory/balances") ?? [];
    }

    public async Task<bool> ReceiptAsync(InventoryOperationRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/inventory/receipt", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> WriteOffAsync(InventoryOperationRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/inventory/writeoff", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<IssueRequestDto>> GetRequestsAsync(string? status = null)
    {
        ApplyAuthHeader();
        var url = string.IsNullOrWhiteSpace(status) ? "api/requests" : $"api/requests?status={Uri.EscapeDataString(status)}";
        return await httpClient.GetFromJsonAsync<List<IssueRequestDto>>(url) ?? [];
    }

    public async Task<List<IssueRequestItemDto>> GetRequestItemsAsync(long id)
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<IssueRequestItemDto>>($"api/requests/{id}/items") ?? [];
    }

    public async Task<bool> CreateRequestAsync(CreateIssueRequestRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/requests", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveRequestAsync(long id, string? comment = null)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync($"api/requests/{id}/approve", new ProcessIssueRequestRequest(comment));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectRequestAsync(long id, string? comment = null)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync($"api/requests/{id}/reject", new ProcessIssueRequestRequest(comment));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> StartIssueAsync(long id)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsync($"api/requests/{id}/start-issue", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IssueItemAsync(long id, IssueItemRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync($"api/requests/{id}/issue-item", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<StockMovementDto>> GetMovementsAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<StockMovementDto>>("api/movements") ?? [];
    }

    public async Task<List<AdminUserDto>> GetAdminUsersAsync()
    {
        ApplyAuthHeader();
        return await httpClient.GetFromJsonAsync<List<AdminUserDto>>("api/admin/users") ?? [];
    }

    public async Task<bool> AssignRoleAsync(AssignRoleRequest payload)
    {
        ApplyAuthHeader();
        var response = await httpClient.PostAsJsonAsync("api/admin/assign-role", payload);
        return response.IsSuccessStatusCode;
    }
}
