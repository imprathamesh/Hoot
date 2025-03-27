using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

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
        var actionName = context.ActionDescriptor.DisplayName;
        var statusCode = context.HttpContext.Response.StatusCode;

        string? responseData = null;

        if (context.Result is ObjectResult objectResult)
        {
            responseData = JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions { WriteIndented = true });
        }
        else if (context.Result is JsonResult jsonResult)
        {
            responseData = JsonSerializer.Serialize(jsonResult.Value, new JsonSerializerOptions { WriteIndented = true });
        }
        else if (context.Result is ContentResult contentResult)
        {
            responseData = contentResult.Content;
        }

        _logger.LogInformation("API Method Executed: {Action} -> Status Code: {StatusCode}, Response: {Response}",
            actionName, statusCode, responseData ?? "No Content");
    }
}
