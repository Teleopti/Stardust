using System.Collections.Generic;
using System.Linq;
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
        private readonly DayOffPlannerSessionRuleSet _ruleSet;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IList<IPerson> _allSelectedPersons;
        private readonly IList<IScheduleMatrixPro> _allMatrixes;
        private readonly IGroupDayOffOptimizerCreator _groupDayOffOptimizerCreator;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5")]
        public GroupDayOffOptimizerContainer(IScheduleMatrixLockableBitArrayConverter converter,
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            DayOffPlannerSessionRuleSet ruleSet,
            IScheduleMatrixPro matrix,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IList<IDayOffLegalStateValidator> validatorList,
            IList<IPerson> allSelectedPersons,
            IList<IScheduleMatrixPro> allMatrixes,
            IGroupDayOffOptimizerCreator groupDayOffOptimizerCreator)
        {
            _converter = converter;
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _ruleSet = ruleSet;
            _matrix = matrix;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _validatorList = validatorList;
            _allSelectedPersons = allSelectedPersons;
            _allMatrixes = allMatrixes;
            _groupDayOffOptimizerCreator = groupDayOffOptimizerCreator;
        }

        public bool Execute()
        {
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {
                return _decisionMakers.Any(decisionMaker => runDecisionMaker(decisionMaker));
            }
        }

        private bool runDecisionMaker(IDayOffDecisionMaker decisionMaker)
        {
            var dayOffOptimizer = _groupDayOffOptimizerCreator.CreateDayOffOptimizer(_converter, decisionMaker, _dayOffDecisionMakerExecuter ,_ruleSet,_validatorList,_allSelectedPersons);

            bool dayOffOptimizerResult = dayOffOptimizer.Execute(_matrix, _allMatrixes);
            if (dayOffOptimizerResult)
            {
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
