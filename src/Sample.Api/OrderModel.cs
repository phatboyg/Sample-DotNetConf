namespace Sample.Api
{
    using System;


    public record OrderModel
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; }
        public string Status { get; init; }
    }
}