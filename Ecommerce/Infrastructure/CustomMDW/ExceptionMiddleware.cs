using Azure;
using Ecommerce.Infrastructure.Responses;
using System.Text.Json;
using Ecommerce.Infrastructure.Exceptions;

namespace Ecommerce.Infrastructure.CustomMDW
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        //Make task involke async for handle the exceptions.every middleware must contain this task

        public async Task InvokeAsync(HttpContext context)  //httpconext ha sthe general info about request
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);

            }

        }

        //Make the Handle part

        public static async Task HandleExceptionAsync(HttpContext context, Exception ex) 
        {
            context.Response.ContentType = "application/json";

            //Now herre use switch case to exception

            int statusCode = ex switch
            { 
                BadRequestException => StatusCodes.Status400BadRequest,

                 UnauthorizedException => StatusCodes.Status401Unauthorized,

                ForbiddenException => StatusCodes.Status403Forbidden,

                NotFoundException => StatusCodes.Status404NotFound,

                _ => StatusCodes.Status500InternalServerError

            };

            context.Response.StatusCode = statusCode;


            //Now after this return the repsones

            var response = new ApiResponse<Object>
            {
                sucess = false,
                message = ex.Message,
                data = null

            };

            //Now convert C# reponse to json

            var json = JsonSerializer.Serialize(response);

            //Npw send back the response

            await context.Response.WriteAsync(json);





        }



        //End

    }
}