using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public static class Extensions
	{
		public static TimeSpan? ToTimeSpan(this TimeOfDay? value)
		{
			if (!value.HasValue) return null;
			return value.Value.Time;
		}

		public static TimeSpan? ToTimeSpan(this TimeOfDay? value, bool nextDay)
		{
			if (!value.HasValue) return null;
			return nextDay ? value.Value.Time.Add(TimeSpan.FromDays(1)) : value.Value.Time;
		}

		public static TimeSpan ToTimeSpan(this TimeOfDay value)
		{
			return value.Time;
		}

		public static TimeSpan ToTimeSpan(this TimeOfDay value, bool nextDay)
		{
			return nextDay ? value.Time.Add(TimeSpan.FromDays(1)) : value.Time;
		}

	}
}