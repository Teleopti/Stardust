using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class DayOffOnPeriodTest
	{
		private MockRepository _mock;
		private DayOffOnPeriod _target;
		private DateOnlyPeriod _period;
		private IList<IScheduleDay> _scheduleDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private DateOnly _dateOnly1;
		private DateOnly _dateOnly2;
		private IHasContractDayOffDefinition _hasContractDayOffDefinition;
		private IScheduleDayAvailableForDayOffSpecification _dayAvailableForDayOffSpecification;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private SchedulingOptions _schedulingOptions;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_dateOnly1 = new DateOnly(2013, 1, 1);
			_dateOnly2 = new DateOnly(2013, 1, 2);
			_period = new DateOnlyPeriod(_dateOnly1, _dateOnly2);
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay>{_scheduleDay1, _scheduleDay2};
			_target = new DayOffOnPeriod(_period, _scheduleDays, 1);
			_hasContractDayOffDefinition = _mock.StrictMock<IHasContractDayOffDefinition>();
			_dayAvailableForDayOffSpecification = _mock.StrictMock<IScheduleDayAvailableForDayOffSpecification>();
			_effectiveRestrictionCreator = _mock.StrictMock<IEffectiveRestrictionCreator>();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldReturnPeriod()
		{
			var period = _target.Period;
			Assert.AreEqual(_period, period);	
		}

		[Test]
		public void ShouldReturnDayOffCount()
		{
			var count = _target.DaysOffCount;
			Assert.AreEqual(1, count);
		}

		[Test]
		public void ShouldReturnNullWhenDayIsNotAvailableForDayOff()
		{
			var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> { scheduleDay1 };
			var dateOnly1 = new DateOnly(2013, 10, 15);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly1, dateOnly1);
			_target = new DayOffOnPeriod(dateOnlyPeriod, scheduleDays, 0);

			using (_mock.Record())
			{
				Expect.Call(scheduleDay1.HasDayOff()).Return(false);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay1)).Return(true);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay1)).Return(false);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.FindBestSpotForDayOff(_hasContractDayOffDefinition, _dayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.IsNull(scheduleDay);
			}	
		}

		[Test]
		public void ShouldReturnNullWhenAllContractDayOffsIsFulFilled()
		{
			var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> { scheduleDay1 };
			var dateOnly1 = new DateOnly(2013, 10, 15);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly1, dateOnly1);
			_target = new DayOffOnPeriod(dateOnlyPeriod, scheduleDays, 0);
			
			using (_mock.Record())
			{
				Expect.Call(scheduleDay1.HasDayOff()).Return(true);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay1)).Return(true);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.FindBestSpotForDayOff(_hasContractDayOffDefinition, _dayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.IsNull(scheduleDay);
			}		
		}

		[Test]
		public void ShouldReturnNullWhenDayHasRestrictionAndNotAvailableForDayOff()
		{
			var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> { scheduleDay1 };
			var dateOnly1 = new DateOnly(2013, 10, 15);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly1, dateOnly1);
			var effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();

			_target = new DayOffOnPeriod(dateOnlyPeriod, scheduleDays, 0);

			using (_mock.Record())
			{
				Expect.Call(scheduleDay1.HasDayOff()).Return(false);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay1)).Return(true);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay1)).Return(true);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay1, _schedulingOptions)).Return(effectiveRestriction);
				Expect.Call(effectiveRestriction.NotAllowedForDayOffs).Return(true);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.FindBestSpotForDayOff(_hasContractDayOffDefinition, _dayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.IsNull(scheduleDay);
			}
		}

		[Test]
		public void ShouldReturnNullWhenNoContractDayOffs()
		{
			var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> {scheduleDay1};
			var dateOnly = new DateOnly(2013, 10, 15);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			_target = new DayOffOnPeriod(dateOnlyPeriod, scheduleDays, 0);

			using (_mock.Record())
			{
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay1)).Return(false);
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.FindBestSpotForDayOff(_hasContractDayOffDefinition, _dayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.IsNull(scheduleDay);
			}
		}

		[Test]
		public void FindBestSpotForDayOff()
		{
			var scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			var scheduleDay3 = _mock.StrictMock<IScheduleDay>();
			var scheduleDay4 = _mock.StrictMock<IScheduleDay>();
			var scheduleDay5 = _mock.StrictMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> {scheduleDay1, scheduleDay2, scheduleDay3, scheduleDay4, scheduleDay5};

			var dateOnly1 = new DateOnly(2013, 10, 15);
			var dateOnly2 = new DateOnly(2013, 10, 16);
			var dateOnly3 = new DateOnly(2013, 10, 17);
			var dateOnly4 = new DateOnly(2013, 10, 18);
			var dateOnly5 = new DateOnly(2013, 10, 19);

			var dateOnlyAsDateTimePeriod1 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnlyAsDateTimePeriod2 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnlyAsDateTimePeriod3 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnlyAsDateTimePeriod4 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnlyAsDateTimePeriod5 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();

			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly1, dateOnly5);

			_target = new DayOffOnPeriod(dateOnlyPeriod, scheduleDays, 0);


			using (_mock.Record())
			{
				Expect.Call(scheduleDay1.HasDayOff()).Return(false);
				Expect.Call(scheduleDay5.HasDayOff()).Return(false);

				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay1)).Return(true);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay2)).Return(false);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay3)).Return(false);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay4)).Return(false);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(scheduleDay5)).Return(true);

				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay1)).Return(false);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay2)).Return(true);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay3)).Return(true);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay4)).Return(true);
				Expect.Call(_dayAvailableForDayOffSpecification.IsSatisfiedBy(scheduleDay5)).Return(false);

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay2, _schedulingOptions)).Return(null);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay3, _schedulingOptions)).Return(null);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay4, _schedulingOptions)).Return(null);
			
				Expect.Call(scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay2.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod2).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay3.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod3).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay4.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod4).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay5.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod5).Repeat.AtLeastOnce();

				Expect.Call(dateOnlyAsDateTimePeriod1.DateOnly).Return(dateOnly1).Repeat.AtLeastOnce();
				Expect.Call(dateOnlyAsDateTimePeriod2.DateOnly).Return(dateOnly2).Repeat.AtLeastOnce();
				Expect.Call(dateOnlyAsDateTimePeriod3.DateOnly).Return(dateOnly3).Repeat.AtLeastOnce();
				Expect.Call(dateOnlyAsDateTimePeriod4.DateOnly).Return(dateOnly4).Repeat.AtLeastOnce();
				Expect.Call(dateOnlyAsDateTimePeriod5.DateOnly).Return(dateOnly5).Repeat.AtLeastOnce();	
			}

			using (_mock.Playback())
			{
				var scheduleDay = _target.FindBestSpotForDayOff(_hasContractDayOffDefinition, _dayAvailableForDayOffSpecification, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.AreEqual(scheduleDay4, scheduleDay);
			}
		}
	}
}
