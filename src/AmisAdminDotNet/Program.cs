using System.Text.Json;
using System.Text.Json.Serialization;
using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Models;
using AmisAdminDotNet.Services;

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

app.Run();

public partial class Program;
