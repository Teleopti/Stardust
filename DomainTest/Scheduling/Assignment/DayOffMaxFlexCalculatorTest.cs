using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class DayOffMaxFlexCalculatorTest
	{
		private DayOffMaxFlexCalculator _flexCalculator;
		private MockRepository _mock;
		private DayOff _dayOff;
		private IScheduleDay _dayOffDay;
		private IScheduleDay _scheduleDay;
		private DateOnly _dateOnly;
		private DateOnly _dateOnlyBefore;
		private DateOnly _dateOnlyAfter;
		private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private DateTime _anchor;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod1;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod2;
		private IPersonAssignment _personAssignment;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_workTimeStartEndExtractor = _mock.StrictMock<IWorkTimeStartEndExtractor>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_dayOffDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_dateOnly = new DateOnly(2013, 1, 2);
			_dateOnlyBefore = new DateOnly(2013, 1, 1);
			_dateOnlyAfter = new DateOnly(2013, 1, 3);
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_anchor = new DateTime(2013, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc);
			_dayOff = new DayOff(_anchor, TimeSpan.FromHours(24), TimeSpan.FromHours(7),new Description("DayOff"), Color.Gray,"payrollColde", Guid.NewGuid());
			_dateOnlyAsDateTimePeriod1 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnlyAsDateTimePeriod2 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_flexCalculator = new DayOffMaxFlexCalculator(_workTimeStartEndExtractor);
		}

		[Test]
		public void ShouldReturnNullIfNullDayOffDay()
		{
			var result = _flexCalculator.MaxFlex(null, _scheduleDay);
			Assert.IsNull(result);	
		}

		[Test]
		public void ShouldReturnNullIfNoDayOff()
		{
			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldReturnNullIfNullScheduleDay()
		{
			var result = _flexCalculator.MaxFlex(_dayOffDay, null);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnNullIfCheckSameDay()
		{
			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly).Repeat.Twice();
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsNull(result);
			}
		}


		[Test]
		public void ShouldReturnDayOffMaxFlexedWhenNoProjectionOnDayBefore()
		{
			var expectedStartTime = _dayOff.Boundary.StartDateTime;
			var expectedEndTime = expectedStartTime.Add(_dayOff.TargetLength);

			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly);
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_dateOnlyBefore);
				Expect.Call(_dayOffDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.HasProjection()).Return(false);
				Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expectedStartTime, result.Value.StartDateTime);
				Assert.AreEqual(expectedEndTime, result.Value.EndDateTime);
			}
		}

		[Test]
		public void ShouldReturnDayOffMaxFlexedWhenNoProjectionOnDayAfter()
		{
			var expectedEndTime = _dayOff.Boundary.EndDateTime;
			var expectedStartTime = expectedEndTime.Add(-_dayOff.TargetLength);
			

			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly);
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_dateOnlyAfter);
				Expect.Call(_dayOffDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.HasProjection()).Return(false);
				Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expectedStartTime, result.Value.StartDateTime);
				Assert.AreEqual(expectedEndTime, result.Value.EndDateTime);
			}
		}

		[Test]
		public void ShouldReturnDayOffMaxFlexedWhenWorktimeOnDayBeforeNotInterfere()
		{
			var expectedStartTime = _dayOff.Boundary.StartDateTime;
			var expectedEndTime = expectedStartTime.Add(_dayOff.TargetLength);

			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly);
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_dateOnlyBefore);
				Expect.Call(_dayOffDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.HasProjection()).Return(true);
				Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(_visualLayerCollection)).Return(expectedStartTime.AddHours(-1));
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expectedStartTime, result.Value.StartDateTime);
				Assert.AreEqual(expectedEndTime, result.Value.EndDateTime);
			}	
		}

		[Test]
		public void ShouldReturnDayOffMaxFlexWhenWorktimeOnDayAfterNotInterfere()
		{
			var expectedEndTime = _dayOff.Boundary.EndDateTime;
			var expectedStartTime = expectedEndTime.Add(-_dayOff.TargetLength);

			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly);
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_dateOnlyAfter);
				Expect.Call(_dayOffDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.HasProjection()).Return(true);
				Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(_visualLayerCollection)).Return(expectedEndTime.AddHours(1));
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expectedStartTime, result.Value.StartDateTime);
				Assert.AreEqual(expectedEndTime, result.Value.EndDateTime);
			}
		}

		[Test]
		public void ShouldReturnDayOffAdjustedFlexWhenWorktimeOnDayBeforeInterfere()
		{
			var expectedStartTime = _dayOff.Boundary.StartDateTime.AddHours(3);
			var expectedEndTime = expectedStartTime.Add(_dayOff.TargetLength);

			using (_mock.Record())
			{
				Expect.Call(_dayOffDay.HasDayOff()).Return(true);
				Expect.Call(_dayOffDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2);
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_dateOnly);
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_dateOnlyBefore);
				Expect.Call(_dayOffDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.HasProjection()).Return(true);
				Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(_visualLayerCollection)).Return(expectedStartTime);
			}

			using (_mock.Playback())
			{
				var result = _flexCalculator.MaxFlex(_dayOffDay, _scheduleDay);
				Assert.IsTrue(result.HasValue);
				Assert.AreEqual(expectedStartTime, result.Value.StartDateTime);
				Assert.AreEqual(expectedEndTime, result.Value.EndDateTime);
			}
		}

	}
}
