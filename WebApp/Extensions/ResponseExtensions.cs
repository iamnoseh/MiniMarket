using Domain.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Extensions;

public static class ResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Responce<T> response)
    {
        if (response is null)
        {
            return controller.StatusCode(StatusCodes.Status500InternalServerError);
        }

        return controller.StatusCode(response.StatusCode, response);
    }
}
