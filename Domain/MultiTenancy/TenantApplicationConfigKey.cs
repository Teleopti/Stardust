namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public static class TenantApplicationConfigKey
	{
		public static string MobileQRCodeUrl => nameof(MobileQRCodeUrl);
		public static string MaximumSessionTimeInMinutes => nameof(MaximumSessionTimeInMinutes);
		public static string NotificationApiKey => nameof(NotificationApiKey);

		public static string InsightsAzureTenantId = "Insights.AzureTenantId";
		public static string InsightsPowerBIClientId = "Insights.PowerBIClientId";
		public static string InsightsPowerBIGroupId = "Insights.PowerBIGroupId";
		public static string InsightsPowerBIUsername = "Insights.PowerBIUsername";
		public static string InsightsPowerBIPassword = "Insights.PowerBIPassword";
	}
}