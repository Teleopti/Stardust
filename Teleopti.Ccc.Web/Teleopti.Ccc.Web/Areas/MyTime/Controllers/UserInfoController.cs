using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserInfoController : Controller
	{
		private readonly IUserCulture _userCulture;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public UserInfoController(IUserCulture userCulture, IUserTimeZone userTimeZone, INow now)
		{
			_userCulture = userCulture;
			_userTimeZone = userTimeZone;
			_now = now;
		}

		[HttpGet]
		public JsonResult Culture()
		{
			return Json(new
			{
				WeekStart = (int)_userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek,
				DateFormatForMoment = _userCulture.GetCulture().DateTimeFormat.ShortDatePattern.ToUpper(),
				BaseUtcOffsetInMinutes = _userTimeZone.TimeZone().BaseUtcOffset.TotalMinutes,
				DaylightSavingTimeAdjustment = GetDaylightSavingTimeAdjustment()
			}, JsonRequestBehavior.AllowGet);
		}

		private DaylightSavingsTimeAdjustmentViewModel GetDaylightSavingTimeAdjustment()
		{
			var daylightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(_userTimeZone.TimeZone(), _now.ServerDateTime_DontUse().Year);
			var daylightModel = daylightSavingAdjustment != null
				? new DaylightSavingsTimeAdjustmentViewModel(daylightSavingAdjustment)
				: null;

			return daylightModel;
		}
	}
}