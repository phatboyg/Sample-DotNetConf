namespace Sample.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;


    public record AcceptOrder
    {
        public Guid OrderId { get; init; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<AcceptOrder>(x => x.OrderId);
        }
    }
}