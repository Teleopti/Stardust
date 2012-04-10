﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class FixedStaffSchedulingServiceTest
	{
		private FixedStaffSchedulingService _schedulingService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private MockRepository _mocks;
		private ICccTimeZoneInfo _timeZoneInfo;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IEffectiveRestriction _effectiveRestriction;
		private IScheduleService _scheduleService;
        private ISchedulingOptions _schedulingOptions;
		private IAbsencePreferenceScheduler _absencePreferenseScheduler;
		private IDayOffScheduler _dayOffScheduler;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IResourceCalculateDelayer _resourceCalculateDelayer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _mocks.DynamicMock<IScheduleDayChangeCallback>();
			
			TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			_timeZoneInfo = new CccTimeZoneInfo(zone);
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>());
            _schedulingOptions = new SchedulingOptions();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			_absencePreferenseScheduler = _mocks.StrictMock<IAbsencePreferenceScheduler>();
			_dayOffScheduler = _mocks.StrictMock<IDayOffScheduler>();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();

			Expect.Call(() => _absencePreferenseScheduler.DayScheduled += null).IgnoreArguments();
			Expect.Call(() => _dayOffScheduler.DayScheduled += null).IgnoreArguments();
			_mocks.ReplayAll();
			_schedulingService = new FixedStaffSchedulingService( _schedulingResultStateHolder,
																 _dayOffsInPeriodCalculator,
																 _effectiveRestrictionCreator, 
                                                                 _scheduleService,
                                                                 _absencePreferenseScheduler,
																 _dayOffScheduler, _resourceOptimizationHelper);
			_mocks.VerifyAll();
			_mocks.BackToRecordAll();
		}

		[Test]
		public void VerifySetup()
		{
			Assert.IsNotNull(_schedulingService);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfDayOffSchedulerIsNull()
		{
			_schedulingService = new FixedStaffSchedulingService( _schedulingResultStateHolder,
																 _dayOffsInPeriodCalculator,
																 _effectiveRestrictionCreator, 
                                                                 _scheduleService,
                                                                 _absencePreferenseScheduler,
																 null, _resourceOptimizationHelper);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfAbsenceSchedulerIsNull()
		{
			_schedulingService = new FixedStaffSchedulingService(_schedulingResultStateHolder,
																 _dayOffsInPeriodCalculator,
																 _effectiveRestrictionCreator, 
                                                                 _scheduleService,
                                                                 null,
																 _dayOffScheduler, _resourceOptimizationHelper);	
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
            
			var dayOffCollection = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>());
			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			int dayOffsNow;
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
			Expect.Call(part1.PersonDayOffCollection()).Return(dayOffCollection).Repeat.Any();

			Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, true, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments().Repeat.AtLeastOnce()
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
            //Expect.Call(person.Equals(person)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(person2.Equals(person)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
            
			_mocks.ReplayAll();

            _schedulingOptions.UseShiftCategoryLimitations = false;
            _schedulingOptions.UseRotations = true;
            _schedulingOptions.UsePreferences = true;
            _schedulingOptions.UseAvailability = true;
            _schedulingOptions.AddContractScheduleDaysOff = false;

		    _schedulingService.DoTheScheduling(new List<IScheduleDay> {part4, part3, part2, part1}, _schedulingOptions, true, false);
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
		    
			var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			Expect.Call(person.VirtualSchedulePeriod(new DateOnly(2009, 2, 2))).Return(virtualSchedulePeriod).Repeat.AtLeastOnce().IgnoreArguments();
			Expect.Call(virtualSchedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			int targetDaysOff;
			int dayOffsNow;
			Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(virtualSchedulePeriod, out targetDaysOff,
																				out dayOffsNow)).Return(true);
            Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part2.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo)).Repeat.AtLeastOnce();
			Expect.Call(part2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 3), _timeZoneInfo)).Repeat.AtLeastOnce();
			Expect.Call(_scheduleService.SchedulePersonOnDay(null, _schedulingOptions, true, _effectiveRestriction, _resourceCalculateDelayer))
                .Return(true).IgnoreArguments();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(range.ScheduledDay(new DateOnly())).IgnoreArguments().Return(part1).Repeat.AtLeastOnce();
			Expect.Call(part1.IsScheduled()).Return(false).Repeat.Any();
			Expect.Call(_schedulingResultStateHolder.SkipResourceCalculation).Return(false).Repeat.Any();

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _schedulingOptions)).Return(_effectiveRestriction);
            Expect.Call(_scheduleService.FinderResults).Return(
                new ReadOnlyCollection<IWorkShiftFinderResult>(new List<IWorkShiftFinderResult>()));
		    Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
			_mocks.ReplayAll();

            _schedulingOptions.UseShiftCategoryLimitations = false;
            _schedulingOptions.UseRotations = true;
            _schedulingOptions.UseAvailability = true;
            _schedulingOptions.UsePreferences = true;
            _schedulingOptions.AddContractScheduleDaysOff = false;

			_schedulingService.DayScheduled += (sender, e) => { e.Cancel = true; };
            _schedulingService.DoTheScheduling(new List<IScheduleDay> { part2, part1 }, _schedulingOptions, true, false);
			Assert.IsNotNull(_schedulingService.FinderResults);
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyFindDates()
		{
			var date1 = new DateOnly(2009, 2, 2);
			var date2 = new DateOnly(2009, 2, 3);
			var date3 = new DateOnly(2009, 2, 4);
			var date4 = new DateOnly(2009, 2, 5);

			var part1 = _mocks.StrictMock<IScheduleDay>();
			var part2 = _mocks.StrictMock<IScheduleDay>();
			var part3 = _mocks.StrictMock<IScheduleDay>();
			var part4 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> parts = new List<IScheduleDay> { part1, part2, part3, part4 };
            
			using (_mocks.Record())
			{
				Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date1, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(part2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date2, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(part3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date3, _timeZoneInfo)).Repeat.AtLeastOnce();
				Expect.Call(part4.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date4, _timeZoneInfo)).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var lst = _schedulingService.GetAllDates(parts);
				Assert.AreEqual(4, lst.Count);
				Assert.AreEqual(new DateOnly(date1), lst[0]);
				Assert.AreEqual(new DateOnly(date2), lst[1]);
				Assert.AreEqual(new DateOnly(date3), lst[2]);
				Assert.AreEqual(new DateOnly(date4), lst[3]);
			}
		}

		[Test]
		public void ShouldAddPreferredAbsencesAndThenDayOffs()
		{
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var person = _mocks.StrictMock<IPerson>();
			var date1 = new DateOnly(2009, 2, 2);
			var rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();

			Expect.Call(part1.Person).Return(person).Repeat.AtLeastOnce();
			Expect.Call(part1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date1, new CccTimeZoneInfo(TimeZoneInfo.Utc))).Repeat.AtLeastOnce();
			Expect.Call(() => _absencePreferenseScheduler.AddPreferredAbsence(new List<DateOnly> {date1}, new List<IPerson> {person})).IgnoreArguments();
            Expect.Call(() => _dayOffScheduler.DayOffScheduling(new List<IScheduleDay> { part1 }, new List<DateOnly> { date1 }, new List<IPerson> { person }, rollbackService)).IgnoreArguments();
			_mocks.ReplayAll();
			_schedulingService.DayOffScheduling(new List<IScheduleDay> { part1 }, rollbackService);
			_mocks.VerifyAll();
		}

		
		[Test]
		public void ShouldReturnFalseOnCorrectNumberOfDaysOffIfDaysOffPeriodCalculatorReturnsFalse()
		{
			int target;
			int current;
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
