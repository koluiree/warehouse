namespace Warehouse.Web.Services;

public class AppSession
{
    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public event Action? Changed;

    public void Set(string token, string username, IEnumerable<string> roles)
    {
        Token = token;
        Username = username;
        Roles = roles.ToList();
        Changed?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        Username = null;
        Roles = [];
        Changed?.Invoke();
    }
}
