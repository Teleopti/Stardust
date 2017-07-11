using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleViewModelFactory
	{
		GroupScheduleViewModel CreateViewModel(SearchDaySchedulesInput input);
		GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate);

		GroupWeekScheduleViewModel CreateWeekScheduleViewModel(Guid[] teamIds,IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly startOfWeekInUserTimeZone, int pageSize, int currentPageIndex);
	}

	public class SearchDaySchedulesInput
	{
		public Guid[] TeamIds { get; set; }
		public IDictionary<PersonFinderField, string> CriteriaDictionary { get; set; }
		public DateOnly DateInUserTimeZone { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
		public bool IsOnlyAbsences { get; set; }
		public TeamScheduleSortOption SortOption { get; set; }

	}

	public enum TeamScheduleSortOption
	{
		Default,
		FirstName,
		LastName,
		EmploymentNumber,
		StartTime,
		EndTime
	}
}