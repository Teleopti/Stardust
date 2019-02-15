using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Notification;


namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture]
	class NotificationServiceClientTest
	{
		[Test]
		[EnabledBy(Toggles.Wfm_User_Password_Reset_74957)]
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

		[Test]
		[EnabledBy(Toggles.Wfm_AutomaticNotificationEnrollment_79679)]
		public void GetSubscriptionKeyOnPremTenant()
		{
			var handler = new MockHandler(request =>
			{
				Assert.True(request.Headers.Contains("Ocp-Apim-Subscription-Key"));
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StringContent("{subscriptionKey: 'thekey', message: 'Created subscription with displayName: OnPrem_tenant'}");
				return response;
			});

			var notificationClient = new NotificationServiceClient(new HttpClient(handler));
			var result = notificationClient.CreateSubscription("tenant", "https://example.org/api", "apikey", isCloudTenant: false);
			
			Assert.True(!result.subscriptionKey.IsNullOrEmpty());
			Assert.True(result.message.Contains("OnPrem"));
		}


		[Test]
		[EnabledBy(Toggles.Wfm_AutomaticNotificationEnrollment_79679)]
		public void GetSubscriptionKeyCloudTenant()
		{
			var handler = new MockHandler(request =>
			{
				Assert.True(request.Headers.Contains("Ocp-Apim-Subscription-Key"));
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StringContent("{subscriptionKey: 'thekey', message: 'Created subscription with displayName: Cloud_tenant'}");
				return response;
			});

			var notificationClient = new NotificationServiceClient(new HttpClient(handler));
			var result = notificationClient.CreateSubscription("tenant", "https://example.org/api", "apikey", isCloudTenant: false);

			Assert.True(!result.subscriptionKey.IsNullOrEmpty());
			Assert.True(result.message.Contains("Cloud"));
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