using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IBaseConfiguration
	{
		int? CultureId { get; }
		int? IntervalLength { get; }
		string TimeZoneCode { get; }
		IToggleManager ToggleManager { get; }
		bool RunIndexMaintenance { get; }
		IJobHelper JobHelper { get; set; }
	}
}