using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class GroupPageController : Controller
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public GroupPageController(IGroupingReadOnlyRepository groupingReadOnlyRepository, ILoggedOnUser loggedOnUser)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableGroupPages(DateTime date)
		{
			var actualGroupPages =
				_groupingReadOnlyRepository.AvailableGroupPages().Select(gp => new
					{
						Name = gp.PageName,
						Groups = _groupingReadOnlyRepository.AvailableGroups(gp, new DateOnly(date)).Select(g => new
							{
								Name = g.GroupName,
								Id = g.GroupId
							}).Distinct().ToList()
					});

			var team = _loggedOnUser.CurrentUser().MyTeam(new DateOnly(date));
			var selectedGroupId = team != null ? team.Id : null;

			return Json(new {GroupPages = actualGroupPages.ToList(), SelectedGroupId = selectedGroupId}, JsonRequestBehavior.AllowGet);
		}
	}
}