using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Warehouse.Web;
using Warehouse.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://localhost:5071/") });
builder.Services.AddScoped<AppSession>();
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
