
using Microsoft.AspNetCore.Http;
using om.shared.logger.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace om.shared.api.middlewares
{
    public class LogContextEnrichment
    {
        private readonly RequestDelegate next;
        private readonly ILogger _logger;
        public LogContextEnrichment(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            this._logger.ResetContext();
            var properties = new List<KeyValuePair<string, string>>();
            if (context != null)
            {
                properties.Add(new KeyValuePair<string, string>("Path", context.Request.Path.ToString()));
                properties.Add(new KeyValuePair<string, string>("Host", context.Request.Host.ToString()));
                properties.Add(new KeyValuePair<string, string>("Method", context.Request.Method.ToString()));
                var user = context.User;
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    properties.Add(new KeyValuePair<string, string>("UserName", user.Identity.Name));
                    var userId = user.Claims.FirstOrDefault(a => a.Type == "nameid").Value;
                    properties.Add(new KeyValuePair<string, string>("UserId", userId));
                }
                else
                {
                    properties.Add(new KeyValuePair<string, string>("UserName", "Anonymous"));
                    properties.Add(new KeyValuePair<string, string>("UserId", "0"));
                }
            }
            foreach (var property in properties)
            {
                this._logger.SetContext(property.Key, property.Value);
            }
            await next(context);

        }
    }
}
