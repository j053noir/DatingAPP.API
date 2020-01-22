using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatinApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatinApp.API.Helpers
{
    public class LogUserActivityFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();

            var userFromRepo = await repo.GetUser(userId);
            userFromRepo.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}
