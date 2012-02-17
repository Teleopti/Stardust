﻿using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizer
    {
        bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer, IDayOffDecisionMaker decisionMaker);
    }

    public class BlockDayOffOptimizer : IBlockDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private readonly IDayOffPlannerSessionRuleSet _ruleSet;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly IBlockSchedulingService _blockSchedulingService;
        private readonly IBlockOptimizerBlockCleaner _blockOptimizerBlockCleaner;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private ILockableBitArray _workingBitArray;


        public BlockDayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IScheduleResultDataExtractor scheduleResultDataExtractor,
            IDayOffPlannerSessionRuleSet ruleSet,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IBlockSchedulingService blockSchedulingService,
            IBlockOptimizerBlockCleaner blockOptimizerBlockCleaner,
            ILockableBitArrayChangesTracker changesTracker,
            IResourceOptimizationHelper resourceOptimizationHelper)
        {
            _converter = converter;
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
            _ruleSet = ruleSet;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _blockSchedulingService = blockSchedulingService;
            _blockOptimizerBlockCleaner = blockOptimizerBlockCleaner;
            _changesTracker = changesTracker;
            _resourceOptimizationHelper = resourceOptimizationHelper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        public bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer, IDayOffDecisionMaker decisionMaker)
        {
            writeToLogDayOffOptimizationInProgressOnCurrentAgent(matrix);

            ILockableBitArray originalArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            _workingBitArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            if (!decisionMaker.Execute(_workingBitArray, _scheduleResultDataExtractor.Values()))
                return false;

            if (!_dayOffDecisionMakerExecuter.Execute(_workingBitArray, originalArray, matrix, originalStateContainer,
                                                      false, false))
                return false;

            
            if (!_blockSchedulingService.Execute(new List<IScheduleMatrixPro> {matrix}))
            {
                //rensa block med hål i
                IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(_workingBitArray, originalArray, matrix,
                                                                             _ruleSet.ConsiderWeekBefore);
                var datesRemoved = _blockOptimizerBlockCleaner.ClearSchedules(matrix, daysOffToRemove);
                foreach (var dateOnly in datesRemoved)
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                }

                if (!_blockSchedulingService.Execute(new List<IScheduleMatrixPro> { matrix }))
                    return false;
            }
            
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogDayOffOptimizationInProgressOnCurrentAgent(IScheduleMatrixPro matrix)
        {
            ILogWriter logWriter = new LogWriter<DayOffOptimizer>();
            string agent = matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);
            logWriter.LogInfo("Day off optimization for " + agent);
        }
    }
}
