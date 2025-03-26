using Microsoft.AspNetCore.Mvc.Filters;

namespace Hoot.Filters;

public class ApiLoggingFilter : IActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        _logger.LogInformation("API Method Called: {Method} {Path} -> {Action}\n", method, path, actionName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // You can log after execution if needed.
    }
}
