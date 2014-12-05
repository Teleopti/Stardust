using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactScheduleDateHandlerTest
	{
		private TestableNow _now;
		private IAnalyticsScheduleRepository _repository;
		private AnalyticsFactScheduleDateHandler _target;
		private List<KeyValuePair<DateOnly, int>> _dimDateList;
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
			_now = new TestableNow(new DateTime(2014, 12, 3, 9, 0, 0, DateTimeKind.Utc));
			_shiftStartDateUtc = new DateTime(2014, 12, 3, 23, 0, 0, DateTimeKind.Utc);
			_shiftEndDateUtc = new DateTime(2014, 12, 4, 3, 0, 0, DateTimeKind.Utc);
			_shiftStartDateLocal = new DateOnly(2014, 12, 4);
			_layer = new ProjectionChangedEventLayer
			{
				StartDateTime = _shiftStartDateUtc,
				EndDateTime = _shiftEndDateUtc,
			};
			_repository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_dimDateList = new List<KeyValuePair<DateOnly, int>>
			{
				new KeyValuePair<DateOnly, int>(new DateOnly(new DateOnly(2014, 12, 3)), firstDateId),
				new KeyValuePair<DateOnly, int>(new DateOnly(new DateOnly(2014, 12, 4)), secondDateId)
			};
			_repository.Stub(x => x.LoadDimDates(_now.UtcDateTime())).Return(_dimDateList);
			_target = new AnalyticsFactScheduleDateHandler(_repository, _now, minutesPerInterval);
		}

		[Test]
		public void ShouldMapShiftStartDateLocalToId()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ScheduleStartDateLocalId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapScheduleDateUtcToId()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ScheduleDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartLocalDateIdNotFound()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, new DateOnly(1974, 12, 27), _layer, _scheduleChangeTime);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldReturnNullWhenShiftStartUtcDateIdNotFound()
		{
			var analyticsFactScheduleDate = _target.Handle(new DateOnly(1974, 12, 27), _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
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
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldReturnNullWhenActivityEndDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc),
			};
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime);
			Assert.Null(analyticsFactScheduleDate);
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
			var analyticsFactScheduleDate = _target.Handle(invalidShiftStart, _shiftEndDateUtc, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldReturnNullWhenShiftEndDateIdNotFound()
		{
			var invalidLayer = new ProjectionChangedEventLayer
			{
				StartDateTime = new DateTime(2014, 12, 3, 0, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2014, 12, 4, 0, 0, 0, DateTimeKind.Utc),
			};
			var invalidShiftEnd = new DateTime(2014, 12, 5, 0, 0, 0, DateTimeKind.Utc);
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, invalidShiftEnd, _shiftStartDateLocal, invalidLayer, _scheduleChangeTime);
			Assert.Null(analyticsFactScheduleDate);
		}

		[Test]
		public void ShouldMapActivityStart()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ActivityStartTime.Should().Be.EqualTo(_layer.StartDateTime);
			analyticsFactScheduleDate.ActivityStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapActivityEnd()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ActivityEndTime.Should().Be.EqualTo(_layer.EndDateTime);
			analyticsFactScheduleDate.ActivityEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapShiftStart()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ShiftStartTime.Should().Be.EqualTo(_shiftStartDateUtc);
			analyticsFactScheduleDate.ShiftStartDateId.Should().Be.EqualTo(firstDateId);
		}

		[Test]
		public void ShouldMapShiftEnd()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ShiftEndTime.Should().Be.EqualTo(_shiftEndDateUtc);
			analyticsFactScheduleDate.ShiftEndDateId.Should().Be.EqualTo(secondDateId);
		}

		[Test]
		public void ShouldMapLayerStartToIntervalId()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.IntervalId.Should().Be.EqualTo(92);
		}

		[Test]
		public void ShouldMapShiftTimesToIntervalId()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.ShiftStartIntervalId.Should().Be.EqualTo(92);
			analyticsFactScheduleDate.ShiftEndIntervalId.Should().Be.EqualTo(11);
		}

		[Test]
		public void ShouldMapDatasourceUpdateDate()
		{
			var analyticsFactScheduleDate = _target.Handle(_shiftStartDateUtc, _shiftEndDateUtc, _shiftStartDateLocal, _layer, _scheduleChangeTime);
			analyticsFactScheduleDate.DatasourceUpdateDate.Should().Be.EqualTo(_scheduleChangeTime);
		}
	}
}