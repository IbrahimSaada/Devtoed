using Devoted.Business.Error;
using Devoted.Domain.Sql.Response.Base;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Devoted.API.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ItemNotFoundOrNullError exception)
            {
                await HandleException(context, exception, exception.Message);
            }
            catch (UserError exception)
            {
                await HandleException(context, exception, exception.Message);
            }
            catch (Exception exception)
            {
                Debug.Print(exception.Message);
                await HandleException(context, exception, "An exception has occurred while processing the request.");
            }
        }

        private async Task HandleException(HttpContext context, Exception exception, string exceptionMessage)
        {
            var statusCode = GetStatusCodeForException(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var responseMsg = new BaseResponse()
            {
                Message = exceptionMessage
            };

            var json = JsonConvert.SerializeObject(responseMsg, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await context.Response.WriteAsync(json);
        }

        private int GetStatusCodeForException(Exception exception)
        {
            return exception switch
            {
                UserError _ => 400,
                ItemNotFoundOrNullError _ => 404,
                _ => 500,
            };
        }
    }
}

