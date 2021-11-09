namespace Sample.Components.StateMachines
{
    using System;
    using Automatonymous;


    public class OrderState :
        SagaStateMachineInstance
    {
        public string CurrentState { get; set; }

        public string OrderNumber { get; set; }

        public Guid CorrelationId { get; set; }
    }
}