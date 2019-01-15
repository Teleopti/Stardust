using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactScheduleDateMapperTest
	{
		public AnalyticsFactScheduleDateMapper Target;
		public FakeAnalyticsDateRepository AnalyticsDates;

		private const int firstDateId = 1;
		private const int secondDateId = 2;
		private DateOnly _shiftStartDateLocal;
		private ProjectionChangedEventLayer _layer;
		private DateTime _shiftStartDateUtc;
		private DateTime _shiftEndDateUtc;
		private const int minutesPerInterval = 15;
		private DateTime _scheduleChangeTime;

		[SetUp]
		public void Setup()
		{
			_scheduleChangeTime = DateTime.Now;
			_shiftStartDateUtc = new DateTime(2014, 12, 3, 23, 0, 0, DateTimeKind.Utc);
			_shiftEndDateUtc = new DateTime(2014, 12, 4, 3, 0, 0, DateTimeKind.Utc);
			_shiftStartDateLocal = new DateOnly(2014, 12, 4);
			_layer = new ProjectionChangedEventLayer
			{
				StartDateTime = _shiftStartDateUtc,
				EndDateTime = _shiftEndDateUtc
			};
		}

		[Test]
		public void ShouldMapShiftStartDateLocalToId()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);

			analyticsFactScheduleDate.ScheduleStartDateLocalId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapScheduleDateUtcToId()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ScheduleDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartLocalDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, new DateOnly(1974, 12, 27), _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartUtcDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(new DateTime(1974, 12, 27), _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWhenActivityStartDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc)
			};
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.Should().Be.Null();
		}

		[Test]
		public void ShouldCreateDatesAndMapWhenActivityEndDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc)
			};
			var maxBefore = AnalyticsDates.MaxDate();

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);

			analyticsFactScheduleDate.Should().Not.Be.Null();
			AnalyticsDates.MaxDate().DateId.Should().Be.GreaterThan(maxBefore.DateId);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 4, 0, 0, 0, DateTimeKind.Utc)
			};
			var invalidShiftStart = new DateTime(2014, 12, 1, 0, 0, 0, DateTimeKind.Utc);
			var analyticsFactScheduleDate = Target.Map(invalidShiftStart, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.Should().Be.Null();
		}

		[Test]
		public void ShouldCreateDatesAndMapWhenShiftEndDateIdNotFound()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 4, 0, 0, 0, DateTimeKind.Utc)
			};
			var invalidShiftEnd = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc);
			var maxBefore = AnalyticsDates.MaxDate();

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, invalidShiftEnd, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);

			analyticsFactScheduleDate.Should().Not.Be.Null();
			AnalyticsDates.MaxDate().DateId.Should().Be.GreaterThan(maxBefore.DateId);
		}

		[Test]
		public void ShouldMapActivityStart()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ActivityStartTime.Should().Be.EqualTo(_layer.StartDateTime);
			analyticsFactScheduleDate.ActivityStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapActivityEnd()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ActivityEndTime.Should().Be.EqualTo(_layer.EndDateTime);
			analyticsFactScheduleDate.ActivityEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapShiftStart()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftStartTime.Should().Be.EqualTo(_shiftStartDateUtc);
			analyticsFactScheduleDate.ShiftStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapShiftEnd()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftEndTime.Should().Be.EqualTo(_shiftEndDateUtc);
			analyticsFactScheduleDate.ShiftEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapLayerStartToIntervalId()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.IntervalId.Should().Be.EqualTo(92);
		}

		[Test]
		public void ShouldMapShiftTimesToIntervalId()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftStartIntervalId.Should().Be.EqualTo(92);
			analyticsFactScheduleDate.ShiftEndIntervalId.Should().Be.EqualTo(11);
		}

		[Test]
		public void ShouldMapDatasourceUpdateDate()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.DatasourceUpdateDate.Should().Be.EqualTo(_scheduleChangeTime);
		}
	}
}