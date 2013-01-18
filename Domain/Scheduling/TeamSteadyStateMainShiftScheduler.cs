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

        //bool ScheduleTeamForAdvanceSchedulingService(DateOnly dateOnly, IGroupPerson groupPerson,
        //                                             IGroupSchedulingService groupSchedulingService,
        //                                             ISchedulePartModifyAndRollbackService rollbackService,
        //                                             ISchedulingOptions schedulingOptions,
        //                                             IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
        //                                             IList<IScheduleMatrixPro> matrixes,
        //                                             IScheduleDictionary scheduleDictionary,
        //                                             IShiftProjectionCache shiftProjectionCache);
	}

	public class TeamSteadyStateMainShiftScheduler : ITeamSteadyStateMainShiftScheduler
	{
		private readonly IGroupMatrixHelper _groupMatrixHelper;
		private readonly ITeamSteadyStateCoherentChecker _coherentChecker;
		private readonly ITeamSteadyStateScheduleMatrixProFinder _teamSteadyStateScheduleMatrixProFinder;

		public TeamSteadyStateMainShiftScheduler(IGroupMatrixHelper groupMatrixHelper, ITeamSteadyStateCoherentChecker coherentChecker, ITeamSteadyStateScheduleMatrixProFinder teamSteadyStateScheduleMatrixProFinder)
		{
			_groupMatrixHelper = groupMatrixHelper;
			_coherentChecker = coherentChecker;
			_teamSteadyStateScheduleMatrixProFinder = teamSteadyStateScheduleMatrixProFinder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ScheduleTeam(DateOnly dateOnly, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> matrixes, IScheduleDictionary scheduleDictionary)	
		{
			if(groupPersonBuilderForOptimization == null) throw new ArgumentNullException("groupPersonBuilderForOptimization");
			if(scheduleDictionary == null) throw new ArgumentNullException("scheduleDictionary");

			var assignedPersons = new List<IPerson>();
			IPersonAssignment personAssignmentSource = null;
			IScheduleDay dayToCalculate = null;

			//schedule first group member or use existing main shift as source, return false if there are non coherent mainshifts
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				var person = groupMember;

				var scheduleRangeSource = scheduleDictionary[person];
				var scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
				var schedulePartViewSource = scheduleDaySource.SignificantPart();

				var theMatrix = _teamSteadyStateScheduleMatrixProFinder.MatrixPro(matrixes, scheduleDaySource);
				if (theMatrix == null) continue;

				scheduleDaySource = _coherentChecker.CheckCoherent(matrixes, dateOnly, scheduleDictionary, scheduleDaySource, groupPerson.GroupMembers);

				if (scheduleDaySource == null)
					return false;

				var locked = !theMatrix.UnlockedDays.Contains(theMatrix.GetScheduleDayByKey(dateOnly));
				
				if (schedulePartViewSource == SchedulePartView.FullDayAbsence || schedulePartViewSource == SchedulePartView.DayOff || schedulePartViewSource == SchedulePartView.ContractDayOff || locked)
				{
					assignedPersons.Add(person);
					continue;
				}

				var matrixList = new List<IScheduleMatrixPro> {theMatrix};

				if (scheduleDaySource.SignificantPart() != SchedulePartView.MainShift)
				{
					if (!groupSchedulingService.ScheduleOneDayOnePersonSteadyState(dateOnly, groupMember, schedulingOptions, groupPerson, matrixList))
					{
						return false;
					}

					scheduleRangeSource = scheduleDictionary[person];
					scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
					assignedPersons.Add(person);
				}

				dayToCalculate = scheduleDaySource;
				personAssignmentSource = scheduleDaySource.AssignmentHighZOrder();
				break;
			}
			
			//add the shift to other group memembers
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

				if (significantPart == SchedulePartView.MainShift) continue;
					
				if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.FullDayAbsence) continue;

				var theMatrix = _teamSteadyStateScheduleMatrixProFinder.MatrixPro(matrixes, scheduleDay);
				if (theMatrix == null) continue;

				var locked = !theMatrix.UnlockedDays.Contains(theMatrix.GetScheduleDayByKey(dateOnly));
				
				if (locked) continue;
				scheduleDay.AddMainShift(cloneMainShift);
				rollbackService.Modify(scheduleDay);
			}

			if (dayToCalculate != null)
				_groupMatrixHelper.SafeResourceCalculate(new List<IScheduleDay> { dayToCalculate });
			return true;
		}


        //public bool ScheduleTeamForAdvanceSchedulingService(DateOnly dateOnly, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService,
        //                                    ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions, 
        //                                    IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> matrixes,
        //                                    IScheduleDictionary scheduleDictionary, IShiftProjectionCache shiftProjectionCache )
        //{
        //    if (groupPersonBuilderForOptimization == null) throw new ArgumentNullException("groupPersonBuilderForOptimization");
        //    if (scheduleDictionary == null) throw new ArgumentNullException("scheduleDictionary");

        //    var assignedPersons = new List<IPerson>();
        //    IPersonAssignment personAssignmentSource = null;
        //    IScheduleDay dayToCalculate = null;

        //    //schedule first group member or use existing main shift as source, return false if there are non coherent mainshifts
        //    foreach (var groupMember in groupPerson.GroupMembers)
        //    {
        //        var person = groupMember;

        //        var scheduleRangeSource = scheduleDictionary[person];
        //        var scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
        //        var schedulePartViewSource = scheduleDaySource.SignificantPart();

        //        var theMatrix = _teamSteadyStateScheduleMatrixProFinder.MatrixPro(matrixes, scheduleDaySource);
        //        if (theMatrix == null) continue;

        //        scheduleDaySource = _coherentChecker.CheckCoherent(matrixes, dateOnly, scheduleDictionary, scheduleDaySource, groupPerson.GroupMembers);

        //        if (scheduleDaySource == null)
        //            return false;

        //        var locked = !theMatrix.UnlockedDays.Contains(theMatrix.GetScheduleDayByKey(dateOnly));

        //        if (schedulePartViewSource == SchedulePartView.FullDayAbsence || schedulePartViewSource == SchedulePartView.DayOff || schedulePartViewSource == SchedulePartView.ContractDayOff || locked)
        //        {
        //            assignedPersons.Add(person);
        //            continue;
        //        }

        //        var matrixList = new List<IScheduleMatrixPro> { theMatrix };

        //        if (scheduleDaySource.SignificantPart() != SchedulePartView.MainShift)
        //        {
        //            if (!groupSchedulingService.ScheduleForTeamSteadyState(dateOnly, groupMember, schedulingOptions, matrixList, shiftProjectionCache))
        //            {
        //                return false;
        //            }

        //            scheduleRangeSource = scheduleDictionary[person];
        //            scheduleDaySource = scheduleRangeSource.ScheduledDay(dateOnly);
        //            assignedPersons.Add(person);
        //        }

        //        dayToCalculate = scheduleDaySource;
        //        personAssignmentSource = scheduleDaySource.AssignmentHighZOrder();
        //        break;
        //    }

        //    //add the shift to other group memembers
        //    foreach (var groupMember in groupPerson.GroupMembers)
        //    {
        //        if (assignedPersons.Contains(groupMember))
        //            continue;

        //        if (personAssignmentSource == null)
        //            return false;

        //        var mainShift = personAssignmentSource.MainShift;
        //        var scheduleRange = scheduleDictionary[groupMember];
        //        var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
        //        var significantPart = scheduleDay.SignificantPart();
        //        var cloneMainShift = mainShift.NoneEntityClone() as IMainShift;

        //        if (significantPart == SchedulePartView.MainShift) continue;

        //        if (significantPart == SchedulePartView.DayOff || significantPart == SchedulePartView.FullDayAbsence) continue;

        //        var theMatrix = _teamSteadyStateScheduleMatrixProFinder.MatrixPro(matrixes, scheduleDay);
        //        if (theMatrix == null) continue;

        //        var locked = !theMatrix.UnlockedDays.Contains(theMatrix.GetScheduleDayByKey(dateOnly));

        //        if (locked) continue;
        //        scheduleDay.AddMainShift(cloneMainShift);
        //        rollbackService.Modify(scheduleDay);
        //    }

        //    if (dayToCalculate != null)
        //        _groupMatrixHelper.SafeResourceCalculate(new List<IScheduleDay> { dayToCalculate });
        //    return true;
        //}
	
    }


}
