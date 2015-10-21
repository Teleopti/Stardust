using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Core.MessageBroker;
using Teleopti.Ccc.WebTest.Areas.Anywhere;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	[TestFixture]
	public class MessageBrokerHubTest
	{
		[Test]
		public void ShouldPongOnPing()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR(), null, new FakeMailboxRepository(), null, new Now()));
			var hubBuilder = new TestHubBuilder();
			var _ponged = false;
			var client = hubBuilder.FakeClient(
				"Pong",
				() =>
				{
					_ponged = true;
				});
			hubBuilder.SetupHub(target, client);

			target.Ping();

			_ponged.Should().Be.True();
		}

		[Test]
		public void Ping_WithAnIdentification_ShouldPongWithsameIdentification()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR(), null, new FakeMailboxRepository(), null, new Now()));
			var hubBuilder = new TestHubBuilder();
			var pongedWith = 0d;
			var client = hubBuilder.FakeClient<double>(
				"Pong",
				d =>
				{
					pongedWith = d;
				});
			hubBuilder.SetupHub(target, client);

			target.PingWithId(12);

			pongedWith.Should().Be(12);
		}

		[Test]
		public void Ping_WithNumberOfMessages_ShouldSendThatNumberOfMessageswithSignalR()
		{
			var expectedNumberOfSentMessages = 17;
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR(), null, new FakeMailboxRepository(), null, new Now()));
			var hubBuilder = new TestHubBuilder();

			var numberOfpongs = 0;
			var client = hubBuilder.FakeClient(
				"Pong",
				() =>
				{
					numberOfpongs++;
				});
			hubBuilder.SetupHub(target, client);

			

			target.Ping(expectedNumberOfSentMessages);

			Assert.That(numberOfpongs, Is.EqualTo(expectedNumberOfSentMessages));

		}

	}
}
