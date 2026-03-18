using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AmisAdminDotNet.Admin.Extensions;

/// <summary>
/// Model admin that requires ASP.NET Core authentication (<c>IsAuthenticated</c>) before
/// granting access to the page.
/// Corresponds to Python <c>fastapi-user-auth</c> login requirement.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class AuthenticatedModelAdmin<TEntity, TKey, TDbContext>
    : ModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected AuthenticatedModelAdmin(TDbContext db) : base(db) { }

    /// <summary>
    /// Returns <c>true</c> only when the request user is authenticated
    /// (<c>IsAuthenticated == true</c>).
    /// </summary>
    public override bool HasPagePermission(HttpContext context) =>
        context.User?.Identity?.IsAuthenticated == true;
}

/// <summary>
/// Model admin that requires the user to be a member of a specific role.
/// Override <see cref="RequiredRole"/> in subclasses to specify the role name.
/// Corresponds to ASP.NET Core role-based authorization.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class RoleBasedModelAdmin<TEntity, TKey, TDbContext>
    : AuthenticatedModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected RoleBasedModelAdmin(TDbContext db) : base(db) { }

    /// <summary>
    /// The role name required to access this admin.
    /// An empty string means no specific role is required beyond authentication.
    /// </summary>
    public virtual string RequiredRole => string.Empty;

    /// <summary>
    /// Returns <c>true</c> when the user is authenticated and belongs to <see cref="RequiredRole"/>
    /// (or <see cref="RequiredRole"/> is empty).
    /// </summary>
    public override bool HasPagePermission(HttpContext context) =>
        base.HasPagePermission(context) &&
        (string.IsNullOrEmpty(RequiredRole) || context.User.IsInRole(RequiredRole));
}

/// <summary>
/// Model admin that enforces an ASP.NET Core Authorization Policy.
/// Inject <see cref="IAuthorizationService"/> and override <see cref="RequiredPolicy"/>
/// to specify which policy is checked.
/// </summary>
/// <typeparam name="TEntity">EF Core entity type.</typeparam>
/// <typeparam name="TKey">Primary-key type.</typeparam>
/// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> type.</typeparam>
public abstract class PolicyBasedModelAdmin<TEntity, TKey, TDbContext>
    : AuthenticatedModelAdmin<TEntity, TKey, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    private readonly IAuthorizationService _authorizationService;

    protected PolicyBasedModelAdmin(TDbContext db, IAuthorizationService authorizationService)
        : base(db)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// The ASP.NET Core Authorization Policy name required to access this admin.
    /// An empty string means no policy check beyond authentication.
    /// </summary>
    public virtual string RequiredPolicy => string.Empty;

    /// <summary>
    /// Synchronous check: only verifies authentication (policy check requires async).
    /// </summary>
    public override bool HasPagePermission(HttpContext context) =>
        base.HasPagePermission(context);

    /// <summary>
    /// Async check: verifies authentication then evaluates the <see cref="RequiredPolicy"/>
    /// using <see cref="IAuthorizationService"/>.
    /// </summary>
    public override async Task<bool> HasPagePermissionAsync(HttpContext context)
    {
        if (!base.HasPagePermission(context)) return false;
        if (string.IsNullOrEmpty(RequiredPolicy)) return true;
        var result = await _authorizationService.AuthorizeAsync(context.User, RequiredPolicy);
        return result.Succeeded;
    }
}
