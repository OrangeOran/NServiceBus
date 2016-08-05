﻿namespace NServiceBus.AcceptanceTests.Recoverability.Retries
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using ScenarioDescriptors;

    public class When_immediate_retries_with_dtc_on : NServiceBusAcceptanceTest
    {
        [Test]
        public Task Should_do_X_retries_by_default_with_dtc_on()
        {
            return Scenario.Define<Context>(c => { c.Id = Guid.NewGuid(); })
                .WithEndpoint<RetryEndpoint>(b => b
                    .When((session, context) => session.SendLocal(new MessageToBeRetried
                    {
                        Id = context.Id
                    }))
                    .DoNotFailOnErrorMessages())
                .Done(c => c.GaveUpOnRetries)
                .Repeat(r => r.For<AllDtcTransports>())
                .Should(c =>
                {
                    //we add 1 since first call + X retries totals to X+1
                    Assert.AreEqual(maxretries + 1, c.NumberOfTimesInvoked, $"The Immediate Retries should by default retry {maxretries} times");
                    Assert.AreEqual(maxretries, c.Logs.Count(l => l.Message
                        .StartsWith($"Immediate Retry is going to retry message '{c.PhysicalMessageId}' because of an exception:")));
                })
                .Run();
        }

        const int maxretries = 4;

        class Context : ScenarioContext
        {
            public Guid Id { get; set; }

            public int NumberOfTimesInvoked { get; set; }

            public bool GaveUpOnRetries { get; set; }

            public string PhysicalMessageId { get; set; }
        }

        public class RetryEndpoint : EndpointConfigurationBuilder
        {
            public RetryEndpoint()
            {
                PerformDefaultRetries();
                EndpointSetup<DefaultServer>((b, context) =>
                {
                    var scenarioContext = (Context) context.ScenarioContext;
                    b.Notifications.Errors.MessageSentToErrorQueue += (sender, message) => scenarioContext.GaveUpOnRetries = true;
                    var recoverability = b.Recoverability();
                    recoverability.Immediate(settings => settings.NumberOfRetries(maxretries));
                });
            }

            class MessageToBeRetriedHandler : IHandleMessages<MessageToBeRetried>
            {
                public Context TestContext { get; set; }

                public Task Handle(MessageToBeRetried message, IMessageHandlerContext context)
                {
                    if (message.Id != TestContext.Id)
                    {
                        return Task.FromResult(0); // messages from previous test runs must be ignored
                    }

                    TestContext.PhysicalMessageId = context.MessageId;
                    TestContext.NumberOfTimesInvoked++;

                    throw new SimulatedException();
                }
            }
        }

        [Serializable]
        public class MessageToBeRetried : IMessage
        {
            public Guid Id { get; set; }
        }
    }
}