using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http.Mailbox
{
    [TestFixture]
    [IoCTest]
    [Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
    public class EmptyConnectionStringTest : ISetup
    {
        public IMessageListener Target;
        public FakeHttpServer Server;

        public void Setup(ISystem system, IIocConfiguration configuration)
        {
            system.UseTestDouble(new FakeUrl("")).For<IMessageBrokerUrl>();
            system.UseTestDouble<FakeHttpServer>().For<IHttpServer>();
            system.UseTestDouble<FakeTime>().For<ITime>();
            system.UseTestDouble(new FakeConfigReader("MessageBrokerMailboxPollingIntervalInSeconds", "60")).For<IConfigReader>();
        }

        [Test]
        public void ShouldNotCreatMailboxWhenEmptyConnectionString()
        {
            Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

            Server.Requests.Should().Be.Empty();
        }
    }
}
