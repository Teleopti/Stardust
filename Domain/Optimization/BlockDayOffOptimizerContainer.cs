using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizerContainer
    {
        bool Execute();
        IPerson Owner { get; }
    }

    public class BlockDayOffOptimizerContainer : IBlockDayOffOptimizerContainer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IList<IDayOffDecisionMaker> _decisionMakers;
        private readonly int _moveMaxDaysOff;
        private readonly DayOffPlannerSessionRuleSet _ruleSet;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IBlockDayOffOptimizer _blockDayOffOptimizer;
        private int _movedDaysOff;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public BlockDayOffOptimizerContainer(IScheduleMatrixLockableBitArrayConverter converter,
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            DayOffPlannerSessionRuleSet ruleSet,
            IScheduleMatrixPro matrix,
            IOptimizerOriginalPreferences optimizerPreferences,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IBlockDayOffOptimizer blockDayOffOptimizer)
        {
            _converter = converter;
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _ruleSet = ruleSet;
            _matrix = matrix;
            _originalStateContainer = originalStateContainer;
            _blockDayOffOptimizer = blockDayOffOptimizer;
            int dayOffCounter = 0;
            ILockableBitArray workingBitArray = _converter.Convert(_ruleSet.ConsiderWeekBefore, _ruleSet.ConsiderWeekAfter);

            for (int i = workingBitArray.PeriodArea.Minimum; i <= workingBitArray.PeriodArea.Maximum; i++)
            {
                if (workingBitArray[i])
                    dayOffCounter++;
            }

            _moveMaxDaysOff = -1;
            if (optimizerPreferences.AdvancedPreferences.MaximumMovableDayOffPercentagePerPerson != 1)
                _moveMaxDaysOff = (int)(dayOffCounter * optimizerPreferences.AdvancedPreferences.MaximumMovableDayOffPercentagePerPerson);

        }

        public bool Execute()
        {

            ILog log = LogManager.GetLogger(typeof(DayOffOptimizerContainer));
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {

                // note that we do not change the order of the decisionmakers
                if (_decisionMakers.Any(decisionMaker => runDecisionMaker(agent, log, decisionMaker)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool runDecisionMaker(string agent, ILog log, IDayOffDecisionMaker decisionMaker)
        {
            if (_moveMaxDaysOff > 0 && _movedDaysOff >= _moveMaxDaysOff)
            {
                log.Info(_moveMaxDaysOff + " have already been moved for " + agent);
                return false;
            }

            bool dayOffOptimizerResult = _blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, decisionMaker);
            if (dayOffOptimizerResult)
            {
                _movedDaysOff += 2;
                return true;
            }

            return false;
        }


        public IPerson Owner
        {
            get { return _matrix.Person; }
        }

    }
}
