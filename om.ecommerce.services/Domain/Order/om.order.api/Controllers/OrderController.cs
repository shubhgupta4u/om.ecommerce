using Microsoft.AspNetCore.Mvc;
using om.shared.logger.Interfaces;
using om.shared.signalr;
using om.shared.signalr.HubModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace om.ecommerce.order.api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IHubClient hubClient;
        protected readonly ILogger _logger;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public OrderController(ILogger logger, IHubClient hubClient)
        {
            this._logger = logger;
            this.hubClient = hubClient;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Order
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderStatus orderStatus)
        {
            await this.hubClient.SendMessage<OrderStatus>(orderStatus);
            return Ok("Success");
        }
    }
}
