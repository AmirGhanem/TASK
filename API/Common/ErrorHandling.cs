using Microsoft.AspNetCore.Mvc;
using Application.Utilities.Models;

namespace Common
{
    public static class ErrorHandling
    {
        public static async Task<ActionResult<TResponse>> HandleCommandResult<TResponse>(this ControllerBase cb, Task<TResponse> commandTask) where TResponse : BaseCommandResult
        {
            var result = await commandTask;
            if (!result.IsSuccess)
            {
                return cb.BadRequest(result);
            }
            return cb.Ok(result);
        }
    }
}
