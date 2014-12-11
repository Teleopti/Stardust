using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
		private const string pageMain = "6CE00B41-0722-4B36-91DD-0A3B63C545CF";

		public TeamViewModelFactory(ITeamProvider teamProvider, IPermissionProvider permissionProvider, IGroupingReadOnlyRepository groupingReadOnlyRepository, IUserTextTranslator userTextTranslator)
		{
			_teamProvider = teamProvider;
			_permissionProvider = permissionProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_userTextTranslator = userTextTranslator;
		}

		public IEnumerable<ISelectOption> CreateTeamOrGroupOptionsViewModel(DateOnly date)
		{
			return _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages) ?
					   createGroupPagesOptions(date) : CreateTeamOptionsViewModel(date, DefinedRaptorApplicationFunctionPaths.TeamSchedule);
		}

		public IEnumerable<ISelectOption> CreateTeamOptionsViewModel(DateOnly date, string applicationFunctionPath)
		{
			var teams = _teamProvider.GetPermittedTeams(date, applicationFunctionPath).ToList();
			var sites = teams
				.Select(t => t.Site)
				.Distinct()
				.OrderBy(s => s.Description.Name);

			var options = new List<ISelectOption>();
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

		private IEnumerable<ISelectOption> createGroupPagesOptions(DateOnly date)
		{
			var pages = _groupingReadOnlyRepository.AvailableGroupPages().ToArray();
			var groupPages = pages
							.Where(p => p.PageId.ToString().ToUpperInvariant() != pageMain)
							.Select(p => new SelectGroup { text = _userTextTranslator.TranslateText(p.PageName), PageId = p.PageId })
							.OrderBy(x => x.text).ToList();
			if (pages.Any(p => p.PageId.ToString().ToUpperInvariant() == pageMain))
			{
				groupPages.Insert(0, pages
							.Where(p => p.PageId.ToString().ToUpperInvariant() == pageMain)
							.Select(p => new SelectGroup { text = _userTextTranslator.TranslateText(p.PageName), PageId = p.PageId }).Single());
			}

			var details = _groupingReadOnlyRepository.AvailableGroups(date).ToArray();

			foreach (var page in groupPages)
			{
				var pageId = page.PageId;

				if (pageId.ToString().ToUpperInvariant() == pageMain)
				{
					constructOptions(date, page, details, pageId, "");
				}
				else
				{
					var prefix = page.text + "/";
					constructOptions(date, page, details, pageId, prefix);
				}
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