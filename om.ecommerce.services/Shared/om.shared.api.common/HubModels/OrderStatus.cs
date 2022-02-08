using System;

namespace om.shared.signalr.HubModels
{
    public class OrderStatus
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string SellerId { get; set; }
        public string ProductId { get; set; }
        public string CurrentStatus { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset LastModifiedDate { get; set; }
    }
}
