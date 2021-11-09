namespace Sample.Contracts
{
    using System;
    using System.Runtime.CompilerServices;
    using MassTransit;
    using MassTransit.Topology.Topologies;


    public record GetOrder
    {
        public Guid OrderId { get; init; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<GetOrder>(x => x.OrderId);
        }
    }
}