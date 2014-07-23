using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class GroupPageController : Controller
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTextTranslator _userTextTranslator;
		private static readonly Guid PageMain = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");

		public GroupPageController(IGroupingReadOnlyRepository groupingReadOnlyRepository, ILoggedOnUser loggedOnUser, IUserTextTranslator userTextTranslator)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_loggedOnUser = loggedOnUser;
			_userTextTranslator = userTextTranslator;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableGroupPages(DateTime date)
		{
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupPages().ToArray();

			var buildInGroupPages = new List<ReadOnlyGroupPage>();
			var customGroupPages = new List<ReadOnlyGroupPage>();
			ReadOnlyGroupPage businessHierarchyPage = null;
			foreach (var readOnlyGroupPage in allGroupPages)
			{
				var name = _userTextTranslator.TranslateText(readOnlyGroupPage.PageName);
				if (name != readOnlyGroupPage.PageName)
				{
					readOnlyGroupPage.PageName = name;

					if (readOnlyGroupPage.PageId == PageMain)
						businessHierarchyPage = readOnlyGroupPage;
					else
						buildInGroupPages.Add(readOnlyGroupPage);
				}
				else
					customGroupPages.Add(readOnlyGroupPage);
			}


			buildInGroupPages = buildInGroupPages.OrderBy(x => x.PageName).ToList();
			if (businessHierarchyPage != null)
				buildInGroupPages.Insert(0, businessHierarchyPage);

			var actualGroupPages = buildInGroupPages.Select(gp =>
				{
					var name = gp.PageName;
					return new
						{
							Name = name,
							Groups = _groupingReadOnlyRepository.AvailableGroups(gp, new DateOnly(date)).Select(g => new
								{
									Name = gp.PageId == PageMain ? g.GroupName : name + "/" + g.GroupName,
									Id = g.GroupId
								}).Distinct().ToArray()
						};
				}).ToList();
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