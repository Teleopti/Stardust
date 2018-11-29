using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class ValidatedTeamBlockInfoExtractorTest
    {
        private MockRepository _mocks;
        private ValidatedTeamBlockInfoExtractor _target;
        private ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
        private SchedulingOptions _schedulingOptions;
        private ITeamBlockInfoFactory _teamBlockInfoFactory;

        private TeamInfo _teamInfo;
        private DateOnly _date;
        private IPerson _person;
        private List<IScheduleMatrixPro> _matrixList;
        private DateOnlyPeriod _dateOnlyPeriod;
        private Group _group;
        private BlockInfo _blockInfo;
        private TeamBlockInfo _teamBlockInfo;
        private IScheduleMatrixPro _matrixPro;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleRange _scheduleRange;
   		private ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
	    private GroupPageLight _groupPageLight;

	    [SetUp ]
        public void Setup()
        {
            _mocks = new MockRepository();
            _teamBlockSteadyStateValidator = _mocks.StrictMock<ITeamBlockSteadyStateValidator>();
            _teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
            _schedulingOptions = new SchedulingOptions();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
            _target = new ValidatedTeamBlockInfoExtractor(_teamBlockSteadyStateValidator,_teamBlockInfoFactory,_teamBlockSchedulingCompletionChecker);

            _date = new DateOnly(2013, 02, 22);
            _person = PersonFactory.CreatePerson();
            _matrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro> { _matrixPro };
            _dateOnlyPeriod = new DateOnlyPeriod(_date, _date);
            _blockInfo = new BlockInfo(_dateOnlyPeriod);
            _group = new Group(new List<IPerson>{_person}, "hjk");
            _teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>>{ _matrixList });
            _teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
	        _groupPageLight = new GroupPageLight();
        }

        [Test] 
        public void ReturnNullWhenTeamBlockIsNull()
        {
            Assert.IsNull(_target.GetTeamBlockInfo(null,new DateOnly(),new List<IScheduleMatrixPro>(),null, new DateOnlyPeriod()));
        }

        [Test]
        public void ReturnNullWhenSchedulingOptionsIsNull()
        {
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, new DateOnly(), new List<IScheduleMatrixPro>(), null, new DateOnlyPeriod()));
        }

        
       

        [Test]
        public void ReturnNullIfNewTeamBlockInfoIsNullWith()
        {
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
				_schedulingOptions.UseBlock = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, new DateOnly(),
                                                                      new SingleDayBlockFinder())).IgnoreArguments().Return(null);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, new DateOnly(), new List<IScheduleMatrixPro>(), _schedulingOptions, new DateOnlyPeriod()));
        }

        [Test]
        public void ReturnNullIfDayIsScheduledInTeamBlock()
        {
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
				_schedulingOptions.UseBlock = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
																																			new SingleDayBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(_teamBlockInfo, _date, _schedulingOptions))
					 .Return(true);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions, new DateOnlyPeriod()));
        }


        [Test]
        public void ReturnNullIfBlockNotInSteadyState()
        {
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
				_schedulingOptions.UseBlock = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
																																			new SingleDayBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);

                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(_teamBlockInfo, _date, _schedulingOptions))
					 .Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).IgnoreArguments() 
                      .Return(false);
            }
            Assert.IsNull(_target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions, new DateOnlyPeriod()));
        }

        [Test]
        public void ReturnValidTeamBlockInfo()
        {
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
				_schedulingOptions.UseBlock = false;
            using (_mocks.Record())
            {
                Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
																																			new SingleDayBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
                Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
	            Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(_teamBlockInfo, _date, _schedulingOptions))
	                  .Return(false);
                Expect.Call(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).IgnoreArguments()
                      .Return(true);
            }

	        var result = _target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions,
		        _dateOnlyPeriod);
            Assert.AreEqual(_teamBlockInfo, result );
			Assert.AreEqual(1, result.BlockInfo.UnLockedDates().Count);
        }

		[Test]
		public void ShouldLockUnselectedDays()
		{
			_schedulingOptions.GroupOnGroupPageForTeamBlockPer = _groupPageLight;
			_schedulingOptions.UseBlock = false;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _date,
																		new SingleDayBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);

				Expect.Call(_matrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_matrixPro.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(_date)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(_teamBlockInfo, _date, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions)).IgnoreArguments()
					  .Return(true);
			}

			var result = _target.GetTeamBlockInfo(_teamInfo, _date, new List<IScheduleMatrixPro>(), _schedulingOptions,
				new DateOnlyPeriod());
			Assert.AreEqual(_teamBlockInfo, result);
			Assert.AreEqual(0, result.BlockInfo.UnLockedDates().Count);
		}
    }
}
