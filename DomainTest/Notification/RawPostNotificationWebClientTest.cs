using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	public class RawPostNotificationWebClientTest
	{
		[Test]
		public void ShouldSendCorrectly()
		{
			var webRequestFactory = new TestWebRequestFactory();
			var target = new RawPostNotificationWebClient(webRequestFactory);
			var queryStringData = "username=user1中文&password=pass1";
			var response = target.MakeRequest(new FakeNotificationConfigReader {Url = new Uri("http://test") }, queryStringData);

			webRequestFactory.TheRequest.Method.Should().Be.EqualTo("POST");
			webRequestFactory.TheRequest.ContentType.Should().Be.EqualTo("application/x-www-form-urlencoded");
			var bytes = Encoding.UTF8.GetBytes(queryStringData);
			webRequestFactory.TheRequest.TheStream.AssertWasCalled(x => x.Write(bytes, 0, bytes.Length));
			response.Should().Be.EqualTo("test_response");
		}

		[Test]
		public void ShouldUseContentTypeFromConfig()
		{
			var webRequestFactory = new TestWebRequestFactory();
			var target = new RawPostNotificationWebClient(webRequestFactory);
			var queryStringData = "username=user1中文&password=pass1";
			var response = target.MakeRequest(new FakeNotificationConfigReader { Url = new Uri("http://test"), ContentType = "application/json" }, queryStringData);

			webRequestFactory.TheRequest.Method.Should().Be.EqualTo("POST");
			webRequestFactory.TheRequest.ContentType.Should().Be.EqualTo("application/json");
			var bytes = Encoding.UTF8.GetBytes(queryStringData);
			webRequestFactory.TheRequest.TheStream.AssertWasCalled(x => x.Write(bytes, 0, bytes.Length));
			response.Should().Be.EqualTo("test_response");
		}

		[Test]
		public void ShouldUseEncodingFromConfig()
		{
			var webRequestFactory = new TestWebRequestFactory();
			var target = new RawPostNotificationWebClient(webRequestFactory);
			var queryStringData = "username=user1中文&password=pass1";
			var response = target.MakeRequest(new FakeNotificationConfigReader { Url = new Uri("http://test"), EncodingName = "ASCII" }, queryStringData);

			webRequestFactory.TheRequest.Method.Should().Be.EqualTo("POST");
			webRequestFactory.TheRequest.ContentType.Should().Be.EqualTo("application/x-www-form-urlencoded");
			var bytes = Encoding.ASCII.GetBytes(queryStringData);
			webRequestFactory.TheRequest.TheStream.AssertWasCalled(x => x.Write(bytes, 0, bytes.Length));
			response.Should().Be.EqualTo("test_response");
		}
	}

	public class TestWebRequestFactory : IWebRequestFactory
	{
		public WebRequest Create(Uri url)
		{
			var webRequest = new FakeWebRequest();
			TheRequest = webRequest;
			return webRequest;
		}

		public FakeWebRequest TheRequest { get; set; }
	}

	public class FakeWebRequest : WebRequest
	{
		public override string ContentType { get; set; }
		public override string Method { get; set; }
		public override long ContentLength { get; set; }
		public override Stream GetRequestStream()
		{
			var requestStream = MockRepository.GenerateMock<Stream>();
			TheStream = requestStream;
			return requestStream;
		}

		public Stream TheStream { get; set; }

		public override WebResponse GetResponse()
		{
			var webResponse = MockRepository.GenerateMock<WebResponse>();
			webResponse.Stub(x => x.GetResponseStream()).Return(new MemoryStream(Encoding.ASCII.GetBytes("test_response")));
			return webResponse;
		}
	}
}