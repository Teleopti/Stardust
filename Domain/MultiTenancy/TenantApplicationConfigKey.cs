namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public static class TenantApplicationConfigKey
	{
		public static string MobileQRCodeUrl => nameof(MobileQRCodeUrl);
		public static string MaximumSessionTimeInMinutes => nameof(MaximumSessionTimeInMinutes);
		public static string NotificationApiKey => nameof(NotificationApiKey);
	}
}