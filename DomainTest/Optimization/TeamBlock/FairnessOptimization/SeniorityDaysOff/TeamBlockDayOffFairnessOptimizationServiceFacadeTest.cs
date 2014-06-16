using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class TeamBlockDayOffFairnessOptimizationServiceFacadeTest
    {
        private MockRepository _mock;
        private ITeamBlockDayOffFairnessOptimizationServiceFacade _target;
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
	        var toggleManager = MockRepository.GenerateMock<IToggleManager>();
	        toggleManager.Stub(x => x.IsEnabled(Toggles.Scheduler_Seniority_11111)).Return(true);
	        _target = new TeamBlockDayOffFairnessOptimizationServiceFacade(_dayOffStep1,_dayOffStep2,_teamBlockSchedulingOption, toggleManager);
        }

        [Test]
        public void ShouldRunStep1()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseBlock  = false;
            schedulingOptions.UseSameDayOffs = true;
				schedulingOptions.UseTeam = true;
            using (_mock.Record())
            {
                Expect.Call(
                    () =>
                    _dayOffStep1.PerformStep1(_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences))
                      .IgnoreArguments();
                Expect.Call(() => _dayOffStep1.BlockSwapped += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep1.BlockSwapped -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                
                _target.Execute(_allPersonMatrixList,_selectedPeriod,_selectedPerson,schedulingOptions,_scheduleDictionary,_rollbackService,_optimizationPreferences );
            }
        }
        [Test]
        public void ShouldRunStep1AndStep2()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseBlock  = false;
            schedulingOptions.UseSameDayOffs = false;
            schedulingOptions.UseTeam = true ;
            using (_mock.Record())
            {
                Expect.Call(
                    () =>
                    _dayOffStep1.PerformStep1(_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences))
                      .IgnoreArguments();

                Expect.Call(
                    () =>
                    _dayOffStep2.PerformStep2(schedulingOptions,_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences))
                      .IgnoreArguments();
                Expect.Call(() => _dayOffStep1 .BlockSwapped  += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep1 .BlockSwapped  -= null).IgnoreArguments();
                Expect.Call(() => _dayOffStep2 .BlockSwapped  += null).IgnoreArguments();
                Expect.Call(() => _dayOffStep2 .BlockSwapped  -= null).IgnoreArguments();
            }
            using (_mock.Playback())
            {

                _target.Execute(_allPersonMatrixList, _selectedPeriod, _selectedPerson, schedulingOptions, _scheduleDictionary, _rollbackService, _optimizationPreferences);
            }
        }
    }
}
