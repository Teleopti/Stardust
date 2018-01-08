using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class BadgeSettingDataConverter
	{
		public int GetBadgeSettingValue(ExternalPerformanceDataType type, string input)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getCountValue(int.Parse(input));
				case ExternalPerformanceDataType.Percent:
					var value = double.Parse(input);
					return getValueFromPercent(new Percent(value));
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		public string GetBadgeSettingValueForViewModel(ExternalPerformanceDataType type, int valueInDB)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return getCountValue(valueInDB).ToString();
				case ExternalPerformanceDataType.Percent:
					return getPercentValue(valueInDB).ValueAsPercent().ToString();
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		private static int getCountValue(int originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0);
			return originalValue;
		}

		private static Percent getPercentValue(int originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0 && originalValue <= 10000);
			return new Percent(originalValue / 10000d);
		}
		
		private static int getValueFromPercent(Percent percent)
		{
			return (int)(percent.Value * 100);
		}
	}
}