using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using om.shared.api.middlewares.Models;
using om.shared.logger.Interfaces;

namespace om.shared.api.middlewares.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;
        public ExceptionFilter(ILogger logger) : base()
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext exceptionContext)
        {
            var errorResponse = new ErrorResponseMessage()
            {
                Message = exceptionContext.Exception.Message,
                FriendlyMessage = "Unexpected error has occured. Kindly contact support team for more details."
            };
            this._logger.LogError(exceptionContext.Exception);
            exceptionContext.Result = new BadRequestObjectResult(errorResponse);
        }
    }
}
