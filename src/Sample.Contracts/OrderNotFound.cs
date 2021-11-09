namespace Sample.Contracts
{
    using System;


    public record OrderNotFound
    {
        public Guid OrderId { get; init; }
    }
}