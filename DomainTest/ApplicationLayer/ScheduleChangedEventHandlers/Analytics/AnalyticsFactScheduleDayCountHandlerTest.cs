using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactScheduleDayCountHandlerTest
	{
		private IAnalyticsFactScheduleDateHandler _dateHandler;
		private IAnalyticsFactScheduleTimeHandler _timeHandler;
		private AnalyticsFactScheduleDayCountHandler _target;
		private ProjectionChangedEventScheduleDay _scheduleDay;
		private AnalyticsFactSchedulePerson _personPart;
		private const int scenarioId = 10;
		private const int shiftCategoryId = 11;
		private const int dateId = 1;

		[SetUp]
		public void Setup()
		{
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_timeHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleTimeHandler>();
			_scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now.Date,
				Shift = new ProjectionChangedEventShift { StartDateTime = DateTime.Now }
			};
			_personPart = new AnalyticsFactSchedulePerson { PersonId = 1, BusinessUnitId = 2 };
			_target = new AnalyticsFactScheduleDayCountHandler(_dateHandler, _timeHandler);
		}

		[Test]
		public void ShouldMapLocalShiftStartId()
		{
			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, shiftCategoryId);
			result.ShiftStartDateLocalId.Should().Be.EqualTo(dateId);
		}

		[Test]
		public void ShouldReturnNullWhenDateMappingFails()
		{
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(false);
			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, shiftCategoryId);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapPersonAndBusienssUnitAndAScenarioId()
		{
			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);

			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, shiftCategoryId);

			result.PersonId.Should().Be.EqualTo(_personPart.PersonId);
			result.BusinessUnitId.Should().Be.EqualTo(_personPart.BusinessUnitId);
			result.ScenarioId.Should().Be.EqualTo(scenarioId);
		}

		[Test]
		public void ShouldMapShift()
		{
			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);

			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, shiftCategoryId);

			result.ShiftCategoryId.Should().Be.EqualTo(shiftCategoryId);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.Shift.StartDateTime);
			result.DayOffName.Should().Be.Null();
			result.DayOffShortName.Should().Be.Null();
			result.AbsenceId.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldReturnNullIfShiftButNoShiftCategory()
		{
			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, -1);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
			_scheduleDay.IsFullDayAbsence = true;
			var absence = new AnalyticsAbsence { AbsenceCode = Guid.NewGuid(), AbsenceId = 44 };
			_scheduleDay.Shift.Layers = new List<ProjectionChangedEventLayer>
			{
				new ProjectionChangedEventLayer
				{
					StartDateTime = DateTime.Now,
					IsAbsence = true,
					PayloadId = absence.AbsenceCode
				}
			};
			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			_timeHandler.Stub(x => x.MapAbsenceId(absence.AbsenceCode)).Return(absence);

			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, -1);

			result.AbsenceId.Should().Be.EqualTo(absence.AbsenceId);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.Shift.Layers.First().StartDateTime);
			result.ShiftCategoryId.Should().Be.EqualTo(-1);
			result.DayOffName.Should().Be.Null();
			result.DayOffShortName.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfAbsenceMappingFails()
		{
			_scheduleDay.IsFullDayAbsence = true;
			var invalidCode = Guid.NewGuid();
			_scheduleDay.Shift.Layers = new List<ProjectionChangedEventLayer>
			{
				new ProjectionChangedEventLayer
				{
					StartDateTime = DateTime.Now,
					IsAbsence = true,
					PayloadId = invalidCode
				}
			};
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_timeHandler.Stub(x => x.MapAbsenceId(invalidCode)).Return(null);

			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, -1);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapDayOff()
		{
			_scheduleDay.DayOff = new ProjectionChangedEventDayOff { Anchor = DateTime.Now };
			_scheduleDay.Name = "MyDayOff";
			_scheduleDay.ShortName = "DO";

			_dateHandler.Stub(x => x.MapDateId(Arg.Is(new DateOnly(_scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);

			IAnalyticsFactScheduleDayCount result = _target.Handle(_scheduleDay, _personPart, scenarioId, -1);

			result.DayOffName.Should().Be.EqualTo(_scheduleDay.Name);
			result.DayOffShortName.Should().Be.EqualTo(_scheduleDay.ShortName);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.DayOff.Anchor);
			result.ShiftCategoryId.Should().Be.EqualTo(-1);
			result.AbsenceId.Should().Be.EqualTo(-1);
		}
	}
}
