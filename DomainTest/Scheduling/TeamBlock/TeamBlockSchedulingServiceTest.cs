using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
        private ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
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
	    private ISafeRollbackAndResourceCalculation _safeRollback;
	    private ISchedulePartModifyAndRollbackService _rollbackService;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleRange _scheduleRange;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
	    private ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;


	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
            _teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
            _teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
            _teamBlockSteadyStateValidator = _mock.StrictMock<ITeamBlockSteadyStateValidator>();
            _teamSteadyStateHolder = _mock.StrictMock<ITeamSteadyStateHolder>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _workShiftMinMaxCalculator = _mock.StrictMock<IWorkShiftMinMaxCalculator>();
            _scheduleRange = _mock.StrictMock<IScheduleRange>();

            _scheduleDay = _mock.StrictMock<IScheduleDay>();
	        _safeRollback = _mock.StrictMock<ISafeRollbackAndResourceCalculation>();
		    _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mock.StrictMock<ITeamBlockMaxSeatChecker>();
		    _target = new TeamBlockSchedulingService(_schedulingOptions, _teamInfoFactory, _teamBlockInfoFactory,
		                                             _teamBlockScheduler, _teamBlockSteadyStateValidator, _safeRollback,
		                                             _workShiftMinMaxCalculator, new List<IWorkShiftFinderResult>(),
		                                             _teamBlockMaxSeatChecker);
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
        public void ShouldNotContinueIfRollbackIsNull()
        {
           Assert.IsFalse( _target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, null));
        }

        [Test]
        public void ShouldContinueIfTeamNotInSteadyState()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() {_matrixList});
             using(_mock.Record())
             {
                 Expect.Call(() => _teamBlockScheduler .DayScheduled += null).IgnoreArguments();
                 Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod , _matrixList)).Return(teamInfo);
                 Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);
                 Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

             }

			 Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));
         
        }

        [Test]
        public void ShouldContinueIfTeamBlockInfoIsNull()
        {
            
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(_teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date, BlockFinderType.SingleDay  ,
																	 false, _matrixList)).Return(null);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                
            }

			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        
        [Test]
        public void ShouldContinueIfBlockNotInSteadyState()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.SingleDay,
																	 false, _matrixList)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false );
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(false);


                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

                expectCallForRollback();

            }

			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        [Test]
        public void ShouldContinueIfScheduleBlockDayIsSuccessful()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.SingleDay,
																	 false, _matrixList)).Return(_teamBlockInfo);
				Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).Return(true);
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                expectCallForRollback();
            }

			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        [Test]
        public void ShouldStillContinueIfScheduleBlockDayIsSuccessful()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.SingleDay ,
																	 false, _matrixList)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(false);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
	            Expect.Call(()=>_safeRollback.Execute(_rollbackService, _schedulingOptions));
                expectCallForRollback();
            }

			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        [Test]
        public void ShouldStillContinueIfScheduleBlockDayIsSuccessfulWithTeamBlock()
        {
            var schedulingOptions = _schedulingOptions;
            schedulingOptions.UseTeamBlockPerOption = true;
	        _target = new TeamBlockSchedulingService(schedulingOptions, _teamInfoFactory, _teamBlockInfoFactory,
	                                                 _teamBlockScheduler, _teamBlockSteadyStateValidator, _safeRollback,
	                                                 _workShiftMinMaxCalculator, new List<IWorkShiftFinderResult>(),
	                                                 _teamBlockMaxSeatChecker);

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, schedulingOptions.BlockFinderTypeForAdvanceScheduling,
                                                                     false, _matrixList)).Return(_teamBlockInfo);
                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(false);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                Expect.Call(() => _safeRollback.Execute(_rollbackService, schedulingOptions));
                expectCallForRollback();
            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        [Test]
        public void ShouldBreakIfCancelMeIsSet()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod , _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo,_date , BlockFinderType.SingleDay,
																	 false, _matrixList)).Return(_teamBlockInfo);
				Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true );
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).Return(true);
                expectCallForRollback();
            }
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;

            _target.RaiseEventForTest(this, args);

			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        private void expectCallForRollback()
        {
            Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
            Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
            Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();

            Expect.Call(() => _rollbackService.ClearModificationCollection());
            Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
            Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions))
                  .IgnoreArguments()
                  .Return(true);

            Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
        }

        [Test]
        public void ShouldBreakIfCancelMeIsSetWithNullDayScheduled()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                expectCallsForShouldBreakIfCancelMeIsSetWithNullDayScheduled(teamInfo);
                expectCallForRollback();
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).Return(true);
            }
            SchedulingServiceBaseEventArgs args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;
            _target.DayScheduled += TargetOnDayScheduled;
            _target.RaiseEventForTest(this, args);
            Assert.IsTrue(_cancelTarget);
			Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }

        private void expectCallsForShouldBreakIfCancelMeIsSetWithNullDayScheduled(TeamInfo teamInfo)
        {
            Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
            Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
            Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

            Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.SingleDay,
																  false, _matrixList)).Return(_teamBlockInfo);
			Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
            Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
            Expect.Call(_scheduleDay.IsScheduled()).Return(false);
            Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                  .Return(true);
            Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                 _dateOnlyPeriod, _personList)).Return(true);

            Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
        }

        private void TargetOnDayScheduled(object sender, SchedulingServiceBaseEventArgs schedulingServiceBaseEventArgs)
        {
            _cancelTarget = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExecuteRollbackIfNotOk()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);

                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, _date, BlockFinderType.SingleDay ,
																	 false, _matrixList)).Return(_teamBlockInfo);
				Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
                      .Return(true);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _date, _schedulingOptions,
                                                                     _dateOnlyPeriod, _personList)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).Return(true);
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();

                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions))
                      .IgnoreArguments()
                      .Return(false);
                Expect.Call(() => _safeRollback.Execute(_rollbackService, _schedulingOptions)).IgnoreArguments();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
            }

            Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _teamSteadyStateHolder, _rollbackService));

        }
    }
}
