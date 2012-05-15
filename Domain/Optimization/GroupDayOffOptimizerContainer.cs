using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using log4net;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizerContainer
    {
        bool Execute();
        IPerson Owner { get; }
    }

    public class GroupDayOffOptimizerContainer : IGroupDayOffOptimizerContainer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IList<IDayOffDecisionMaker> _decisionMakers;
        private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IList<IPerson> _allSelectedPersons;
        private readonly IList<IScheduleMatrixPro> _allMatrixes;
        private readonly IGroupDayOffOptimizerCreator _groupDayOffOptimizerCreator;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5")]
        public GroupDayOffOptimizerContainer(IScheduleMatrixLockableBitArrayConverter converter,
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            IOptimizationPreferences optimizationPreferences,
            IScheduleMatrixPro matrix,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IList<IDayOffLegalStateValidator> validatorList,
            IList<IPerson> allSelectedPersons,
            IList<IScheduleMatrixPro> allMatrixes,
            IGroupDayOffOptimizerCreator groupDayOffOptimizerCreator, 
            ISchedulingOptionsCreator schedulingOptionsCreator)
        {
            _converter = converter;
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _optimizationPreferences = optimizationPreferences;
            _matrix = matrix;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _validatorList = validatorList;
            _allSelectedPersons = allSelectedPersons;
            _allMatrixes = allMatrixes;
            _groupDayOffOptimizerCreator = groupDayOffOptimizerCreator;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        }

        public bool Execute()
        {
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {
                return _decisionMakers.Any(dayOffDecisionMaker => runDecisionMaker(dayOffDecisionMaker, schedulingOptions));
            }
        }

        private bool runDecisionMaker(IDayOffDecisionMaker decisionMaker, ISchedulingOptions schedulingOptions)
        {
            IDaysOffPreferences daysOffPreferences = _optimizationPreferences.DaysOff;

            var dayOffOptimizer =
                _groupDayOffOptimizerCreator.CreateDayOffOptimizer(_converter, decisionMaker, _dayOffDecisionMakerExecuter , daysOffPreferences,_validatorList,_allSelectedPersons);

            bool dayOffOptimizerResult = dayOffOptimizer.Execute(_matrix, _allMatrixes, schedulingOptions);
            return dayOffOptimizerResult;
        }

        public IPerson Owner
        {
            get { return _matrix.Person; }
        }

    }
}
