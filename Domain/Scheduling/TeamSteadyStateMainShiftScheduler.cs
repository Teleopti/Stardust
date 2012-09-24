using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class TeamSteadyStateMainShiftScheduler
	{
		private readonly IGroupMatrixHelper _groupMatrixHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IScheduleTagSetter _scheduleTagSetter;

		public TeamSteadyStateMainShiftScheduler(IGroupMatrixHelper groupMatrixHelper, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTagSetter scheduleTagSetter)
		{
			_groupMatrixHelper = groupMatrixHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleTagSetter = scheduleTagSetter;
		}

		public bool ScheduleTeam(DateOnly dateOnly, IPerson person, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> matrixes, IScheduleDictionary scheduleDictionary)
		{
			if(groupPersonBuilderForOptimization == null) throw new ArgumentNullException("groupPersonBuilderForOptimization");
			if(scheduleDictionary == null) throw new ArgumentNullException("scheduleDictionary");

			if (!_groupMatrixHelper.ScheduleSinglePerson(dateOnly, person, groupSchedulingService, rollbackService, schedulingOptions, groupPersonBuilderForOptimization, matrixes))
				return false;

			var scheduleRangeFirstPerson = scheduleDictionary[person];
			var scheduleDayFirstPerson = scheduleRangeFirstPerson.ScheduledDay(dateOnly);
			var personAssignmentFirstPerson = scheduleDayFirstPerson.AssignmentHighZOrder();
			var mainShift = personAssignmentFirstPerson.MainShift;
			var groupPerson = groupPersonBuilderForOptimization.BuildGroupPerson(person, dateOnly);

			foreach (var groupMember in groupPerson.GroupMembers)
			{
				if (groupMember.Equals(person)) continue;
				var scheduleRange = scheduleDictionary[groupMember];
				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
				var cloneMainShift = mainShift.NoneEntityClone() as IMainShift;
				scheduleDay.AddMainShift(cloneMainShift);
				scheduleDictionary.Modify(ScheduleModifier.AutomaticScheduling, new List<IScheduleDay> {scheduleDay}, null, _scheduleDayChangeCallback, _scheduleTagSetter);
			}

			return true;
		}
	}
}
