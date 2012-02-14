using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizer : IDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private readonly DayOffPlannerSessionRuleSet _ruleSet;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private ILockableBitArray _workingBitArray;


        public DayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IDayOffDecisionMaker decisionMaker,
            IScheduleResultDataExtractor scheduleResultDataExtractor,
            DayOffPlannerSessionRuleSet ruleSet,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter)
        {
            _converter = converter;
            _decisionMaker = decisionMaker;
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
            _ruleSet = ruleSet;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        public bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            writeToLogDayOffOptimizationInProgressOnCurrentAgent(matrix);

            ILockableBitArray originalArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            _workingBitArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            bool decisionMakerFoundDays = _decisionMaker.Execute(_workingBitArray, _scheduleResultDataExtractor.Values());
            if (!decisionMakerFoundDays)
                return false;
            bool decisionMakerChoiceResultedInBetterPeriod =
                _dayOffDecisionMakerExecuter.Execute(_workingBitArray, originalArray, matrix, originalStateContainer, true, true);
            if (!decisionMakerChoiceResultedInBetterPeriod)
                return false;
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
