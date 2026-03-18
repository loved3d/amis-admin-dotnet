namespace AmisAdminDotNet.Services;

public static class AdminApiResponse
{
    public static object Ok(object? data = null, string msg = "ok") => new
    {
        status = 0,
        msg,
        data = data ?? new { }
    };

    public static object Fail(string msg) => new
    {
        status = 1,
        msg
    };
}
