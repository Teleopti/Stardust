namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public class InsightsConfiguration
	{
		public string ServiceBusAddress { get; set; }
		public string TopicName { get; set; }
		public string ModelLocation { get; set; }
		public string ModelName { get; set; }
		public string AnalyticsDatabase { get; set; }
		public string AnalysisService { get; set; }
		public string Location { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(ServiceBusAddress) &&
				   !string.IsNullOrEmpty(TopicName) &&
				   !string.IsNullOrEmpty(ModelLocation) &&
				   !string.IsNullOrEmpty(ModelName) &&
				   !string.IsNullOrEmpty(AnalyticsDatabase) &&
				   !string.IsNullOrEmpty(AnalysisService) &&
				   !string.IsNullOrEmpty(Location);
		}
	}
}