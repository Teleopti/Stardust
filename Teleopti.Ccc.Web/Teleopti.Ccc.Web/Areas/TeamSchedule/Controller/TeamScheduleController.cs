using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controller
{
	public class TeamScheduleController : ApiController
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly ILoggedOnUser _user;

		public TeamScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory, ILoggedOnUser user)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_user = user;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/Group")]
		public virtual JsonResult<GroupScheduleViewModel> GroupSchedule(Guid groupId, DateTime date)
		{
			var userTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dateTimeInUtc = TimeZoneInfo.ConvertTime(date, userTimeZone, TimeZoneInfo.Utc);

			return Json(new GroupScheduleViewModel
			{
				BaseDate = dateTimeInUtc,
				Schedules = _groupScheduleViewModelFactory.CreateViewModel(groupId, date)
			});
		}
	}

	public class GroupScheduleViewModel
	{
		public DateTime BaseDate { get; set; }
		public IEnumerable<GroupScheduleShiftViewModel> Schedules { get; set; }
	}
}