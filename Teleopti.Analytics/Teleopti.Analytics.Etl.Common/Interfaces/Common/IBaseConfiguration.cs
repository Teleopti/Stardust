using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IBaseConfiguration
	{
		int? CultureId { get; }
		int? IntervalLength { get; }
		string TimeZoneCode { get; }
		bool RunIndexMaintenance { get; }
		InsightsConfiguration InsightsConfig { get; }
		IJobHelper JobHelper { get; set; }
	}
}