using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class BadgeSettingDataConverter
	{
		public double GetBadgeSettingValue(ExternalPerformanceDataType type, double input)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getNumericValue(input);
				case ExternalPerformanceDataType.Percent:
					return getPercentValue(input);
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		public double GetBadgeSettingValueForViewModel(ExternalPerformanceDataType type, double valueInDB)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getNumericValue(valueInDB);
				case ExternalPerformanceDataType.Percent:
					return getPercentValue(valueInDB);
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		private static double getNumericValue(double originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue >= 0 && originalValue <= 999999.9999);
			return originalValue;
		}

		private static double getPercentValue(double originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue >= 0 && originalValue <= 1);
			return originalValue;
		}
	}
}