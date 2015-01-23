using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public class BaseConfiguration : IBaseConfiguration
	{
		public BaseConfiguration(int? cultureId, int? intervalLength, string timeZoneCode, IToggleManager toggleManager, bool runIndexMaintenance)
		{
			ToggleManager = toggleManager;
			RunIndexMaintenance = runIndexMaintenance;
			CultureId = cultureId;
			IntervalLength = intervalLength;
			TimeZoneCode = timeZoneCode;

		}

		public int? CultureId { get; private set; }
		public int? IntervalLength { get; private set; }
		public string TimeZoneCode { get; private set; }
		public IToggleManager ToggleManager { get; private set; }
		public bool RunIndexMaintenance { get; private set; }
	}
}