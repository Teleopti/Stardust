using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Messaging;

namespace Teleopti.MessagingTest.Http
{
	[TestFixture]
	[MessagingTest]
	[Setting("MessageBrokerHttpSenderIntervalMilliseconds", 5000)]
	[Toggle(Toggles.MessageBroker_HttpSenderThrottleRequests_79140)]
	public class HttpSenderThrottleTest
	{
		public IMessageSender Target;
		public MessageBrokerServerBridge Server;
		public FakeTime Time;

		[Test]
		public void ShouldSend()
		{
			Target.Send(new Message());
			Time.Passes("6".Seconds());

			Server.Requests.Should().Have.Count.EqualTo(1);
		}

		[Test]
		[ToggleOff(Toggles.MessageBroker_HttpSenderThrottleRequests_79140)]
		public void ShouldSendDirectlyInOldVersion()
		{
			Target.Send(new Message());

			Server.Requests.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSendSinglesAsMultiple()
		{
			Target.Send(new Message());
			Target.Send(new Message());
			Time.Passes("6".Seconds());

			var request = Server.Requests.Single();
			request.Uri.Should().Contain("NotifyClientsMultiple");
			(request.Data as IEnumerable<Message>).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldSendMultiple()
		{
			Target.SendMultiple(new[] {new Message(), new Message()});
			Target.SendMultiple(new[] {new Message(), new Message()});
			Time.Passes("6".Seconds());

			var request = Server.Requests.Single();
			(request.Data as IEnumerable<Message>).Should().Have.Count.EqualTo(4);
		}

		[Test]
		public void ShouldNotSendBeforeInterval()
		{
			Target.Send(new Message());
			Time.Passes("4".Seconds());

			Server.Requests.Should().Be.Empty();
		}

		[Test]
		[Setting("MessageBrokerHttpSenderMessagesPerRequest", 3)]
		public void ShouldSendBatches()
		{
			Target.Send(new Message());
			Target.Send(new Message());
			Target.Send(new Message());
			Target.Send(new Message());
			Time.Passes("11".Seconds());

			Server.Requests.Should().Have.Count.EqualTo(2);
			(Server.Requests.First().Data as IEnumerable<Message>).Should().Have.Count.EqualTo(3);
			(Server.Requests.Second().Data as IEnumerable<Message>).Should().Have.Count.EqualTo(1);
		}
	}
}