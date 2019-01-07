using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Models;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{
	public class GroupPageViewModelFactory
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IUserTextTranslator _userTextTranslator;
		private readonly IUserUiCulture _uiCulture;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly ITeamRepository _teamRepository;

		public GroupPageViewModelFactory(
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			IUserTextTranslator userTextTranslator,
			IUserUiCulture uiCulture, ILoggedOnUser loggedOnUser,
			IPermissionProvider permissionProvider, IOptionalColumnRepository optionalColumnRepository, ITeamRepository teamRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
			_uiCulture = uiCulture;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_optionalColumnRepository = optionalColumnRepository;
			_teamRepository = teamRepository;
		}

		public GroupPagesViewModel CreateViewModel(DateOnlyPeriod period)
		{
			return createGroupPagesViewModel(period, null);
		}
		public GroupPagesViewModel CreateViewModelWithPermissionCheck(DateOnlyPeriod period, string functionPath)
		{
			return createGroupPagesViewModel(period, functionPath);
		}

		private GroupPagesViewModel createGroupPagesViewModel(DateOnlyPeriod period, string functionPath)
		{
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);


			var allAvailableGroups = _groupingReadOnlyRepository.AllAvailableGroups(period).ToLookup(g => g.PageId);

			var allGroupPages = allAvailableGroups
				.Select(gp => new ReadOnlyGroupPage
				{
					PageId = gp.Key,
					PageName = gp.First().PageName
				});

			var orgsLookup = allAvailableGroups[Group.PageMainId].ToLookup(g => g.SiteId);
			var actualOrgs = new List<SiteViewModelWithTeams>(orgsLookup.Count);

			foreach (var siteLookUp in orgsLookup)
			{
				IEnumerable<ReadOnlyGroup> permittedTeamGroups;

				if (functionPath != null)
				{
					permittedTeamGroups = siteLookUp;
				}
				else
				{
					permittedTeamGroups = siteLookUp.Where(teamWithPerson =>
						_permissionProvider.HasOrganisationDetailPermission(functionPath, period.StartDate,
							teamWithPerson));
				}

				if (!permittedTeamGroups.Any())
					continue;

				var permittedTeams = _teamRepository.FindTeams(permittedTeamGroups.Select(x => x.TeamId.Value));
				if (!permittedTeams.Any())
					continue;
				var children = permittedTeams.Select(t => new TeamViewModel
				{
					Name = t.Description.Name,
					Id = t.Id.GetValueOrDefault()
				}).OrderBy(c => c.Name, stringComparer);

				actualOrgs.Add(new SiteViewModelWithTeams
				{
					Name = permittedTeams.First().Site.Description.Name,
					Id = siteLookUp.Key.GetValueOrDefault(),
					Children = children.ToList()
				});
			}

			var actualGroupPages = allGroupPages.Where(gp => gp.PageId != Group.PageMainId).Select(groupPage =>
			{
				var childGroups = allAvailableGroups[groupPage.PageId].ToLookup(g => g.GroupId);
				return new GroupPageViewModel
				{
					Id = groupPage.PageId,
					Name = _userTextTranslator.TranslateText(groupPage.PageName),
					Children = childGroups.Select(g =>
					{
						var first = g.First();
						return new GroupViewModel
						{
							Name = first.GroupName,
							Id = first.GroupId.ToString()
						};
					}).OrderBy(c => c.Name, stringComparer).ToList()
				};
			}).ToList();

			var allDynamicOptionalColumns = _optionalColumnRepository.GetOptionalColumns<Person>().Where(o => o.AvailableAsGroupPage).ToList();
			foreach (var optionalColumn in allDynamicOptionalColumns)
			{
				var children = _optionalColumnRepository.UniqueValuesOnColumnWithValidPerson(optionalColumn.Id.GetValueOrDefault())
					.Select(value => new GroupViewModel
					{
						Id = value.Description,
						Name = value.Description
					})
					.OrderBy(c => c.Name, stringComparer)
					.ToList();
				;
				if (!children.Any())
				{
					continue;
				}
				actualGroupPages.Add(new GroupPageViewModel
				{
					Id = optionalColumn.Id.Value,
					Name = optionalColumn.Name,
					Children = children
				});
			}

			return new GroupPagesViewModel
			{
				BusinessHierarchy = actualOrgs.OrderBy(o => o.Name, stringComparer).ToArray(),
				GroupPages = actualGroupPages.OrderBy(g => g.Name, stringComparer).ToArray(),
				LogonUserTeamId = _loggedOnUser.CurrentUser().MyTeam(period.StartDate)?.Id.GetValueOrDefault()
			};
		}
	}
}