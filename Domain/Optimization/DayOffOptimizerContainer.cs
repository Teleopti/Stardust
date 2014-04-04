using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class DayOffOptimizerContainer : IDayOffOptimizerContainer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IList<IDayOffDecisionMaker> _decisionMakers;
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;


        public DayOffOptimizerContainer(IScheduleMatrixLockableBitArrayConverter converter,
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            IScheduleResultDataExtractor scheduleResultDataExtractor,
            IDaysOffPreferences daysOffPreferences,
            IScheduleMatrixPro matrix,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            _converter = converter;
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
            _daysOffPreferences = daysOffPreferences;
            _matrix = matrix;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _originalStateContainer = originalStateContainer;
        }

        public bool Execute()
        {
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {
                // note that we do not change the order of the decisionmakers
                if (_decisionMakers.Any(runDayOffOptimizer))
                    return true;
            }
            return false;
        }

        private bool runDayOffOptimizer(IDayOffDecisionMaker decisionMaker)
        {

            IDayOffOptimizer dayOffOptimizer =
                new DayOffOptimizer(_converter, 
                                    decisionMaker, 
                                    _scheduleResultDataExtractor,
                                    _daysOffPreferences,
                                    _dayOffDecisionMakerExecuter);

            bool dayOffOptimizerResult = dayOffOptimizer.Execute(_matrix, _originalStateContainer);
            return dayOffOptimizerResult;
        }


        public IPerson Owner
        {
            get { return _matrix.Person; }
        }
    }
}
