using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaceDayMVC.Services;

namespace RaceDayMVC.Filters;

/// <summary>
/// Turns API failures into friendly pages so users never see raw exceptions.
/// An expired API token clears the session and sends the user to the login page.
/// </summary>
public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var http = context.HttpContext;

        if (context.Exception is ApiUnauthorizedException)
        {
            http.Session.Clear();
            var tempData = http.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(http);
            tempData["Error"] = "Your session has expired. Please log in again.";
            context.Result = new RedirectToActionResult("Login", "Account", null);
            context.ExceptionHandled = true;
            return;
        }

        string? message = context.Exception switch
        {
            ApiException ex => ex.Message,
            HttpRequestException => "RaceDay can't reach its server right now. Please try again in a moment.",
            TaskCanceledException => "RaceDay took too long to respond. Please try again.",
            _ => null
        };
        if (message is null) return; // anything else goes to the global exception handler page

        logger.LogWarning(context.Exception, "API call failed for {Path}", http.Request.Path);
        context.Result = new ViewResult
        {
            ViewName = "Error",
            StatusCode = StatusCodes.Status503ServiceUnavailable,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ["Message"] = message
            }
        };
        context.ExceptionHandled = true;
    }
}
