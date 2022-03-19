namespace Sample.Tests
{
    using System.Threading.Tasks;
    using Components.StateMachines;
    using Contracts;
    using Internals;
    using MassTransit;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using NUnit.Framework.Internal;


    public class Submitting_an_order_via_the_state_machine
    {
        TestExecutionContext _fixtureContext;
        ITestHarness _harness;
        TestOutputLoggerFactory _loggerFactory;
        OrderStateMachine _machine;
        ServiceProvider _provider;
        ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness;

        [Test]
        public async Task Should_support_the_status_check()
        {
            var orderId = NewId.NextGuid();

            await _harness.Bus.Publish<SubmitOrder>(new
            {
                orderId,
                OrderNumber = "8675309"
            });

            await _harness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId);

            IRequestClient<GetOrder> client = _harness.GetRequestClient<GetOrder>();

            Response<Order, OrderNotFound> response = await client.GetResponse<Order, OrderNotFound>(new { orderId });

            Assert.That(response.Is<Order>(out Response<Order> order), Is.True);
            Assert.That(order.Message.Status, Is.EqualTo("Submitted"));

            Assert.That(response.Is<OrderNotFound>(out _), Is.False);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            _loggerFactory = new TestOutputLoggerFactory(true);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => _loggerFactory);

            services.AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();

                    cfg.ConfigureEndpoints(context);
                });
            });

            _provider = services.BuildServiceProvider(true);
            _provider.ConfigureLogging();

            _harness = _provider.GetTestHarness();

            _fixtureContext = TestExecutionContext.CurrentContext;

            _loggerFactory.Current = _fixtureContext;

            await _harness.Start();
            _sagaHarness = _provider.GetRequiredService<ISagaStateMachineTestHarness<OrderStateMachine, OrderState>>();
            _machine = _provider.GetRequiredService<OrderStateMachine>();
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            _loggerFactory.Current = _fixtureContext;

            await _provider.DisposeAsync();
        }
    }
}