using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces;
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
			var target =new HttpSender(new HttpClientM(server, new MutableUrl(), null));

			target.Send(new Message());

			server.Requests.Should().Have.Count.EqualTo(1);
		}

		[CLSCompliant(false)]
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
			var target = new HttpSender(new HttpClientM(server, mutableUrl, null));
			mutableUrl.Configure(url);

			target.Send(new Message());

			server.Requests.Single().Uri.Should().Be(url.TrimEnd('/') + "/MessageBroker/NotifyClients");
		}

		[Test]
		public void ShouldReuseSameClient()
		{
			var server = new FakeHttpServer();
			var target = new HttpSender(new HttpClientM(server, null, null));

			target.Send(new Message());
			target.Send(new Message());

			server.Requests.ElementAt(0).Client.Should().Be.SameInstanceAs(server.Requests.ElementAt(1).Client);
		}

		[Test]
		public void ShouldPostDataAsJson()
		{
			var notification = new Message();
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notification)).Return("serialized!");
			var server = new FakeHttpServer();
			var target = new HttpSender(new HttpClientM(server, null, serializer));

			target.Send(notification);

			server.Requests.Single().HttpContent.ReadAsStringAsync().Result.Should().Be("serialized!");
			server.Requests.Single().HttpContent.Headers.ContentType.MediaType.Should().Be("application/json");
		}

		[Test]
		public void ShouldPostMultiple()
		{
			var notifications = new[] { new Message { DataSource = "one" }, new Message { DataSource = "two" } };
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notifications)).Return("many");
			var url = new MutableUrl();
			var server = new FakeHttpServer();
			var target = new HttpSender(new HttpClientM(server, url, serializer));
			url.Configure("http://a");

			target.SendMultiple(notifications);

			server.Requests.Single().HttpContent.ReadAsStringAsync().Result.Should().Be("many");
			server.Requests.Single().Uri.Should().Be("http://a/MessageBroker/NotifyClientsMultiple");
		}

	}

}
