using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizer
    {
		bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer, IDayOffDecisionMaker decisionMaker, ISchedulingOptions schedulingOptions);
        ILockableBitArray WorkingBitArray { get; }
    }

    public class BlockDayOffOptimizer : IBlockDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly IBlockSchedulingService _blockSchedulingService;
        private readonly IBlockOptimizerBlockCleaner _blockOptimizerBlockCleaner;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;


        public BlockDayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IScheduleResultDataExtractor scheduleResultDataExtractor,
            IDaysOffPreferences daysOffPreferences,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IBlockSchedulingService blockSchedulingService,
            IBlockOptimizerBlockCleaner blockOptimizerBlockCleaner,
            ILockableBitArrayChangesTracker changesTracker,
            IResourceOptimizationHelper resourceOptimizationHelper)
        {
            _converter = converter;
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
            _daysOffPreferences = daysOffPreferences;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _blockSchedulingService = blockSchedulingService;
            _blockOptimizerBlockCleaner = blockOptimizerBlockCleaner;
            _changesTracker = changesTracker;
            _resourceOptimizationHelper = resourceOptimizationHelper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
		public bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer, IDayOffDecisionMaker decisionMaker, ISchedulingOptions schedulingOptions)
        {
            writeToLogDayOffOptimizationInProgressOnCurrentAgent(matrix);

            ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);

            WorkingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);

            if (!decisionMaker.Execute(WorkingBitArray, _scheduleResultDataExtractor.Values()))
                return false;

            if (!_dayOffDecisionMakerExecuter.Execute(WorkingBitArray, originalArray, matrix, originalStateContainer,
                                                      false, false, true))
                return false;

            
            if (!_blockSchedulingService.Execute(new List<IScheduleMatrixPro> {matrix}, schedulingOptions))
            {
                //rensa block med hål i
                IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(WorkingBitArray, originalArray, matrix,
                                                                             _daysOffPreferences.ConsiderWeekBefore);
                var datesRemoved = _blockOptimizerBlockCleaner.ClearSchedules(matrix, daysOffToRemove, schedulingOptions);
                foreach (var dateOnly in datesRemoved)
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                }

                if (!_blockSchedulingService.Execute(new List<IScheduleMatrixPro> { matrix }, schedulingOptions))
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

        public ILockableBitArray WorkingBitArray { get; private set; }
    }
}
