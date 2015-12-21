using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleViewModelFactory
	{
		PagingGroupScheduleShiftViewModel CreateViewModel(Guid groupId, DateTime dateInUserTimeZone, int pageSize, int currentPageIndex);
		GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex);
		GroupScheduleViewModel CreateViewModel(GroupScheduleInput input);
	}
}