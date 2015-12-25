using System;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class GroupPageController : ApiController
	{
		private readonly ITeamScheduleGroupPageViewModelFactory _groupPageViewModelFactory;

		public GroupPageController(ITeamScheduleGroupPageViewModelFactory groupPageViewModelFactory)
		{
			_groupPageViewModelFactory = groupPageViewModelFactory;
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AllTeams")]
		public virtual JsonResult<TeamScheduleGroupPageViewModel[]> AllTeams(DateTime date)
		{
			var viewModel = _groupPageViewModelFactory.GetBusinessHierarchyPageGroupViewModelsByDate(new DateOnly(date));
			return Json(viewModel);
		}
	}
}
