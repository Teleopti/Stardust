using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class AbsencePreferenceSchedulerTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private ISchedulingOptions _options;
		private ISchedulePartModifyAndRollbackService _rollBackService;
		private AbsencePreferenceScheduler _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_rollBackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_options = new SchedulingOptions();
			_target = new AbsencePreferenceScheduler(_schedulingResultStateHolder, _effectiveRestrictionCreator, _options,_rollBackService);
		}

		[Test]
		public void ShouldNothingIfDatesOrPersonsAreNull()
		{
			var dates = new List<DateOnly>();
			var persons = new List<IPerson>();
			_target.AddPreferredAbsence(dates, null);
			_target.AddPreferredAbsence(null, persons);
		}

		[Test]
		public void ShouldDoNothingIfNoAbsence()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>()) {IsPreferenceDay = true};
			var date1 = new DateOnly(2009, 2, 2);
			var dates = new List<DateOnly> { date1 };
			
			var person = _mocks.StrictMock<IPerson>();
			var persons = new List<IPerson> { person };

			var part1 = _mocks.StrictMock<IScheduleDay>();
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedules[person]).Return(range).Repeat.AtLeastOnce();
			Expect.Call(range.ScheduledDay(date1)).Return(part1).Repeat.AtLeastOnce().IgnoreArguments();

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _options)).Return(
					effectiveRestriction);
			
			_mocks.ReplayAll();

			_target.AddPreferredAbsence(dates, persons);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldDoNothingIfPreferenceDay()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>()) { IsPreferenceDay = false };
			var date1 = new DateOnly(2009, 2, 2);
			var dates = new List<DateOnly> { date1 };
			
			var person = _mocks.StrictMock<IPerson>();
			var persons = new List<IPerson> { person };
			
			var part1 = _mocks.StrictMock<IScheduleDay>();
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedules[person]).Return(range).Repeat.AtLeastOnce();
			Expect.Call(range.ScheduledDay(date1)).Return(part1).Repeat.AtLeastOnce().IgnoreArguments();

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _options)).Return(
					effectiveRestriction);

			_mocks.ReplayAll();

			_target.AddPreferredAbsence(dates, persons);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddAbsenceIfPreferred()
		{
			var absence = new Absence();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, absence,
																				  new List<IActivityRestriction>()) {IsPreferenceDay = true};
			var date11 = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var date1 = new DateOnly(2009, 2, 2);
			var dates = new List<DateOnly> {date1};
			var period = new DateTimePeriod(date11, date11.AddDays(1));
			var person = _mocks.StrictMock<IPerson>();
			var persons = new List<IPerson> {person};
			var part1 = _mocks.StrictMock<IScheduleDay>();
			
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules);
			Expect.Call(schedules[person]).Return(range);
			Expect.Call(range.ScheduledDay(date1)).Return(part1);

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _options)).Return(
					effectiveRestriction);
			Expect.Call(part1.Period).Return(period);
			Expect.Call(() => part1.CreateAndAddAbsence(new AbsenceLayer(absence, period))).IgnoreArguments();
			Expect.Call(() => _rollBackService.Modify(part1));
			_mocks.ReplayAll();

			_target.AddPreferredAbsence(dates, persons);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAddAbsenceOnFirstDayAndJumpOutIfCanceled()
		{
			var absence = new Absence();
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, absence,
																				  new List<IActivityRestriction>()) { IsPreferenceDay = true };
			_target.DayScheduled += targetDayScheduled;
			var date11 = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var date1 = new DateOnly(2009, 2, 2);
			var date2 = new DateOnly(2009, 2, 3);
			var dates = new List<DateOnly> { date1, date2 };
			var period = new DateTimePeriod(date11, date11.AddDays(1));
			var person = _mocks.StrictMock<IPerson>();
			var persons = new List<IPerson> { person };
			var part1 = _mocks.StrictMock<IScheduleDay>();

			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();

			Expect.Call(_schedulingResultStateHolder.Schedules).Return(schedules);
			Expect.Call(schedules[person]).Return(range);
			Expect.Call(range.ScheduledDay(date1)).Return(part1);

			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(part1, _options)).Return(
					effectiveRestriction);
			Expect.Call(part1.Period).Return(period);
			Expect.Call(() => part1.CreateAndAddAbsence(new AbsenceLayer(absence, period))).IgnoreArguments();
			Expect.Call(() => _rollBackService.Modify(part1));
			_mocks.ReplayAll();

			_target.AddPreferredAbsence(dates, persons);
			_mocks.VerifyAll();
		}

		static void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}
	}

	
}