using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.DataProvider
{
	[TestFixture]
	public class TeamScheduleProjectionProviderTests
	{
		private TeamScheduleProjectionProvider target;
		private readonly Scenario scenario = new Scenario("d");

		[SetUp]
		public void SetupTeamScheduleProjectionProvider()
		{
			var loggonUser = new FakeLoggedOnUser();
			var projectionProvider = new ProjectionProvider();
			var fakeGlobalSettingRepo = new FakeGlobalSettingDataRepository();
			fakeGlobalSettingRepo.PersistSettingValue("CommonNameDescription", new CommonNameDescriptionSetting("{FirstName}{LastName}"));
			var commandNameProvider = new CommonAgentNameProvider(fakeGlobalSettingRepo);

			target = new TeamScheduleProjectionProvider(projectionProvider, loggonUser, commandNameProvider);
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
			scheduleDayOnePerson1.CreateAndAddAbsence(absenceLayer);

			var vm = target.Projection(scheduleDayOnePerson1, true);

			vm.DayOff.Should().Be(null);
			vm.Name.Should().Be("agent1");
			vm.Projection.Count().Should().Be(4);
			vm.IsFullDayAbsence.Should().Be(false);
			vm.Date.Should().Be(date.ToFixedDateFormat());

			var absenceProjection = vm.Projection.ElementAt(2);

			vm.Projection.First().ParentAbsence.Should().Be(null);
			vm.Projection.Second().ParentAbsence.Should().Be(null);
			absenceProjection.ParentAbsence.Should().Be(absenceLayer.Payload.Id);
			vm.Projection.Last().ParentAbsence.Should().Be(null);

			vm.Projection.First().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.Second().Description.Should().Be(lunchActivity.Description.Name);
			absenceProjection.Description.Should().Be(testAbsence.Name);
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


			var vm = target.Projection(scheduleDayOnePerson1, true);

			vm.PersonId.Should().Be(person1.Id.ToString());
			vm.Projection.Count().Should().Be(2);
			vm.Projection.First().Description.Should().Be(overTimeActivity.Description.Name);
			vm.Projection.Last().Description.Should().Be(phoneActivity.Description.Name);
			vm.Projection.First().IsOvertime.Should().Be(true);
			vm.Projection.Last().IsOvertime.Should().Be(false);
		}

		private double getTimeSpanInMinutesFromPeriod(DateTimePeriod period)
		{
			return (period.EndDateTime - period.StartDateTime).TotalMinutes;
		}
	}
}
