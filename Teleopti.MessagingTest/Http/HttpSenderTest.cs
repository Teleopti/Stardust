using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.TestCommon;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http
{
	[TestFixture]
	public class HttpSenderTest
	{
		[Test]
		public void ShouldPost()
		{
			var server = new FakeHttpServer();
			var target =new HttpSender(new HttpClientM(server, new MutableUrl()));

			target.Send(new Message());

			server.Requests.Should().Have.Count.EqualTo(1);
		}

		
		[Test]
		public void ShouldPostToCorrectUrl(
			[Values(
				"http://a/",
				"http://a",
				"http://a/b/c/",
				"http://a/b/c"
				)] string url)
		{
			var mutableUrl = new MutableUrl();
			var server = new FakeHttpServer();
			var target = new HttpSender(new HttpClientM(server, mutableUrl));
			mutableUrl.Configure(url);

			target.Send(new Message());

			server.Requests.Single().Uri.Should().Be(url.TrimEnd('/') + "/MessageBroker/NotifyClients");
		}
		
		[Test]
		public void ShouldPostMultiple()
		{
			var notifications = new[] { new Message { DataSource = "one" }, new Message { DataSource = "two" } };
			var url = new MutableUrl();
			var server = new FakeHttpServer();
			var target = new HttpSender(new HttpClientM(server, url));
			url.Configure("http://a");

			target.SendMultiple(notifications);
			
			server.Requests.Single().Uri.Should().Be("http://a/MessageBroker/NotifyClientsMultiple");
		}
	}
}
