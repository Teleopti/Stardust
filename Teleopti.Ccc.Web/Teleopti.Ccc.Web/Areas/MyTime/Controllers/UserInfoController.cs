using System.Web.Mvc;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UserInfoController : Controller
	{
		private readonly IUserCulture _userCulture;

		public UserInfoController(IUserCulture userCulture)
		{
			_userCulture = userCulture;
		}

		[HttpGet]
		public JsonResult Culture()
		{
			return Json(new
			{
				WeekStart = (int)_userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek,
				DateFormatForMoment = _userCulture.GetCulture().DateTimeFormat.ShortDatePattern.ToUpper()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}