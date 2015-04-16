using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using List = NHibernate.Mapping.List;

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
        private Group _group;
        private ITeamBlockInfo _teamBlockInfoMock;
	    private ISafeRollbackAndResourceCalculation _safeRollback;
	    private ISchedulePartModifyAndRollbackService _rollbackService;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
	    private ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
        private IValidatedTeamBlockInfoExtractor _validatedTeamBlockExtractor;
        private ITeamInfo _teamInfoMock;
        private IBlockInfo _blockInfoMock;
        private bool _cancelTarget;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private ITeamMatrixChecker _teamMatrixChecker;


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
			_person = PersonFactory.CreatePerson();
			_personList = new List<IPerson> { _person };
			_group = new Group(_personList, "");
            _workShiftMinMaxCalculator = _mock.StrictMock<IWorkShiftMinMaxCalculator>();
            _blockInfoMock = _mock.StrictMock<IBlockInfo>();
	        _safeRollback = _mock.StrictMock<ISafeRollbackAndResourceCalculation>();
		    _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamBlockMaxSeatChecker = _mock.StrictMock<ITeamBlockMaxSeatChecker>();
            _validatedTeamBlockExtractor = _mock.StrictMock<IValidatedTeamBlockInfoExtractor>();
		    _teamMatrixChecker = _mock.StrictMock<ITeamMatrixChecker>();
		    _target = new TeamBlockSchedulingService(_schedulingOptions, _teamInfoFactory,
		                                             _teamBlockScheduler, _safeRollback,
		                                             _workShiftMinMaxCalculator,
		                                             _teamBlockMaxSeatChecker,
													 _validatedTeamBlockExtractor,
													 _teamMatrixChecker);
            _date = new DateOnly(2013, 02, 22);
            
            _matrixList = new List<IScheduleMatrixPro> {_matrixPro};
            
            _dateOnlyPeriod = new DateOnlyPeriod(_date,_date);
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
	        _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
        }

		[Test]
		public void ShouldNotContinueIfRollbackIsNull()
		{
			_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, null, _resourceCalculateDelayer,
				_schedulingResultStateHolder);
		}

		[Test]
		public void ShouldContinueIfTeamBlockNotValidated()
		{
			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo>{_teamInfoMock}, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		 _schedulingOptions, _dateOnlyPeriod))
					 .IgnoreArguments()
					 .Return(null);
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}

		}



		[Test]
		public void ShouldNotVerifyIfScheduleWasNotSuccessfull()
		{

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { _teamInfoMock }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(false);
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldVerifyIfScheduleWasSuccessfullButPersonNotInSelectedPerson()
		{

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { _teamInfoMock }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
				Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(new Person());
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock);
				Expect.Call(_blockInfoMock.BlockPeriod).Return(new DateOnlyPeriod(_date, _date));
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldVerifyIfScheduleWasSuccessfullButPersonIsInSelectedPerson()
		{

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { _teamInfoMock }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
				Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(_person);
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock);
				Expect.Call(_blockInfoMock.BlockPeriod).Return(new DateOnlyPeriod(_date, _date));
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldRollbackWhenPeriodNotInLeagelState()
		{

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { _teamInfoMock }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
				Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(_person);
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(false);
				Expect.Call(() => _safeRollback.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock).Repeat.Twice();
				Expect.Call(_blockInfoMock.BlockPeriod).Return(_dateOnlyPeriod).Repeat.Twice();
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldRollbackWhenMaxSeatBroken()
		{

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).IgnoreArguments().Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { _teamInfoMock }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
				Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(_person);
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(() => _safeRollback.Execute(_rollbackService, _schedulingOptions));

				Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock).Repeat.Twice();
				Expect.Call(_blockInfoMock.BlockPeriod).Return(_dateOnlyPeriod).Repeat.Twice();

				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(false);

			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldBreakIfCancelMeIsSet()
		{
			using (_mock.Record())
			{
				_teamBlockScheduler.DayScheduled += null;
				IEventRaiser eventRaiser = LastCall.IgnoreArguments().GetEventRaiser();

				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
				Expect.Call(_teamInfoMock.GroupMembers).Return(new List<IPerson>());
				Expect.Call(_teamMatrixChecker.CheckTeamList(null, _dateOnlyPeriod))
					.IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo>(), new List<ITeamInfo>()));
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList))
					.Return(_teamInfoMock)
					.WhenCalled(_ =>
						eventRaiser.Raise(null, new SchedulingServiceSuccessfulEventArgs(null) {Cancel = true}));
			}
			
			using (_mock.Playback())
			{
				_target.DayScheduled += targetOnDayScheduled;
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
				Assert.IsTrue(_cancelTarget);
				_target.DayScheduled -= targetOnDayScheduled;
			}
		}

		private void targetOnDayScheduled(object sender, SchedulingServiceBaseEventArgs schedulingServiceBaseEventArgs)
		{
			_cancelTarget = true;
		}

		[Test]
		public void ShouldIgoreATeamIfSomeoneFailedButContinueWithOtherTeams()
		{
			var person2 = PersonFactory.CreatePerson("2");
			var group2 = new Group(new List<IPerson> { person2 }, "");
			var teamBlockInfoMock2 = _mock.StrictMock<ITeamBlockInfo>();
			var teamInfoMock2 = _mock.StrictMock<ITeamInfo>();

			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			var teamInfo2 = new TeamInfo(group2, new List<IList<IScheduleMatrixPro>> { _matrixList });
			var personList = new List<IPerson> { _person, person2 };
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _matrixList)).Return(teamInfo);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(person2, _dateOnlyPeriod, _matrixList)).Return(teamInfo2);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { teamInfo, teamInfo2 }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(_teamBlockInfoMock);
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfoMock2, _date, _matrixList,
																		  _schedulingOptions, _dateOnlyPeriod))
					  .IgnoreArguments()
					  .Return(teamBlockInfoMock2);
				Expect.Call(() => _rollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfoMock2, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, null)).IgnoreArguments().Return(true);
				Expect.Call(teamBlockInfoMock2.TeamInfo).Return(teamInfoMock2);
				Expect.Call(teamInfoMock2.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(new Person());
				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(_date, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(teamBlockInfoMock2.BlockInfo).Return(_blockInfoMock);
				Expect.Call(_blockInfoMock.BlockPeriod).Return(new DateOnlyPeriod(_date, _date));
			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

		[Test]
		public void ShouldRollbackWhenMaxSeatBrokenOnAnyDayOfBlock()
		{
			var datePeriod = new DateOnlyPeriod(new DateOnly(2014, 05, 21), new DateOnly(2014, 05, 25));
			var teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>> { _matrixList });
			using (_mock.Record())
			{
				Expect.Call(() => _teamBlockScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, datePeriod, _matrixList)).IgnoreArguments().Return(teamInfo);
				Expect.Call(_teamMatrixChecker.CheckTeamList(new HashSet<ITeamInfo>(), _dateOnlyPeriod)).IgnoreArguments()
					.Return(new TeamMatrixCheckerResult(new List<ITeamInfo> { teamInfo }, new List<ITeamInfo>()));
				Expect.Call(_validatedTeamBlockExtractor.GetTeamBlockInfo(_teamInfoMock, _date, _matrixList,
																	 _schedulingOptions, datePeriod))
						.IgnoreArguments()
						.Return(_teamBlockInfoMock);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfoMock, _date, _schedulingOptions,
					_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null))
					.IgnoreArguments()
					.Return(true);
				Expect.Call(_teamBlockInfoMock.TeamInfo).Return(_teamInfoMock);
				Expect.Call(_teamInfoMock.MatrixesForGroupAndDate(_date)).Return(_matrixList);
				Expect.Call(_matrixPro.Person).Return(_person);
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_matrixPro, _schedulingOptions)).IgnoreArguments().Return(true);
				Expect.Call(() => _safeRollback.Execute(_rollbackService, _schedulingOptions));

				Expect.Call(_teamBlockInfoMock.BlockInfo).Return(_blockInfoMock).Repeat.Twice();
				Expect.Call(_blockInfoMock.BlockPeriod).Return(datePeriod).Repeat.Twice();

				Expect.Call(() => _teamBlockScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(new DateOnly(2014, 05, 21), _schedulingOptions)).Return(true);
				Expect.Call(_teamBlockMaxSeatChecker.CheckMaxSeat(new DateOnly(2014, 05, 22), _schedulingOptions)).Return(false);

			}

			using (_mock.Playback())
			{
				_target.ScheduleSelected(_matrixList, _dateOnlyPeriod, _personList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			}
		}

    }
}
