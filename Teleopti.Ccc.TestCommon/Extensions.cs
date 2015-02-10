using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class Extensions
	{
		public static void Times(this int times, Action action)
		{
			Enumerable.Range(0, times).ForEach(i => action());
		}

		public static void Times(this int times, Action<int> action)
		{
			Enumerable.Range(0, times).ForEach(action);
		}

		public static TimeSpan Minutes(this string value)
		{
			return TimeSpan.FromMinutes(Convert.ToInt32(value));
		}

		public static TimeSpan Seconds(this string value)
		{
			return TimeSpan.FromSeconds(Convert.ToInt32(value));
		}

		public static DateOnly Date(this string dateString)
		{
			return new DateOnly(dateString.Utc());
		}

	}
}
