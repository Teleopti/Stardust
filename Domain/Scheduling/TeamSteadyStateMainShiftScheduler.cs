using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateMainShiftScheduler
	{
		bool ScheduleTeam(DateOnly dateOnly, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService,
		                  ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
		                  IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
		                  IList<IScheduleMatrixPro> matrixes, IScheduleDictionary scheduleDictionary);
	}

	public class TeamSteadyStateMainShiftScheduler : ITeamSteadyStateMainShiftScheduler
	{
		private readonly IGroupMatrixHelper _groupMatrixHelper;
		private DateOnly _dateOnly;

		public TeamSteadyStateMainShiftScheduler(IGroupMatrixHelper groupMatrixHelper)
		{
			_groupMatrixHelper = groupMatrixHelper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ScheduleTeam(DateOnly dateOnly, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> matrixes, IScheduleDictionary scheduleDictionary)	
		{
			if(groupPersonBuilderForOptimization == null) throw new ArgumentNullException("groupPersonBuilderForOptimization");
			if(scheduleDictionary == null) throw new ArgumentNullException("scheduleDictionary");

			_dateOnly = dateOnly;

			var assignedPersons = new List<IPerson>();
			IPersonAssignment personAssignmentSource = null;

			foreach (var groupMember in groupPerson.GroupMembers)
			{
				var person = groupMember;

				var scheduleRangeSource = scheduleDictionary[person];
				var scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
				var schedulePartViewSource = scheduleDaySource.SignificantPart();
				var locked = IsLocked(matrixes, scheduleDaySource);
				
				if (schedulePartViewSource == SchedulePartView.FullDayAbsence || schedulePartViewSource == SchedulePartView.DayOff || schedulePartViewSource == SchedulePartView.ContractDayOff || locked)
				{
					assignedPersons.Add(person);
					continue;
				}

				if (!_groupMatrixHelper.ScheduleSinglePerson(dateOnly, groupMember, groupSchedulingService, schedulingOptions, groupPersonBuilderForOptimization, matrixes))
				{
					return false;
				}

				scheduleRangeSource = scheduleDictionary[person];
				scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
				personAssignmentSource = scheduleDaySource.AssignmentHighZOrder();
				assignedPersons.Add(person);
				break;
			}
			
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				if (assignedPersons.Contains(groupMember)) 
					continue;

				if (personAssignmentSource == null) 
					return false;

				var mainShift = personAssignmentSource.MainShift;
				var scheduleRange = scheduleDictionary[groupMember];
				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
				var significantPart = scheduleDay.SignificantPart();
				var cloneMainShift = mainShift.NoneEntityClone() as IMainShift;

				if (significantPart == SchedulePartView.MainShift)
					return false;

				if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.FullDayAbsence) 
					continue;

				var locked = IsLocked(matrixes, scheduleDay);

				if (locked) continue;
				scheduleDay.AddMainShift(cloneMainShift);
				rollbackService.Modify(scheduleDay);
			}

			return true;
		}

		private bool IsLocked(IEnumerable<IScheduleMatrixPro> matrixes, IScheduleDay scheduleDay)
		{
			var locked = false;
			foreach (var scheduleMatrixPro in matrixes)
			{
				if (scheduleMatrixPro.Person != scheduleDay.Person) continue;
				if (!scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(_dateOnly)) continue;
				if (!scheduleMatrixPro.UnlockedDays.Contains(scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)))
					locked = true;
			}

			return locked;
		}
	}
}
