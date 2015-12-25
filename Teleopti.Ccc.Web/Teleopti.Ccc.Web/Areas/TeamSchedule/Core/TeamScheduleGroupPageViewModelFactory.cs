using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ITeamScheduleGroupPageViewModelFactory
	{
		TeamScheduleGroupPageViewModel[] GetBusinessHierarchyPageGroupViewModelsByDate(DateOnly date);
	}

	public class TeamScheduleGroupPageViewModelFactory : ITeamScheduleGroupPageViewModelFactory
	{
		private readonly IGroupingReadOnlyRepository _groupingPageRepo;

		public TeamScheduleGroupPageViewModelFactory(IGroupingReadOnlyRepository groupingPageRepo)
		{
			_groupingPageRepo = groupingPageRepo;
		}

		public TeamScheduleGroupPageViewModel[] GetBusinessHierarchyPageGroupViewModelsByDate(DateOnly date)
		{
			var bussinessHierarchyPageGroup = getBusinessHierarchyPageGroup();
			if (bussinessHierarchyPageGroup == null)
			{
				return new TeamScheduleGroupPageViewModel[0];
			}
			var allTeams = _groupingPageRepo.AvailableGroups(bussinessHierarchyPageGroup, date);
			var teamsViewModel = createGroupPageViewModels(allTeams);
			return teamsViewModel;
		}

		private static TeamScheduleGroupPageViewModel[] createGroupPageViewModels(IEnumerable<ReadOnlyGroupDetail> groupDetails)
		{
			return groupDetails.Select(groupDetail => new TeamScheduleGroupPageViewModel
			{
				Name = groupDetail.GroupName,
				Id = groupDetail.GroupId
			}).Distinct().ToArray();
		}

		private ReadOnlyGroupPage getBusinessHierarchyPageGroup()
		{
			var allGroupPages = _groupingPageRepo.AvailableGroupPages();
			return allGroupPages.SingleOrDefault(gp => gp.PageId == Group.PageMainId);
		}
	}
}


public class TeamScheduleGroupPageViewModel
{
	public Guid Id { get; set; }
	public string Name { get; set; }
}