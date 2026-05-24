using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using USJR_COMES_WEBSITE.APIs;
using USJR_COMES_WEBSITE.Services;
using USJR_COMES_WEBSITE.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Read API URL from wwwroot/appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7265/";
if (!apiBaseUrl.EndsWith('/')) apiBaseUrl += '/';

var clientBaseUrl = builder.Configuration["ClientBaseUrl"] ?? "https://localhost:7118/";

// Configure the static ApiEndpoints helper used throughout the app
ApiEndpoints.ServerBaseUrl = apiBaseUrl.TrimEnd('/');
ApiEndpoints.ClientBaseUrl = clientBaseUrl.TrimEnd('/');

// ── HttpClient registrations ────────────────────────────────────────────────
// Named "api" client used by OfflineSyncService for queue replay
builder.Services.AddHttpClient("api", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IUserServices, UserServices>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICreateAccountService, CreateAccountService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.Timeout = TimeSpan.FromSeconds(90);
});

builder.Services.AddHttpClient<INewsFeedService, NewsFeedService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IServicesOfferedService, ServicesOfferedService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IOfficersService, OfficersService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAboutUsService, AboutUsService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMembersService, MembersService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IBudgetService, BudgetService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IHomeService, HomeService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISiteSettingsService, SiteSettingsService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

// Named general client (CSV import, password reset, etc.)
builder.Services.AddHttpClient("general", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.Timeout = TimeSpan.FromMinutes(5);
});

// Named SiteSettings client (kept for compatibility)
builder.Services.AddHttpClient("SiteSettings", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

// ── Scoped services (one instance per browser session) ─────────────────────
builder.Services.AddScoped<UserStateService>();
builder.Services.AddScoped<IOfflineSyncService, OfflineSyncService>();

// ── ViewModels ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<UserViewModel>();
builder.Services.AddScoped<CreateAccountViewModel>();
builder.Services.AddScoped<ManagementPostsViewModel>();
builder.Services.AddScoped<ManagementServicesViewModel>(sp =>
    new ManagementServicesViewModel(
        sp.GetRequiredService<IServicesOfferedService>(),
        sp.GetRequiredService<UserStateService>(),
        sp.GetRequiredService<IOfflineSyncService>()));

await builder.Build().RunAsync();
