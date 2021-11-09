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
        InMemoryTestHarness _harness;
        TestOutputLoggerFactory _loggerFactory;
        OrderStateMachine _machine;
        ServiceProvider _provider;
        IStateMachineSagaTestHarness<OrderState, OrderStateMachine> _sagaHarness;

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

            using var scope = _provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<GetOrder>>();

            Response<Order, OrderNotFound> response = await client.GetResponse<Order, OrderNotFound>(new { orderId });

            Assert.That(response.Is<Order>(out var order), Is.True);
            Assert.That(order.Message.Status, Is.EqualTo("Submitted"));

            Assert.That(response.Is<OrderNotFound>(out _), Is.False);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            _loggerFactory = new TestOutputLoggerFactory(true);

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => _loggerFactory);

            services.AddMassTransitInMemoryTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();

                x.AddSagaStateMachineTestHarness<OrderStateMachine, OrderState>();

                x.AddRequestClient<AcceptOrder>();
                x.AddRequestClient<GetOrder>();
            });

            _provider = services.BuildServiceProvider(true);
            _provider.ConfigureLogging();

            _harness = _provider.GetRequiredService<InMemoryTestHarness>();
            _harness.OnConfigureInMemoryBus += configurator =>
            {
                configurator.UseDelayedMessageScheduler();
            };

            _fixtureContext = TestExecutionContext.CurrentContext;

            _loggerFactory.Current = _fixtureContext;

            await _harness.Start();
            _sagaHarness = _provider.GetRequiredService<IStateMachineSagaTestHarness<OrderState, OrderStateMachine>>();
            _machine = _provider.GetRequiredService<OrderStateMachine>();
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            _loggerFactory.Current = _fixtureContext;

            try
            {
                await _harness.Stop();
            }
            finally
            {
                await _provider.DisposeAsync();
            }
        }
    }
}