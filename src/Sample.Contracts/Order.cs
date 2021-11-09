namespace Sample.Contracts
{
    using System;


    public record Order
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; }
        public string Status { get; init; }
    }
}