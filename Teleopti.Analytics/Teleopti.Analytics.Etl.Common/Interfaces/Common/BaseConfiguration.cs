using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public class BaseConfiguration : IBaseConfiguration
	{
		public BaseConfiguration(int? cultureId, int? intervalLength, string timeZoneCode, bool runIndexMaintenance)
		{
			RunIndexMaintenance = runIndexMaintenance;
			CultureId = cultureId;
			IntervalLength = intervalLength;
			TimeZoneCode = timeZoneCode;
			InsightsConfig = new InsightsConfiguration();
		}

		public int? CultureId { get; private set; }
		public int? IntervalLength { get; private set; }
		public string TimeZoneCode { get; private set; }
		public bool RunIndexMaintenance { get; private set; }
		public InsightsConfiguration InsightsConfig { get; private set; }
		public IJobHelper JobHelper { get; set; }
	}
}