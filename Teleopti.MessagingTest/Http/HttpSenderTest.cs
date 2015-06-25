﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
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
			var poster = MockRepository.GenerateMock<IPoster>();
			var target =
				new HttpSender(new HttpRequests(new MutableUrl(), null)
				{
					PostAsync = (client, url, content) => poster.PostAsync(client, url, content)
				});
			
			target.Send(new Message());

			poster.AssertWasCalled(x => x.PostAsync(null, null, null), o => o.IgnoreArguments());
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
			var postedUrl = "";
			var mutableUrl = new MutableUrl();
			var target = new HttpSender(new HttpRequests(mutableUrl, null) { PostAsync = (c, u, cn) => postedUrl = u });
			mutableUrl.Configure(url);

			target.Send(new Message());

			postedUrl.Should().Be(url.TrimEnd('/') + "/MessageBroker/NotifyClients");
		}

		[Test]
		public void ShouldReuseSameClient()
		{
			var calledClients = new List<HttpClient>();
			var target = new HttpSender(new HttpRequests(null, null) { PostAsync = (c, u, cn) => calledClients.Add(c) });

			target.Send(new Message());
			target.Send(new Message());

			calledClients[0].Should().Be.SameInstanceAs(calledClients[1]);
		}

		[Test]
		public void ShouldPostDataAsJson()
		{
			var notification = new Message();
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notification)).Return("serialized!");
			HttpContent postedContent = null;
			var target = new HttpSender(new HttpRequests(null, serializer) { PostAsync = (c, u, cn) => { postedContent = cn; } });

			target.Send(notification);

			postedContent.ReadAsStringAsync().Result.Should().Be("serialized!");
			postedContent.Headers.ContentType.MediaType.Should().Be("application/json");
		}

		[Test]
		public void ShouldPostMultiple()
		{
			var notifications = new[] {new Message {DataSource = "one"}, new Message {DataSource = "two"}};
			var serializer = MockRepository.GenerateMock<IJsonSerializer>();
			serializer.Stub(x => x.SerializeObject(notifications)).Return("many");
			HttpContent postedContent = null;
			var postedUrl = "";
			var url = new MutableUrl();
			var target = new HttpSender(new HttpRequests(url, serializer)
			{
				PostAsync = (c, u, cn) =>
				{
					postedUrl = u;
					postedContent = cn;
				}
			});
			url.Configure("http://a");

			target.SendMultiple(notifications);

			postedContent.ReadAsStringAsync().Result.Should().Be("many");
			postedUrl.Should().Be("http://a/MessageBroker/NotifyClientsMultiple");
		}

		public interface IPoster
		{
			void PostAsync(HttpClient client, string url, HttpContent content);
		}

	}

}
