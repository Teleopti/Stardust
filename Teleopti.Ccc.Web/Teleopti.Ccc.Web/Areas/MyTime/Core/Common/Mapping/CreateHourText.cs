using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class CreateHourText : ICreateHourText
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly IUserCulture _userCulture;

		public CreateHourText(IUserTimeZone userTimeZone, IUserCulture userCulture)
		{
			_userTimeZone = userTimeZone;
			_userCulture = userCulture;
		}

		public string CreateText(DateTime time)
		{
			var timeZone = _userTimeZone.TimeZone();
			var localTime = TimeZoneHelper.ConvertFromUtc(time, timeZone);
			var culture = _userCulture.GetCulture();
			return localTime.ToString(culture.DateTimeFormat.ShortTimePattern,culture);
		}
	}
}