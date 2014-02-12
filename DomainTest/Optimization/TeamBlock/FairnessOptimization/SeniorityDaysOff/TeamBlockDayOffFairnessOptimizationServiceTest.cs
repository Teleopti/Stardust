﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class TeamBlockDayOffFairnessOptimizationServiceTest
    {
        private MockRepository _mock;
        private ITeamBlockDayOffFairnessOptimizationService _target;
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
            _target = new TeamBlockDayOffFairnessOptimizationService(_dayOffStep1,_dayOffStep2,_teamBlockSchedulingOption);
        }

        [Test]
        public void ShouldRunStep1()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockPerOption = false;
            schedulingOptions.UseSameDayOffs = true;
            schedulingOptions.UseGroupScheduling = true ;
            using (_mock.Record())
            {
                Expect.Call(
                    () =>
                    _dayOffStep1.PerformStep1(_allPersonMatrixList, _selectedPeriod, _selectedPerson, _rollbackService,
                                              _scheduleDictionary, _weekDayPoints, _optimizationPreferences))
                      .IgnoreArguments();
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
            schedulingOptions.UseTeamBlockPerOption = false;
            schedulingOptions.UseSameDayOffs = false;
            schedulingOptions.UseGroupScheduling = true ;
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
            }
            using (_mock.Playback())
            {

                _target.Execute(_allPersonMatrixList, _selectedPeriod, _selectedPerson, schedulingOptions, _scheduleDictionary, _rollbackService, _optimizationPreferences);
            }
        }
    }
}
