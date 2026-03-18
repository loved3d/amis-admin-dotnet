using System.Text.Json;
using System.Text.Json.Serialization;
using AmisAdminDotNet.Admin;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Demo;
using AmisAdminDotNet.Models;
using AmisAdminDotNet.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── JSON serialization ────────────────────────────────────────────────────
// Configure camelCase naming + ignore null values to match the amis SDK
// convention and the Python fastapi-amis-admin JSON output.
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// ─── CORS / app settings ───────────────────────────────────────────────────
var appSettings = AppSettings.FromConfiguration(builder.Configuration);
var adminSettings = appSettings.ToAdminSiteSettings();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(appSettings);
builder.Services.AddSingleton(adminSettings);
builder.Services.AddSingleton<II18nService>(_ => new I18nService(new Dictionary<string, string>
{
    ["admin.title"] = "Amis Admin .NET Core",
    ["tabs.dashboard"] = "Dashboard",
    ["tabs.users"] = "Users",
    ["users.create"] = "Create user",
    ["users.edit"] = "Edit user",
    ["users.deleteConfirm"] = "Delete user ${name}?",
    ["common.search"] = "Search",
    ["common.apply"] = "Apply",
    ["common.keywords"] = "Keywords",
    ["users.searchPlaceholder"] = "Search by name, email or role",
    ["common.edit"] = "Edit",
    ["common.delete"] = "Delete"
}));

if (adminSettings.CorsOrigins.Length > 0)
{
    builder.Services.AddCors(opts =>
        opts.AddDefaultPolicy(policy =>
            policy.WithOrigins(adminSettings.CorsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()));
}

// ─── Application services ──────────────────────────────────────────────────
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<AdminSchemaService>();

// ─── AdminSite demo: DI registration ─────────────────────────────────────
// NOTE: DemoDbContext is registered as a singleton instance intentionally.
// AdminApp.RegisterAdmin<T>() resolves admin instances from the root
// IServiceProvider (once at startup), which cannot resolve scoped services.
// Using a singleton DbContext is safe here because the in-memory EF Core
// provider is single-threaded by nature and this is a demo; in a production
// app you would use IServiceScopeFactory inside RegisterAdmin to create a
// per-request scope, or switch to a different admin resolution strategy.
var demoDbOptions = new DbContextOptionsBuilder<DemoDbContext>()
    .UseInMemoryDatabase("demo-admin")
    .Options;
builder.Services.AddSingleton(new DemoDbContext(demoDbOptions));
builder.Services.AddTransient<DemoAdmin>(sp =>
    new DemoAdmin(sp.GetRequiredService<DemoDbContext>()));

var app = builder.Build();

if (adminSettings.CorsOrigins.Length > 0)
    app.UseCors();

app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/admin"));

app.MapGet("/admin", () => Results.Content(AdminHostPage.RenderHtml(appSettings), "text/html; charset=utf-8"));

app.MapGet("/api/admin/schema", (AdminSchemaService schemaService) =>
    Results.Text(schemaService.BuildAdminPageJson(), "application/json; charset=utf-8"));

app.MapGet("/api/admin/users", (IUserStore userStore, string? keywords, int page = 1, int perPage = 10) =>
{
    var result = userStore.Query(keywords, page, perPage);
    return Results.Json(AdminApiResponse.Ok(new { items = result.Items, total = result.Total }));
});

app.MapPost("/api/admin/users", (SaveUserRequest request, IUserStore userStore) =>
{
    if (!UserRequestValidator.TryNormalize(request, out var normalized, out var error))
    {
        return Results.Json(AdminApiResponse.Fail(error!));
    }

    var created = userStore.Create(normalized);
    return Results.Json(AdminApiResponse.Ok(new { item = created }, "User created."));
});

app.MapPut("/api/admin/users/{id:int}", (int id, SaveUserRequest request, IUserStore userStore) =>
{
    if (!UserRequestValidator.TryNormalize(request, out var normalized, out var error))
    {
        return Results.Json(AdminApiResponse.Fail(error!));
    }

    var updated = userStore.Update(id, normalized);
    return updated is null
        ? Results.Json(AdminApiResponse.Fail($"User {id} was not found."))
        : Results.Json(AdminApiResponse.Ok(new { item = updated }, "User updated."));
});

app.MapDelete("/api/admin/users/{id:int}", (int id, IUserStore userStore) =>
{
    return userStore.Delete(id)
        ? Results.Json(AdminApiResponse.Ok(msg: "User deleted."))
        : Results.Json(AdminApiResponse.Fail($"User {id} was not found."));
});

// ─── AdminSite demo: demonstrates the full ModelAdmin framework ─────────────
// Build an AdminSite, register the DemoAdmin (which maps DemoItem to amis schema
// and CRUD routes automatically), then mount it.  This wires up:
//   GET  /api/demo/schema             — amis Page schema for the demo admin
//   GET  /admin/demo-items            — paged list
//   POST /admin/demo-items            — create
//   PUT  /admin/demo-items/{id}       — update
//   DELETE /admin/demo-items/{id}     — delete
var demoSite = new AdminSite(adminSettings, app.Services);
var demoApp  = demoSite.CreateApp("Demo");
demoApp.RegisterAdmin<DemoAdmin>();
demoApp.Mount(app);

app.MapGet("/api/demo/schema", () =>
    Results.Text(
        JsonSerializer.Serialize(demoSite.BuildPageSchema(), AmisJsonOptions.Default),
        "application/json; charset=utf-8"));

app.Run();

public partial class Program;
