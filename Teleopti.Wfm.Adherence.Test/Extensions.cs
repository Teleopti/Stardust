using System;
using System.Globalization;
using Teleopti.Ccc.TestCommon.IoC;

[assembly: AddDatasourceId]

namespace Teleopti.Wfm.Adherence.Test
{
	public static class Extensions
	{
		public static DateOnly Date(this string dateString)
		{
			return new DateOnly(dateString.Utc());
		}
		
		public static DateTime Utc(this string dateTimeString)
		{
			return DateTime.SpecifyKind(DateTime.Parse(dateTimeString, CultureInfo.GetCultureInfo("sv-SE")), DateTimeKind.Utc);
		}
	}
}
