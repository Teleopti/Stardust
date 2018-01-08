using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{

	public class BadgeSettingDataConverter
	{
		public static int GetCountValue(int originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0);
			return originalValue;
		}

		public static Percent GetPercentValue(int originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0 && originalValue <= 10000);
			return new Percent(originalValue / 10000d);
		}

		public static TimeSpan GetTimeSpanValue(int originalValue)
		{
			InParameter.MustBeTrue(nameof(originalValue), originalValue > 0);
			return TimeSpan.FromSeconds(originalValue);
		}

		public static int GetValueFromTimeSpan(TimeSpan timeSpan)
		{
			return (int)timeSpan.TotalSeconds;
		}

		public static int GetValueFromPercent(Percent percent)
		{
			return (int)(percent.Value * 100);
		}

		public static int GetBadgeSettingValue(ExternalPerformanceDataType type, string input)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return GetCountValue(int.Parse(input));
				case ExternalPerformanceDataType.Percent:
					var value = double.Parse(input);
					return GetValueFromPercent(new Percent(value));
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}

		public static string GetBadgeSettingValueForViewModel(ExternalPerformanceDataType type, int valueInDB)
		{
			switch (type)
			{
				case ExternalPerformanceDataType.Numeric:
					return GetCountValue(valueInDB).ToString();
				case ExternalPerformanceDataType.Percent:
					return GetPercentValue(valueInDB).ValueAsPercent().ToString();
				default:
					throw new ArgumentException($@"Unsupported badge unit type '{type}'", nameof(type));
			}
		}
	}
}