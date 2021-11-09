namespace Sample.Components.StateMachines
{
    using Automatonymous;
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
                    .Then(x => x.Instance.OrderNumber = x.Data.OrderNumber)
                    .TransitionTo(Submitted));

            During(Submitted, Accepted,
                When(AcceptOrder)
                    .TransitionTo(Accepted)
                    .RespondAsync(x => x.Init<Order>(new
                    {
                        x.Data.OrderId,
                        x.Instance.OrderNumber,
                        Status = x.Instance.CurrentState
                    })));

            DuringAny(
                When(SubmitOrder)
                    .Then(x => x.Instance.OrderNumber = x.Data.OrderNumber),
                When(GetOrder)
                    .RespondAsync(x => x.Init<Order>(new
                    {
                        x.Data.OrderId,
                        x.Instance.OrderNumber,
                        Status = x.Instance.CurrentState
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