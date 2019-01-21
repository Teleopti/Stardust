using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class TeamBlockDayOffDaySwapDecisionMakerTest
	{
		private ITeamBlockDayOffDaySwapDecisionMaker _target;
		private MockRepository _mocks;
		private ILockableBitArrayFactory _lockableBitArrayFactory;
		private IScheduleDictionary _scheduleDictionary;
		private IOptimizationPreferences _optimizationPreferences;
		private IPerson _personSenior;
		private IPerson _personJunior;
		private Group _groupSenior;
		private Group _groupJunior;
		private IScheduleMatrixPro _matrixSenior;
		private TeamInfo _teamSenior;
		private TeamInfo _teamJunior;
		private DateOnly _dateOnly;
		private TeamBlockInfo _teamBlockInfoSenior;
		private TeamBlockInfo _teamBlockInfoJunior;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private List<DateOnly> _dayOffsToGiveAway;
		private IScheduleMatrixPro _matrixJunior;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IDayOffRulesValidator _dayOffRulesValidator;
		private DateOnly _dateBefore;
		private LockableBitArray _seniorBitArray;
		private LockableBitArray _juniorBitArray;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffRulesValidator = _mocks.StrictMock<IDayOffRulesValidator>();
			_lockableBitArrayFactory = _mocks.StrictMock<ILockableBitArrayFactory>();
			_target = new TeamBlockDayOffDaySwapDecisionMaker(_lockableBitArrayFactory, _dayOffRulesValidator);

			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_optimizationPreferences = new OptimizationPreferences();
	
			_personSenior = PersonFactory.CreatePerson();
			_personJunior = PersonFactory.CreatePerson();
			_groupSenior = new Group(new List<IPerson> { _personSenior }, "Senior");
			_groupJunior = new Group(new List<IPerson> { _personJunior }, "Junior");
			_matrixSenior = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixJunior = _mocks.StrictMock<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixesSenior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesSenior.Add(new List<IScheduleMatrixPro> { _matrixSenior });
			IList<IList<IScheduleMatrixPro>> groupMatrixesJunior = new List<IList<IScheduleMatrixPro>>();
			groupMatrixesJunior.Add(new List<IScheduleMatrixPro> { _matrixJunior });
			_teamSenior = new TeamInfo(_groupSenior, groupMatrixesSenior);
			_teamJunior = new TeamInfo(_groupJunior, groupMatrixesJunior);
			_dateOnly = new DateOnly(2014, 1, 27);
			_dateBefore = _dateOnly.AddDays(-1);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateBefore, _dateOnly));
			_teamBlockInfoSenior = new TeamBlockInfo(_teamSenior, blockInfo);
			_teamBlockInfoJunior = new TeamBlockInfo(_teamJunior, blockInfo);
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_dayOffsToGiveAway = new List<DateOnly> {_dateBefore};
			_seniorBitArray = new LockableBitArray(21, true, true);
			_juniorBitArray = new LockableBitArray(21, true, true);
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();

			_daysOffPreferences = new DaysOffPreferences() {ConsiderWeekBefore = true, ConsiderWeekAfter = true};
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);	
		}

		[Test]
		public void ShouldIgnoreIfTwoScheduleDaysAreDayOff()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences,
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfSeniorIsDayOff()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences, 
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldNotSwappOnBestLockedSeniorDays()
		{
			_seniorBitArray.Lock(1, true);
			
			using (_mocks.Record())
			{
				commonMocks();
				bestLockMock();
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences, 
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}	
		}

		[Test]
		public void ShouldNotSwappOnBestLockedJuniorDays()
		{
			_juniorBitArray.Lock(1, true);

			using (_mocks.Record())
			{
				commonMocks();
				bestLockMock();	
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
										_scheduleDictionary, _optimizationPreferences, 
										_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldNotSwappOnLockedSeniorDays()
		{
			_seniorBitArray.Lock(0, true);
			
			using (_mocks.Record())
			{
				commonMocks();
				bestLockMock();
				lockMock();
				Expect.Call(_matrixSenior.EffectivePeriodDays).Return(new [] { _scheduleDayPro1, _scheduleDayPro2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences, 
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}	
		}

		[Test]
		public void ShouldNotSwappOnLockedJuniorDays()
		{
			_juniorBitArray.Lock(0, true);

			using (_mocks.Record())
			{
				commonMocks();
				bestLockMock();
				lockMock();
				Expect.Call(_matrixSenior.EffectivePeriodDays).Return(new [] { _scheduleDayPro1, _scheduleDayPro2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences, 
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldSwapDayOffsBettweenSeniorAndJunior()
		{
			var scheduleDaySeniorToGiveAway = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayJuniorToAccept = _mocks.StrictMock<IScheduleDay>();
			var clonedBitArrayOfSenior = new LockableBitArray(21, true, true);
			var clonedBitArrayOfJunior = new LockableBitArray(21, true, true);
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_matrixSenior.OuterWeeksPeriodDays)
					.Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
				Expect.Call(_matrixSenior.EffectivePeriodDays)
					.Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
				Expect.Call(_matrixJunior.OuterWeeksPeriodDays)
					.Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.Day).Return(_dateBefore).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixSenior)).Return(_seniorBitArray);
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixJunior)).Return(_juniorBitArray);
				Expect.Call(_range1.ScheduledDay(_dateBefore)).Return(scheduleDaySeniorToGiveAway);
				Expect.Call(_range2.ScheduledDay(_dateBefore)).Return(scheduleDayJuniorToAccept);
				Expect.Call(scheduleDaySeniorToGiveAway.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(scheduleDayJuniorToAccept.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_dayOffRulesValidator.Validate(clonedBitArrayOfSenior, _optimizationPreferences, _daysOffPreferences)).IgnoreArguments().Return(true);
				Expect.Call(_dayOffRulesValidator.Validate(clonedBitArrayOfJunior, _optimizationPreferences, _daysOffPreferences)).IgnoreArguments().Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary, 
											_optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.That(result.DateForSeniorDayOff, Is.EqualTo(_dateOnly));
				Assert.That(result.DateForRemovingSeniorDayOff, Is.EqualTo(_dateBefore));
			}
		}

		[Test]
		public void ShouldNotSwapIfAnyDayOffRulesValidationFailed()
		{
			var scheduleDaySeniorToGiveAway = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayJuniorToAccept = _mocks.StrictMock<IScheduleDay>();
			var clonedBitArrayOfSenior = new LockableBitArray(21, true, true);
			var clonedBitArrayOfJunior = new LockableBitArray(21, true, true);
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_matrixSenior.OuterWeeksPeriodDays)
					  .Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
				Expect.Call(_matrixSenior.EffectivePeriodDays)
					.Return(new [] { _scheduleDayPro1, _scheduleDayPro2 });
				Expect.Call(_matrixJunior.OuterWeeksPeriodDays)
					  .Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro1.Day).Return(_dateBefore).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixSenior)).Return(_seniorBitArray);
				Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixJunior)).Return(_juniorBitArray);
				Expect.Call(_range1.ScheduledDay(_dateBefore)).Return(scheduleDaySeniorToGiveAway);
				Expect.Call(_range2.ScheduledDay(_dateBefore)).Return(scheduleDayJuniorToAccept);
				Expect.Call(scheduleDaySeniorToGiveAway.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(scheduleDayJuniorToAccept.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_dayOffRulesValidator.Validate(clonedBitArrayOfSenior, _optimizationPreferences, _daysOffPreferences)).IgnoreArguments().Return(true);
				Expect.Call(_dayOffRulesValidator.Validate(clonedBitArrayOfJunior, _optimizationPreferences, _daysOffPreferences)).IgnoreArguments().Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, 
											_scheduleDictionary, _optimizationPreferences, 
											_dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
				Assert.IsNull(result);

			}
		}

		[Test]
		public void ShouldBeAbleToCancelBeforeOptimizingANewDay()
		{
			_target.Cancel();
			var result = _target.Decide(_dateOnly, _teamBlockInfoSenior, _teamBlockInfoJunior, _scheduleDictionary,
			                            _optimizationPreferences, _dayOffsToGiveAway, _dayOffOptimizationPreferenceProvider);
			Assert.IsNull(result);
		}

		private void commonMocks()
		{
			Expect.Call(_scheduleDictionary[_personSenior]).Return(_range1);
			Expect.Call(_range1.ScheduledDay(_dateOnly)).Return(_scheduleDay1);
			Expect.Call(_scheduleDictionary[_personJunior]).Return(_range2);
			Expect.Call(_range2.ScheduledDay(_dateOnly)).Return(_scheduleDay2);
			Expect.Call(_matrixSenior.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_matrixJunior.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly)).Repeat.AtLeastOnce();
			Expect.Call(_matrixSenior.Person).Return(_personSenior);
			Expect.Call(_matrixJunior.Person).Return(_personJunior);
		}

		private void bestLockMock()
		{
			Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
			Expect.Call(_matrixSenior.OuterWeeksPeriodDays).Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
			Expect.Call(_matrixJunior.OuterWeeksPeriodDays).Return(new [] { _scheduleDayPro1, _scheduleDayPro2 }).Repeat.AtLeastOnce();
			Expect.Call(_scheduleDayPro1.Day).Return(_dateBefore).Repeat.AtLeastOnce();
			Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly).Repeat.AtLeastOnce();
			Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixSenior)).Return(_seniorBitArray);
			Expect.Call(_lockableBitArrayFactory.ConvertFromMatrix(true, true, _matrixJunior)).Return(_juniorBitArray);	
		}

		private void lockMock()
		{
			var scheduleDaySeniorToGiveAway = _mocks.StrictMock<IScheduleDay>();
			var scheduleDayJuniorToAccept = _mocks.StrictMock<IScheduleDay>();

			Expect.Call(_range1.ScheduledDay(_dateBefore)).Return(scheduleDaySeniorToGiveAway);
			Expect.Call(_range2.ScheduledDay(_dateBefore)).Return(scheduleDayJuniorToAccept);
			Expect.Call(scheduleDaySeniorToGiveAway.SignificantPart()).Return(SchedulePartView.DayOff);
			Expect.Call(scheduleDayJuniorToAccept.SignificantPart()).Return(SchedulePartView.MainShift);
		}
	}
}
