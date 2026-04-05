using Microsoft.AspNetCore.Mvc;
using RealTimeChatApp.DTOs;
using System.Runtime.CompilerServices;

namespace RealTimeChatApp.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult ToActionResult<T>(this OperationResult<T> result)
        {
            switch (result.StatusCode)
            {
                case 200: return new OkObjectResult(result.Data);
                case 201: return new OkObjectResult(result.Data) { StatusCode = 201 };
                case 204: return new NoContentResult();
                case 400: return new BadRequestObjectResult(result.ErrorMessage);
                case 403: return new ForbidResult();
                case 404: return new NotFoundObjectResult(result.ErrorMessage);
                default: return new ObjectResult(result.ErrorMessage);
            }
         }
    }
}
