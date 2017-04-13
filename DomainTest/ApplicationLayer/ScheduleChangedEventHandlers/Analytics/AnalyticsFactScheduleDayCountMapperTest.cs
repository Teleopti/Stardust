using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactScheduleDayCountMapperTest
	{
		public IAnalyticsFactScheduleDayCountMapper Target;
		public FakeAnalyticsAbsenceRepository AnalyticsAbsences;
		public FakeAnalyticsDateRepository AnalyticsDates;

		private ProjectionChangedEventScheduleDay _scheduleDay;
		private AnalyticsFactSchedulePerson _personPart;
		private const int scenarioId = 10;
		private const int shiftCategoryId = 11;
		private const int dateId = 1;

		[SetUp]
		public void Setup()
		{
			_scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = new DateTime(2014, 12, 3),
				Shift = new ProjectionChangedEventShift { StartDateTime = new DateTime(2014, 12, 3, 8, 0, 0) }
			};
			_personPart = new AnalyticsFactSchedulePerson { PersonId = 1, BusinessUnitId = 2 };
		}

		[Test]
		public void ShouldReturnNullWhenDateMappingFails()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			_scheduleDay.Date = new DateTime(2014, 12, 01);

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, shiftCategoryId);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMap()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, shiftCategoryId);

			result.ShiftStartDateLocalId.Should().Be.EqualTo(dateId);

			result.PersonId.Should().Be.EqualTo(_personPart.PersonId);
			result.BusinessUnitId.Should().Be.EqualTo(_personPart.BusinessUnitId);
			result.ScenarioId.Should().Be.EqualTo(scenarioId);

			result.ShiftCategoryId.Should().Be.EqualTo(shiftCategoryId);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.Shift.StartDateTime);
			result.DayOffName.Should().Be.Null();
			result.DayOffShortName.Should().Be.Null();
			result.AbsenceId.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldReturnNullIfShiftButNoShiftCategory()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, -1);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);
			_scheduleDay.IsFullDayAbsence = true;
			var absence = new AnalyticsAbsence { AbsenceCode = Guid.NewGuid(), AbsenceId = 44 };
			_scheduleDay.Shift.Layers = new List<ProjectionChangedEventLayer>
			{
				new ProjectionChangedEventLayer
				{
					StartDateTime = new DateTime(2014, 12, 3, 8, 0, 0),
					IsAbsence = true,
					PayloadId = absence.AbsenceCode
				}
			};
			AnalyticsAbsences.AddAbsence(absence);

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, -1);

			result.AbsenceId.Should().Be.EqualTo(absence.AbsenceId);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.Shift.Layers.First().StartDateTime);
			result.ShiftCategoryId.Should().Be.EqualTo(-1);
			result.DayOffName.Should().Be.Null();
			result.DayOffShortName.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfAbsenceMappingFails()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);
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

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, -1);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapDayOff()
		{
			AnalyticsDates.HasDatesBetween(new DateTime(2014, 12, 3), new DateTime(2014, 12, 4), 1);
			_scheduleDay.DayOff = new ProjectionChangedEventDayOff { Anchor = DateTime.Now };
			_scheduleDay.Name = "MyDayOff";
			_scheduleDay.ShortName = "DO";

			var result = Target.Map(_scheduleDay, _personPart, scenarioId, -1);

			result.DayOffName.Should().Be.EqualTo(_scheduleDay.Name);
			result.DayOffShortName.Should().Be.EqualTo(_scheduleDay.ShortName);
			result.StartTime.Should().Be.EqualTo(_scheduleDay.DayOff.Anchor);
			result.ShiftCategoryId.Should().Be.EqualTo(-1);
			result.AbsenceId.Should().Be.EqualTo(-1);
		}
	}
}
