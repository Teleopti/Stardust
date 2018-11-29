using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class ScheduleDayDataMapperTest
	{
		private IScheduleDayDataMapper _target;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDay _scheduleDay1;
		private MockRepository _mocks;
		private SchedulingOptions _schedulingOptions;
		private IEffectiveRestriction _effectiveRestriction;
		private IHasContractDayOffDefinition _hasContractDayOffDefinition;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_hasContractDayOffDefinition = _mocks.StrictMock<IHasContractDayOffDefinition>();
			_target = new ScheduleDayDataMapper(_effectiveRestrictionCreator, _hasContractDayOffDefinition);
		}

		[Test]
		public void CanMap()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayPro1.Day).Return(DateOnly.MinValue);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(_scheduleDay1)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).Return(
					_effectiveRestriction);
				Expect.Call(_effectiveRestriction.IsAvailabilityDay).Return(false);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				IScheduleDayData result = _target.Map(_scheduleDayPro1, _schedulingOptions);
				Assert.AreEqual(DateOnly.MinValue, result.DateOnly);
				Assert.IsTrue(result.HaveRestriction);
				Assert.IsFalse(result.IsContractDayOff);
				Assert.IsTrue(result.IsDayOff);
				Assert.IsTrue(result.IsScheduled);
			}
			
		}

		[Test]
		public void EffectiveRestrictionCanBeNull()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayPro1.Day).Return(DateOnly.MinValue);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(_scheduleDay1)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).Return(null);
			}

			using (_mocks.Playback())
			{
				IScheduleDayData result = _target.Map(_scheduleDayPro1, _schedulingOptions);
				Assert.AreEqual(DateOnly.MinValue, result.DateOnly);
				Assert.IsFalse(result.HaveRestriction);
				Assert.IsFalse(result.IsContractDayOff);
				Assert.IsTrue(result.IsDayOff);
				Assert.IsTrue(result.IsScheduled);
			}
		}

		[Test]
		public void AvailabilityDayIsNotARestrictionIfNotScheduleOnlyAvailabilityDays()
		{
			_schedulingOptions.AvailabilityDaysOnly = false;
			_schedulingOptions.UseAvailability = true;

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayPro1.Day).Return(DateOnly.MinValue);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_hasContractDayOffDefinition.IsDayOff(_scheduleDay1)).Return(false);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_effectiveRestriction.IsAvailabilityDay).Return(true);
			}

			using (_mocks.Playback())
			{
				IScheduleDayData result = _target.Map(_scheduleDayPro1, _schedulingOptions);
				Assert.AreEqual(DateOnly.MinValue, result.DateOnly);
				Assert.IsFalse(result.HaveRestriction);
				Assert.IsFalse(result.IsContractDayOff);
				Assert.IsTrue(result.IsDayOff);
				Assert.IsTrue(result.IsScheduled);
			}
		}
	}
}