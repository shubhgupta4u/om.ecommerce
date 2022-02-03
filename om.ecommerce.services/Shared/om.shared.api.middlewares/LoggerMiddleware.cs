using Microsoft.AspNetCore.Http;
using om.shared.logger.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace om.shared.api.middlewares
{
    class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LoggerMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //Stream bodyStream = httpContext.Response.Body;
            //MemoryStream responseBodyStream = new MemoryStream();
            //httpContext.Response.Body = responseBodyStream;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await _next(httpContext);
            stopwatch.Stop();
            //responseBodyStream.Seek(0, SeekOrigin.Begin);

            int responseCode = httpContext.Response.StatusCode;
            this._logger.SetContext("RequestTime", stopwatch.ElapsedMilliseconds);
            this._logger.SetContext("ResponseCode", responseCode);
            this._logger.Log.Information("Api Request Completed");

            //responseBodyStream.Seek(0, SeekOrigin.Begin);
            //await responseBodyStream.CopyToAsync(bodyStream);
        }
    }
}
