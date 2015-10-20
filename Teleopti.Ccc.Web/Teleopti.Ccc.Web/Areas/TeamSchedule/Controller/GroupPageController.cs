using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using GroupPage = Teleopti.Ccc.Domain.GroupPageCreator.Group;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controller
{
	public class GroupPageController : ApiController
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTextTranslator _userTextTranslator;

		public GroupPageController(IGroupingReadOnlyRepository groupingReadOnlyRepository, ILoggedOnUser loggedOnUser,
			IUserTextTranslator userTextTranslator)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_loggedOnUser = loggedOnUser;
			_userTextTranslator = userTextTranslator;
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AllAvailable")]
		public virtual JsonResult<GroupPageListViewModel> AvailableGroupPages(DateTime date)
		{
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupPages();

			var buildInGroupPages = new List<ReadOnlyGroupPage>();
			var customGroupPages = new List<ReadOnlyGroupPage>();
			ReadOnlyGroupPage businessHierarchyPage = null;
			foreach (var readOnlyGroupPage in allGroupPages)
			{
				var name = _userTextTranslator.TranslateText(readOnlyGroupPage.PageName);
				if (name != readOnlyGroupPage.PageName)
				{
					readOnlyGroupPage.PageName = name;

					if (readOnlyGroupPage.PageId == GroupPage.PageMainId)
						businessHierarchyPage = readOnlyGroupPage;
					else
						buildInGroupPages.Add(readOnlyGroupPage);
				}
				else
					customGroupPages.Add(readOnlyGroupPage);
			}

			buildInGroupPages = buildInGroupPages.OrderBy(x => x.PageName).ToList();
			customGroupPages = customGroupPages.OrderBy(x => x.PageName).ToList();

			if (businessHierarchyPage != null)
			{
				buildInGroupPages.Insert(0, businessHierarchyPage);
			}

			var groupPages = buildInGroupPages.Union(customGroupPages).ToList();
			var allAvailableGroups = _groupingReadOnlyRepository.AvailableGroups(groupPages, new DateOnly(date));

			var actualGroupPages = groupPages.Select(gp =>
			{
				var name = gp.PageName;
				return new GroupPageViewModel
				{
					Name = name,
					Groups = allAvailableGroups.Where(g => g.PageId == gp.PageId).Select(g => new GroupViewModel
					{
						Name = gp.PageId == GroupPage.PageMainId ? g.GroupName : name + "/" + g.GroupName,
						Id = g.GroupId
					}).Distinct()
				};
			});

			var team = _loggedOnUser.CurrentUser().MyTeam(new DateOnly(date));
			var defaultGroupId = team != null ? team.Id : null;

			return Json(new GroupPageListViewModel
			{
				GroupPages = actualGroupPages,
				DefaultGroupId = defaultGroupId
			});
		}
	}

	public class GroupPageListViewModel
	{
		public IEnumerable<GroupPageViewModel> GroupPages { get; set; }
		public Guid? DefaultGroupId { get; set; }
	}

	public class GroupPageViewModel
	{
		public string Name { get; set; }
		public IEnumerable<GroupViewModel> Groups { get; set; }
	}

	public class GroupViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}
