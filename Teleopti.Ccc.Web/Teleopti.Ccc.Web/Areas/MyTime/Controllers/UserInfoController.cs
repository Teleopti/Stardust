using System.Web.Mvc;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserInfoController : Controller
	{
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _userTimeZone;

		public UserInfoController(IUserCulture userCulture, IUserTimeZone userTimeZone)
		{
			_userCulture = userCulture;
			_userTimeZone = userTimeZone;
		}

		[HttpGet]
		public JsonResult Culture()
		{
			return Json(new
			{
				WeekStart = (int)_userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek,
				DateFormatForMoment = _userCulture.GetCulture().DateTimeFormat.ShortDatePattern.ToUpper(),
				BaseUtcOffsetInMinutes = _userTimeZone.TimeZone().BaseUtcOffset.TotalMinutes
			}, JsonRequestBehavior.AllowGet);
		}
	}
}