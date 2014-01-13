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
		private const string PageMain = "6CE00B41-0722-4B36-91DD-0A3B63C545CF";

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
			foreach (var readOnlyGroupPage in allGroupPages)
			{
				var name = _userTextTranslator.TranslateText(readOnlyGroupPage.PageName);
				if (name != readOnlyGroupPage.PageName)
				{
					readOnlyGroupPage.PageName = name;
					buildInGroupPages.Add(readOnlyGroupPage);
				}
				else
					customGroupPages.Add(readOnlyGroupPage);
			}

			var actualGroupPages = buildInGroupPages.Select(gp =>
				{
					var name = gp.PageName;
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
			var selectedGroupId = team != null ? team.Id : null;

			return Json(new {GroupPages = actualGroupPages, SelectedGroupId = selectedGroupId}, JsonRequestBehavior.AllowGet);
		}
	}
}