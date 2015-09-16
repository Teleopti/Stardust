using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class GroupScheduleController : Controller
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly ILoggedOnUser _user;

		public GroupScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory, ILoggedOnUser user)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_user = user;
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult Get(Guid groupId, DateTime date)
		{
			var userTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dateTimeInUtc = TimeZoneInfo.ConvertTime(date, userTimeZone, TimeZoneInfo.Utc);
			
			var schedules = _groupScheduleViewModelFactory.CreateViewModel(groupId, date).ToArray();
			return Json(new
			{
				BaseDate = dateTimeInUtc,
				Schedules = schedules.ToArray(),
				TotalCount = schedules.Count()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}