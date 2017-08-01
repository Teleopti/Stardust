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

		public GroupPageViewModelFactory(
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			IUserTextTranslator userTextTranslator,
			IUserUiCulture uiCulture)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
			_uiCulture = uiCulture;
		}

		public object CreateViewModel(DateOnlyPeriod period)
		{
			var stringComparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);
			var allGroupPages = _groupingReadOnlyRepository.AvailableGroupsBasedOnPeriod(period);

			var allAvailableGroups =
				_groupingReadOnlyRepository.AvailableGroupsBasedOnPeriod(allGroupPages.ToList(), period)
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
	}
}