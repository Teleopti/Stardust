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

		public TeamViewModelFactory(ITeamProvider teamProvider, IPermissionProvider permissionProvider, IGroupingReadOnlyRepository groupingReadOnlyRepository, IUserTextTranslator userTextTranslator)
		{
			_teamProvider = teamProvider;
			_permissionProvider = permissionProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
		}

		public IEnumerable<SelectBase> CreateTeamOrGroupOptionsViewModel(DateOnly date)
		{
			return _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages) ?
					   createGroupPagesOptions(date) : createNotEmptyTeamOptionsViewModel(date, DefinedRaptorApplicationFunctionPaths.TeamSchedule);
		}

		private IEnumerable<SelectBase> createNotEmptyTeamOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var options = new List<SelectOptionItem>();

			var teams = _teamProvider.GetPermittedNotEmptyTeams(date, applicationFunctionPath);
			if (teams == null) return options;
			var sites = teams.ToList().GroupBy(t => t.Site)
				.OrderBy(s => s.Key.Description.Name);

			sites.ForEach(s =>
			{
				var teamOptions = from t in s
					select new SelectOptionItem
					{
						id = t.Id.ToString(),
						text = s.Key.Description.Name + "/" + t.Description.Name
					};
				options.AddRange(teamOptions);
			});

			return options;
		}

		public IEnumerable<SelectOptionItem> CreateTeamOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var options = new List<SelectOptionItem>();

			var teams = _teamProvider.GetPermittedTeams(date, applicationFunctionPath);
			if (teams == null) return options;

			var sites = teams.ToList().GroupBy(t => t.Site)
				.OrderBy(s => s.Key.Description.Name);

			sites.ForEach(s =>
			{
				var teamOptions = from t in s
								  select new SelectOptionItem
								  {
									  id = t.Id.ToString(),
									  text = s.Key.Description.Name + "/" + t.Description.Name
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