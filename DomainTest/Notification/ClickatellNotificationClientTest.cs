using System;
using System.Collections.Specialized;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class ClickatellNotificationClientTest
	{
		[Test]
		public void ShouldSendWithCorrectHeaders()
		{
			var webClientFactory = new TestWebClientFactory();
			var target = new ClickatellNotificationWebClient(webClientFactory);

			target.MakeRequest(new FakeNotificationConfigReader {Url = new Uri("http://test") }, "username=user1&password=pass1");

			webClientFactory.TheClient.Headers["user-agent"].Should().Be.EqualTo("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			webClientFactory.TheClient.Headers["Accept-Language"].Should().Be.EqualTo("en-US,en;q=0.8,sv-SE;q=0.7,sv;q=0.5");
		}

		[Test]
		public void ShouldPostDataParameter()
		{
			var webClientFactory = new TestWebClientFactory();
			var target = new ClickatellNotificationWebClient(webClientFactory);

			var url = new Uri("http://test");
			var data = "username=user1&password=pass1";
			target.MakeRequest(new FakeNotificationConfigReader { Url = url }, data);

			webClientFactory.TheClient.Address.Should().Be.EqualTo(url);
			webClientFactory.TheClient.Method.Should().Be.EqualTo("POST");
			webClientFactory.TheClient.Data["data"].Should().Be.EqualTo(data);
		}
	}

	public class TestWebClient : IWebClient
	{
		public TestWebClient()
		{
			Headers = new WebHeaderCollection();
		}
		public void Dispose()
		{
		}

		public WebHeaderCollection Headers { get; set; }
		public byte[] UploadValues(Uri address, string method, NameValueCollection data)
		{
			Method = method;
			Data = data;
			Address = address;
			return new byte[] { };
		}

		public Uri Address { get; set; }
		public NameValueCollection Data { get; set; }
		public string Method { get; set; }
	}

	public class TestWebClientFactory : IWebClientFactory
	{
		public IWebClient Create()
		{
			var webClient = new TestWebClient();
			TheClient = webClient;
			return webClient;
		}


		public TestWebClient TheClient { get; set; }
	}
}