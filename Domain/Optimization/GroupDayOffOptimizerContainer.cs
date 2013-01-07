using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizerContainer
    {
        bool Execute();
        IPerson Owner { get; }
    	IScheduleMatrixPro Matrix { get; }
        ILockableBitArray WorkingBitArray { get; }
    }

    public class GroupDayOffOptimizerContainer : IGroupDayOffOptimizerContainer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IList<IDayOffDecisionMaker> _decisionMakers;
        private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly IList<IScheduleMatrixPro> _allMatrixes;
        private readonly IGroupDayOffOptimizerCreator _groupDayOffOptimizerCreator;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
    	private readonly ITeamSteadyStateHolder _teamSteadyStateHolder;
    	private readonly IScheduleDictionary _scheduleDictionary;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5")]
        public GroupDayOffOptimizerContainer(IScheduleMatrixLockableBitArrayConverter converter,
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            IOptimizationPreferences optimizationPreferences,
            IScheduleMatrixPro matrix,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IList<IScheduleMatrixPro> allMatrixes,
            IGroupDayOffOptimizerCreator groupDayOffOptimizerCreator, 
            ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
			ITeamSteadyStateHolder teamSteadyStateHolder,
			IScheduleDictionary scheduleDictionary)
        {
            _converter = converter;
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _optimizationPreferences = optimizationPreferences;
            _matrix = matrix;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _allMatrixes = allMatrixes;
            _groupDayOffOptimizerCreator = groupDayOffOptimizerCreator;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_teamSteadyStateMainShiftScheduler = teamSteadyStateMainShiftScheduler;
        	_teamSteadyStateHolder = teamSteadyStateHolder;
        	_scheduleDictionary = scheduleDictionary;
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
                _groupDayOffOptimizerCreator.CreateDayOffOptimizer(_converter, decisionMaker, _dayOffDecisionMakerExecuter , daysOffPreferences);

			bool dayOffOptimizerResult = dayOffOptimizer.Execute(_matrix, _allMatrixes, schedulingOptions, _optimizationPreferences, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            if (dayOffOptimizerResult)
                WorkingBitArray = dayOffOptimizer.WorkingBitArray;
            return dayOffOptimizerResult;
        }

        public IPerson Owner
        {
            get { return _matrix.Person; }
        }

    	public IScheduleMatrixPro Matrix
    	{
			get { return _matrix; }
    	}

        public ILockableBitArray WorkingBitArray { get; private set; }
    }
}
