using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class BadgeSettingDataConverter
	{
		public double GetBadgeSettingValue(ExternalPerformanceDataType type, string input)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getNumericValue(double.Parse(input));
				case ExternalPerformanceDataType.Percent:
					var value = double.Parse(input);
					return getPercentValue(value);
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		public string GetBadgeSettingValueForViewModel(ExternalPerformanceDataType type, double valueInDB)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getNumericValue(valueInDB).ToString();
				case ExternalPerformanceDataType.Percent:
					return getPercentValue(valueInDB).ToString();
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		private static double getNumericValue(double originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0);
			return originalValue;
		}

		private static double getPercentValue(double originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0 && originalValue <= 1);
			return originalValue;
		}
	}
}