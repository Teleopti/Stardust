using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class BaseConfiguration : IBaseConfiguration
	{
		public BaseConfiguration(int? cultureId, int? intervalLength, string timeZoneCode,IEtlToggleManager etlToggleManager)
		{
			EtlToggleManager = etlToggleManager;
			CultureId = cultureId;
			IntervalLength = intervalLength;
			TimeZoneCode = timeZoneCode;

		}

		public int? CultureId { get; private set; }
		public int? IntervalLength { get; private set; }
		public string TimeZoneCode { get; private set; }
		public IEtlToggleManager EtlToggleManager { get; private set; }
	}
}