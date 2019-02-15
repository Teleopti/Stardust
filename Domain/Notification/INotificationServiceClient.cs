namespace Teleopti.Ccc.Domain.Notification
{
	public interface INotificationServiceClient
	{
		bool SendEmail(SGMailMessage msg, string apiUri, string apiKey);

		NotificationSubscriptionMessage CreateSubscription(string tenantName, string apiUri, string apiKey, bool isCloudTenant);
	}
}
