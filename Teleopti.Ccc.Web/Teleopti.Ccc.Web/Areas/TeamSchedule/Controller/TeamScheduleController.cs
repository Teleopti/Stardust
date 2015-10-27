using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controller
{
	public class TeamScheduleController : ApiController
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;

		public TeamScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/Group")]
		public virtual JsonResult<IEnumerable<GroupScheduleShiftViewModel>> GroupSchedule(Guid groupId, DateTime date)
		{
			return Json(_groupScheduleViewModelFactory.CreateViewModel(groupId, date));
		}
	}
}