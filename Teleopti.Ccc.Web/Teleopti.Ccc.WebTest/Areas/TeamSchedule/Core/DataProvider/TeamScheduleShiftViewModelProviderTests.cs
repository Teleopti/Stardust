using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.WebTest.Areas.Global;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleShiftViewModelFactoryTests : IIsolateSystem
	{
		public TeamScheduleShiftViewModelProvider Target;
		private readonly Scenario scenario = new Scenario("d");
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserTimeZone UserTimeZone;
		public IScheduleStorage ScheduleStorage;
		public FakeMeetingRepository MeetingRepository;
		private ICommonNameDescriptionSetting commonNameDescriptionSetting;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeIanaTimeZoneProvider>().For<IIanaTimeZoneProvider>();
			isolate.UseTestDouble(new FakeCommonAgentNameProvider("{FirstName}{LastName}")).For<ICommonAgentNameProvider>();
			commonNameDescriptionSetting = new FakeCommonAgentNameProvider("{FirstName}{LastName}").CommonAgentNameSettings;
		}

		[Test]
		public void ShouldMakeViewModelForAgent()
		{
			var date = new DateOnly(2015, 01, 01);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("mds", MultiplicatorType.Overtime).WithId();
			contract.AddMultiplicatorDefinitionSetCollection(mds);

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);

			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);

			var canViewConfidential = false;
			var canViewUnpublished = false;

			var viewModel = Target.MakeViewModel(person, date, scheduleDay, null, commonNameDescriptionSetting, canViewConfidential, canViewUnpublished);

			viewModel.PersonId.Should().Be.EqualTo(person.Id.Value.ToString());
			viewModel.Name.Should().Be.EqualTo("billgates");
			viewModel.Date.Should().Be.EqualTo(date.Date.ToGregorianDateTimeString().Remove(10));
			viewModel.Projection.Count().Should().Be(0);
			viewModel.MultiplicatorDefinitionSetIds.Should().Be.Equals(mds.Id.Value);
			viewModel.InternalNotes.Should().Be.NullOrEmpty();
			viewModel.Timezone.IanaId.Should().Be(timezoneChina.Id);
			viewModel.Timezone.DisplayName.Should().Be(timezoneChina.DisplayName);
		}

		[Test]
		public void ShouldMakeViewModelWithIsProtected()
		{
			var date = new DateOnly(2018, 05, 15);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("mds", MultiplicatorType.Overtime).WithId();
			contract.AddMultiplicatorDefinitionSetCollection(mds);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.SetWriteProtection, LoggedOnUser.CurrentUser(), date);

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);
			person.PersonWriteProtection.PersonWriteProtectedDate = date;
			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);
			scheduleDay.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity("activity"), new DateTimePeriod(2018, 05, 15, 10, 2018, 05, 15, 11));

			var person2 = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person2.AddPersonPeriod(personPeriod);
			person2.PermissionInformation.SetDefaultTimeZone(timezoneChina);
			var scheduleDayForPerson2 = ScheduleDayFactory.Create(date, person2, scenario);
			scheduleDayForPerson2.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity("activity"), new DateTimePeriod(2018, 05, 15, 10, 2018, 05, 15, 11));


			var canViewConfidential = false;
			var canViewUnpublished = false;

			var viewModel = Target.MakeViewModel(person, date, scheduleDay, null, commonNameDescriptionSetting, canViewConfidential, canViewUnpublished);

			var viewModelForPerson2 = Target.MakeViewModel(person2, date, scheduleDayForPerson2, null, commonNameDescriptionSetting, canViewConfidential, canViewUnpublished);

			viewModel.IsProtected.Should().Be(true);
			viewModelForPerson2.IsProtected.Should().Be(false);
		}

		[Test]
		public void ShouldNotReturnMultiplicatorDefinitionSetIdsInViewModelWhenAgentHasNoPersonPeriod()
		{
			var date = new DateOnly(2015, 01, 01);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);

			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);

			var canViewConfidential = false;
			var canViewUnpublished = false;

			var viewModel = Target.MakeViewModel(person, date, scheduleDay, null, commonNameDescriptionSetting, canViewConfidential, canViewUnpublished);

			viewModel.MultiplicatorDefinitionSetIds.Should().Be.Null();
		}

		[Test]
		public void ShouldNotReturnMultiplicatorDefinitionSetIdsInViewModelWhenAgentsContractHasNoOvertimeMultiplicator()
		{
			var date = new DateOnly(2015, 01, 01);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);

			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);

			var canViewConfidential = false;
			var canViewUnpublished = false;

			var viewModel = Target.MakeViewModel(person, date, scheduleDay, null, commonNameDescriptionSetting, canViewConfidential, canViewUnpublished);
			viewModel.MultiplicatorDefinitionSetIds.Should().Be.Empty();
		}

		[Test]
		public void ShouldFetchPublicNote()
		{
			var date = new DateOnly(2015, 01, 01);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);

			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);
			var note = new PublicNote(person, date, scenario, "Oh my God");
			scheduleDay.Add(note);

			var viewModel = Target.MakeViewModel(person, date, scheduleDay, null, commonNameDescriptionSetting, false, false, true);

			viewModel.PublicNotes.Should().Be.EqualTo("Oh my God");
		}

		[Test]
		public void ShouldGetProjection()
		{
			shouldGetProjection();
		}

		private void shouldGetProjection()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(17));
			var lunchActivityPeriod = new DateTimePeriod(date.AddHours(11), date.AddHours(12));
			var absencePeriod = new DateTimePeriod(date.AddHours(12), date.AddHours(13));
			var shortBreakActivityPeriod = new DateTimePeriod(date.AddHours(13), date.AddHours(13).AddMinutes(15));
			var personalActivityPeriod = new DateTimePeriod(date.AddHours(14), date.AddHours(15));
			var overtimePeriod = new DateTimePeriod(date.AddHours(15), date.AddHours(16));
			var meetingPeriod = new DateTimePeriod(date.AddHours(16), date.AddHours(17));

			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var lunchActivity = ActivityFactory.CreateActivity("Lunch", Color.Red);
			var testAbsence = AbsenceFactory.CreateAbsence("test");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short break", Color.Red).WithId();
			var personalActivity = ActivityFactory.CreateActivity("personal activity", Color.Blue);
			var overtimeActivity = ActivityFactory.CreateActivity("overtime activity", Color.Brown);
			var meetingActivity = ActivityFactory.CreateActivity("meeting activity", Color.AntiqueWhite);

			phoneActivity.InContractTime = true;
			phoneActivity.InWorkTime = true;

			lunchActivity.InContractTime = true;
			lunchActivity.InWorkTime = false;
			lunchActivity.ReportLevelDetail = ReportLevelDetail.Lunch;

			shortBreakActivity.InContractTime = true;
			shortBreakActivity.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			shortBreakActivity.InWorkTime = false;

			personalActivity.InContractTime = true;
			personalActivity.InWorkTime = true;

			overtimeActivity.InContractTime = true;
			overtimeActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddActivity(lunchActivity, lunchActivityPeriod);
			assignment1Person1.AddActivity(shortBreakActivity, shortBreakActivityPeriod);
			assignment1Person1.AddPersonalActivity(personalActivity, personalActivityPeriod);
			assignment1Person1.AddOvertimeActivity(overtimeActivity,
				overtimePeriod,
				MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime paid", MultiplicatorType.Overtime));

			scheduleDayOnePerson1.Add(assignment1Person1);

			var absenceLayer = new AbsenceLayer(testAbsence, absencePeriod);
			var personAbsence = scheduleDayOnePerson1.CreateAndAddAbsence(absenceLayer);
			personAbsence.SetId(Guid.NewGuid());

			var meeting = new Meeting(person1, new[] { new MeetingPerson(person1, false) }, "subj", "loc",
				"desc", meetingActivity, scenario);
			meeting.StartDate = meeting.EndDate = new DateOnly(date);
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);
			MeetingRepository.Has(meeting);

			PersonMeeting personMeeting = new PersonMeeting(meeting, new MeetingPerson(person1, false), meetingPeriod);

			scheduleDayOnePerson1.Add(personMeeting);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.Date.Should().Be.EqualTo("2015-01-01");

			vm.DayOff.Should().Be(null);
			vm.Name.Should().Be("agent1");
			vm.Projection.Count().Should().Be(8);
			vm.IsFullDayAbsence.Should().Be(false);
			vm.Date.Should().Be(date.ToFixedDateFormat());

			vm.Projection.ElementAt(0).ActivityId.Should().Be.EqualTo(phoneActivity.Id);
			vm.Projection.ElementAt(0).Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.ElementAt(0).FloatOnTop.Should().Be.False();
			vm.Projection.ElementAt(0).StartInUtc.Should().Be.EqualTo("2015-01-01 08:00");
			vm.Projection.ElementAt(0).EndInUtc.Should().Be.EqualTo("2015-01-01 11:00");

			vm.Projection.ElementAt(1).ActivityId.Should().Be.EqualTo(lunchActivity.Id);
			vm.Projection.ElementAt(1).Description.Should().Be(lunchActivity.Description.Name);
			vm.Projection.ElementAt(1).FloatOnTop.Should().Be.True();
			vm.Projection.ElementAt(1).StartInUtc.Should().Be.EqualTo("2015-01-01 11:00");
			vm.Projection.ElementAt(1).EndInUtc.Should().Be.EqualTo("2015-01-01 12:00");

			var personAbsenceProjection = vm.Projection.ElementAt(2);
			personAbsenceProjection.ParentPersonAbsences.First().Should().Be(personAbsence.Id);
			personAbsenceProjection.Description.Should().Be(testAbsence.Name);
			personAbsenceProjection.FloatOnTop.Should().Be(true);
			personAbsenceProjection.StartInUtc.Should().Be.EqualTo("2015-01-01 12:00");
			personAbsenceProjection.EndInUtc.Should().Be.EqualTo("2015-01-01 13:00");

			vm.Projection.ElementAt(3).ActivityId.Should().Be.EqualTo(shortBreakActivity.Id);
			vm.Projection.ElementAt(3).Description.Should().Be(shortBreakActivity.Description.Name);
			vm.Projection.ElementAt(3).FloatOnTop.Should().Be.True();
			vm.Projection.ElementAt(3).StartInUtc.Should().Be.EqualTo("2015-01-01 13:00");
			vm.Projection.ElementAt(3).EndInUtc.Should().Be.EqualTo("2015-01-01 13:15");

			vm.Projection.ElementAt(4).ActivityId.Should().Be.EqualTo(phoneActivity.Id);
			vm.Projection.ElementAt(4).Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.ElementAt(4).FloatOnTop.Should().Be.False();
			vm.Projection.ElementAt(4).StartInUtc.Should().Be.EqualTo("2015-01-01 13:15");
			vm.Projection.ElementAt(4).EndInUtc.Should().Be.EqualTo("2015-01-01 14:00");

			vm.Projection.ElementAt(5).ActivityId.Should().Be.EqualTo(personalActivity.Id);
			vm.Projection.ElementAt(5).Description.Should().Be(personalActivity.Description.Name);
			vm.Projection.ElementAt(5).IsPersonalActivity.Should().Be(true);
			vm.Projection.ElementAt(5).FloatOnTop.Should().Be(true);
			vm.Projection.ElementAt(5).StartInUtc.Should().Be.EqualTo("2015-01-01 14:00");
			vm.Projection.ElementAt(5).EndInUtc.Should().Be.EqualTo("2015-01-01 15:00");

			vm.Projection.ElementAt(6).ActivityId.Should().Be(overtimeActivity.Id);
			vm.Projection.ElementAt(6).Description.Should().Be(overtimeActivity.Description.Name);
			vm.Projection.ElementAt(6).IsOvertime.Should().Be(true);
			vm.Projection.ElementAt(6).FloatOnTop.Should().Be(true);
			vm.Projection.ElementAt(6).StartInUtc.Should().Be.EqualTo("2015-01-01 15:00");
			vm.Projection.ElementAt(6).EndInUtc.Should().Be.EqualTo("2015-01-01 16:00");

			vm.Projection.ElementAt(7).IsMeeting.Should().Be(true);
			vm.Projection.ElementAt(7).FloatOnTop.Should().Be(true);

			vm.ContractTimeMinutes.Should().Be(420);
			vm.WorkTimeMinutes.Should().Be(345);
		}

		[Test, SetUICulture("fa-IR")]
		public void ShouldGetProjectionWithStartAndEndInServiceDateFormat()
		{
			shouldGetProjection();
		}

		[Test]
		public void ShouldGetProjectionWithEndInDST()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.GmtTimeZoneInfo());

			var date = new DateTime(2018, 10, 28, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddMinutes(35), date.AddHours(1));

			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);

			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.ElementAt(0).End.Should().Be.EqualTo("2018-10-28 01:00");
		}

		[Test]
		public void ShouldGetProjectionWithTopShiftLayerIdWhenAnActivityOverlapsAnotherSameTypeActivity()
		{
			var date = new DateTime(2018, 06, 21, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);

			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));
			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(8), date.AddHours(16)));

			assignment1Person1.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.Single().TopShiftLayerId.Should().Be.EqualTo(assignment1Person1.ShiftLayers.Second().Id.Value);
		}

		[Test]
		public void ShouldGetProjectionWithTopShiftLayerIdWhenAnTopLayerNotSplitByAnotherActivity()
		{
			var date = new DateTime(2018, 06, 21, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var emailActivity = ActivityFactory.CreateActivity("Email", Color.Blue);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);

			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(8), date.AddHours(11)));
			assignment1Person1.AddActivity(emailActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));
			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(10), date.AddHours(11)));

			assignment1Person1.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.Last().TopShiftLayerId.Should().Be.EqualTo(assignment1Person1.ShiftLayers.Last().Id.Value);

		}


		[Test]
		public void ShouldGetProjectionWithTopShiftLayerIdWhenAnTopLayerIntersectAnotherSameTypePersonalActivity()
		{

			var date = new DateTime(2018, 07, 31, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var emailActivity = ActivityFactory.CreateActivity("Email", Color.Blue);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);

			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(8), date.AddHours(12)));
			assignment1Person1.AddActivity(emailActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));
			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(10), date.AddHours(12)));
			assignment1Person1.AddPersonalActivity(phoneActivity, new DateTimePeriod(date.AddHours(11), date.AddHours(13)));

			assignment1Person1.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.Third().TopShiftLayerId.Should().Be.EqualTo(assignment1Person1.ShiftLayers.Third().Id.Value);
		}

		[Test]
		public void ShouldGetProjectionWithoutTopShiftLayerIdWhenTopLayerSplitByAnotherActivity()
		{
			var date = new DateTime(2018, 06, 21, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var emailActivity = ActivityFactory.CreateActivity("Email", Color.Blue);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);

			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(7), date.AddHours(13)));
			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(14)));
			assignment1Person1.AddActivity(emailActivity, new DateTimePeriod(date.AddHours(10), date.AddHours(11)));

			assignment1Person1.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.Last().TopShiftLayerId.Should().Be.EqualTo(null);

		}

		[Test]
		public void ShouldGetProjectionWithoutTopShiftLayerIdWhenAnActivityIntersectAnotherActivity()
		{
			var date = new DateTime(2018, 06, 21, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);

			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(8), date.AddHours(10)));
			assignment1Person1.AddActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(11)));

			assignment1Person1.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);
			vm.Projection.Single().TopShiftLayerId.Should().Be(null);
		}

		[Test]
		public void ShouldGetProjectionWithTopShiftLayerIdWhenAnPersonalActivityOverlapsAnotherSameTypePersonalActivity()
		{
			var date = new DateTime(2018, 06, 22, 0, 0, 0, DateTimeKind.Utc);
			var emailActivity = ActivityFactory.CreateActivity("E-mail", Color.Yellow);
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var person = PersonFactory.CreatePersonWithGuid("Stock", "Holm");

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 6, 22), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			pa.AddActivity(emailActivity, new DateTimePeriod(date.AddHours(7), date.AddHours(17)));
			pa.AddPersonalActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));
			pa.AddPersonalActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));

			pa.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDay.Add(pa);

			var result = Target.Projection(scheduleDay, commonNameDescriptionSetting, true);

			result.Projection.Second().TopShiftLayerId.Should().Be.EqualTo(pa.ShiftLayers.Last().Id);
		}

		[Test]
		public void ShouldGetProjectionWithoutTopShiftLayerIdWhenAnPersonalActivityIntersectAnotherSameTypePersonalActivity()
		{
			var date = new DateTime(2018, 06, 22, 0, 0, 0, DateTimeKind.Utc);
			var emailActivity = ActivityFactory.CreateActivity("E-mail", Color.Yellow);
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var person = PersonFactory.CreatePersonWithGuid("Stock", "Holm");

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 6, 22), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			pa.AddActivity(emailActivity, new DateTimePeriod(date.AddHours(7), date.AddHours(17)));
			pa.AddPersonalActivity(phoneActivity, new DateTimePeriod(date.AddHours(9), date.AddHours(10)));
			pa.AddPersonalActivity(phoneActivity, new DateTimePeriod(date.AddHours(10), date.AddHours(11)));

			pa.ShiftLayers.ForEach(sl =>
			{
				sl.WithId();
			});
			scheduleDay.Add(pa);

			var result = Target.Projection(scheduleDay, commonNameDescriptionSetting, true);

			result.Projection.Second().TopShiftLayerId.Should().Be(null);
		}

		[Test]
		public void ShouldProjectWithOverTime()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var overTimeActivityPeriod = new DateTimePeriod(date.AddHours(6), date.AddHours(8));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var overTimeActivity = ActivityFactory.CreateActivity("Lunch", Color.Red);

			phoneActivity.InContractTime = true;
			overTimeActivity.InContractTime = true;
			overTimeActivity.InWorkTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod, def);
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(2);
			vm.Projection.First().Description.Should().Be(overTimeActivity.Description.Name);
			vm.Projection.Last().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.First().IsOvertime.Should().Be(true);
			vm.Projection.Last().IsOvertime.Should().Be(false);
		}

		[Test]
		public void ShouldOvertimeActivityContainsOnlyOverTimeShiftLayerIdWhenIntersectingWithAnotherSameTypeNormalActivity()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var overTimePhoneActivityPeriod = new DateTimePeriod(date.AddHours(14), date.AddHours(16));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			phoneActivity.InContractTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddOvertimeActivity(phoneActivity, overTimePhoneActivityPeriod, def);
			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(2);
			vm.Projection.First().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.Last().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.First().IsOvertime.Should().Be(false);
			vm.Projection.Last().IsOvertime.Should().Be(true);
			vm.Projection.Last().ShiftLayerIds.Length.Should().Be(1);
			vm.Projection.Last().ShiftLayerIds.First().Should().Be(assignment1Person1.ShiftLayers.Last().Id);
		}

		[Test]
		public void ShouldProjectionStillBeOvertimeWhenAddingASecondSameTypeOvertimeActivityWithAnOvertimeActivityNeighboringOutsideShift()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(date), person, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			var overTimeActivityPeriod = new DateTimePeriod(date.AddHours(15), date.AddHours(16));
			var overTimeActivityPeriod2 = new DateTimePeriod(date.AddHours(16), date.AddHours(17));
			var overTimeActivity = ActivityFactory.CreateActivity("Chat", Color.Red);

			assignment.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod, def);
			assignment.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod2, def);

			scheduleDay.Add(assignment);

			var vm = Target.Projection(scheduleDay, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person.Id.ToString());
			vm.Projection.Count().Should().Be(2);
			vm.Projection.First().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.Last().Description.Should().Be(overTimeActivity.Description.Name);
			vm.Projection.Last().IsOvertime.Should().Be.True();
		}

		[Test]
		public void ShouldProjectionStillBeOvertimeWhenAddingASecondSameTypeOvertimeActivityWithAnOvertimeActivityNeighboringWithinShift()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(date), person, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			var overTimeActivityPeriod = new DateTimePeriod(date.AddHours(11), date.AddHours(12));
			var overTimeActivityPeriod2 = new DateTimePeriod(date.AddHours(12), date.AddHours(13));
			var overTimeActivity = ActivityFactory.CreateActivity("Lunch", Color.Red);

			assignment.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod, def);
			assignment.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod2, def);

			scheduleDay.Add(assignment);

			var vm = Target.Projection(scheduleDay, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person.Id.ToString());
			vm.Projection.Count().Should().Be(3);
			vm.Projection.First().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.Second().Description.Should().Be(overTimeActivity.Description.Name);
			vm.Projection.Second().IsOvertime.Should().Be.True();
			vm.Projection.Last().Description.Should().Be(phoneActivity.Description.Name);
		}

		[Test]
		public void ShouldProjectionStillBeOvertimeWhenAddingAIntersectingPersonalActivityOutsideShift()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(date));

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(date), person, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(17));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);

			var overTimeActivityPeriod = new DateTimePeriod(date.AddHours(15), date.AddHours(16));
			var overTimeActivity = ActivityFactory.CreateActivity("Chat", Color.Red);

			var peronalActivityPeriod = new DateTimePeriod(date.AddHours(15).AddMinutes(30), date.AddHours(17));
			var personalActivity = ActivityFactory.CreateActivity("Chat", Color.Red);

			assignment.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment.AddOvertimeActivity(overTimeActivity, overTimeActivityPeriod, def);
			assignment.AddPersonalActivity(personalActivity, peronalActivityPeriod);

			scheduleDay.Add(assignment);

			var vm = Target.Projection(scheduleDay, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person.Id.ToString());

			var projections = vm.Projection.ToArray();

			projections.Length.Should().Be(3);
			projections[0].Description.Should().Be(phoneActivity.Description.Name);
			projections[1].Description.Should().Be(overTimeActivity.Description.Name);
			projections[1].IsOvertime.Should().Be.True();
			projections[2].Start.Should().Be("2015-01-01 15:30");
			projections[2].End.Should().Be("2015-01-01 17:00");
			projections[2].StartInUtc.Should().Be("2015-01-01 15:30");
			projections[2].EndInUtc.Should().Be("2015-01-01 17:00");
			projections[2].IsOvertime.Should().Be.False();
		}

		[Test]
		public void ShouldNotSplitMergedMainShiftLayer()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var normalMeetingPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(9));
			var anotherNormalMeetingPeriod = new DateTimePeriod(date.AddHours(9), date.AddHours(10));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var meetingActivity = ActivityFactory.CreateActivity("Meeting", Color.Red);

			phoneActivity.InContractTime = true;
			meetingActivity.InContractTime = true;
			meetingActivity.InWorkTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddActivity(meetingActivity, normalMeetingPeriod);
			assignment1Person1.AddActivity(meetingActivity, anotherNormalMeetingPeriod);
			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(2);
			var visualLayers = vm.Projection.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].Minutes.Should().Be(120);
			visualLayers[1].Description.Should().Be("Phone");
			visualLayers[1].Minutes.Should().Be(300);
		}

		[Test]
		public void ShouldSplitMergedPersonalActivityInProjectionWithSinglePersonalLayer()
		{
			var chinaTimeZoneInfo = TimeZoneInfoFactory.ChinaTimeZoneInfo();
			LoggedOnUser.SetDefaultTimeZone(chinaTimeZoneInfo);
			UserTimeZone.Is(chinaTimeZoneInfo);

			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var normalMeetingPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(9));
			var personalMeetingPeriod = new DateTimePeriod(date.AddHours(8).AddMinutes(30), date.AddHours(10));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var meetingActivity = ActivityFactory.CreateActivity("Meeting", Color.Red);

			phoneActivity.InContractTime = true;
			meetingActivity.InContractTime = true;
			meetingActivity.InWorkTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddActivity(meetingActivity, normalMeetingPeriod);
			assignment1Person1.AddPersonalActivity(meetingActivity, personalMeetingPeriod);
			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(3);
			var visualLayers = vm.Projection.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].ActivityId.Should().Be(meetingActivity.Id);
			visualLayers[0].ShiftLayerIds.Single().Should().Be(assignment1Person1.ShiftLayers.Single(l => l is MainShiftLayer && l.Payload.Description.Name == "Meeting").Id);
			visualLayers[0].Start.Should().Be("2015-01-01 16:00");
			visualLayers[0].StartInUtc.Should().Be("2015-01-01 08:00");
			visualLayers[0].End.Should().Be("2015-01-01 16:30");
			visualLayers[0].EndInUtc.Should().Be("2015-01-01 08:30");
			visualLayers[0].Minutes.Should().Be(30);

			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].ActivityId.Should().Be(meetingActivity.Id);
			visualLayers[1].ShiftLayerIds.Single().Should().Be(assignment1Person1.ShiftLayers.Single(l => l is PersonalShiftLayer && l.Payload.Description.Name == "Meeting").Id);
			visualLayers[1].Start.Should().Be("2015-01-01 16:30");
			visualLayers[1].End.Should().Be("2015-01-01 18:00");
			visualLayers[1].StartInUtc.Should().Be("2015-01-01 08:30");
			visualLayers[1].EndInUtc.Should().Be("2015-01-01 10:00");
			visualLayers[1].Minutes.Should().Be(90);

			visualLayers[2].Description.Should().Be("Phone");
			visualLayers[2].ActivityId.Should().Be(phoneActivity.Id);
			visualLayers[2].Start.Should().Be("2015-01-01 18:00");
			visualLayers[2].StartInUtc.Should().Be("2015-01-01 10:00");
			visualLayers[2].End.Should().Be("2015-01-01 23:00");
			visualLayers[2].EndInUtc.Should().Be("2015-01-01 15:00");
			visualLayers[2].Minutes.Should().Be(300);
		}

		[Test]
		public void ShouldSplitMergedPersonalActivityInProjectionWithSinglePersonalLayerWhenAddingPersonalLayerFirst()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var normalMeetingPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(9));
			var personalMeetingPeriod = new DateTimePeriod(date.AddHours(8).AddMinutes(30), date.AddHours(10));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var meetingActivity = ActivityFactory.CreateActivity("Meeting", Color.Red);

			phoneActivity.InContractTime = true;
			meetingActivity.InContractTime = true;
			meetingActivity.InWorkTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddPersonalActivity(meetingActivity, personalMeetingPeriod);
			assignment1Person1.AddActivity(meetingActivity, normalMeetingPeriod);

			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(3);
			var visualLayers = vm.Projection.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].ShiftLayerIds.Single().Should().Be(assignment1Person1.ShiftLayers.Single(l => l is MainShiftLayer && l.Payload.Description.Name == "Meeting").Id);
			visualLayers[0].Start.Should().Be("2015-01-01 08:00");
			visualLayers[0].StartInUtc.Should().Be("2015-01-01 08:00");
			visualLayers[0].End.Should().Be("2015-01-01 08:30");
			visualLayers[0].EndInUtc.Should().Be("2015-01-01 08:30");
			visualLayers[0].Minutes.Should().Be(30);

			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].ShiftLayerIds.Single().Should().Be(assignment1Person1.ShiftLayers.Single(l => l is PersonalShiftLayer && l.Payload.Description.Name == "Meeting").Id);
			visualLayers[1].Start.Should().Be("2015-01-01 08:30");
			visualLayers[1].StartInUtc.Should().Be("2015-01-01 08:30");
			visualLayers[1].End.Should().Be("2015-01-01 10:00");
			visualLayers[1].EndInUtc.Should().Be("2015-01-01 10:00");
			visualLayers[1].Minutes.Should().Be(90);

			visualLayers[2].Description.Should().Be("Phone");
			visualLayers[2].Start.Should().Be("2015-01-01 10:00");
			visualLayers[2].StartInUtc.Should().Be("2015-01-01 10:00");
			visualLayers[2].End.Should().Be("2015-01-01 15:00");
			visualLayers[2].EndInUtc.Should().Be("2015-01-01 15:00");
			visualLayers[2].Minutes.Should().Be(300);
		}

		[Test]
		public void ShouldSplitMergedPersonalActivityInProjectionWithMultiplePersonalLayer()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(15));
			var normalMeetingPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(9));
			var personalMeetingPeriod = new DateTimePeriod(date.AddHours(9), date.AddHours(10));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var meetingActivity = ActivityFactory.CreateActivity("Meeting", Color.Red);

			phoneActivity.InContractTime = true;
			meetingActivity.InContractTime = true;
			meetingActivity.InWorkTime = true;
			phoneActivity.InWorkTime = true;

			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddActivity(meetingActivity, normalMeetingPeriod);
			assignment1Person1.AddPersonalActivity(meetingActivity, personalMeetingPeriod);
			assignment1Person1.AddPersonalActivity(meetingActivity, new DateTimePeriod(new DateTime(2015, 1, 1, 9, 30, 0, DateTimeKind.Utc), new DateTime(2015, 1, 1, 10, 30, 0, DateTimeKind.Utc)));
			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(4);
			var visualLayers = vm.Projection.ToArray();
			var shiftLayers = assignment1Person1.ShiftLayers.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].Start.Should().Be("2015-01-01 08:00");
			visualLayers[0].End.Should().Be("2015-01-01 09:00");
			visualLayers[0].StartInUtc.Should().Be("2015-01-01 08:00");
			visualLayers[0].EndInUtc.Should().Be("2015-01-01 09:00");
			visualLayers[0].Minutes.Should().Be(60);
			visualLayers[0].ShiftLayerIds.First().Should().Be(shiftLayers[1].Id);

			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].Start.Should().Be("2015-01-01 09:00");
			visualLayers[1].End.Should().Be("2015-01-01 09:30");
			visualLayers[1].StartInUtc.Should().Be("2015-01-01 09:00");
			visualLayers[1].EndInUtc.Should().Be("2015-01-01 09:30");
			visualLayers[1].Minutes.Should().Be(30);
			visualLayers[1].ShiftLayerIds.First().Should().Be(shiftLayers[2].Id);

			visualLayers[2].Description.Should().Be("Meeting");
			visualLayers[2].Start.Should().Be("2015-01-01 09:30");
			visualLayers[2].End.Should().Be("2015-01-01 10:30");
			visualLayers[2].StartInUtc.Should().Be("2015-01-01 09:30");
			visualLayers[2].EndInUtc.Should().Be("2015-01-01 10:30");
			visualLayers[2].Minutes.Should().Be(60);
			visualLayers[2].ShiftLayerIds.First().Should().Be(shiftLayers[3].Id);

			visualLayers[3].Description.Should().Be("Phone");
			visualLayers[3].Start.Should().Be("2015-01-01 10:30");
			visualLayers[3].End.Should().Be("2015-01-01 15:00");
			visualLayers[3].StartInUtc.Should().Be("2015-01-01 10:30");
			visualLayers[3].EndInUtc.Should().Be("2015-01-01 15:00");
			visualLayers[3].Minutes.Should().Be(270);
			visualLayers[3].ShiftLayerIds.First().Should().Be(shiftLayers[0].Id);
		}

		[Test]
		public void ShouldMakeAgentInTeamScheduleViewModel()
		{
			const string firstName = "Sherlock";
			const string lastName = "Holmes";

			var baseDate = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid(firstName, lastName);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(baseDate));
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(baseDate), person, scenario);

			var phoneActivityStart = baseDate.AddHours(8);
			var phoneActivityEnd = baseDate.AddHours(16);
			var phoneActivityPeriod = new DateTimePeriod(phoneActivityStart, phoneActivityEnd);

			var lunchActivityStart = baseDate.AddHours(11);
			var lunchActivityEnd = baseDate.AddHours(12);
			var lunchActivityPeriod = new DateTimePeriod(lunchActivityStart, lunchActivityEnd);

			var absencePeriodStart = baseDate.AddHours(12);
			var absencePeriodEnd = baseDate.AddHours(13);
			var absencePeriod = new DateTimePeriod(absencePeriodStart, absencePeriodEnd);

			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			phoneActivity.InContractTime = true;
			phoneActivity.InWorkTime = true;
			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);

			var lunchActivity = ActivityFactory.CreateActivity("Lunch", Color.Yellow);
			lunchActivity.InContractTime = true;
			lunchActivity.InWorkTime = false;
			assignment1Person1.AddActivity(lunchActivity, lunchActivityPeriod);

			scheduleDay.Add(assignment1Person1);

			var testAbsence = AbsenceFactory.CreateAbsence("TestAbsence");
			var absenceLayer = new AbsenceLayer(testAbsence, absencePeriod);
			scheduleDay.CreateAndAddAbsence(absenceLayer).WithId();

			var timeSpanForPhoneActivityPeriod = getTimeSpanInMinutesFromPeriod(phoneActivityPeriod);
			var timeSpanForAbsencePeriod = getTimeSpanInMinutesFromPeriod(absencePeriod);
			var expectedContactTime = timeSpanForPhoneActivityPeriod - timeSpanForAbsencePeriod;

			var result = Target.MakeScheduleReadModel(LoggedOnUser.CurrentUser(), person, scheduleDay, false);

			Assert.AreEqual(result.ScheduleLayers.Length, 4);

			#region Check layer details

			var layer = result.ScheduleLayers[0];
			Assert.AreEqual(phoneActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(phoneActivityStart, lunchActivityStart), layer.TitleTime);
			Assert.AreEqual(phoneActivity.DisplayColor.ToCSV(), layer.Color);
			Assert.AreEqual(phoneActivityStart, layer.Start);
			Assert.AreEqual(lunchActivityStart, layer.End);
			Assert.AreEqual((int)(lunchActivityStart - phoneActivityStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[1];
			Assert.AreEqual(lunchActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(lunchActivityStart, absencePeriodStart), layer.TitleTime);
			Assert.AreEqual(lunchActivity.DisplayColor.ToCSV(), layer.Color);
			Assert.AreEqual(lunchActivityStart, layer.Start);
			Assert.AreEqual(absencePeriodStart, layer.End);
			Assert.AreEqual((int)(absencePeriodStart - lunchActivityStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[2];
			Assert.AreEqual(testAbsence.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(absencePeriodStart, absencePeriodEnd), layer.TitleTime);
			Assert.AreEqual(testAbsence.DisplayColor.ToCSV(), layer.Color);
			Assert.AreEqual(absencePeriodStart, layer.Start);
			Assert.AreEqual(absencePeriodEnd, layer.End);
			Assert.AreEqual((int)(absencePeriodEnd - absencePeriodStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[3];
			Assert.AreEqual(phoneActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(absencePeriodEnd, phoneActivityEnd), layer.TitleTime);
			Assert.AreEqual(phoneActivity.DisplayColor.ToCSV(), layer.Color);
			Assert.AreEqual(absencePeriodEnd, layer.Start);
			Assert.AreEqual(phoneActivityEnd, layer.End);
			Assert.AreEqual((int)(phoneActivityEnd - absencePeriodEnd).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			#endregion Check layer details

			Assert.AreEqual($"{firstName}{lastName}", result.Name);
			Assert.AreEqual(baseDate.AddHours(8), result.StartTimeUtc);
			Assert.AreEqual(person.Id, result.PersonId);
			Assert.AreEqual(null, result.MinStart);
			Assert.AreEqual(false, result.IsDayOff);
			Assert.AreEqual(false, result.IsFullDayAbsence);
			Assert.AreEqual(4, result.Total);
			Assert.AreEqual(null, result.DayOffName);
			Assert.AreEqual(expectedContactTime, result.ContractTimeInMinute);
		}

		[Test]
		public void ShouldSetIsNotScheduledToTrueWhenScheduleDayIsNullInTeamScheduleViewModel()
		{
			const string firstName = "Sherlock";
			const string lastName = "Holmes";

			var person = PersonFactory.CreatePersonWithGuid(firstName, lastName);
			var result = Target.MakeScheduleReadModel(LoggedOnUser.CurrentUser(), person, null, false);

			Assert.AreEqual(result.IsNotScheduled, true);
		}

		[Test]
		public void ShouldSetIsNotScheduledToTrueWhenAllActivitiesAreDeletedOnThatScheduleDayInTeamScheduleViewModel()
		{
			const string firstName = "Sherlock";
			const string lastName = "Holmes";

			var baseDate = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid(firstName, lastName);
			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(baseDate));
			assignment1Person1.AddActivity(ActivityFactory.CreateActivity("Phone", Color.Blue), new DateTimePeriod(baseDate.AddHours(8), baseDate.AddHours(16)));
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(baseDate), person, scenario);

			scheduleDay.DeleteMainShift();
			var result = Target.MakeScheduleReadModel(LoggedOnUser.CurrentUser(), person, scheduleDay, false);

			Assert.AreEqual(result.IsNotScheduled, true);
		}

		[Test]
		public void ShouldReturnShiftCategoryInfo()
		{
			const string firstName = "Sherlock";
			const string lastName = "Holmes";

			var baseDate = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid(firstName, lastName);

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(baseDate));
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(baseDate), person, scenario);

			var phoneActivityStart = baseDate.AddHours(8);
			var phoneActivityEnd = baseDate.AddHours(16);
			var phoneActivityPeriod = new DateTimePeriod(phoneActivityStart, phoneActivityEnd);

			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			phoneActivity.InContractTime = true;
			phoneActivity.InWorkTime = true;
			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day", Color.Green.ToString());

			assignment1Person1.SetShiftCategory(shiftCategory);

			scheduleDay.Add(assignment1Person1);

			var result = Target.MakeScheduleReadModel(LoggedOnUser.CurrentUser(), person, scheduleDay, false);

			result.ShiftCategory.Name.Should().Be.EqualTo(shiftCategory.Description.Name);
			result.ShiftCategory.DisplayColor.Should().Be.EqualTo(shiftCategory.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldReturnShiftCategoryInfoForDayOff()
		{
			const string firstName = "Sherlock";
			const string lastName = "Holmes";

			var baseDate = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid(firstName, lastName);

			var assignment1Person1 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(baseDate),
				new DayOffTemplate(new Description("Day Off", "DO")));

			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(baseDate), person, scenario);
			scheduleDay.Add(assignment1Person1);

			var result = Target.MakeScheduleReadModel(LoggedOnUser.CurrentUser(), person, scheduleDay, false);

			result.ShiftCategory.Name.Should().Be.EqualTo("Day Off");
			result.ShiftCategory.ShortName.Should().Be.EqualTo("DO");
		}

		[Test]
		public void ShouldShowCorrectDescriptionNameForConfidentialAbsenceBasedOnUICulture()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");

			var assignment1Person1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario,
				ActivityFactory.CreateActivity("Phone", Color.Blue),
				new DateTimePeriod(date.AddHours(8), date.AddHours(16)),
				ShiftCategoryFactory.CreateShiftCategory("day"));

			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			scheduleDayOnePerson1.Add(assignment1Person1);

			var testAbsence = AbsenceFactory.CreateAbsence("test");
			var absenceLayer = new AbsenceLayer(testAbsence, new DateTimePeriod(date.AddHours(12), date.AddHours(13)));
			absenceLayer.Payload.Confidential = true;
			var personAbsence = scheduleDayOnePerson1.CreateAndAddAbsence(absenceLayer);

			personAbsence.SetId(Guid.NewGuid());

			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			var vmInEnglish = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, false);
			var personAbsenceProjectionInEnglish = vmInEnglish.Projection.ElementAt(1);
			personAbsenceProjectionInEnglish.Description.Should().Be("Other");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");
			var vmInChineses = Target.Projection(scheduleDayOnePerson1, commonNameDescriptionSetting, false);
			var personAbsenceProjectionInChineses = vmInChineses.Projection.ElementAt(1);
			personAbsenceProjectionInChineses.Description.Should().Be("其他");
		}

		private double getTimeSpanInMinutesFromPeriod(DateTimePeriod period)
		{
			return (period.EndDateTime - period.StartDateTime).TotalMinutes;
		}

		private string getPeriodString(DateTime start, DateTime end)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
				start.ToShortTimeString(), end.ToShortTimeString());
		}


	}
}