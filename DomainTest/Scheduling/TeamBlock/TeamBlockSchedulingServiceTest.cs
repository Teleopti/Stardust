using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockSchedulingServiceTest
    {
        private TeamBlockSchedulingService _target;
        private MockRepository _mock;
        private IScheduleMatrixPro _matrixPro;
        private TeamInfo _teamInfo;
        private DateOnly _date;
        private IPerson _person;
        private IScheduleDay _scheduleDay;
        private ISchedulingOptions _schedulingOptions;
        private ITeamInfoFactory _teamInfoFactory;
        private ITeamBlockInfoFactory _teamBlockInfoFactory;
        private ITeamBlockScheduler _teamBlockScheduler;
        private IBlockSteadyStateValidator _blockSteadyStateValidator;
        private List<IScheduleMatrixPro> _matrixList;
        private IList<IPerson> _personList;
        private DateOnlyPeriod _dateOnlyPeriod;
        private ITeamSteadyStateHolder _teamSteadyStateHolder;
        private IGroupPerson _groupPerson;
        private BlockInfo _blockInfo;
        private TeamBlockInfo _teamBlockInfo;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private IScheduleDayPro _scheduleDayPro;
        private bool _cancelTarget;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
            _teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
            _teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
            _blockSteadyStateValidator = _mock.StrictMock<IBlockSteadyStateValidator>();
            _teamSteadyStateHolder = _mock.StrictMock<ITeamSteadyStateHolder>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _target = new TeamBlockSchedulingService(_schedulingOptions, _teamInfoFactory, _teamBlockInfoFactory, _teamBlockScheduler, _blockSteadyStateValidator);
            _date = new DateOnly(2013, 02, 22);
            _person = PersonFactory.CreatePerson();
            _matrixList = new List<IScheduleMatrixPro> {_matrixPro};
            _personList = new List<IPerson> {_person};
            _dateOnlyPeriod = new DateOnlyPeriod(_date,_date);
            _blockInfo = new BlockInfo(_dateOnlyPeriod);
            _teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            _teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
        }

        [Test]
        public void ShouldContinueIfTeamNotInSteadyState()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() {_matrixList});
             using(_mock.Record())
             {
                 Expect.Call(() => _teamBlockScheduler .DayScheduled += null).IgnoreArguments();
                 Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                 Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
                 Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
             }

            Assert.IsTrue(_target.ScheduleSelected( _matrixList,_dateOnlyPeriod,_personList,_teamSteadyStateHolder));
         
        }

        [Test]
        public void ShouldContinueIfTeamBlockInfoIsNull()
        {
            
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(_teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date, BlockFinderType.None ,
                                                                     false)).Return(null);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                
            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldContinueWithoutDoingAnythingIfTeamBlockIsScheduled()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            
           
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
               
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldContinueIfBlockNotInSteadyState()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false );
                Expect.Call(_blockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(false);


                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldContinueIfScheduleBlockDayIsSuccessfull()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_blockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldStillContinueIfScheduleBlockDayIsSuccessfull()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_blockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(false);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldBreakIfCancelMeIsSet()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_blockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true );

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;

            _target.RaiseEventForTest(this, args);

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        [Test]
        public void ShouldBreakIfCancelMeIsSetWithNullDayScheduled()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _date, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.None,
                                                                     false)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_blockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

            }
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;
            _target.DayScheduled += TargetOnDayScheduled;
            _target.RaiseEventForTest(this, args);
            Assert.IsTrue(_cancelTarget);
            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder));

        }

        private void TargetOnDayScheduled(object sender, SchedulingServiceBaseEventArgs schedulingServiceBaseEventArgs)
        {
            _cancelTarget = true;
        }
    }
}
