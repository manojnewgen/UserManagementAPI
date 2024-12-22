using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log Request
            var request = await FormatRequest(context.Request);
            _logger.LogInformation($"Request: {request}");

            // Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                // Set the temporary response body
                context.Response.Body = responseBody;

                // Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                // Log Response
                var response = await FormatResponse(context.Response);
                _logger.LogInformation($"Response: {response}");

                // Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

       private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;
            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }


        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return $"{response.StatusCode}: {text}";
        }
    }
}
