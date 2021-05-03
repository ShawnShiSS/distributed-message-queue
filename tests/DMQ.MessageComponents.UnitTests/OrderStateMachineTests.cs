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

                var instance = saga.Sagas.Contains(instanceId.Value);
                Assert.That(instance.CustomerNumber, Is.EqualTo(customerNumber));

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
    }
}
