namespace Sample.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;


    public record SubmitOrder
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<SubmitOrder>(x => x.OrderId);
        }
    }
}