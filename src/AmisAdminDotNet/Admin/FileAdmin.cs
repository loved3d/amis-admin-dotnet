using AmisAdminDotNet.AmisComponents;
using AmisAdminDotNet.Services;
using Microsoft.AspNetCore.Http;

namespace AmisAdminDotNet.Admin;

/// <summary>
/// File-upload admin. Corresponds to Python <c>site.FileAdmin</c>.
/// Registers a <c>POST /upload</c> endpoint that accepts a multipart file,
/// saves it under <see cref="FileDirectory"/>, and returns the public URL.
/// </summary>
public class FileAdmin : RouterAdmin
{
    /// <inheritdoc/>
    public override string RouterPath => "file";

    /// <inheritdoc/>
    public override string Label => "File Upload";

    /// <summary>
    /// Directory where uploaded files are saved.
    /// Defaults to <c>"wwwroot/upload"</c> (relative to the working directory).
    /// </summary>
    public virtual string FileDirectory => "wwwroot/upload";

    /// <summary>
    /// URL prefix used to construct the public file URL returned to the client.
    /// Defaults to <c>"/upload"</c>.
    /// </summary>
    public virtual string FileUrlPrefix => "/upload";

    /// <summary>Maximum allowed file size in bytes. Defaults to 10 MB.</summary>
    public virtual long MaxFileSize => 10 * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions (lower-case, including the leading dot, e.g. <c>".png"</c>).
    /// An empty array means all extensions are accepted.
    /// </summary>
    public virtual string[] AllowedExtensions => [];

    /// <summary>
    /// Returns a placeholder page schema. The actual upload functionality is
    /// provided by the route registered in <see cref="RegisterRoutes"/>.
    /// </summary>
    public override Page BuildPageSchema() =>
        new() { Title = "File Upload", Body = $"File upload endpoint: POST {RouterPrefix}/upload" };

    /// <summary>
    /// Registers the file-upload endpoint at <c>POST {RouterPrefix}/upload</c>.
    /// </summary>
    public override void RegisterRoutes(WebApplication app)
    {
        var prefix = RouterPrefix;

        app.MapPost(prefix + "/upload", async (IFormFile file, HttpContext ctx) =>
        {
            // The multipart form-data field must be named "file" to bind to the IFormFile parameter.
            if (!HasPagePermission(ctx))
                return Results.Json(AdminApiResponse.Fail("Unauthorized"), statusCode: 401);

            // Validate file size
            if (file.Length > MaxFileSize)
                return Results.Json(AdminApiResponse.Fail(
                    $"File too large. Maximum allowed size: {MaxFileSize / 1024 / 1024} MB."));

            // Validate extension
            if (AllowedExtensions.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    return Results.Json(AdminApiResponse.Fail(
                        $"File extension '{ext}' is not allowed."));
            }

            // Save file with a GUID filename to avoid collisions
            Directory.CreateDirectory(FileDirectory);
            var extension = Path.GetExtension(file.FileName);
            var fileName  = $"{Guid.NewGuid()}{extension}";
            var filePath  = Path.Combine(FileDirectory, fileName);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var url = $"{FileUrlPrefix.TrimEnd('/')}/{fileName}";
            return Results.Json(AdminApiResponse.Ok(new { value = url }));
        }).DisableAntiforgery();
    }
}
