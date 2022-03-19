namespace Sample.Components.StateMachines
{
    using Contracts;
    using MassTransit;


    public class OrderStateMachine :
        MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => AcceptOrder, x =>
            {
                x.OnMissingInstance(m => m.ExecuteAsync(context => context.RespondAsync<OrderNotFound>(new { context.Message.OrderId })));
            });
            Event(() => GetOrder, x =>
            {
                x.OnMissingInstance(m => m.ExecuteAsync(context => context.RespondAsync<OrderNotFound>(new { context.Message.OrderId })));
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(SubmitOrder)
                    .Then(x => x.Saga.OrderNumber = x.Message.OrderNumber)
                    .TransitionTo(Submitted));

            During(Submitted, Accepted,
                When(AcceptOrder)
                    .TransitionTo(Accepted)
                    .RespondAsync(x => x.Init<Order>(new
                    {
                        x.Message.OrderId,
                        x.Saga.OrderNumber,
                        Status = x.Saga.CurrentState
                    })));

            DuringAny(
                When(SubmitOrder)
                    .Then(x => x.Saga.OrderNumber = x.Message.OrderNumber),
                When(GetOrder)
                    .RespondAsync(x => x.Init<Order>(new
                    {
                        x.Message.OrderId,
                        x.Saga.OrderNumber,
                        Status = x.StateMachine.Accessor.Get(x)
                    })));
        }

        //
        // ReSharper disable UnassignedGetOnlyAutoProperty
        // ReSharper disable MemberCanBePrivate.Global
        public Event<SubmitOrder> SubmitOrder { get; }
        public Event<AcceptOrder> AcceptOrder { get; }
        public Event<GetOrder> GetOrder { get; }

        public State Submitted { get; }
        public State Accepted { get; }
    }
}