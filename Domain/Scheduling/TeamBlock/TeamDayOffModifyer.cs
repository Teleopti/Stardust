

using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamDayOffModifyer
	{
		void AddDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions);

		void RemoveDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                      ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions);
	}

	public class TeamDayOffModifyer : ITeamDayOffModifyer
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ISchedulingResultStateHolder _stateHolder;

		public TeamDayOffModifyer(IResourceOptimizationHelper resourceOptimizationHelper, ISchedulingResultStateHolder stateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
		}

		public void AddDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
								   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
			IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();
			IList<IScheduleDay> toAdd = new List<IScheduleDay>();
			if (schedulingOptions.UseSameDayOffs) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleRange range = scheduleDictionary[person];
					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
					toRemove.Add((IScheduleDay)scheduleDay.Clone());
					scheduleDay.DeleteMainShift(scheduleDay);
					scheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
					toAdd.Add(range.ReFetch(scheduleDay));
				}
			}
			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, schedulingOptions.ConsiderShortBreaks, toRemove, toAdd);
		}

		public void RemoveDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
								   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
		    if (schedulePartModifyAndRollbackService == null)
		        throw new ArgumentNullException("schedulePartModifyAndRollbackService");
		    IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();
			IList<IScheduleDay> toAdd = new List<IScheduleDay>();
			if (schedulingOptions.UseSameDayOffs) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleRange range = scheduleDictionary[person];
					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
					toRemove.Add((IScheduleDay)scheduleDay.Clone());
					scheduleDay.DeleteDayOff();
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
					toAdd.Add(range.ReFetch(scheduleDay));
				}
			}
			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, schedulingOptions.ConsiderShortBreaks, toRemove, toAdd);

		}
	}
}