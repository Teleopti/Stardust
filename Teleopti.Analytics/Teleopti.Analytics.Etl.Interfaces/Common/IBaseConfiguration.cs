using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IBaseConfiguration
	{
		int? CultureId { get; }
		int? IntervalLength { get; }
		string TimeZoneCode { get; }
		bool RunIndexMaintenance { get; }
		IJobHelper JobHelper { get; set; }
	}
}