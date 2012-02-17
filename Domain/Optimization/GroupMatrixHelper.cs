using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupMatrixHelper
    {
        IGroupMatrixContainerCreator GroupMatrixContainerCreator { get; } 

        /// <summary>
        /// Validates the day off moves.
        /// </summary>
        /// <param name="containers">The containers.</param>
        /// <param name="validatorList">The validator list.</param>
        /// <returns></returns>
        bool ValidateDayOffMoves(
            IList<GroupMatrixContainer> containers,
            IList<IDayOffLegalStateValidator> validatorList);

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
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="groupPerson">The group person.</param>
        /// <param name="groupSchedulingService">The group scheduling service.</param>
        /// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
        /// <returns></returns>
        bool ScheduleRemovedDayOffDays(
            IList<DateOnly> daysOffToRemove,
            IGroupPerson groupPerson,
            IGroupSchedulingService groupSchedulingService,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);

        /// <summary>
        /// Creates the group matrix containers.
        /// </summary>
        /// <param name="allMatrixes">All matrixes.</param>
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="daysOffToAdd">The days off to add.</param>
        /// <param name="groupPerson">The group person.</param>
        /// <param name="ruleSet">The rule set.</param>
        /// <returns>
        /// Returns the created list of matrix container.
        /// Returns null if no matrix for a groupperson found or some problem turn up with the creation of matrix container
        /// </returns>
        IList<GroupMatrixContainer> CreateGroupMatrixContainers(
            IList<IScheduleMatrixPro> allMatrixes,
            IList<DateOnly> daysOffToRemove,
            IList<DateOnly> daysOffToAdd,
            IGroupPerson groupPerson,
            IDayOffPlannerSessionRuleSet ruleSet);
    }

    public class GroupMatrixHelper : IGroupMatrixHelper
    {
        private readonly IGroupMatrixContainerCreator _groupMatrixContainerCreator;

        public GroupMatrixHelper(IGroupMatrixContainerCreator groupMatrixContainerCreator)
        {
            _groupMatrixContainerCreator = groupMatrixContainerCreator;
        }

        /// <summary>
        /// Creates the group matrix containers.
        /// </summary>
        /// <param name="allMatrixes">All matrixes.</param>
        /// <param name="daysOffToRemove">The days off to remove.</param>
        /// <param name="daysOffToAdd">The days off to add.</param>
        /// <param name="groupPerson">The group person.</param>
        /// <param name="ruleSet">The rule set.</param>
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
            IDayOffPlannerSessionRuleSet ruleSet
            )
        {
            IList<GroupMatrixContainer> containers = new List<GroupMatrixContainer>();
            foreach (var groupMember in groupPerson.GroupMembers)
            {
                IScheduleMatrixPro scheduleMatrix = findGroupMatrix(allMatrixes, groupMember, daysOffToRemove[0]);
                if (scheduleMatrix == null)
                    return null;

                GroupMatrixContainer matrixContainer =
                    _groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, scheduleMatrix, ruleSet);
                if (matrixContainer == null)
                    return null;
                containers.Add(matrixContainer);
            }
            return containers;
        }

        public IGroupMatrixContainerCreator GroupMatrixContainerCreator
        {
            get { return _groupMatrixContainerCreator; }
        }

        public bool ValidateDayOffMoves(IList<GroupMatrixContainer> containers, IList<IDayOffLegalStateValidator> validatorList)
        {
            return containers.All(matrixContainer => validateMatrixContainer(matrixContainer, validatorList));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool ExecuteDayOffMoves(IList<GroupMatrixContainer> containers, IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            foreach (var matrixContainer in containers)
            {
                if (!dayOffDecisionMakerExecuter.Execute(matrixContainer.WorkingArray, matrixContainer.OriginalArray, matrixContainer.Matrix, null, false, false))
                {
                    schedulePartModifyAndRollbackService.Rollback();
                    return false;
                }
            }
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool ScheduleRemovedDayOffDays(IList<DateOnly> daysOffToRemove, IGroupPerson groupPerson, IGroupSchedulingService groupSchedulingService, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            foreach (var dateOnly in daysOffToRemove)
            {
                if (!groupSchedulingService.ScheduleOneDay(dateOnly, groupPerson))
                {
                    schedulePartModifyAndRollbackService.Rollback();
                    return false;
                }
            }
            return true;
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

        private static bool validateMatrixContainer(GroupMatrixContainer matrixContainer, IList<IDayOffLegalStateValidator> validatorList)
        {
            ILockableBitArray clone = (LockableBitArray)matrixContainer.WorkingArray.Clone();
            BitArray longBitArray = clone.ToLongBitArray();
            int offset = 0;
            if (clone.PeriodArea.Minimum < 7)
                offset = 7;
            for (int i = clone.PeriodArea.Minimum; i <= clone.PeriodArea.Maximum; i++)
            {
                if (longBitArray[i + offset])
                {
                    if (!validateDayOffIndex(longBitArray, i + offset, validatorList))
                        return false;
                }
            }
            return true;
        }

        private static bool validateDayOffIndex(BitArray daysOffArray, int index, IEnumerable<IDayOffLegalStateValidator> validatorList)
        {
            return validatorList.All(validator => validator.IsValid(daysOffArray, index));
        }

    }
}