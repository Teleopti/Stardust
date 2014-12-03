using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class CalculateBestOvertimeBeforeOrAfterTest
	{
		private CalculateBestOvertimeBeforeOrAfter _target;
		private List<OvertimePeriodValue> _mappedData;
		private DateTimePeriod _periodBefore1;
		private DateTimePeriod _periodBefore2;
		private DateTimePeriod _periodBefore3;
		private DateTimePeriod _periodBefore4;
		private DateTimePeriod _periodBefore5;
		private DateTimePeriod _periodBefore6;
		private DateTimePeriod _periodAfter1;
		private DateTimePeriod _periodAfter2;
		private DateTimePeriod _periodAfter3;
		private DateTimePeriod _periodAfter4;
		private DateTimePeriod _periodAfter5;
		private DateTimePeriod _periodAfter6;
		private IScheduleDay _scheduleDay;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private DateTimePeriod _dateTimePeriod;
		private MockRepository _mock;
		private DateTime _shiftEndingTime;
		private DateTime _shiftStartTime;
		private IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_analyzePersonAccordingToAvailability = _mock.StrictMock<IAnalyzePersonAccordingToAvailability>();
			_target = new CalculateBestOvertimeBeforeOrAfter(_analyzePersonAccordingToAvailability);

			_periodBefore1 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 15, 0, DateTimeKind.Utc));
			_periodBefore2 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc));
			_periodBefore3 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 14, 45, 0, DateTimeKind.Utc));
			_periodBefore4 = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 00, 0, DateTimeKind.Utc));
			_periodBefore5 = new DateTimePeriod(new DateTime(2014, 02, 26, 15, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 15, 0, DateTimeKind.Utc));
			_periodBefore6 = new DateTimePeriod(new DateTime(2014, 02, 26, 15, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc));


			_periodAfter1 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc));
			_periodAfter2 = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));
			_periodAfter3 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
			_periodAfter4 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));
			_periodAfter5 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc));
			_periodAfter6 = new DateTimePeriod(new DateTime(2014, 02, 26, 17, 45, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 18, 00, 0, DateTimeKind.Utc));

			_mappedData = new List<OvertimePeriodValue>();

			_mappedData.Add(new OvertimePeriodValue(_periodBefore1, -25.0));
			_mappedData.Add(new OvertimePeriodValue(_periodBefore2, -20.0));
			_mappedData.Add(new OvertimePeriodValue(_periodBefore3, -7.0));
			_mappedData.Add(new OvertimePeriodValue(_periodBefore4, -6.0));
			_mappedData.Add(new OvertimePeriodValue(_periodBefore5, 3.0));
			_mappedData.Add(new OvertimePeriodValue(_periodBefore6, 2.0));

			_mappedData.Add(new OvertimePeriodValue(_periodAfter1, 1.0));
			_mappedData.Add(new OvertimePeriodValue(_periodAfter2, -2.0));
			_mappedData.Add(new OvertimePeriodValue(_periodAfter3, -7.0));
			_mappedData.Add(new OvertimePeriodValue(_periodAfter4, -8.0));
			_mappedData.Add(new OvertimePeriodValue(_periodAfter5, -9.0));
			_mappedData.Add(new OvertimePeriodValue(_periodAfter6, -10.0));

			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_shiftStartTime = new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc);
			_shiftEndingTime = new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
		}

		[Test]
		public void ShouldGet1HourPeriodAfter()
		{
			var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
			var expected = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 30, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(expected, result.First());
			}
		}

		[Test]
		public void ShouldGet1Hour30MinPeriodBefore()
		{
			var oneHourTimeSpan = new TimeSpan(0, 1, 30, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
			var expected = new DateTimePeriod(new DateTime(2014, 02, 26, 14, 00, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 15, 30, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(expected, result.First());
			}
		}

		[Test]
		public void ShouldGetBestWhenOvertimeDurationIsFlexible()
		{
			var oneIntervalTimeSpan = new TimeSpan(0, 0, 15, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneIntervalTimeSpan, oneIntervalTimeSpan.Add(TimeSpan.FromMinutes(15)));
			var expected = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 00, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(expected, result.First());
			}
		}

		[Test]
		public void ShouldOnlyConsiderResultvaluesBelowZero()
		{
			var oneIntervalTimeSpan = new TimeSpan(0, 0, 15, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneIntervalTimeSpan, oneIntervalTimeSpan);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldSkipZeroLengthDurations()
		{
			var overtimeDuration = new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(0, result.Count);
			}	
		}

		[Test]
		public void ShouldSkipWhenNoSkillData()
		{
			var oneHourTimeSpan = new TimeSpan(0, 1, 30, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
			_mappedData.Clear();

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, false);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldAdjustPeriodToOvertimeAvailability()
		{
			var oneHourTimeSpan = new TimeSpan(0, 1, 0, 0);
			var overtimeDuration = new MinMax<TimeSpan>(oneHourTimeSpan, oneHourTimeSpan);
			var expected = new DateTimePeriod(new DateTime(2014, 02, 26, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 02, 26, 17, 15, 0, DateTimeKind.Utc));
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			var dateOnly = new DateOnly(_shiftEndingTime);
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			var dateTimePeriod = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(45));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);
				Expect.Call(_scheduleDay.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_analyzePersonAccordingToAvailability.AdustOvertimeAvailability(_scheduleDay, dateOnly, person.PermissionInformation.DefaultTimeZone(),new List<DateTimePeriod>())).Return(new List<DateTimePeriod> { dateTimePeriod }).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.GetBestOvertime(overtimeDuration, _mappedData, _scheduleDay, 15, true);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(expected, result.First());
			}	
		}
	}
}
