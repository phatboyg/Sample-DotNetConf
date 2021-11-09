namespace Sample.Contracts
{
    using System;


    public record OrderAccepted
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; }
        public string Status { get; init; }
    }
}