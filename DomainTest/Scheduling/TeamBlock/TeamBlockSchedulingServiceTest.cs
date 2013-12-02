using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
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
        private DateOnly _date;
        private IPerson _person;
        private ISchedulingOptions _schedulingOptions;
        private ITeamInfoFactory _teamInfoFactory;
        private ITeamBlockScheduler _teamBlockScheduler;
        private List<IScheduleMatrixPro> _matrixList;
        private IList<IPerson> _personList;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IGroupPerson _groupPerson;
        private ITeamBlockInfo _teamBlockInfoMock;
	    private ISafeRollbackAndResourceCalculation _safeRollback;
	    private ISchedulePartModifyAndRollbackService _rollbackService;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
	    private ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
        private IValidatedTeamBlockInfoExtractor _validatedTeamBlockExtractor;
        private ITeamInfo _teamInfoMock;
        private IBlockInfo _blockInfoMock;
        private bool _cancelTarget;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _teamInfoMock = _mock.StrictMock<ITeamInfo>();
            _teamBlockInfoMock = _mock.StrictMock<ITeamBlockInfo >();
            _schedulingOptions = new SchedulingOptions();
            _teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
            _teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _workShiftMinMaxCalculator = _mock.StrictMock<IWorkShiftMinMaxCalculator>();
            _blockInfoMock = _mock.StrictMock<IBlockInfo>();
	        _safeRollback = _mock.StrictMock<ISafeRollbackAndResourceCalculation>();
		    _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mock.StrictMock<ITeamBlockMaxSeatChecker>();
            _validatedTeamBlockExtractor = _mock.StrictMock<IValidatedTeamBlockInfoExtractor>();
		    _target = new TeamBlockSchedulingService(_schedulingOptions, _teamInfoFactory,
		                                             _teamBlockScheduler, _safeRollback,
		                                             _workShiftMinMaxCalculator, new List<IWorkShiftFinderResult>(),
		                                             _teamBlockMaxSeatChecker,_validatedTeamBlockExtractor);
            _date = new DateOnly(2013, 02, 22);
            _person = PersonFactory.CreatePerson();
            _matrixList = new List<IScheduleMatrixPro> {_matrixPro};
            _personList = new List<IPerson> {_person};
            _dateOnlyPeriod = new DateOnlyPeriod(_date,_date);
        }

        [Test]
        public void ShouldNotContinueIfRollbackIsNull()
        {
           Assert.IsFalse( _target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, null));
        }

        [Test]
        public void ShouldContinueIfTeamBlockNotValidated()
        {
            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() {_matrixList});
             using(_mock.Record())
             {
                 Expect.Call(() => _teamBlockScheduler .DayScheduled += null).IgnoreArguments();
                 Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod , _matrixList)).Return(teamInfo);
                 Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                 Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(null);
             }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }

        }

        
       
        [Test]
        public void ShouldNotVerifyIfScheduleWasNotSuccessfull()
        {

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList }); 
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock , _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock  );
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).IgnoreArguments().Return(false);
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }
        }

        [Test]
        public void ShouldVerifyIfScheduleWasSuccessfullButPersonNotInSelectedPerson()
        {

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).IgnoreArguments().Return(true);
                Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
                Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
                Expect.Call(_matrixPro.Person).Return(new Person());
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments() .Return(true);
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList , _rollbackService));
            }
        }

        [Test]
        public void ShouldVerifyIfScheduleWasSuccessfullButPersonIsInSelectedPerson()
        {

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).IgnoreArguments().Return(true);
                Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
                Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro,_schedulingOptions )).IgnoreArguments().Return(true);
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                
                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }
        }

        [Test]
        public void ShouldRollbackWhenPeriodNotInLeagelState()
        {

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).IgnoreArguments().Return(true);
                Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
                Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(false );
                Expect.Call(()=>_safeRollback.Execute(_rollbackService, _schedulingOptions));
                Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock);
                Expect.Call(_blockInfoMock.BlockPeriod ).Return(_dateOnlyPeriod );
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }
        }

        [Test]
        public void ShouldRollbackWhenMaxSeatBroken()
        {

            var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });
            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).IgnoreArguments().Return(true);
                Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
                Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(true);
                Expect.Call(() => _safeRollback.Execute(_rollbackService, _schedulingOptions));

                Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock);
                Expect.Call(_blockInfoMock.BlockPeriod).Return(_dateOnlyPeriod);

                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(false);
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }
        }

        [Test]
        public void ShouldBreakIfCancelMeIsSet()
        {
            //var teamInfo = new TeamInfo(_groupPerson, new List<IList<IScheduleMatrixPro>>() { _matrixList });


            using (_mock.Record())
            {
                Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
                Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).IgnoreArguments().Return(_teamInfoMock);
                Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
                                                                          _schedulingOptions))
                      .IgnoreArguments()
                      .Return(_teamBlockInfoMock);
                Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
																	 _dateOnlyPeriod, _personList, _rollbackService)).Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
                Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
                Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
                Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
                Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);

            }
            var args = new SchedulingServiceBaseEventArgs(null);
            args.Cancel = true;
            _target.DayScheduled += targetOnDayScheduled;
            _target.RaiseEventForTest(this, args);
            using (_mock.Playback())
            {
                Assert.IsTrue(_cancelTarget);
                Assert.IsTrue(_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService));
            }
            

        }

        private void targetOnDayScheduled(object sender, SchedulingServiceBaseEventArgs schedulingServiceBaseEventArgs)
        {
            _cancelTarget = true;
        }


    }
}
