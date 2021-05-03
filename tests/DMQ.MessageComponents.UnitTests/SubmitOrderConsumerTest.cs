using DMQ.MessageComponents.Consumers;
using DMQ.MessageContracts;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.UnitTests
{
    [TestFixture]
    public class SubmitOrderConsumerTest
    {
        [Test]
        public async Task Should_consume_submit_order_command()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.InputQueueSendEndpoint.Send<ISubmitOrder>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                // Since message is sent and no response is expected
                Assert.That(harness.Sent.Select<IOrderSubmissionAccepted>().Any(), Is.False);
                Assert.That(harness.Sent.Select<IOrderSubmissionRejected>().Any(), Is.False);

            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_respond_with_acceptance_if_ok()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try 
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                var response = await requestClient.GetResponse<IOrderSubmissionAccepted>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                Assert.That(harness.Sent.Select<IOrderSubmissionAccepted>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_respond_with_rejected_for_test_customer()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "TEST";

                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                var response = await requestClient.GetResponse<IOrderSubmissionRejected>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                Assert.That(harness.Sent.Select<IOrderSubmissionRejected>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_publish_order_submitted_event()
        {
            // Mass Transit test harness has in-memory transport and in-memory saga repository
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                Guid orderId = Guid.NewGuid();
                string customerNumber = "123";

                await harness.InputQueueSendEndpoint.Send<ISubmitOrder>(new
                {
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow,
                    CustomerNumber = customerNumber
                });

                Assert.That(harness.Published.Select<IOrderSubmitted>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }

        }
    }
}
