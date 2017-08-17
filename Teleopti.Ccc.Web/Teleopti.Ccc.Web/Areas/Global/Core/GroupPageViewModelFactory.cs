using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{
	public class GroupPageViewModelFactory
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IUserTextTranslator _userTextTranslator;
		private readonly IUserUiCulture _uiCulture;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;

		public GroupPageViewModelFactory(
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			IUserTextTranslator userTextTranslator,
			IUserUiCulture uiCulture, ILoggedOnUser loggedOnUser,
			IPermissionProvider permissionProvider)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
			_uiCulture = uiCulture;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
		}

		public GroupPagesViewModel CreateViewModel(DateOnlyPeriod period, string functionPath)
		{
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupsBasedOnPeriod(period);

			var allAvailableGroups =
				_groupingReadOnlyRepository.AvailableGroups(period, allGroupPages.Select(gp=>gp.PageId).ToArray())
					.ToLookup(t => t.PageId);

			var actualOrgs = new List<SiteViewModelWithTeams>();
			
			var orgsLookup = allAvailableGroups[Group.PageMainId].ToLookup(g => g.SiteId);
			foreach (var siteLookUp in orgsLookup)
			{
				var permittedTeams = orgsLookup[siteLookUp.Key].Where(team => _permissionProvider.HasOrganisationDetailPermission(functionPath, period.StartDate, team));
				if (!permittedTeams.Any())
					continue;
				var children = permittedTeams.Select(t => new TeamViewModel
				{
					Name = t.GroupName.Split('/')[1],
					Id = t.TeamId.GetValueOrDefault()
				}).OrderBy(c => c.Name, stringComparer);
				actualOrgs.Add(new SiteViewModelWithTeams
				{
					Name = permittedTeams.First().GroupName.Split('/')[0],
					Id = siteLookUp.Key.GetValueOrDefault(),
					Children = children.ToList()
				});
			}

			var actualGroupPages = new List<GroupPageViewModel>();
			foreach (var groupPage in allGroupPages.Where(gp => gp.PageId != Group.PageMainId))
			{
				var childGroups = allAvailableGroups[groupPage.PageId].ToLookup(g => g.GroupId);

				actualGroupPages.Add(new GroupPageViewModel
				{
					Id = groupPage.PageId,
					Name = _userTextTranslator.TranslateText(groupPage.PageName),
					Children = childGroups.Select(g => new GroupViewModel
					{
						Name = g.First().GroupName,
						Id = g.First().GroupId
					}).OrderBy(c => c.Name, stringComparer).ToList()
				});
			}
			return new GroupPagesViewModel
			{
				BusinessHierarchy = actualOrgs.OrderBy(o => o.Name, stringComparer).ToArray(),
				GroupPages = actualGroupPages.OrderBy(g => g.Name, stringComparer).ToArray(),
				LogonUserTeamId = _loggedOnUser.CurrentUser().MyTeam(period.StartDate)?.Id.GetValueOrDefault()
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