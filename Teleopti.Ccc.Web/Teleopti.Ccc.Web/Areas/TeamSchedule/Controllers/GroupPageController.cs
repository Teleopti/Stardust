using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using GroupPage = Teleopti.Ccc.Domain.GroupPageCreator.Group;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controller
{
	public class GroupPageController : ApiController
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public GroupPageController(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AllTeams")]
		public virtual JsonResult<IEnumerable<TeamScheduleGroupPageViewModel>> AllTeams(DateTime date)
		{
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupPages();
			var businessHierarchyPage = allGroupPages.Single(gp => gp.PageId == GroupPage.PageMainId);
			var allTeams = _groupingReadOnlyRepository.AvailableGroups(businessHierarchyPage, new DateOnly(date));

			var teams = allTeams.Select(g => new TeamScheduleGroupPageViewModel
				{
					Name = g.GroupName,
					Id = g.GroupId
				}).Distinct();

			return Json(teams);
		}
	}
}
