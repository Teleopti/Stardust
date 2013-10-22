using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockSchedulerTest
	{
		private TeamBlockScheduler _target;
		private MockRepository _mocks;
		private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private IRestrictionAggregator _restrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private ITeamScheduling _teamScheduling;
		private IWorkShiftSelector _workShiftSelector;
		private SchedulingOptions _schedulingOptions;
		private DateOnly _dateOnly;
		private IPerson _person;
		private TeamBlockInfo _teamBlockInfo;
		private DateOnlyPeriod _selectedPeriod;
		private Activity _activity;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private TimeZoneInfo _timeZoneInfo;
		private ShiftCategory _category;
		private WorkShift _workShift1;
		private WorkShift _workShift2;
		private WorkShift _workShift3;
		private IScheduleMatrixPro _matrix1;
		private IGroupPerson _groupPerson;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleRange _scheduleRange;
		private IOpenHoursToEffectiveRestrictionConverter _openHoursToEffectiveRestrictionConverter;
		private ITeamBlockClearer _teamBlockCleaner;
		private ISchedulePartModifyAndRollbackService _rollbackService;
	    private IOpenHourRestrictionForTeamBlock _openHourRestrictionforTeamBlock;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillDayPeriodIntervalDataGenerator = _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
			_restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_teamScheduling = _mocks.StrictMock<ITeamScheduling>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_mocks.StrictMock<IShiftProjectionCache>();

	        _openHourRestrictionforTeamBlock = _mocks.StrictMock<IOpenHourRestrictionForTeamBlock>();
			_schedulingOptions = new SchedulingOptions();
			_dateOnly = new DateOnly(2013, 4, 8);
			var zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
			_timeZoneInfo = (zone);
			_person = PersonFactory.CreatePerson("bill");
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
			var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
			_selectedPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			var blockInfo = new BlockInfo(_selectedPeriod);
			_teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_openHoursToEffectiveRestrictionConverter = _mocks.StrictMock<IOpenHoursToEffectiveRestrictionConverter>();
			_teamBlockCleaner = _mocks.StrictMock<ITeamBlockClearer>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_target = new TeamBlockScheduler(_skillDayPeriodIntervalDataGenerator, _restrictionAggregator,
											 _workShiftFilterService, _teamScheduling, _workShiftSelector,
											 _openHoursToEffectiveRestrictionConverter, _teamBlockCleaner, _rollbackService,_openHourRestrictionforTeamBlock );
		}

		[Test]
		public void ShouldCheckParameters()
		{
			Assert.Throws<ArgumentNullException>(() => _target.ScheduleTeamBlockDay(null, _dateOnly, _schedulingOptions,
														_selectedPeriod, new List<IPerson> { _person }));
		}

		[Test]
		public void ShouldBeFalseIfNoShiftsAvailable()
		{
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
	  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true)).Return(null);
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[Test]
		public void ShouldBeFalseIfNoShiftsIsEmpty()
		{
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(new List<IShiftProjectionCache>());
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldBeFalseIfDayIsOutsideOfSelectedPeriod()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var selectedPeriod = new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1));
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
.Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true)).Return(shifts);

				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldScheduleSelectedBlock()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var finderResult = new WorkShiftFinderResult(_groupPerson, _dateOnly);
			using (_mocks.Record())
			{

				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.AggregatePerDayPerPerson(_dateOnly, _person, _teamBlockInfo, _schedulingOptions, shifts[0], false)).Return(restriction);
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions, finderResult, true))
					  .Return(shifts).Repeat.AtLeastOnce();
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person, _teamBlockInfo, restriction, shifts[0],
														   _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, shifts[0], _selectedPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);

				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));

				expectCallForChecker(scheduleDay);

				Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		private void expectCallForChecker(IScheduleDay scheduleDay)
		{
			IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();

			Expect.Call(_matrix1.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();

			Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
			Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
			Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
			Expect.Call(_matrix1.Person).Return(_person).Repeat.AtLeastOnce();
			Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay).Repeat.AtLeastOnce();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldContinueScheduleSelectedBlockForShiftsAvailable()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.AggregatePerDayPerPerson(_dateOnly, _person, _teamBlockInfo, _schedulingOptions, shifts[0], false)).Return(restriction);
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);

				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();

				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();

				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person, _teamBlockInfo, restriction, shifts[0], _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly)))
					  .Return(shifts);

				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, shifts[0], _selectedPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);

				expectCallForChecker(scheduleDay);
				Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[Test]
		public void ShouldSkipScheduleSelectedBlockIfNoShiftsAvailable()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
				Expect.Call(_restrictionAggregator.AggregatePerDayPerPerson(_dateOnly, _person, _teamBlockInfo, _schedulingOptions, shifts[0], false)).Return(restriction);
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);

				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person, _teamBlockInfo, restriction, shifts[0], _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly)))
					  .Return(null);

				expectCallForChecker(scheduleDay);
				Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person });
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipScheduleSelectedBlockDoesNotContainThePerson()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var finderResult = new WorkShiftFinderResult(_groupPerson, _dateOnly);

			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   finderResult, true))
					  .Return(shifts).Repeat.AtLeastOnce();
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { PersonFactory.CreatePerson("test1") }));
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();

				IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
				DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
				var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
				Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();

				Expect.Call(_matrix1.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_matrix1.Person).Return(_person).Repeat.Twice();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_matrix1.Person).Return(_person).Repeat.Twice();
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipScheduleSelectedBlockIfItIsAlreadyScheduled()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(true);

				expectCallForChecker(scheduleDay);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldScheduleSelectedBlockForSameShift()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions.UseTeamBlockSameShift = true;
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_restrictionAggregator.AggregatePerDayPerPerson(_dateOnly, _person, _teamBlockInfo, _schedulingOptions,
																			shifts[0], false)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person, _teamBlockInfo, restriction, shifts[0], _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly)))
					  .Return(shifts);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons))
					  .Return(shifts[0])
					  .Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, shifts[0], _selectedPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));

				expectCallForChecker(scheduleDay);
				Expect.Call(scheduleDay.IsScheduled()).Return(true);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
														   new List<IPerson> { _person }));
			}
		}

		[Test]
		public void ShouldInvokeEventHandler()
		{
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_target.DayScheduled += _target_DayScheduled;
			_target.OnDayScheduled(null, new SchedulingServiceBaseEventArgs(scheduleDay));
			_target.DayScheduled -= _target_DayScheduled;
		}

		private void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipScheduleSelectedBlockForSameShiftIfItIsAlreadyScheduled()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions.UseTeamBlockSameShift = true;
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(true);

				expectCallForChecker(scheduleDay);
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipScheduleSelectedBlockForSameShiftIfItDoesNotContainThePerson()
		{
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions.UseTeamBlockSameShift = true;
			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
														   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { PersonFactory.CreatePerson("Test3") }));

				//expectCallForChecker(scheduleDay);
				IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
				DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
				var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
				Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();

				Expect.Call(_matrix1.SchedulePeriod).Return(virtualSchedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_matrix1.Person).Return(_person).Repeat.Twice();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay.IsScheduled()).Return(false);

				Expect.Call(_matrix1.Person).Return(_person).Repeat.Twice();
                Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
			}
		}

		[Test]
		public void ShouldAttemptToScheduleTwice()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseTeamBlockSameShiftCategory = true;
			var restriction =
				new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var shifts = getCashes();
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			using (_mocks.Record())
			{
				Expect.Call(_matrix1.SchedulePeriod).Return(schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(_selectedPeriod).Repeat.AtLeastOnce();
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.AggregatePerDayPerPerson(_dateOnly, _person, _teamBlockInfo, _schedulingOptions,
				                                                            shifts[0], false)).Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person, _teamBlockInfo, restriction, shifts[0], _schedulingOptions,
														new WorkShiftFinderResult(_groupPerson, _dateOnly)))
				  .Return(shifts).Repeat.AtLeastOnce();
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction).Repeat.Twice();
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
															new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts).Repeat.AtLeastOnce();
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo))
					  .Return(activityData).Repeat.AtLeastOnce();
				//Expect.Call(_openHoursToEffectiveRestrictionConverter.Convert(activityData)).Return(restriction).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, shifts[0], _selectedPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(scheduleDay.IsScheduled()).Return(false).Repeat.Times(3);
				Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.Twice();
				Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));

				Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
				Expect.Call(scheduleDictionary[_person]).Return(_scheduleRange).Repeat.AtLeastOnce();
				Expect.Call(_matrix1.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(() => _teamBlockCleaner.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
			    Expect.Call(_openHourRestrictionforTeamBlock.HasSameOpeningHours(_teamBlockInfo)).IgnoreArguments().Return(true).Repeat.AtLeastOnce() ;
			}
			using (_mocks.Playback())
			{
				Assert.That(_schedulingOptions.NotAllowedShiftCategories.Count, Is.EqualTo(0));
				Assert.IsTrue(_target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _selectedPeriod,
															new List<IPerson> { _person }));
				Assert.That(_schedulingOptions.NotAllowedShiftCategories.Count, Is.EqualTo(0));
			}
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnly, _timeZoneInfo);
				retList.Add(cache);
			}
			return retList;
		}

		private IEnumerable<IWorkShift> getWorkShifts()
		{
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  _activity, _category);
			_workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			_workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);

			return new List<IWorkShift> { _workShift1, _workShift2, _workShift3 };
		}
	}
}
