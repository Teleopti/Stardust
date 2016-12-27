using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleViewModelFactory
	{
		GroupScheduleViewModel CreateViewModel(Guid[] teamIds, IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex, bool isOnlyAbsences);
		GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate);

		GroupWeekScheduleViewModel CreateWeekScheduleViewModel(Guid[] teamIds,IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly startOfWeekInUserTimeZone, int pageSize, int currentPageIndex);
	}
}