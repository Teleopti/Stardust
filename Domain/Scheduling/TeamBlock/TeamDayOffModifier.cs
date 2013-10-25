﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamDayOffModifier
	{
		void AddDayOffForTeamAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
														   ITeamInfo teamInfo, DateOnly dateOnly, IDayOffTemplate dayOffTemplate);

		void RemoveDayOffForTeam(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                      ITeamInfo teamInfo, DateOnly dateOnly);

		void RemoveDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                           IPerson person, DateOnly dateOnly);

		void AddDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IPerson person,
		                        DateOnly dateOnly, IDayOffTemplate dayOffTemplate, bool resourceCalculate);
	}

	public class TeamDayOffModifier : ITeamDayOffModifier
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ISchedulingResultStateHolder _stateHolder;

		public TeamDayOffModifier(IResourceOptimizationHelper resourceOptimizationHelper, ISchedulingResultStateHolder stateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
		}

		public void AddDayOffForTeamAndResourceCalculate(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ITeamInfo teamInfo, DateOnly dateOnly, IDayOffTemplate dayOffTemplate)
		{
			foreach (var person in teamInfo.GroupPerson.GroupMembers)
			{
				AddDayOffForMember(schedulePartModifyAndRollbackService, person, dateOnly, dayOffTemplate, false);
			}

			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
		}

		public void AddDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IPerson person, DateOnly dateOnly, IDayOffTemplate dayOffTemplate,
		                               bool resourceCalculate)
		{
			IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			IScheduleRange range = scheduleDictionary[person];
			IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
			scheduleDay.DeleteMainShift(scheduleDay);
			scheduleDay.CreateAndAddDayOff(dayOffTemplate);
			schedulePartModifyAndRollbackService.Modify(scheduleDay);

			if (resourceCalculate)
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
		}

		public void RemoveDayOffForTeam(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                ITeamInfo teamInfo, DateOnly dateOnly)
		{
			foreach (var person in teamInfo.GroupPerson.GroupMembers)
			{
				RemoveDayOffForMember(schedulePartModifyAndRollbackService, person, dateOnly);
			}

		}

		public void RemoveDayOffForMember(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
										  IPerson person, DateOnly dateOnly)
		{
			IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			IScheduleRange range = scheduleDictionary[person];
			IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
			scheduleDay.DeleteDayOff();
			schedulePartModifyAndRollbackService.Modify(scheduleDay);
		}
	}
}