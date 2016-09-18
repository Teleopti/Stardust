using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture]
	public class TeamScheduleProjectionProviderTests
	{
		private TeamScheduleProjectionProvider target;
		private readonly Scenario scenario = new Scenario("d");
		private CommonAgentNameProvider _commonAgentNameProvider;
		private FakeToggleManager _toggleManager;

		[SetUp]
		public void SetupTeamScheduleProjectionProvider()
		{
			var loggonUser = new FakeLoggedOnUser();
			var projectionProvider = new ProjectionProvider();
			var fakeGlobalSettingRepo = new FakeGlobalSettingDataRepository();
			fakeGlobalSettingRepo.PersistSettingValue("CommonNameDescription", new CommonNameDescriptionSetting("{FirstName}{LastName}"));
			_commonAgentNameProvider = new CommonAgentNameProvider(fakeGlobalSettingRepo);
			_toggleManager = new FakeToggleManager();
			target = new TeamScheduleProjectionProvider(projectionProvider, loggonUser, new FakePersonNameProvider(), _toggleManager, new ScheduleProjectionHelper(), new ProjectionSplitter(projectionProvider, new ScheduleProjectionHelper()));
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

			var expectedContactTime = getTimeSpanInMinutesFromPeriod(phoneActivityPeriod) -
									  getTimeSpanInMinutesFromPeriod(absencePeriod);
			vm.ContractTimeMinutes.Should().Be(expectedContactTime);

			var expectedWorktimeMinutes = getTimeSpanInMinutesFromPeriod(phoneActivityPeriod) -
										  getTimeSpanInMinutesFromPeriod(lunchActivityPeriod) -
										  getTimeSpanInMinutesFromPeriod(absencePeriod);
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
			_toggleManager.Enable(Toggles.WfmTeamSchedule_MakePersonalActivityUnmerged_40252);


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
			_toggleManager.Enable(Toggles.WfmTeamSchedule_MakePersonalActivityUnmerged_40252);

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
			visualLayers[0].Minutes.Should().Be(30);
			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].Minutes.Should().Be(90);
			visualLayers[2].Description.Should().Be("Phone");
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
			_toggleManager.Enable(Toggles.WfmTeamSchedule_MakePersonalActivityUnmerged_40252);

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
			visualLayers[0].Minutes.Should().Be(60);
			visualLayers[0].ShiftLayerIds.First().Should().Be(shiftLayers[1].Id);
			visualLayers[1].Description.Should().Be("Meeting");
			visualLayers[1].Minutes.Should().Be(30);
			visualLayers[1].ShiftLayerIds.First().Should().Be(shiftLayers[2].Id);
			visualLayers[2].Description.Should().Be("Meeting");
			visualLayers[2].Minutes.Should().Be(60);
			visualLayers[2].ShiftLayerIds.First().Should().Be(shiftLayers[3].Id);
			visualLayers[3].Description.Should().Be("Phone");
			visualLayers[3].Minutes.Should().Be(270);
			visualLayers[3].ShiftLayerIds.First().Should().Be(shiftLayers[0].Id);
		}

		private double getTimeSpanInMinutesFromPeriod(DateTimePeriod period)
		{
			return (period.EndDateTime - period.StartDateTime).TotalMinutes;
		}
	}
}
