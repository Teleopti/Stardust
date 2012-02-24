using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class GroupDayOffSingleOptimizer : IGroupDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IList<IPerson> _allSelectedPersons;
        private readonly IGroupPersonsBuilder _groupPersonsBuilder;
        private readonly IGroupMatrixHelper _groupMatrixHelper;
        
        public GroupDayOffSingleOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IDayOffDecisionMaker decisionMaker,
            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
            IDaysOffPreferences daysOffPreferences,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            ILockableBitArrayChangesTracker changesTracker,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            IGroupSchedulingService groupSchedulingService,
            IList<IDayOffLegalStateValidator> validatorList,
            IList<IPerson> allSelectedPersons,
            IGroupPersonsBuilder groupPersonsBuilder,
             IGroupMatrixHelper groupMatrixHelper)
        {
            _converter = converter;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _decisionMaker = decisionMaker;
            _daysOffPreferences = daysOffPreferences;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupSchedulingService = groupSchedulingService;
            _validatorList = validatorList;
            _allSelectedPersons = allSelectedPersons;
            _groupPersonsBuilder = groupPersonsBuilder;
            _groupMatrixHelper = groupMatrixHelper;
        }

        public bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes)
        {
            if (matrix == null || allMatrixes == null)
                return false;

            ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
            var scheduleResultDataExtractor = _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);

            var workingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);

            bool decisionMakerFoundDays = _decisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values());
            if (!decisionMakerFoundDays)
                return false;

            IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(workingBitArray, originalArray,
                                                                             matrix, _daysOffPreferences.ConsiderWeekBefore);
            IList<DateOnly> daysOffToAdd = _changesTracker.DaysOffAdded(workingBitArray, originalArray,
                                                                             matrix, _daysOffPreferences.ConsiderWeekBefore);

           var containers = new List<GroupMatrixContainer>();
           
            GroupMatrixContainer matrixContainer =
                    _groupMatrixHelper.GroupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, matrix, _daysOffPreferences);
            if (matrixContainer == null)
                return false;
            containers.Add(matrixContainer);

            foreach (var dateOnly in daysOffToRemove)
            {
                var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, _allSelectedPersons, true);
                var groupPerson = findGroupPersonForPerson(matrix.Person, groupPersons);
                if (groupPerson == null)
                    return false;

                if (!_groupMatrixHelper.ValidateDayOffMoves(containers, _validatorList))
                    return false;

                if (!_groupMatrixHelper.ExecuteDayOffMoves(containers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService))
                    return false;

                if (!_groupSchedulingService.ScheduleOneDay(dateOnly, groupPerson))
                {
                    return false;
                }
            }

            return true;
        }

        private static IGroupPerson findGroupPersonForPerson(IPerson person, IEnumerable<IGroupPerson> groupPersons)
        {
            if (groupPersons == null || groupPersons.Count() == 0) return null;

            return groupPersons.FirstOrDefault(groupPerson => groupPerson.GroupMembers.Contains(person));
        }

    }
}
