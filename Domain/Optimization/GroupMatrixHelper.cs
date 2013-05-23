using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMatrixHelper
    {
		/// <summary>
		/// Calculate resources
		/// </summary>
		/// <param name="scheduleDays"></param>
    	void SafeResourceCalculate(IList<IScheduleDay> scheduleDays);

        /// <summary>
        /// Executes the day off moves.
        /// </summary>
        /// <param name="containers">The containers.</param>
        /// <param name="dayOffDecisionMakerExecuter">The day off decision maker executer.</param>
        /// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
        /// <returns></returns>
        bool ExecuteDayOffMoves(
            IList<GroupMatrixContainer> containers,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);

		/// <summary>
		/// Schedules the removed day off days.
		/// </summary>
		/// <param name="daysOffToReschedule">The days off to reschedule.</param>
		/// <param name="groupPerson">The group person.</param>
		/// <param name="groupSchedulingService">The group scheduling service.</param>
		/// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="groupPersonBuilderForOptimization">The group person builder for optimization.</param>
		/// <param name="allMatrixes">All matrixes.</param>
		/// <returns></returns>
    	bool ScheduleRemovedDayOffDays(
    		IList<DateOnly> daysOffToReschedule,
    		IGroupPerson groupPerson,
    		IGroupSchedulingService groupSchedulingService,
    		ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
    		ISchedulingOptions schedulingOptions,
    		IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			IList<IScheduleMatrixPro> allMatrixes);

    	IList<IScheduleDay> GoBackToLegalState(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson,
										   ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, ISchedulePartModifyAndRollbackService rollbackService);

    	bool ScheduleSinglePerson(DateOnly dayToReschedule, IPerson person,
    	                          IGroupSchedulingService groupSchedulingService,
    	                          ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
    	                          ISchedulingOptions schedulingOptions,
    	                          IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
    	                          IList<IScheduleMatrixPro> allMatrixes);

		//bool ScheduleSinglePerson(DateOnly dayToReschedule, IPerson person,
		//                          IGroupSchedulingService groupSchedulingService,
		//                          ISchedulingOptions schedulingOptions,
		//                          IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
		//                          IList<IScheduleMatrixPro> allMatrixes);

        /// <summary>
        /// Creates the group matrix containers.
        /// </summary>
        /// <param name="allMatrixes">All matrixes.</param>
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="daysOffToAdd">The days off to add.</param>
        /// <param name="groupPerson">The group person.</param>
        /// <param name="daysOffPreferences">The rule set.</param>
        /// <returns>
        /// Returns the created list of matrix container.
        /// Returns null if no matrix for a groupperson found or some problem turn up with the creation of matrix container
        /// </returns>
        IList<GroupMatrixContainer> CreateGroupMatrixContainers(
            IList<IScheduleMatrixPro> allMatrixes,
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IGroupPerson groupPerson,
            IDaysOffPreferences daysOffPreferences);

		IList<GroupMatrixContainer> CreateGroupMatrixContainers(
			IList<IScheduleMatrixPro> allMatrixes,
			IList<DateOnly> daysOffToRemove,
			IList<DateOnly> daysOffToAdd,
			IPerson person,
			IDaysOffPreferences daysOffPreferences);

    	bool ScheduleBackToLegalStateDays(IList<IScheduleDay> removedDays, IGroupSchedulingService groupSchedulingService,
    	                                  ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
    	                                  ISchedulingOptions schedulingOptions,
    	                                  IOptimizationPreferences optimizationPreferences,
    	                                  IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
    	                                  IList<IScheduleMatrixPro> allMatrixes);
    }

	public class GroupMatrixHelper : IGroupMatrixHelper
    {
        private readonly IGroupMatrixContainerCreator _groupMatrixContainerCreator;
    	private readonly IGroupPersonConsistentChecker _groupPersonConsistentChecker;
		private readonly IWorkShiftBackToLegalStateServicePro _workShiftBackToLegalStateServicePro;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public GroupMatrixHelper(IGroupMatrixContainerCreator groupMatrixContainerCreator, IGroupPersonConsistentChecker groupPersonConsistentChecker,
			IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro, IResourceOptimizationHelper resourceOptimizationHelper,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
        {
        	_groupMatrixContainerCreator = groupMatrixContainerCreator;
        	_groupPersonConsistentChecker = groupPersonConsistentChecker;
        	_workShiftBackToLegalStateServicePro = workShiftBackToLegalStateServicePro;
        	_resourceOptimizationHelper = resourceOptimizationHelper;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
        }

		

		/// <summary>
        /// Creates the group matrix containers.
        /// </summary>
        /// <param name="allMatrixes">All matrixes.</param>
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="daysOffToAdd">The days off to add.</param>
        /// <param name="groupPerson">The group person.</param>
        /// <param name="daysOffPreferences">The rule set.</param>
        /// <returns>
        /// Returns the created list of matrix container.
        /// Returns null if no matrix for a groupperson found or some problem turn up with the creation of matrix container
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IList<GroupMatrixContainer> CreateGroupMatrixContainers(
            IList<IScheduleMatrixPro> allMatrixes, 
            IList<DateOnly> daysOffToRemove, 
            IList<DateOnly> daysOffToAdd, 
            IGroupPerson groupPerson, 
            IDaysOffPreferences daysOffPreferences
            )
        {
            IList<GroupMatrixContainer> containers = new List<GroupMatrixContainer>();

			//GroupMatrixContainer lastContainer = null;
            foreach (var groupMember in groupPerson.GroupMembers)
            {
				
            	GroupMatrixContainer container = findContainerForPerson(allMatrixes, daysOffToRemove, daysOffToAdd,
            	                                                        groupMember, daysOffPreferences);
				//if (lastContainer != null)
				//{
				//    for (int i = 0; i < container.OriginalArray.DaysOffBitArray.Count; i++)
				//    {
				//        if (lastContainer.OriginalArray.DaysOffBitArray[i] != container.OriginalArray.DaysOffBitArray[i])
				//            return null;
				//        if (lastContainer.WorkingArray.DaysOffBitArray[i] != container.WorkingArray.DaysOffBitArray[i])
				//            return null;
				//    }
				//}

				if (container == null)
					return null;
					
				containers.Add(container);

				//lastContainer = container;
            }
            return containers;
        }

		private GroupMatrixContainer findContainerForPerson(IEnumerable<IScheduleMatrixPro> allMatrixes, 
            IList<DateOnly> daysOffToRemove, 
            IList<DateOnly> daysOffToAdd, 
			IPerson groupMember,
			IDaysOffPreferences daysOffPreferences)
		{
			IScheduleMatrixPro scheduleMatrix = findGroupMatrix(allMatrixes, groupMember, daysOffToRemove[0]);
			if (scheduleMatrix == null)
				return null;

			GroupMatrixContainer matrixContainer =
				_groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, scheduleMatrix, daysOffPreferences);
			if (matrixContainer == null)
				return null;

			return matrixContainer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IList<GroupMatrixContainer> CreateGroupMatrixContainers(
		   IList<IScheduleMatrixPro> allMatrixes,
		   IList<DateOnly> daysOffToRemove,
		   IList<DateOnly> daysOffToAdd,
		   IPerson person,
		   IDaysOffPreferences daysOffPreferences
		   )
		{
			IList<GroupMatrixContainer> containers = new List<GroupMatrixContainer>();

			GroupMatrixContainer container = findContainerForPerson(allMatrixes, daysOffToRemove, daysOffToAdd,
																		person, daysOffPreferences);
			if (container != null)
				containers.Add(container);

			return containers;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ScheduleBackToLegalStateDays(IList<IScheduleDay> removedDays, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> allMatrixes)
		{
			foreach (var scheduleDay in removedDays)
			{
				DateOnly date = scheduleDay.DateOnlyAsPeriod.DateOnly;
				_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, optimizationPreferences, scheduleDay.AssignmentHighZOrder().ToMainShift(), date);
				if (!ScheduleSinglePerson(date, scheduleDay.Person, groupSchedulingService, schedulePartModifyAndRollbackService, schedulingOptions, groupPersonBuilderForOptimization, allMatrixes))
					return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool ExecuteDayOffMoves(IList<GroupMatrixContainer> containers, IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
			for (int index = 0; index < containers.Count; index++)
            {
				//if (containers[0].WorkingArray.ToString() != containers[index].WorkingArray.ToString())
				//    return false;
				//if (containers[0].OriginalArray.ToString() != containers[index].OriginalArray.ToString())
				//    return false;
				if (!dayOffDecisionMakerExecuter.Execute(containers[index].WorkingArray, containers[index].OriginalArray, containers[index].Matrix, null, false, false, false))
                {
					IList<IScheduleDay> days = new List<IScheduleDay>(schedulePartModifyAndRollbackService.ModificationCollection);
                    schedulePartModifyAndRollbackService.Rollback();
					SafeResourceCalculate(days);
                    return false;
                }
				//if (containers[0].WorkingArray.ToString() != containers[index].WorkingArray.ToString())
				//    return false;
				//if (containers[0].OriginalArray.ToString() != containers[index].OriginalArray.ToString())
				//    return false;
            }
            return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ScheduleRemovedDayOffDays(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> allMatrixes)
        {
			foreach (var dateOnly in daysOffToReschedule)
			{
				IGroupPerson groupPersonToRun = groupPersonBuilderForOptimization.BuildGroupPerson(groupPerson.GroupMembers[0],
				                                                                                   dateOnly);
				_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPersonToRun,
																		   dateOnly, schedulingOptions);
				if (!groupSchedulingService.ScheduleOneDay(dateOnly, schedulingOptions, groupPersonToRun, allMatrixes))
				{
					IList<IScheduleDay> days = new List<IScheduleDay>(schedulePartModifyAndRollbackService.ModificationCollection);
                    schedulePartModifyAndRollbackService.Rollback();
					SafeResourceCalculate(days);
                    return false;
                }
            }
            return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ScheduleSinglePerson(DateOnly dayToReschedule, IPerson person, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> allMatrixes)
		{
			IGroupPerson groupPersonToRun = groupPersonBuilderForOptimization.BuildGroupPerson(person,
																								   dayToReschedule);
			if(!_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPersonToRun, dayToReschedule, schedulingOptions))
				return false;
			if (!groupSchedulingService.ScheduleOneDayOnOnePerson(dayToReschedule, person, schedulingOptions, groupPersonToRun, allMatrixes))
			{
				schedulePartModifyAndRollbackService.Rollback();
				return false;
			}
			return true;
		}

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		//public bool ScheduleSinglePerson(DateOnly dayToReschedule, IPerson person, IGroupSchedulingService groupSchedulingService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IList<IScheduleMatrixPro> allMatrixes)
		//{
		//    IGroupPerson groupPersonToRun = groupPersonBuilderForOptimization.BuildGroupPerson(person,
		//                                                                                           dayToReschedule);
		//    if (!_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPersonToRun, dayToReschedule, schedulingOptions))
		//        return false;
		//    if (!groupSchedulingService.ScheduleOneDayOnOnePerson(dayToReschedule, person, schedulingOptions, groupPersonToRun, allMatrixes))
		//    {
		//        return false;
		//    }
		//    return true;
		//}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IScheduleDay> GoBackToLegalState(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson,
			ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var returnList = new List<IScheduleDay>();
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				foreach (var dateOnly in daysOffToReschedule)
				{
					foreach (var scheduleMatrixPro in allMatrixes)
					{
						if (scheduleMatrixPro.Person == groupMember)
						{
							if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
							{
								var result = removeIllegalWorkTimeDays(scheduleMatrixPro, schedulingOptions, rollbackService);
								if (result == null)
									return null;

								returnList.AddRange(result);
							}
						}

					}
				}
			}

			SafeResourceCalculate(returnList);

			return returnList;

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SafeResourceCalculate(IList<IScheduleDay> scheduleDays)
		{
			var uniqeDates = new HashSet<DateOnly>();
			foreach (var scheduleDay in scheduleDays)
			{
				uniqeDates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
			}


			foreach (var uniqeDate in uniqeDates)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(uniqeDate, true, true);
				_resourceOptimizationHelper.ResourceCalculateDate(uniqeDate.AddDays(1), true, true);
			}
		}

		private IEnumerable<IScheduleDay> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			if (!_workShiftBackToLegalStateServicePro.Execute(matrix, schedulingOptions, rollbackService))
				return null;

			var removedIllegalDates = _workShiftBackToLegalStateServicePro.RemovedSchedules;
			return removedIllegalDates;
		}

        private static IScheduleMatrixPro findGroupMatrix(IEnumerable<IScheduleMatrixPro> allMatrixes, IPerson groupMember, DateOnly dateOnly)
        {
            foreach (var scheduleMatrix in allMatrixes)
            {
                if (scheduleMatrix.Person == groupMember)
                {
                    if (scheduleMatrix.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
                    {
                        return scheduleMatrix;
                    }
                }
            }
            return null;
        }
    }
}