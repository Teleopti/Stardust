using System;
using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http
{
	[TestFixture]
	public class HttpSenderTest
	{
		[Test]
		public void ShouldPost()
		{
			var poster = MockRepository.GenerateMock<IPoster>();
			var target = new HttpSender(new MutableUrl(), null) {PostAsync = (client, url, content) => poster.PostAsync(client, url, content)};
			
			target.Send(new Notification());

			poster.AssertWasCalled(x => x.PostAsync(null, null, null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldPostToCorrectUrl(
			[Values("http://a/", "http://a")] string url)
		{
			var postedUrl = "";
			var mutableUrl = new MutableUrl();
			var target = new HttpSender(mutableUrl, null) { PostAsync = (c, u, cn) => postedUrl = u };
			mutableUrl.Configure(url);

			target.Send(new Notification());

			postedUrl.Should().Be(new Uri(new Uri(url), "/MessageBroker/NotifyClients").ToString());
		}

		[Test]
		public void ShouldReuseSameClient()
		{
			var calledClients = new List<HttpClient>();
			var target = new HttpSender(null, null) { PostAsync = (c, u, cn) => calledClients.Add(c)};

			target.Send(new Notification());
			target.Send(new Notification());

			calledClients[0].Should().Be.SameInstanceAs(calledClients[1]);
		}

		[Test]
		public void ShouldPostDataAsJson()
		{
			var notification = new Notification();
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notification)).Return("serialized!");
			var postedContent = "";
			var target = new HttpSender(null, serializer) { PostAsync = (c, u, cn) => postedContent = cn.ReadAsStringAsync().Result };

			target.Send(notification);

			postedContent.Should().Be("serialized!");
		}

		[Test]
		public void ShouldPostMultiple()
		{
			var notifications = new[] {new Notification {DataSource = "one"}, new Notification {DataSource = "two"}};
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notifications[0])).Return("one");
			serializer.Stub(x => x.SerializeObject(notifications[1])).Return("two");
			var postedContent = new List<string>();
			var target = new HttpSender(null, serializer) { PostAsync = (c, u, cn) => postedContent.Add(cn.ReadAsStringAsync().Result) };

			target.SendMultiple(notifications);

			postedContent.Should().Have.SameValuesAs(new[] {"one", "two"});
		}

		public interface IPoster
		{
			void PostAsync(HttpClient client, string url, HttpContent content);
		}

	}

}
