using System;
using System.Text.RegularExpressions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class CreateHourText : ICreateHourText
	{
		private readonly IUserTimeZone _userTimeZone;

		public CreateHourText(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public string CreateText(DateTime time)
		{
			var timeZone = _userTimeZone.TimeZone();
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			return localTime.ToShortTimeString();
		}
	}
}