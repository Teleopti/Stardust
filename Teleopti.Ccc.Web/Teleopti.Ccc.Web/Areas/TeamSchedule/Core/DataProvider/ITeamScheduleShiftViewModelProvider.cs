using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleShiftViewModelProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
		AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);

		GroupScheduleShiftViewModel MakeViewModel(IPerson person,
			DateOnly date,
			IScheduleDay scheduleDay,
			IScheduleDay previousScheduleDay,
			bool canViewConfidential,
			bool canViewUnpublished);

		PersonWeekScheduleViewModel MakeWeekViewModel(
				IPerson person,
				IList<DateOnly> weekDays,
				IScheduleRange scheduleRange,
				IDictionary<DateOnly, List<Guid>> peopleCanSeeSchedulesFor,
				IDictionary<DateOnly, List<Guid>> peopleCanSeeUnpublishedSchedulesFor,
				IDictionary<DateOnly, List<Guid>> viewableConfidentialAbsenceAgents);

		bool IsOvertimeOnDayOff(IScheduleDay scheduleDay);
	}
}