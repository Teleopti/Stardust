using System;
using System.Text.RegularExpressions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class CreateHourText : ICreateHourText
	{
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _userTimeZone;

		public CreateHourText(IUserCulture userCulture,IUserTimeZone userTimeZone)
		{
			_userCulture = userCulture;
			_userTimeZone = userTimeZone;
		}

		public string CreateText(DateTime time)
		{
			var timeZone = _userTimeZone.TimeZone();
			var culture = _userCulture.GetCulture();
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			var hourString = string.Format(culture, localTime.ToShortTimeString());

			return hourString;
		}

	}
}