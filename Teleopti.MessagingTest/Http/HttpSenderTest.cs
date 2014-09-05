using System;
using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
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
			var target = new HttpSender(null) {PostAsync = (client, url, content) => poster.PostAsync(client, url, content)};
			
			target.Send(new Notification());

			poster.AssertWasCalled(x => x.PostAsync(null, null, null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldPostToCorrectUrl(
			[Values("http://a/", "http://a")] string url)
		{
			var poster = MockRepository.GenerateMock<IPoster>();
			var target = new HttpSender(url) {PostAsync = (c, u, cn) => poster.PostAsync(c, u, cn)};

			target.Send(new Notification());

			poster.AssertWasCalled(x =>
				x.PostAsync(
					Arg<HttpClient>.Is.Anything,
					Arg<string>.Is.Equal(new Uri(new Uri(url), "/MessageBroker/NotifyClients").ToString()),
					Arg<HttpContent>.Is.Anything
					));
		}

		[Test]
		public void ShouldReuseSameClient()
		{
			var calledClients = new List<HttpClient>();
			var target = new HttpSender(null) { PostAsync = (c, u, cn) => calledClients.Add(c)};

			target.Send(new Notification());
			target.Send(new Notification());

			calledClients[0].Should().Be.SameInstanceAs(calledClients[1]);
		}

		public interface IPoster
		{
			void PostAsync(HttpClient client, string url, HttpContent content);
		}

	}
}
