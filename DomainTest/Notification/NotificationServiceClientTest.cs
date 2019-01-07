using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	class NotificationServiceClientTest
	{
		[Test]
		public void SendEmail()
		{
			var handler = new MockHandler(request =>
			{
				Assert.True(request.Headers.Contains("Ocp-Apim-Subscription-Key"));
				return new HttpResponseMessage(HttpStatusCode.OK);
			});

			var notificationClient = new NotificationServiceClient(new HttpClient(handler));
			var message = new SGMailMessage()
			{
				ContentType = "application/json"
			};

			var response = notificationClient.SendEmail(message, "https://example.org/api", "apikey");
			Assert.True(response);
		}

		private class MockHandler : HttpClientHandler
		{
			private Func<HttpRequestMessage, HttpResponseMessage> handler;
			public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
			{
				this.handler = handler;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(handler(request));
			}
		}
	}


}