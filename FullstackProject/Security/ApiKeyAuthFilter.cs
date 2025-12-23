using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FullstackProject.Security;

public sealed class ApiKeyAuthFilter : IAsyncActionFilter
{
    private readonly string _requiredPermission;

    public ApiKeyAuthFilter(string requiredPermission)
    {
        _requiredPermission = requiredPermission;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Required header: X-Api-Key
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var keyValues))
        {
            context.Result = new UnauthorizedResult(); // 401
            return;
        }

        var apiKey = keyValues.ToString().Trim();

        // Keys from the brief
        var readKey = "FS_Read";
        var readWriteKey = "FS_ReadWrite";

        // Validate key itself
        var isRead = string.Equals(apiKey, readKey, StringComparison.Ordinal);
        var isReadWrite = string.Equals(apiKey, readWriteKey, StringComparison.Ordinal);

        if (!isRead && !isReadWrite)
        {
            context.Result = new UnauthorizedResult(); // 401
            return;
        }

        // Permission check: ReadWrite can do everything, Read cannot do SetMap
        var needsReadWrite = string.Equals(_requiredPermission, "ReadWrite", StringComparison.OrdinalIgnoreCase);

        if (needsReadWrite && !isReadWrite)
        {
            context.Result = new UnauthorizedResult(); // 401 (wrong permission)
            return;
        }

        await next();
    }
}
