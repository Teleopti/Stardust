using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;


namespace Teleopti.Ccc.WebTest.Core.MonthSchedule.Mapping
{
	[TestFixture]
	public class MonthScheduleViewModelMappingTest
	{
		private IProjectionProvider _projectionProvider;
		private MonthScheduleViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			target = new MonthScheduleViewModelMapper(_projectionProvider,
				new PushMessageProvider(new FakeLoggedOnUser(), new FakePushMessageDialogueRepository()), new BankHolidayCalendarViewModelMapper());
		}

		[Test]
		public void ShouldMapDate()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };

			var result = target.Map(new MonthScheduleDomainData {Days = new [] { domainData }});

			result.ScheduleDays.First().Date.Should().Be.EqualTo(DateTime.Today);
		}

		[Test]
		public void ShouldMapFixedDate()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };

			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapDaysOfMonth()
		{
			var stub = new StubFactory();
			var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { domainData } };

			var result = target.Map(monthDomainData);

			result.ScheduleDays.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapCurrentDate()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
			var result = target.Map(monthDomainData);
			result.CurrentDate.Should().Be.EqualTo(DateTime.Today);
		}

		[Test]
		public void ShouldMapFixedCurrentDate()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
			var result = target.Map(monthDomainData);
			result.FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		[SetUICulture("de-DE")]
		[SetCulture("en-GB")]
		public void ShouldMapDayHeaderOfWeek()
		{
			var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };

			var result = target.Map(monthDomainData);

			result.DayHeaders.First().Name.Should().Be.EqualTo("Montag");
			result.DayHeaders.First().ShortName.Should().Be.EqualTo("Mo");
		}

		[Test]
		public void ShouldMapAbsenceName()
		{
			//arrange
			var stubs = new StubFactory();
			var personAbsence = new PersonAbsence(new Person(), new Scenario("s"), new AbsenceLayer(new Absence { Description = new Description("Illness") }, new DateTimePeriod()));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { new MonthScheduleDayDomainData { ScheduleDay = scheduleDay } }, CurrentDate = DateOnly.Today };

			//act
			var result = target.Map(monthDomainData);

			//assert
			result.ScheduleDays.Single().Absences[0].Name.Should().Be.EqualTo("Illness");
		}

		[Test]
		public void ShouldMapAbsenceShortName()
		{
			var stubs = new StubFactory();
			var scenario = new Scenario("s");
			var personAbsence0 = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description("a", "FI") }, new DateTimePeriod()));
			personAbsence0.Layer.Payload.Priority = 1;
			var personAbsence = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description("a", "IL") }, new DateTimePeriod()));
			personAbsence.Layer.Payload.Priority = 1;
			var personAbsence2 = new PersonAbsence(new Person(), scenario, new AbsenceLayer(new Absence { Description = new Description("a", "HO") }, new DateTimePeriod()));
			personAbsence2.Layer.Payload.Priority = 100;
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData { Days = new[] { new MonthScheduleDayDomainData { ScheduleDay = scheduleDay } }, CurrentDate = DateOnly.Today };

			var result = target.Map(monthDomainData);

			result.ScheduleDays.Single().Absences[0].ShortName.Should().Be.EqualTo("IL");
		}

		[Test]
		public void ShouldMapIsFullDayAbsence()
		{
			//arrange
			var stubs = new StubFactory();
			var personAbsence = new PersonAbsence(new Person(), new Scenario("s"),
			                                      new AbsenceLayer(new Absence {Description = new Description("Illness")},
			                                                       new DateTimePeriod()));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);
			var monthDomainData = new MonthScheduleDomainData
				{
					Days = new[] {new MonthScheduleDayDomainData {ScheduleDay = scheduleDay}},
					CurrentDate = DateOnly.Today
				};

			//act
			var result = target.Map(monthDomainData);

			//assert
			result.ScheduleDays.Single().Absences[0].IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsDayOff()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.DayOff, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, PersonAssignment = personAssignment, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay()};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsNotDayOffForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay };
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldMapIsDayOffForContractDayOff()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.ContractDayOff, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(), PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftCategoryForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity("a"), new DateTimePeriod(2011, 5, 18, 8, 2011, 5, 18, 9));
			personAssignment.SetShiftCategory(new ShiftCategory("Late"));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(), PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().Shift.Name.Should().Be.EqualTo("Late");
		}

		[Test]
		public void ShouldMapShiftCategoryShortNameForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity("a"), new DateTimePeriod(2011, 5, 18, 8, 2011, 5, 18, 9));
			personAssignment.SetShiftCategory(new ShiftCategory("sc") { Description = new Description("sc", "PM") });
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(), PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().Shift.ShortName.Should().Be.EqualTo("PM");
		}

		[Test]
		public void ShouldMapShiftColorForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity("a"), new DateTimePeriod(2011, 5, 18, 8, 2011, 5, 18, 9));
			personAssignment.SetShiftCategory(new ShiftCategory("sc") { DisplayColor = Color.Green });
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(), PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } });

			result.ScheduleDays.First().Shift.Color.Should().Be.EqualTo("rgb(0,128,0)");
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDay()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true }, new DateTimePeriod(2011, 5, 18, 7, 2011, 5, 18, 16));
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(),PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } }).ScheduleDays.First();

			result.Shift.Should().Not.Be.Null();
			result.Shift.TimeSpan.Should().Be.EqualTo(personAssignment.Period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapTimeSpanForWorkingDayExcludingPersonalActivity()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(),new Scenario("s"),new DateOnly(2011,5,18));
			var period = new DateTimePeriod(2011, 5, 18, 7, 2011, 5, 18, 16);
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },period);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			personAssignment.AddPersonalActivity(new Activity("b") { InWorkTime = true },period.MovePeriod(TimeSpan.FromHours(-2)));

			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay, SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(), PersonAssignment = personAssignment};
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } }).ScheduleDays.First();

			result.Shift.Should().Not.Be.Null();
			result.Shift.TimeSpan.Should().Be.EqualTo(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldMapWorkingHoursForWorkingDay()
		{
			var contractTime = TimeSpan.FromHours(8);
			var personAssignment = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly(2011, 5, 18));
			var scheduleDay = new StubFactory().ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);
			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.ContractTime()).Return(contractTime);
			_projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay };
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } }).ScheduleDays.First();

			result.Shift.WorkingHours.Should().Be(TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapShiftForEmptyDay()
		{
			PersonAssignment personAssignment = null;
			var scheduleDay = new StubFactory().ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);

			var domainData = new MonthScheduleDayDomainData { ScheduleDay = scheduleDay };
			var result = target.Map(new MonthScheduleDomainData { Days = new[] { domainData } }).ScheduleDays.First();
			result.Shift.Should().Not.Be.Null();
		}
	}
}