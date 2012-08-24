using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMatrixHelper
    {

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
		/// <returns></returns>
        bool ScheduleRemovedDayOffDays(
            IList<DateOnly> daysOffToReschedule,
            IGroupPerson groupPerson,
            IGroupSchedulingService groupSchedulingService,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            ISchedulingOptions schedulingOptions,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);

    	IList<DateOnly> GoBackToLegalState(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson,
    	                                   ISchedulingOptions schedulingOptions);

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
    }

	public class GroupMatrixHelper : IGroupMatrixHelper
    {
        private readonly IGroupMatrixContainerCreator _groupMatrixContainerCreator;
    	private readonly IGroupPersonConsistentChecker _groupPersonConsistentChecker;
		private readonly IWorkShiftBackToLegalStateServicePro _workShiftBackToLegalStateServicePro;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private IList<IScheduleMatrixPro> _allMatrixes;

        public GroupMatrixHelper(IGroupMatrixContainerCreator groupMatrixContainerCreator, IGroupPersonConsistentChecker groupPersonConsistentChecker,
			IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro, IResourceOptimizationHelper resourceOptimizationHelper)
        {
        	_groupMatrixContainerCreator = groupMatrixContainerCreator;
        	_groupPersonConsistentChecker = groupPersonConsistentChecker;
        	_workShiftBackToLegalStateServicePro = workShiftBackToLegalStateServicePro;
        	_resourceOptimizationHelper = resourceOptimizationHelper;
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
            foreach (var groupMember in groupPerson.GroupMembers)
            {
                IScheduleMatrixPro scheduleMatrix = findGroupMatrix(allMatrixes, groupMember, daysOffToRemove[0]);
                if (scheduleMatrix == null)
                    return null;

                _allMatrixes = allMatrixes;

                GroupMatrixContainer matrixContainer =
                    _groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, scheduleMatrix, daysOffPreferences);
                if (matrixContainer == null)
                    return null;
                containers.Add(matrixContainer);
            }
            return containers;
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

			IScheduleMatrixPro scheduleMatrix = findGroupMatrix(allMatrixes, person, daysOffToRemove[0]);
			if (scheduleMatrix == null)
				return null;

			_allMatrixes = allMatrixes;

			GroupMatrixContainer matrixContainer =
				_groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, scheduleMatrix,
				                                                        daysOffPreferences);
			if (matrixContainer == null)
				return null;
			containers.Add(matrixContainer);

			return containers;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool ExecuteDayOffMoves(IList<GroupMatrixContainer> containers, IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            foreach (var matrixContainer in containers)
            {
                if (!dayOffDecisionMakerExecuter.Execute(matrixContainer.WorkingArray, matrixContainer.OriginalArray, matrixContainer.Matrix, null, false, false, false))
                {
                    schedulePartModifyAndRollbackService.Rollback();
                    return false;
                }
            }
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ScheduleRemovedDayOffDays(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
			foreach (var dateOnly in daysOffToReschedule)
			{
				IGroupPerson groupPersonToRun = groupPersonBuilderForOptimization.BuildGroupPerson(groupPerson.GroupMembers[0],
				                                                                                   dateOnly);
				_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPersonToRun,
																		   dateOnly, schedulingOptions);
				if (!groupSchedulingService.ScheduleOneDay(dateOnly, schedulingOptions, groupPersonToRun, _allMatrixes))
                {
                    schedulePartModifyAndRollbackService.Rollback();
                    return false;
                }
            }
            return true;
        }

		public IList<DateOnly> GoBackToLegalState(IList<DateOnly> daysOffToReschedule, IGroupPerson groupPerson,
			ISchedulingOptions schedulingOptions)
		{
			List<DateOnly> returnList = new List<DateOnly>();
			foreach (var groupMember in groupPerson.GroupMembers)
			{
				foreach (var dateOnly in daysOffToReschedule)
				{
					foreach (var scheduleMatrixPro in _allMatrixes)
					{
						if (scheduleMatrixPro.Person == groupMember)
						{
							if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
							{
								var result = removeIllegalWorkTimeDays(scheduleMatrixPro, schedulingOptions);
								if (result == null)
									return null;

								returnList.AddRange(result);
							}
						}

					}
				}
			}
			HashSet<DateOnly> uniqeDates = new HashSet<DateOnly>(returnList);

			foreach (var uniqeDate in uniqeDates)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(uniqeDate, true, true);
				_resourceOptimizationHelper.ResourceCalculateDate(uniqeDate.AddDays(1), true, true);
			}

			return new List<DateOnly>(uniqeDates);

		}

		private IList<DateOnly> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
		{
			if (!_workShiftBackToLegalStateServicePro.Execute(matrix, schedulingOptions))
				return null;

			IList<DateOnly> removedIllegalDates = _workShiftBackToLegalStateServicePro.RemovedDays;
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