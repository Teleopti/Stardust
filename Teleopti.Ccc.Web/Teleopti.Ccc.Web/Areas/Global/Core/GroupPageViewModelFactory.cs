using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{
	public class GroupPageViewModelFactory
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IUserTextTranslator _userTextTranslator;
		private readonly IUserUiCulture _uiCulture;
		private readonly ILoggedOnUser _loggedOnUser;

		public GroupPageViewModelFactory(
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			IUserTextTranslator userTextTranslator,
			IUserUiCulture uiCulture, ILoggedOnUser loggedOnUser)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
			_uiCulture = uiCulture;
			_loggedOnUser = loggedOnUser;
		}

		public dynamic CreateViewModel(DateOnlyPeriod period)
		{
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupsBasedOnPeriod(period);

			var allAvailableGroups =
				_groupingReadOnlyRepository.AvailableGroups(period, allGroupPages.Select(gp=>gp.PageId).ToArray())
					.ToLookup(t => t.PageId);

			var actualOrgs = new List<dynamic>();
			
			var orgsLookup = allAvailableGroups[Group.PageMainId].ToLookup(g => g.SiteId);
			foreach (var siteLookUp in orgsLookup)
			{
				var teams = orgsLookup[siteLookUp.Key];
				var children = teams.Select(t => new
				{
					Name = t.GroupName.Split('/')[1],
					Id = t.TeamId
				}).OrderBy(c => c.Name, stringComparer);
				actualOrgs.Add(new
				{
					Name = teams.First().GroupName.Split('/')[0],
					Id = siteLookUp.Key,
					Children = children
				});
			}

			var actualGroupPages = new List<dynamic>();
			foreach (var groupPage in allGroupPages.Where(gp => gp.PageId != Group.PageMainId))
			{
				var childGroups = allAvailableGroups[groupPage.PageId];
				actualGroupPages.Add(new
				{
					Id = groupPage.PageId,
					Name = _userTextTranslator.TranslateText(groupPage.PageName),
					Children = childGroups.Select(g => new
					{
						Name = g.GroupName,
						Id = g.GroupId
					}).Distinct().ToArray().OrderBy(c => c.Name, stringComparer)
				});
			}
			return new
			{
				BusinessHierarchy = actualOrgs.OrderBy(o => o.Name as string, stringComparer),
				GroupPages = actualGroupPages.OrderBy(g => g.Name as string, stringComparer)
			};
		}

		public dynamic CreateViewModel(DateOnly date)
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

					if (readOnlyGroupPage.PageId == Group.PageMainId)
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
				buildInGroupPages.Insert(0, businessHierarchyPage);

			var groupPages = buildInGroupPages.Union(customGroupPages).ToList();
			
			var allAvailableGroups = _groupingReadOnlyRepository
				.AvailableGroups( new DateOnlyPeriod(date, date), groupPages.Select(gp=>gp.PageId).ToArray())
				.ToLookup(t => t.PageId);

			var actualGroupPages = groupPages.Select(gp =>
			{
				var name = gp.PageName;
				return new
				{
					Name = name,
					Groups = allAvailableGroups[gp.PageId].Select(g => new
					{
						Name = gp.PageId == Group.PageMainId ? g.GroupName : name + "/" + g.GroupName,
						Id = g.GroupId
					}).Distinct().ToArray()
				};
			}).ToList();

			var team = _loggedOnUser.CurrentUser().MyTeam(date);
			var defaultGroupId = team?.Id;

			return new { GroupPages = actualGroupPages, DefaultGroupId = defaultGroupId };
		}
	}
}