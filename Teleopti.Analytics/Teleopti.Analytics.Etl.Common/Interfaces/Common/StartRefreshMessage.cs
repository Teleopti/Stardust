namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	/// <summary>
	/// Message to trigger Insights data refresh in Azure tabular model
	/// </summary>
	public class StartRefreshMessage
	{
		/// <summary>
		/// Customer azure location, in format https://$(AnalysisServiceRegionName)/servers/$(AnalysisServiceName)/models/$(ModelName)/
		/// </summary>
		public string ModelLocation { get; set; }

		/// <summary>
		/// true: this is a full refresh; false: this is a normal refresh
		/// </summary>
		public bool IsFullRefresh { get; set; }

		/// <summary>
		/// Customer name, ex: addisonleegroup
		/// </summary>
		public string ModelName  { get; set; }

		/// <summary>
		/// Customer source db name, ex: addisonleegroup
		/// </summary>
		public string AnalyticsDatabase  { get; set; }

		/// <summary>
		/// Customer Analysis Service instance, ex: ASUKSO01_addisonleegroup
		/// TODO: How to know number
		/// </summary>
		public string AnalysisService  { get; set; }

		/// <summary>
		/// This will be the location of the started refresh, null from start
		/// </summary>
		public string Location { get; set; }
	}
}