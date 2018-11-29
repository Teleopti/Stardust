using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class TeamBlockDayOffFairnessOptimizationServiceFacadeTest
    {
        private MockRepository _mock;
        private TeamBlockDayOffFairnessOptimizationServiceFacade _target;
        private IDayOffStep1 _dayOffStep1;
        private IDayOffStep2 _dayOffStep2;
        private ITeamBlockSchedulingOptions _teamBlockSchedulingOption;
        private IList<IScheduleMatrixPro> _allPersonMatrixList;
        private DateOnlyPeriod _selectedPeriod;
        private IList<IPerson> _selectedPerson;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IScheduleDictionary _scheduleDictionary;
        private IDictionary<DayOfWeek, int> _weekDayPoints;
        private IOptimizationPreferences _optimizationPreferences;
	    private ISeniorityWorkDayRanks _seniorityWorkDayRanks;
	    private IDaysOffPreferences _daysOffPreferences;
	    private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dayOffStep1 = _mock.StrictMock<IDayOffStep1>();
            _dayOffStep2 = _mock.StrictMock<IDayOffStep2>();
            _teamBlockSchedulingOption = new TeamBlockSchedulingOptions();
            _allPersonMatrixList = new List<IScheduleMatrixPro>();
            _selectedPeriod = new DateOnlyPeriod(2014,02,11,2014,02,12);
            _selectedPerson = new List<IPerson>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            _weekDayPoints = new Dictionary<DayOfWeek, int>();
            _optimizationPreferences = _mock.StrictMock<IOptimizationPreferences>();
	        _target = new TeamBlockDayOffFairnessOptimizationServiceFacade(_dayOffStep1,_dayOffStep2,_teamBlockSchedulingOption);
			_seniorityWorkDayRanks = new SeniorityWorkDayRanks();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
        }

        [Test]
        public void ShouldRunStep1()
        {
	        var schedulingOptions = new SchedulingOptions
	        {
		        UseBlock = false,
		        UseSameDayOffs = true,
		        UseTeam = true
	        };
	        using (_mock.Record())
            {
                Expect.Call(
                    () =>
                    _dayOffStep1.PerformStep1(_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
                      .IgnoreArguments();
                Expect.Call(() => _dayOffStep1.BlockSwapped += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep1.BlockSwapped -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {

				_target.Execute(_allPersonMatrixList, _selectedPeriod, _selectedPerson, schedulingOptions, _scheduleDictionary, _rollbackService, 
							_optimizationPreferences, _seniorityWorkDayRanks, _dayOffOptimizationPreferenceProvider);
            }
        }
        [Test]
        public void ShouldRunStep1AndStep2()
        {
	        var schedulingOptions = new SchedulingOptions
	        {
		        UseBlock = false,
		        UseSameDayOffs = false,
		        UseTeam = true
	        };
	        using (_mock.Record())
            {
                Expect.Call(
                    () =>
                    _dayOffStep1.PerformStep1(_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
                      .IgnoreArguments();

                Expect.Call(
                    () =>
                    _dayOffStep2.PerformStep2(schedulingOptions,_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences, _dayOffOptimizationPreferenceProvider))
                      .IgnoreArguments();
                Expect.Call(() => _dayOffStep1 .BlockSwapped  += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep1 .BlockSwapped  -= null).IgnoreArguments();
                Expect.Call(() => _dayOffStep2 .BlockSwapped  += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep2 .BlockSwapped  -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {

				_target.Execute(_allPersonMatrixList, _selectedPeriod, _selectedPerson, schedulingOptions, _scheduleDictionary, _rollbackService, 
					_optimizationPreferences, _seniorityWorkDayRanks, _dayOffOptimizationPreferenceProvider);
            }
        }
    }
}
