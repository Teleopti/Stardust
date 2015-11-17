using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class TeamScheduleController : ApiController
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;

		public TeamScheduleController(IGroupScheduleViewModelFactory groupScheduleViewModelFactory, ITeamScheduleViewModelFactory teamScheduleViewModelFactory)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/Group")]
		public virtual JsonResult<PagingGroupScheduleShiftViewModel> GroupSchedule(Guid groupId, DateTime date,
			int pageSize, int currentPageIndex)
		{
			int totalPage;
			var schedules =
				_groupScheduleViewModelFactory.LoadSchedulesWithPaging(groupId, date, pageSize, currentPageIndex, out totalPage).ToList();
			var result = new PagingGroupScheduleShiftViewModel
			{
				GroupSchedule = schedules,
				TotalPages = totalPage
			};
			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("api/TeamSchedule/GroupNoReadModel")]
		public virtual JsonResult<IEnumerable<GroupScheduleShiftViewModel>> GroupScheduleNoReadModel(Guid groupId, DateTime date)
		{
			var schedules =
				_teamScheduleViewModelFactory.CreateViewModel(groupId, date);

			return Json(schedules);
		}
	}

	public class PagingGroupScheduleShiftViewModel
	{
		public IEnumerable<GroupScheduleShiftViewModel> GroupSchedule { get; set; }
		public int TotalPages { get; set; }
	}
}
