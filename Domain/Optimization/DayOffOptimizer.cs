﻿using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizer : IDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;


        public DayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IDayOffDecisionMaker decisionMaker,
            IScheduleResultDataExtractor scheduleResultDataExtractor,
            IDaysOffPreferences daysOffPreferences,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter)
        {
            _converter = converter;
            _decisionMaker = decisionMaker;
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
            _daysOffPreferences = daysOffPreferences;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        public bool Execute(IScheduleMatrixPro matrix, IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            writeToLogDayOffOptimizationInProgressOnCurrentAgent(matrix);

            ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);
            ILockableBitArray workingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);

            bool decisionMakerFoundDays = _decisionMaker.Execute(workingBitArray, _scheduleResultDataExtractor.Values());
            if (!decisionMakerFoundDays)
                return false;
            bool decisionMakerChoiceResultedInBetterPeriod =
                _dayOffDecisionMakerExecuter.Execute(workingBitArray, originalArray, matrix, originalStateContainer, true, true, true);
            return decisionMakerChoiceResultedInBetterPeriod;
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
