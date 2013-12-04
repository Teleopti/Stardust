using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly ITeamProvider _teamProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private const string PageMain = "6CE00B41-0722-4B36-91DD-0A3B63C545CF";

		public TeamScheduleViewModelFactory(IMappingEngine mapper, ITeamProvider teamProvider, IPermissionProvider permissionProvider, IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_mapper = mapper;
			_teamProvider = teamProvider;
			_permissionProvider = permissionProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}


		public TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id)
		{
			var domainData = _mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(date, id));
			var viewmodel = _mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(domainData);
			viewmodel.ShiftTradePermisssion =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			return viewmodel;
		}

		public IEnumerable<ISelectOption> CreateTeamOrGroupOptionsViewModel(DateOnly date)
		{
			return _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages) ?
				       createGroupPagesOptions(date) : createTeamOptions(date);
		}

		private IEnumerable<ISelectOption> createTeamOptions(DateOnly date)
		{
			var teams = _teamProvider.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.TeamSchedule).ToList();
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
			var groupPages = _groupingReadOnlyRepository.AvailableGroupPages().Select(
				p =>
					{
						if (p.PageName.StartsWith("xx", StringComparison.OrdinalIgnoreCase))
						{
							p.PageName = Resources.ResourceManager.GetString(p.PageName.Substring(2));
						}
						return new SelectGroup {text = p.PageName, PageId = p.PageId};
					}).OrderBy(x => x.text).ToArray();


			var details = _groupingReadOnlyRepository.AvailableGroups(date).ToArray();

			foreach (var page in groupPages)
			{
				var pageId = page.PageId;

				if (pageId.ToString().ToUpperInvariant() == PageMain)
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
			                     group d by new {d.GroupId, GroupName = prefix + d.GroupName}
			                     into g
			                     select g;
			var groups = detailsByGroup.Where(
				p => p.Any(d =>
				           _permissionProvider.HasOrganisationDetailPermission(
					           DefinedRaptorApplicationFunctionPaths.ViewSchedules,
					           date, d))).Select(
						           p =>
						           new SelectOptionItem {text = p.Key.GroupName, id = p.Key.GroupId.ToString()});
			page.children = groups.ToArray();
		}
	}
}