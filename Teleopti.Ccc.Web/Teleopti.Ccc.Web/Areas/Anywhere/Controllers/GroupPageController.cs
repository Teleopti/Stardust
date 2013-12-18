using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class GroupPageController : Controller
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private const string PageMain = "6CE00B41-0722-4B36-91DD-0A3B63C545CF";

		public GroupPageController(IGroupingReadOnlyRepository groupingReadOnlyRepository, ILoggedOnUser loggedOnUser)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableGroupPages(DateTime date)
		{
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupPages().ToArray();
			var buildInGroupPages = allGroupPages.Where(pg => pg.PageName.StartsWith("xx", StringComparison.OrdinalIgnoreCase));
			var customGroupPages = allGroupPages.Where(pg => !pg.PageName.StartsWith("xx", StringComparison.OrdinalIgnoreCase));
			var actualGroupPages =
				buildInGroupPages.Select(gp =>
					{
						var name = Resources.ResourceManager.GetString(gp.PageName.Substring(2));
						return new
							{
								Name = name,
								Groups = _groupingReadOnlyRepository.AvailableGroups(gp, new DateOnly(date)).Select(g => new
									{
										Name = gp.PageId.ToString().ToUpperInvariant() == PageMain ? g.GroupName : name + "/" + g.GroupName,
										Id = g.GroupId
									}).Distinct().ToArray()
							};
					}).OrderBy(x => x.Name).ToList();
			actualGroupPages.AddRange(customGroupPages.Select(gp =>
				{
					var name = gp.PageName;
					return new
						{
							Name = name,
							Groups = _groupingReadOnlyRepository.AvailableGroups(gp, new DateOnly(date)).Select(g => new
								{
									Name = name + "/" + g.GroupName,
									Id = g.GroupId
								}).Distinct().ToArray()
						};
				}).OrderBy(x => x.Name).ToList());

			var team = _loggedOnUser.CurrentUser().MyTeam(new DateOnly(date));
			var defaultGroupId = team != null ? team.Id : null;

			return Json(new {GroupPages = actualGroupPages, DefaultGroupId = defaultGroupId}, JsonRequestBehavior.AllowGet);
		}
	}
}