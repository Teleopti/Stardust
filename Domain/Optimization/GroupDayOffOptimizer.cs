using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizer
    {
        bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes);
    }

    public class GroupDayOffOptimizer : IGroupDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IDayOffPlannerSessionRuleSet _ruleSet;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IList<IPerson> _allSelectedPersons;
        private readonly IGroupPersonPreOptimizationChecker _groupPersonPreOptimizationChecker;
        private readonly IGroupMatrixHelper _groupMatrixHelper;


        public GroupDayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IDayOffDecisionMaker decisionMaker,
            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
            IDayOffPlannerSessionRuleSet ruleSet,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            ILockableBitArrayChangesTracker changesTracker,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            IGroupSchedulingService groupSchedulingService,
            IList<IDayOffLegalStateValidator> validatorList,
            IList<IPerson> allSelectedPersons,
            IGroupPersonPreOptimizationChecker groupPersonPreOptimizationChecker, 
            IGroupMatrixHelper groupMatrixHelper)
        {
            _converter = converter;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _decisionMaker = decisionMaker;
            _ruleSet = ruleSet;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupSchedulingService = groupSchedulingService;
            _validatorList = validatorList;
            _allSelectedPersons = allSelectedPersons;
            _groupPersonPreOptimizationChecker = groupPersonPreOptimizationChecker;
            _groupMatrixHelper = groupMatrixHelper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes)
        {
            //_schedulePartModifyAndRollbackService.ClearModificationCollection();

            ILockableBitArray originalArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);
            ILockableBitArray workingBitArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            IScheduleResultDataExtractor scheduleResultDataExtractor = _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);
            bool decisionMakerFoundDays = _decisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values());
            if (!decisionMakerFoundDays)
                return false;

            IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(workingBitArray, originalArray, matrix, _ruleSet.ConsiderWeekBefore);
            IList<DateOnly> daysOffToAdd = _changesTracker.DaysOffAdded(workingBitArray, originalArray, matrix, _ruleSet.ConsiderWeekBefore);

            IGroupPerson groupPerson = _groupPersonPreOptimizationChecker.CheckPersonOnDates(allMatrixes, matrix.Person, daysOffToRemove, daysOffToAdd, _allSelectedPersons);
            if (groupPerson == null)
                return false;

            IList<GroupMatrixContainer> containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, groupPerson, _ruleSet);
            if (containers == null || containers.Count() == 0)
                return false;

            if (!_groupMatrixHelper.ValidateDayOffMoves(containers, _validatorList))
                return false;

            if (!_groupMatrixHelper.ExecuteDayOffMoves(containers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService))
                return false;

            if (!_groupMatrixHelper.ScheduleRemovedDayOffDays(daysOffToRemove, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService))
                return false;

            return true;
        }


    }

    public class GroupMatrixContainer
    {
        public IScheduleMatrixPro Matrix { get; set; }
        public ILockableBitArray OriginalArray { get; set; }
        public ILockableBitArray WorkingArray { get; set; }
    }
}
