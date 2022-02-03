
using Microsoft.AspNetCore.Http;
using om.shared.logger.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using om.shared.logger;

namespace om.shared.api.middlewares
{
    public class LogContextEnrichment
    {
        private readonly RequestDelegate next;

        public LogContextEnrichment(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ILogger logger)
        {
            logger.ResetContext();
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
                logger.SetContext(property.Key, property.Value);
            }
            await next(context);

        }
    }
}
