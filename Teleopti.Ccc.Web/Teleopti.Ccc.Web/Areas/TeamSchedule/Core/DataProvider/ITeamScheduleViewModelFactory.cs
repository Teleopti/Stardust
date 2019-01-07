using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleViewModelFactory
	{
		GroupScheduleViewModel CreateViewModel(SearchDaySchedulesInput input);
		GroupScheduleViewModel CreateViewModelForPeople(Guid[] personIds, DateOnly scheduleDate);

		GroupWeekScheduleViewModel CreateWeekScheduleViewModel(SearchSchedulesInput input);
	}

	public class SearchDaySchedulesInput : SearchSchedulesInput
	{
		public bool IsOnlyAbsences { get; set; }
		public TeamScheduleSortOption SortOption { get; set; }
	}
	public class SearchSchedulesInput
	{
		public IDictionary<PersonFinderField, string> CriteriaDictionary { get; set; }
		public DateOnly DateInUserTimeZone { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
		public Guid[] GroupIds { get; set; }
		public Guid? GroupPageId { get; set; }
		public string[] DynamicOptionalValues { get; set; }

		public bool IsDynamic => DynamicOptionalValues?.Any() ?? false;

		public bool NoGroupInput => (GroupIds == null || !GroupIds.Any())
				&& (DynamicOptionalValues == null || !DynamicOptionalValues.Any());
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