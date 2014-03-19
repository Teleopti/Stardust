using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.MonthSchedule.Mapping
{
	[TestFixture]
	public class MonthScheduleViewModelMappingTest
	{
		private IProjectionProvider _projectionProvider;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new MonthScheduleViewModelMappingProfile(_projectionProvider)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(domainData);

			result.Date.Should().Be.EqualTo(DateTime.Today);
		}

		[Test]
		public void ShouldMapFixedDate()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(domainData);

			result.FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapDaysOfMonth()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { domainData } };

			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

			result.ScheduleDays.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapCurrentDate()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
			result.CurrentDate.Should().Be.EqualTo(DateTime.Today);
		}

		[Test]
		public void ShouldMapFixedCurrentDate()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
			result.FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		[SetCulture("de-DE")]
		public void ShouldMapDayHeaderOfWeek()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };

			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

			result.DayHeaders.First().Name.Should().Be.EqualTo("Montag");
			result.DayHeaders.First().ShortName.Should().Be.EqualTo("Mo");
		}

		[Test]
		public void ShouldMapAbsenceName()
		{
			//arrange
			var stubs = new StubFactory();
			var personAbsence = new PersonAbsence(new Person(), new Scenario(" "), new AbsenceLayer(new Absence { Description = new Description("Illness") }, new DateTimePeriod()));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { new MonthScheduleDayDomainData { ScheduleDay = scheduleDay } }, CurrentDate = DateOnly.Today };

			//act
			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

			//assert
			result.ScheduleDays.Single().Absence.Name.Should().Be.EqualTo("Illness");
		}

		[Test]
		public void ShouldMapAbsenceShortName()
		{
			var stubs = new StubFactory();
			var scenario = new Scenario(" ");
			var personAbsence0 = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description(" ", "FI") }, new DateTimePeriod()));
			personAbsence0.Layer.Payload.Priority = 1;
			var personAbsence = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description(" ", "IL") }, new DateTimePeriod()));
			personAbsence.Layer.Payload.Priority = 1;
			var personAbsence2 = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description(" ", "HO") }, new DateTimePeriod()));
			personAbsence2.Layer.Payload.Priority = 100;
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { new MonthScheduleDayDomainData { ScheduleDay = scheduleDay } }, CurrentDate = DateOnly.Today };

			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

			result.ScheduleDays.Single().Absence.ShortName.Should().Be.EqualTo("IL");
		}

		[Test]
		public void ShouldMapIsFullDayAbsence()
		{
			//arrange
			var stubs = new StubFactory();
			var personAbsence = new PersonAbsence(new Person(), new Scenario(" "),
			                                      new AbsenceLayer(new Absence {Description = new Description("Illness")},
			                                                       new DateTimePeriod()));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData
				{
					Days = new[] {new MonthScheduleDayDomainData {ScheduleDay = scheduleDay}},
					CurrentDate = DateOnly.Today
				};

			//act
			var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

			//assert
			result.ScheduleDays.Single().Absence.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsDayOff()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.DayOff, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsNotDayOffForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldMapIsDayOffForContractDayOff()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.ContractDayOff, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftCategoryForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity(" "), new DateTimePeriod(2011, 5, 18, 2011, 5, 18));
			personAssignment.SetShiftCategory(new ShiftCategory("Late"));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.Shift.Name.Should().Be.EqualTo("Late");
		}

		[Test]
		public void ShouldMapShiftCategoryShortNameForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity(" "), new DateTimePeriod(2011, 5, 18, 2011, 5, 18));
			personAssignment.SetShiftCategory(new ShiftCategory(" ") { Description = new Description(" ", "PM") });
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.Shift.ShortName.Should().Be.EqualTo("PM");
		}

		[Test]
		public void ShouldMapShiftColorForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity(" "), new DateTimePeriod(2011, 5, 18, 2011, 5, 18));
			personAssignment.SetShiftCategory(new ShiftCategory(" ") { DisplayColor = Color.Green });
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.Shift.Color.Should().Be.EqualTo("rgb(0,128,0)");
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity(" ") { InWorkTime = true }, new DateTimePeriod(2011, 5, 18, 7, 2011, 5, 18, 16));
			personAssignment.SetShiftCategory(new ShiftCategory(" "));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.Shift.Should().Not.Be.Null();
			result.Shift.TimeSpan.Should().Be.EqualTo(personAssignment.Period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapWorkingHoursForWorkingDay()
		{
			var contractTime = TimeSpan.FromHours(8);
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "), new DateOnly(2011, 5, 18));
			var scheduleDay = new StubFactory().ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

			result.Shift.WorkingHours.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapShiftForEmptyDay()
		{
			PersonAssignment personAssignment = null;
			var scheduleDay = new StubFactory().ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });
			result.Shift.Should().Not.Be.Null();
		}
	}
}