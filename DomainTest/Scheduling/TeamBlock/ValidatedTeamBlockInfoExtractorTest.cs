﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class ValidatedTeamBlockInfoExtractorTest
    {
        private MockRepository _mocks;
        private IValidatedTeamBlockInfoExtractor _target;
        private ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
        private ISchedulingOptions _schedulingOptions;
        private ITeamBlockInfoFactory _teamBlockInfoFactory;

        private TeamInfo _teamInfo;
        private DateOnly _date;
        private IPerson _person;
        private List<IScheduleMatrixPro> _matrixList;
        private DateOnlyPeriod _dateOnlyPeriod;
        private ITeamSteadyStateHolder _teamSteadyStateHolder;
        private IGroupPerson _groupPerson;
        private BlockInfo _blockInfo;
        private TeamBlockInfo _teamBlockInfo;
        private IScheduleMatrixPro _matrixPro;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleRange _scheduleRange;

        [SetUp ]
        public void Setup()
        {
            _mocks = new MockRepository();
            _teamBlockSteadyStateValidator = _mocks.StrictMock<ITeamBlockSteadyStateValidator>();
            _teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
            _schedulingOptions = new SchedulingOptions();
            _teamSteadyStateHolder = _mocks.StrictMock<ITeamSteadyStateHolder>();
            _target = new ValidatedTeamBlockInfoExtractor(_teamBlockSteadyStateValidator,_teamBlockInfoFactory,_teamSteadyStateHolder);

            _date = new DateOnly(2013, 02, 22);
            _person = PersonFactory.CreatePerson();
            _matrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro> { _matrixPro };
            _dateOnlyPeriod = new DateOnlyPeriod(_date, _date);
            _blockInfo = new BlockInfo(_dateOnlyPeriod);
            _groupPerson = _mocks.StrictMock<IGroupPerson>();
            _teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            _teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
        }

        [Test] 
        public void ReturnNullWhenTeamBlockIsNull()
        {
            Assert.IsNull(_target.GetTeamBlockInfo(null,new DateOnly(),new List<IScheduleMatrixPro>(),null));
        }

        [Test]
        public void ReturnNullWhenSchedulingOptionsIsNull()
        {
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, new DateOnly(), new List<IScheduleMatrixPro>(), null));
        }

        [Test]
        public void ReturnNullIfGroupPersonNotInSteadyState()
        {
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo,new DateOnly(),new List<IScheduleMatrixPro>(),_schedulingOptions  ));
        }

        [Test]
        public void ReturnNullIfNewTeamBlockInfoIsNullWithSingleAgentTeam()
        {
            IGroupPageLight groupPageLight = new GroupPageLight { Key = "SingleAgentTeam", Name =  "SingleAgentTeam"};
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPageLight ;
            _schedulingOptions.UseTeamBlockPerOption = true;
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(),
                                                                      BlockFinderType.BetweenDayOff, true,
                                                                      new List<IScheduleMatrixPro>())).IgnoreArguments().Return(null);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, new DateOnly(), new List<IScheduleMatrixPro>(), _schedulingOptions));
        }

        [Test]
        public void ReturnNullIfNewTeamBlockInfoIsNullWith()
        {
            IGroupPageLight groupPageLight = new GroupPageLight { Key = "ABC", Name = "ABC" };
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPageLight;
            _schedulingOptions.UseTeamBlockPerOption = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson )).Return(true) ;
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(),
                                                                      BlockFinderType.SingleDay, false,
                                                                      new List<IScheduleMatrixPro>())).IgnoreArguments().Return(null);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, new DateOnly(), new List<IScheduleMatrixPro>(), _schedulingOptions));
        }

        [Test]
        public void ReturnNullIfDayIsScheduledInTeamBlock()
        {
            IGroupPageLight groupPageLight = new GroupPageLight { Key = "ABC", Name = "ABC" };
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPageLight;
            _schedulingOptions.UseTeamBlockPerOption = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
                                                                      BlockFinderType.SingleDay, false,
                                                                      new List<IScheduleMatrixPro>())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions));
        }


        [Test]
        public void ReturnNullIfBlockNotInSteadyState()
        {
            IGroupPageLight groupPageLight = new GroupPageLight { Key = "ABC", Name = "ABC" };
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPageLight;
            _schedulingOptions.UseTeamBlockPerOption = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
                                                                      BlockFinderType.SingleDay, false,
                                                                      new List<IScheduleMatrixPro>())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);

                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).IgnoreArguments() 
                      .Return(false);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions));
        }

        [Test]
        public void ReturnValidTeamBlockInfo()
        {
            IGroupPageLight groupPageLight = new GroupPageLight { Key = "ABC", Name = "ABC" };
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = groupPageLight;
            _schedulingOptions.UseTeamBlockPerOption = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
                                                                      BlockFinderType.SingleDay, false,
                                                                      new List<IScheduleMatrixPro>())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);

                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).IgnoreArguments()
                      .Return(true);
            }
            Assert.AreEqual( _target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions),_teamBlockInfo );
        }
    }
}
