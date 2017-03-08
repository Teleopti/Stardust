using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactScheduleDateMapperTest : ISetup
	{
		public IAnalyticsFactScheduleDateMapper Target;
		public FakeAnalyticsDateRepository AnalyticsDates;

		private const int firstDateId = 1;
		private const int secondDateId = 2;
		private DateOnly _shiftStartDateLocal;
		private ProjectionChangedEventLayer _layer;
		private DateTime _shiftStartDateUtc;
		private DateTime _shiftEndDateUtc;
		private const int minutesPerInterval = 15;
		private DateTime _scheduleChangeTime;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService(new FakeAnalyticsDateRepository(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1));
		}

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
				EndDateTime = _shiftEndDateUtc,
			};
		}

		[Test]
		public void ShouldMapShiftStartDateLocalToId()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ScheduleStartDateLocalId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapScheduleDateUtcToId()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ScheduleDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartLocalDateIdNotFound()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, new DateOnly(1974, 12, 27), _layer, _scheduleChangeTime, minutesPerInterval);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartUtcDateIdNotFound()
		{
			var analyticsFactScheduleDate = Target.Map(new DateTime(1974, 12, 27), _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldReturnNullWhenActivityStartDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
			};
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldCreateDatesAndMapWhenActivityEndDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc),
			};
			var maxBefore = AnalyticsDates.MaxDate();

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);

			Assert.NotNull(analyticsFactScheduleDate);
			AnalyticsDates.MaxDate().DateId.Should().Be.GreaterThan(maxBefore.DateId);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 4, 0, 0, 0, DateTimeKind.Utc),
			};
			var invalidShiftStart = new DateTime(2014, 12, 1, 0, 0, 0, DateTimeKind.Utc);
			var analyticsFactScheduleDate = Target.Map(invalidShiftStart, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldCreateDatesAndMapWhenShiftEndDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 4, 0, 0, 0, DateTimeKind.Utc),
			};
			var invalidShiftEnd = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc);
			var maxBefore = AnalyticsDates.MaxDate();

			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, invalidShiftEnd, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime, minutesPerInterval);

			Assert.NotNull(analyticsFactScheduleDate);
			AnalyticsDates.MaxDate().DateId.Should().Be.GreaterThan(maxBefore.DateId);
		}

		[Test]
		public void ShouldMapActivityStart()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ActivityStartTime.Should().Be.EqualTo(_layer.StartDateTime);
			analyticsFactScheduleDate.ActivityStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapActivityEnd()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ActivityEndTime.Should().Be.EqualTo(_layer.EndDateTime);
			analyticsFactScheduleDate.ActivityEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapShiftStart()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftStartTime.Should().Be.EqualTo(_shiftStartDateUtc);
			analyticsFactScheduleDate.ShiftStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapShiftEnd()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftEndTime.Should().Be.EqualTo(_shiftEndDateUtc);
			analyticsFactScheduleDate.ShiftEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapLayerStartToIntervalId()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.IntervalId.Should().Be.EqualTo(92);
		}

		[Test]
		public void ShouldMapShiftTimesToIntervalId()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.ShiftStartIntervalId.Should().Be.EqualTo(92);
			analyticsFactScheduleDate.ShiftEndIntervalId.Should().Be.EqualTo(11);
		}

		[Test]
		public void ShouldMapDatasourceUpdateDate()
		{
			var analyticsFactScheduleDate = Target.Map(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime, minutesPerInterval);
			analyticsFactScheduleDate.DatasourceUpdateDate.Should().Be.EqualTo(_scheduleChangeTime);
		}
	}
}