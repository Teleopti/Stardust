using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public class TeamViewModelFactory : ITeamViewModelFactory
	{
		private readonly ITeamProvider _teamProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IUserTextTranslator _userTextTranslator;
		private readonly IAuthorization _authorization;

		public TeamViewModelFactory(ITeamProvider teamProvider, IPermissionProvider permissionProvider, IGroupingReadOnlyRepository groupingReadOnlyRepository, IUserTextTranslator userTextTranslator, IAuthorization authorization)
		{
			_teamProvider = teamProvider;
			_permissionProvider = permissionProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
			_authorization = authorization;
		}

		public IEnumerable<SelectBase> CreateTeamOrGroupOptionsViewModel(DateOnly date)
		{
			return _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages) ?
					   createGroupPagesOptions(date) : CreateTeamOptionsViewModel(date, DefinedRaptorApplicationFunctionPaths.TeamSchedule);
		}

		public IEnumerable<SelectOptionItem> CreateTeamOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var teams = _teamProvider.GetPermittedTeams(date, applicationFunctionPath).ToList();
			var sites = teams
				.Select(t => t.Site)
				.Distinct()
				.OrderBy(s => s.Description.Name);

			var options = new List<SelectOptionItem>();
			sites.ForEach(s =>
			{
				var teamOptions = from t in teams
								  where t.Site == s
								  select new SelectOptionItem
								  {
									  id = t.Id.ToString(),
									  text = s.Description.Name + "/" + t.Description.Name
								  };
				options.AddRange(teamOptions);
			});

			return options;
		}

		public IEnumerable<dynamic> CreateLeaderboardOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var teams = _teamProvider.GetPermittedTeams(date, applicationFunctionPath).ToArray();
			var sites = teams.GroupBy(t => t.Site)
				.OrderBy(s => s.Key.Description.Name);

			var options = new List<dynamic>();
			options.Add( new
			{
				id = Guid.Empty,
				text = UserTexts.Resources.Everyone,
				type = LeadboardQueryType.Everyone
			});
			sites.ForEach(s =>
			{
				if (_authorization.IsPermitted(applicationFunctionPath, date, s.Key))
				{
					options.Add(new 
					{
						id = s.Key.Id.ToString(),
						text = s.Key.Description.Name,
						type = LeadboardQueryType.Site
					});
				}
				
				var teamOptions = from t in s
								  select new
								  {
									  id = t.Id.ToString(),
									  text = t.Description.Name,
									  type = LeadboardQueryType.Team
								  };
				options.AddRange(teamOptions);
			});
			return options;
		}

		private IEnumerable<SelectBase> createGroupPagesOptions(DateOnly date)
		{
			var pages = _groupingReadOnlyRepository.AvailableGroupPages().ToArray();
			var groupPages = pages
							.Where(p => p.PageId != Group.PageMainId)
							.Select(p => new SelectGroup { text = _userTextTranslator.TranslateText(p.PageName), PageId = p.PageId })
							.OrderBy(x => x.text).ToList();
			if (pages.Any(p => p.PageId == Group.PageMainId))
			{
				groupPages.Insert(0, pages
							.Where(p => p.PageId == Group.PageMainId)
							.Select(p => new SelectGroup { text = _userTextTranslator.TranslateText(p.PageName), PageId = p.PageId }).Single());
			}

			var details = _groupingReadOnlyRepository.AvailableGroups(date).ToArray();

			foreach (var page in groupPages)
			{
				var pageId = page.PageId;
				var prefix = pageId == Group.PageMainId ? string.Empty : string.Concat(page.text, "/");

				constructOptions(date, page, details, pageId, prefix);
			}

			return groupPages;
		}

		private void constructOptions(DateOnly date, SelectGroup page, IEnumerable<ReadOnlyGroupDetail> details, Guid pageId, string prefix)
		{
			var detailsByGroup = from d in details
								 where d.PageId == pageId
								 group d by new { d.GroupId, GroupName = prefix + d.GroupName }
									 into g
									 select g;
			var groups = detailsByGroup.Where(
				p => p.Any(d =>
						   _permissionProvider.HasOrganisationDetailPermission(
							   DefinedRaptorApplicationFunctionPaths.ViewSchedules,
							   date, d))).Select(
								   p =>
								   new SelectOptionItem { text = p.Key.GroupName, id = p.Key.GroupId.ToString() });
			page.children = groups.ToArray();
		}
	}
}