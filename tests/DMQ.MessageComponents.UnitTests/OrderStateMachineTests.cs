using Automatonymous.Graphing;
using Automatonymous.Visualizer;
using DMQ.MessageComponents.StateMachines;
using DMQ.MessageContracts;
using MassTransit.Testing;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.UnitTests
{
    [TestFixture]
    public class OrderStateMachineTests
    {
        [Test]
        public async Task Should_create_a_state_machine_instance()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.Bus.Publish<IOrderSubmitted>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });
                
                // Expect a saga instance to be created
                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Avoid race condition.
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var instance = saga.Sagas.Contains(instanceId.Value);
                Assert.That(instance.CustomerNumber, Is.EqualTo(customerNumber));
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_response_to_check_status_request()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.Bus.Publish<IOrderSubmitted>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Avoid race condition.
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var resquestClient = await harness.ConnectRequestClient<ICheckOrder>();
                var response = await resquestClient.GetResponse<IOrderStatus>(new 
                {
                    OrderId = orderId
                });

                Assert.That(response.Message.State, Is.EqualTo(orderStateMachine.Submitted.Name));
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_cancel_when_customer_account_closed()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.Bus.Publish<IOrderSubmitted>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Avoid race condition.
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                // Close account
                await harness.Bus.Publish<ICustomerAccountClosed>(new
                {
                    CustomerId = orderId,
                    CustomerNumber = customerNumber
                });

                // Avoid race condition.
                instanceId = await saga.Exists(orderId, x => x.Canceled);
                Assert.That(instanceId, Is.Not.Null);

            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_be_accepted_state_when_order_is_accepted()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.Bus.Publish<IOrderSubmitted>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Avoid race condition.
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                // Accept
                await harness.Bus.Publish<IOrderAccepted>(new
                {
                    OrderId = orderId
                });

                // Avoid race condition.
                instanceId = await saga.Exists(orderId, x => x.Accepted);
                Assert.That(instanceId, Is.Not.Null);

            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public void Show_me_the_state_machine()
        {
            var orderStateMachine = new OrderStateMachine();

            var graph = orderStateMachine.GetGraph();

            var generator = new StateMachineGraphvizGenerator(graph);

            string dots = generator.CreateDotFile();

            Console.WriteLine(dots);
        }
    }
}
