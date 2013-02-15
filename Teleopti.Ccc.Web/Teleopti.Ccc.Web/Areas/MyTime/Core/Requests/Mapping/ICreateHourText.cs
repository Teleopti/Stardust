using System;
using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface ICreateHourText
	{
		string CreateText(DateTime time, TimeZoneInfo timeZone, CultureInfo culture);
	}
}