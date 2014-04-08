using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class FixedStaffSchedulingServiceTest
	{
		private FixedStaffSchedulingService _schedulingService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private MockRepository _mocks;
		private TimeZoneInfo _timeZoneInfo;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IEffectiveRestriction _effectiveRestriction;
		private IScheduleService _scheduleService;
        private ISchedulingOptions _schedulingOptions;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IDaysOffSchedulingService _daysOffSchedulingService;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _mocks.DynamicMock<IScheduleDayChangeCallback>();
			
			TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			_timeZoneInfo = (zone);
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>());
            _schedulingOptions = new SchedulingOptions();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_daysOffSchedulingService = _mocks.StrictMock<IDaysOffSchedulingService>();

			Expect.Call(() => _daysOffSchedulingService.DayScheduled += null).IgnoreArguments();
			_mocks.ReplayAll();
			_schedulingService = new FixedStaffSchedulingService( _schedulingResultStateHolder,
																 _dayOffsInPeriodCalculator,
																 _effectiveRestrictionCreator, 
                                                                 _scheduleService,
																 _daysOffSchedulingService,
																 _resourceOptimizationHelper,
																 new PersonSkillProvider());
			_mocks.VerifyAll();
			_mocks.BackToRecordAll();
		}

		[Test]
		public void VerifySetup()
		{
			Assert.IsNotNull(_schedulingService);
		}

		[Test]
		public void VerifyTheSchedulingCycle()
		{
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();

            var period1 = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);

			var person = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();

			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
            var skills = new List<ISkill> { SkillFactory.CreateSkill("Phone") };
		    
			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			//int dayOffsNow;
			IList<IScheduleDay> dayOffsNow = new List<IScheduleDay>();
			int targetDaysOff;
			
			Expect.Call(person2.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).Return(virtualSchedulePeriod).Repeat.AtLeastOnce().IgnoreArguments();
			Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).Return(virtualSchedulePeriod).Repeat.AtLeastOnce().IgnoreArguments();
			Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(virtualSchedulePeriod, out targetDaysOff,
																	out dayOffsNow)).Return(true).Repeat.AtLeastOnce();
			Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part3.Person).Return(person2).Repeat.AtLeastOnce();
			Expect.Call(part4.Person).Return(person2).Repeat.AtLeastOnce();
            Expect.Call(part1.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();
            Expect.Call(part2.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();
            Expect.Call(part3.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();
            Expect.Call(part4.DateOnlyAsPeriod).Return(period1).Repeat.AtLeastOnce();

			Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments().Repeat.AtLeastOnce()
                .Return(true);

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(range.ScheduledDay(new DateOnly())).IgnoreArguments().Return(part1).Repeat.AtLeastOnce();
			Expect.Call(range.ReFetch(part1)).Return(part2).Repeat.Any();

			Expect.Call(part1.IsScheduled()).Return(false).Repeat.Any();
			Expect.Call(_schedulingResultStateHolder.SkipResourceCalculation).Return(false).Repeat.Any();

            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _schedulingOptions)).Return(
					_effectiveRestriction).Repeat.Any();
            Expect.Call(_scheduleService.FinderResults).Return(
                new ReadOnlyCollection<IWorkShiftFinderResult>(new List<IWorkShiftFinderResult>()));

			_mocks.ReplayAll();

            _schedulingOptions.UseShiftCategoryLimitations = false;
            _schedulingOptions.UseRotations = true;
            _schedulingOptions.UsePreferences = true;
            _schedulingOptions.UseAvailability = true;

			_schedulingService.DoTheScheduling(new List<IScheduleDay> { part4, part3, part2, part1 }, _schedulingOptions, true, false, _rollbackService);
			Assert.IsNotNull(_schedulingService.FinderResults);

			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyCanCancelTheSchedulingCycle()
		{
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var person = _mocks.StrictMock<IPerson>();
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
            var skills = new List<ISkill> { SkillFactory.CreateSkill("Phone") };
		    
			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).Return(virtualSchedulePeriod).Repeat.AtLeastOnce().IgnoreArguments();
			Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			int targetDaysOff;
			//int dayOffsNow;
			IList<IScheduleDay> dayOffsNow = new List<IScheduleDay>();
			Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(virtualSchedulePeriod, out targetDaysOff,
																				out dayOffsNow)).Return(true);
            Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo)).Repeat.AtLeastOnce();
			Expect.Call(part2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 3), _timeZoneInfo)).Repeat.AtLeastOnce();
			Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService))
                .Return(true).IgnoreArguments();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(range.ScheduledDay(new DateOnly())).IgnoreArguments().Return(part1).Repeat.AtLeastOnce();
			Expect.Call(part1.IsScheduled()).Return(false).Repeat.Any();
			Expect.Call(_schedulingResultStateHolder.SkipResourceCalculation).Return(false).Repeat.Any();

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _schedulingOptions)).Return(_effectiveRestriction);
            Expect.Call(_scheduleService.FinderResults).Return(
                new ReadOnlyCollection<IWorkShiftFinderResult>(new List<IWorkShiftFinderResult>()));
			_mocks.ReplayAll();

            _schedulingOptions.UseShiftCategoryLimitations = false;
            _schedulingOptions.UseRotations = true;
            _schedulingOptions.UseAvailability = true;
            _schedulingOptions.UsePreferences = true;

			_schedulingService.DayScheduled += (sender, e) => { e.Cancel = true; };
			_schedulingService.DoTheScheduling(new List<IScheduleDay> { part2, part1 }, _schedulingOptions, true, false, _rollbackService);
			Assert.IsNotNull(_schedulingService.FinderResults);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseOnCorrectNumberOfDaysOffIfDaysOffPeriodCalculatorReturnsFalse()
		{
			int target;
			//int current;
			IList<IScheduleDay> current = new List<IScheduleDay>();
			var person = new Person { Name = new Name("hej", "svejs") };
			var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(virtualPeriod, out target, out current)).Return(false);
			Expect.Call(virtualPeriod.Person).Return(person);
			_mocks.ReplayAll();
			var result = _schedulingService.HasCorrectNumberOfDaysOff(virtualPeriod, new DateOnly());
			Assert.That(result, Is.False);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldClearResultOnSchedulingServiceOnClear()
        {
            Expect.Call(_scheduleService.ClearFinderResults);

            _mocks.ReplayAll();
            _schedulingService.ClearFinderResults();
            _mocks.VerifyAll();
        }

	}

}
