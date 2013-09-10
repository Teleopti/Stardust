using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamDayOffModifier
	{
		void AddDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions);

		void RemoveDayOff(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                                      ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void AddDayOffAndResourceCalculate(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
								   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
			IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			if (schedulingOptions.UseSameDayOffs) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleRange range = scheduleDictionary[person];
					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);

					scheduleDay.DeleteMainShift(scheduleDay);
					scheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
				}
			}
			_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, schedulingOptions.ConsiderShortBreaks);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void RemoveDayOff(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
								   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
			if (schedulePartModifyAndRollbackService == null)
				throw new ArgumentNullException("schedulePartModifyAndRollbackService");

			IScheduleDictionary scheduleDictionary = _stateHolder.Schedules;
			if (schedulingOptions.UseSameDayOffs) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleRange range = scheduleDictionary[person];
					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
					scheduleDay.DeleteDayOff();
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
				}
			}
		}
	}
}