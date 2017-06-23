using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture]
	public class TeamScheduleProjectionProviderTests
	{
		private TeamScheduleProjectionProvider target;
		private readonly Scenario scenario = new Scenario("d");
		private FakeToggleManager _toggleManager;
		private CommonAgentNameProvider _commonAgentNameProvider;

		[SetUp]
		public void SetupTeamScheduleProjectionProvider()
		{
			var loggonUser = new FakeLoggedOnUser();
			var projectionProvider = new ProjectionProvider();
			var fakeGlobalSettingRepo = new FakeGlobalSettingDataRepository();
			fakeGlobalSettingRepo.PersistSettingValue("CommonNameDescription", new CommonNameDescriptionSetting("{FirstName}{LastName}"));
			_commonAgentNameProvider = new CommonAgentNameProvider(fakeGlobalSettingRepo);
			_toggleManager = new FakeToggleManager();
			var nameFormatSettingsPersisterAndProvider = new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository());
			nameFormatSettingsPersisterAndProvider.Persist(
				new NameFormatSettings {NameFormatId = (int) NameFormatSetting.LastNameThenFirstName});
			target = new TeamScheduleProjectionProvider(projectionProvider, loggonUser, _toggleManager,
				new ScheduleProjectionHelper(), new ProjectionSplitter(projectionProvider, new ScheduleProjectionHelper()),
				new FakeIanaTimeZoneProvider(), new PersonNameProvider(nameFormatSettingsPersisterAndProvider));
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
			var includeNote = false;

			var viewModel = target.MakeViewModel(person, date, scheduleDay, canViewConfidential, canViewUnpublished, includeNote,
				_commonAgentNameProvider.CommonAgentNameSettings);

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
		public void ShouldNotReturnMultiplicatorDefinitionSetIdsInViewModelWhenAgentHasNoPersonPeriod()
		{
			var date = new DateOnly(2015, 01, 01);
			var timezoneChina = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var person = PersonFactory.CreatePersonWithGuid("bill", "gates");
			person.PermissionInformation.SetDefaultTimeZone(timezoneChina);

			var scheduleDay = ScheduleDayFactory.Create(date, person, scenario);

			var canViewConfidential = false;
			var canViewUnpublished = false;
			var includeNote = false;

			var viewModel = target.MakeViewModel(person, date, scheduleDay, canViewConfidential, canViewUnpublished, includeNote,
				_commonAgentNameProvider.CommonAgentNameSettings);

			viewModel.PersonId.Should().Be.EqualTo(person.Id.Value.ToString());
			viewModel.Name.Should().Be.EqualTo("billgates");
			viewModel.Date.Should().Be.EqualTo(date.Date.ToGregorianDateTimeString().Remove(10));
			viewModel.Projection.Count().Should().Be(0);
			viewModel.MultiplicatorDefinitionSetIds.Should().Be.Null();
			viewModel.InternalNotes.Should().Be.NullOrEmpty();
			viewModel.Timezone.IanaId.Should().Be(timezoneChina.Id);
			viewModel.Timezone.DisplayName.Should().Be(timezoneChina.DisplayName);
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
			var includeNote = false;

			var viewModel = target.MakeViewModel(person, date, scheduleDay, canViewConfidential, canViewUnpublished, includeNote,
				_commonAgentNameProvider.CommonAgentNameSettings);

			viewModel.PersonId.Should().Be.EqualTo(person.Id.Value.ToString());
			viewModel.Name.Should().Be.EqualTo("billgates");
			viewModel.Date.Should().Be.EqualTo(date.Date.ToGregorianDateTimeString().Remove(10));
			viewModel.Projection.Count().Should().Be(0);
			viewModel.MultiplicatorDefinitionSetIds.Should().Be.Null();
			viewModel.InternalNotes.Should().Be.NullOrEmpty();
			viewModel.Timezone.IanaId.Should().Be(timezoneChina.Id);
			viewModel.Timezone.DisplayName.Should().Be(timezoneChina.DisplayName);
		}

		[Test]
		public void ShouldGetProjection()
		{
			var date = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("agent", "1");

			var assignment1Person1 = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario, new DateOnly(date));
			var scheduleDayOnePerson1 = ScheduleDayFactory.Create(new DateOnly(date), person1, scenario);
			var phoneActivityPeriod = new DateTimePeriod(date.AddHours(8), date.AddHours(16));
			var lunchActivityPeriod = new DateTimePeriod(date.AddHours(11), date.AddHours(12));
			var absencePeriod = new DateTimePeriod(date.AddHours(12), date.AddHours(13));
			var phoneActivity = ActivityFactory.CreateActivity("Phone", Color.Blue);
			var lunchActivity = ActivityFactory.CreateActivity("Lunch", Color.Red);
			var testAbsence = AbsenceFactory.CreateAbsence("test");

			phoneActivity.InContractTime = true;
			lunchActivity.InContractTime = true;
			lunchActivity.InWorkTime = false;
			phoneActivity.InWorkTime = true;
			assignment1Person1.AddActivity(phoneActivity, phoneActivityPeriod);
			assignment1Person1.AddActivity(lunchActivity, lunchActivityPeriod);
			scheduleDayOnePerson1.Add(assignment1Person1);

			var absenceLayer = new AbsenceLayer(testAbsence, absencePeriod);
			var personAbsence = scheduleDayOnePerson1.CreateAndAddAbsence(absenceLayer);
			personAbsence.SetId(Guid.NewGuid());

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

			vm.DayOff.Should().Be(null);
			vm.Name.Should().Be("agent1");
			vm.Projection.Count().Should().Be(4);
			vm.IsFullDayAbsence.Should().Be(false);
			vm.Date.Should().Be(date.ToFixedDateFormat());

			var personAbsenceProjection = vm.Projection.ElementAt(2);

			vm.Projection.First().ParentPersonAbsences.Should().Be.Null();
			vm.Projection.Second().ParentPersonAbsences.Should().Be.Null();
			personAbsenceProjection.ParentPersonAbsences.First().Should().Be(personAbsence.Id);
			vm.Projection.Last().ParentPersonAbsences.Should().Be.Null();

			vm.Projection.First().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.Second().Description.Should().Be(lunchActivity.Description.Name);
			personAbsenceProjection.Description.Should().Be(testAbsence.Name);
			vm.Projection.Last().Description.Should().Be(phoneActivity.Description.Name);

			var timeSpanForPhoneActivityPeriod = getTimeSpanInMinutesFromPeriod(phoneActivityPeriod);
			var timeSpanForAbsencePeriod = getTimeSpanInMinutesFromPeriod(absencePeriod);
			var timeSpanForLunchActivityPeriod = getTimeSpanInMinutesFromPeriod(lunchActivityPeriod);

			var expectedContactTime = timeSpanForPhoneActivityPeriod - timeSpanForAbsencePeriod;
			vm.ContractTimeMinutes.Should().Be(expectedContactTime);

			var expectedWorktimeMinutes = timeSpanForPhoneActivityPeriod - timeSpanForLunchActivityPeriod -
										  timeSpanForAbsencePeriod;
			vm.WorkTimeMinutes.Should().Be(expectedWorktimeMinutes);
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

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

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

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

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

			var vm = target.Projection(scheduleDay, true, _commonAgentNameProvider.CommonAgentNameSettings);

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

			var vm = target.Projection(scheduleDay, true, _commonAgentNameProvider.CommonAgentNameSettings);

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

			var vm = target.Projection(scheduleDay, true, _commonAgentNameProvider.CommonAgentNameSettings);

			vm.PersonId.Should().Be(person.Id.ToString());

			var projections = vm.Projection.ToArray();

			projections.Length.Should().Be(3);
			projections[0].Description.Should().Be(phoneActivity.Description.Name);
			projections[1].Description.Should().Be(overTimeActivity.Description.Name);
			projections[1].IsOvertime.Should().Be.True();
			projections[2].Start.Should().Be("2015-01-01 15:30");
			projections[2].End.Should().Be("2015-01-01 17:00");
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

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

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

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(3);
			var visualLayers = vm.Projection.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].Start.Should().Be("2015-01-01 08:00");
			visualLayers[0].End.Should().Be("2015-01-01 08:30");
			visualLayers[0].Minutes.Should().Be(30);

			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].Start.Should().Be("2015-01-01 08:30");
			visualLayers[1].End.Should().Be("2015-01-01 10:00");
			visualLayers[1].Minutes.Should().Be(90);

			visualLayers[2].Description.Should().Be("Phone");
			visualLayers[2].Start.Should().Be("2015-01-01 10:00");
			visualLayers[2].End.Should().Be("2015-01-01 15:00");
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
			assignment1Person1.AddPersonalActivity(meetingActivity, new DateTimePeriod(new DateTime(2015, 1, 1,9, 30, 0, DateTimeKind.Utc), new DateTime(2015, 1, 1, 10, 30, 0, DateTimeKind.Utc)));
			assignment1Person1.ShiftLayers.ForEach(l => l.WithId());
			scheduleDayOnePerson1.Add(assignment1Person1);

			var vm = target.Projection(scheduleDayOnePerson1, true, _commonAgentNameProvider.CommonAgentNameSettings);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(4);
			var visualLayers = vm.Projection.ToArray();
			var shiftLayers = assignment1Person1.ShiftLayers.ToArray();
			visualLayers[0].Description.Should().Be("Meeting");
			visualLayers[0].Start.Should().Be("2015-01-01 08:00");
			visualLayers[0].End.Should().Be("2015-01-01 09:00");
			visualLayers[0].Minutes.Should().Be(60);
			visualLayers[0].ShiftLayerIds.First().Should().Be(shiftLayers[1].Id);

			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].Start.Should().Be("2015-01-01 09:00");
			visualLayers[1].End.Should().Be("2015-01-01 09:30");
			visualLayers[1].Minutes.Should().Be(30);
			visualLayers[1].ShiftLayerIds.First().Should().Be(shiftLayers[2].Id);

			visualLayers[2].Description.Should().Be("Meeting");
			visualLayers[2].Start.Should().Be("2015-01-01 09:30");
			visualLayers[2].End.Should().Be("2015-01-01 10:30");
			visualLayers[2].Minutes.Should().Be(60);
			visualLayers[2].ShiftLayerIds.First().Should().Be(shiftLayers[3].Id);

			visualLayers[3].Description.Should().Be("Phone");
			visualLayers[3].Start.Should().Be("2015-01-01 10:30");
			visualLayers[3].End.Should().Be("2015-01-01 15:00");
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
			var personAbsence = scheduleDay.CreateAndAddAbsence(absenceLayer).WithId();

			var timeSpanForPhoneActivityPeriod = getTimeSpanInMinutesFromPeriod(phoneActivityPeriod);
			var timeSpanForAbsencePeriod = getTimeSpanInMinutesFromPeriod(absencePeriod);
			var expectedContactTime = timeSpanForPhoneActivityPeriod - timeSpanForAbsencePeriod;

			var result = target.MakeScheduleReadModel(person, scheduleDay, false);

			Assert.AreEqual(result.ScheduleLayers.Length, 4);

			#region Check layer details

			var layer = result.ScheduleLayers[0];
			Assert.AreEqual(phoneActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(phoneActivityStart, lunchActivityStart), layer.TitleTime);
			Assert.AreEqual(phoneActivity.DisplayColor.Name, layer.Color);
			Assert.AreEqual(phoneActivityStart, layer.Start);
			Assert.AreEqual(lunchActivityStart, layer.End);
			Assert.AreEqual((int)(lunchActivityStart - phoneActivityStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[1];
			Assert.AreEqual(lunchActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(lunchActivityStart, absencePeriodStart), layer.TitleTime);
			Assert.AreEqual(lunchActivity.DisplayColor.Name, layer.Color);
			Assert.AreEqual(lunchActivityStart, layer.Start);
			Assert.AreEqual(absencePeriodStart, layer.End);
			Assert.AreEqual((int)(absencePeriodStart - lunchActivityStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[2];
			Assert.AreEqual(testAbsence.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(absencePeriodStart, absencePeriodEnd), layer.TitleTime);
			Assert.AreEqual(testAbsence.DisplayColor.Name, layer.Color);
			Assert.AreEqual(absencePeriodStart, layer.Start);
			Assert.AreEqual(absencePeriodEnd, layer.End);
			Assert.AreEqual((int)(absencePeriodEnd - absencePeriodStart).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			layer = result.ScheduleLayers[3];
			Assert.AreEqual(phoneActivity.Name, layer.TitleHeader);
			Assert.AreEqual(getPeriodString(absencePeriodEnd, phoneActivityEnd), layer.TitleTime);
			Assert.AreEqual(phoneActivity.DisplayColor.Name, layer.Color);
			Assert.AreEqual(absencePeriodEnd, layer.Start);
			Assert.AreEqual(phoneActivityEnd, layer.End);
			Assert.AreEqual((int)(phoneActivityEnd - absencePeriodEnd).TotalMinutes, layer.LengthInMinutes);
			Assert.AreEqual(false, layer.IsAbsenceConfidential);
			Assert.AreEqual(false, layer.IsOvertime);

			#endregion Check layer details

			Assert.AreEqual($"{lastName} {firstName}", result.Name);
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
			var result = target.MakeScheduleReadModel(person, null, false);

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
			var result = target.MakeScheduleReadModel(person, scheduleDay, false);

			Assert.AreEqual(result.IsNotScheduled, true);
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