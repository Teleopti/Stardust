using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	public class MyTimeWebTeamScheduleViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			isolate.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
		}
	}

	[TestFixture, MyTimeWebTeamScheduleViewModelFactoryTest]
	public class TeamScheduleViewModelFactoryTest
	{
		public ITeamScheduleViewModelFactoryToggle75989Off Target;
		public IPersonRepository PersonRepository;
		public IPersonForScheduleFinder PersonForScheduleFinder;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ITeamRepository TeamRepository;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePermissionProvider PermissionProvider;
		public FakeScheduleProvider ScheduleProvider;
		public FakeLoggedOnUser LoggedOnUser;

		private ITeam team;
		private IBusinessUnit businessUnit;

		protected void SetUp()
		{
			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "a");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			var site = SiteFactory.CreateSimpleSite("s");
			team = TeamFactory.CreateTeamWithId("team1");
			team.Site = site;
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);


			var person1Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 14));
			var person1Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 21));
			person1Assignment_2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));


			var person2Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 19));
			person2Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 11, 2015, 5, 19, 20));

			var person2Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 23));
			person2Assignment_2.SetDayOff(new DayOffTemplate(new Description("Day Off", "DO")));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2015, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 12, 2015, 5, 19, 15));
			PersonAssignmentRepository.Add(person1Assignment_1);
			PersonAssignmentRepository.Add(person1Assignment_2);
			PersonAssignmentRepository.Add(person2Assignment_1);
			PersonAssignmentRepository.Add(person2Assignment_2);
			PersonAssignmentRepository.Add(person3Assignment);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person1Schedule = ScheduleDayFactory.Create(new DateOnly(2015, 5, 21), person1, scenario);
			var person1Assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));
			person1Assignment1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));
			person1Assignment1.AddOvertimeActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 6, 2015, 5, 21, 8), new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			person1Assignment1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 11, 2015, 5, 21, 12));
			person1Schedule.Add(person1Assignment1);
			ScheduleProvider.AddScheduleDay(person1Schedule);
			var person3Schedule = ScheduleDayFactory.Create(new DateOnly(2015, 5, 21), person3, scenario);
			person3Schedule.CreateAndAddActivity(ActivityFactory.CreateActivity("Phone"),
				new DateTimePeriod(2015, 5, 21, 5, 2015, 5, 21, 16));
			ScheduleProvider.AddScheduleDay(person3Schedule);

			var person1Schedule2 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 22), person1, scenario);
			var person1Assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 22, 10, 2015, 5, 22, 16));
			person1Assignment2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 22, 8, 2015, 5, 22, 16));
			person1Assignment2.AddOvertimeActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 22, 6, 2015, 5, 22, 8), new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			person1Schedule2.Add(person1Assignment2);
			ScheduleProvider.AddScheduleDay(person1Schedule2);

			var person2Schedule = ScheduleDayFactory.Create(new DateOnly(2015, 5, 22), person2, scenario);
			var person2Assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, new DateTimePeriod(2015, 5, 22, 10, 2015, 5, 22, 16));
			person2Assignment2.AddOvertimeActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 22, 6, 2015, 5, 22, 8), new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			person2Schedule.Add(person2Assignment2);
			ScheduleProvider.AddScheduleDay(person2Schedule);
		}

		[Test]
		public void TargetFactoryNotNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void PermissionProviderShouldBeUsed()
		{
			PermissionProvider.Should().Not.Be.Null();
		}

		[Test]
		public void PersonForScheduleFinderShouldWork()
		{
			SetUp();

			var team = TeamRepository.LoadAll().First();
			var persons = PersonForScheduleFinder.GetPersonFor(new DateOnly(2015, 5, 19), new[] { team.Id.Value }, "");

			persons.Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void PersonScheduleDayReadModelFinderShouldWork()
		{
			SetUp();

			var personScheduleReadModels = PersonScheduleDayReadModelFinder.ForPersons(new DateOnly(2015, 5, 19),
				PersonRepository.LoadAll().Select(x => x.Id.Value), new Paging());

			personScheduleReadModels.Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLineNoReadModel()
		{
			SetUp();

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.TimeLine.Max(t => (t as TeamScheduleTimeLineViewModelToggle75989Off).EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 21, 16, 15, 0));
			result.TimeLine.Min(t => (t as TeamScheduleTimeLineViewModelToggle75989Off).StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 21, 5, 45, 0));
		}

		
		[Test]
		public void ShouldReturnMyScheduleWithDateNoReadModel()
		{
			SetUp();
			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
			LoggedOnUser.SetFakeLoggedOnUser(person1);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var mySchedule = result.MySchedule;
			mySchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2015, 5, 21, 6, 0, 0));
			result.AgentSchedules.Count().Should().Be.EqualTo(2);
			result.TimeLine.Count().Should().Be.EqualTo(12);
		}

		[Test]
		public void ShouldReturnMyScheduleCorrectlyWithDateWhenMyScheduleUnpublishedNoReadModel()
		{
			SetUp();
			var person = PersonRepository.LoadAll().First(p => p.Name.LastName == "3");
			LoggedOnUser.SetFakeLoggedOnUser(person);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var mySchedule = result.MySchedule;
			mySchedule.ScheduleLayers.Should().Be.Null();
			result.AgentSchedules.Count().Should().Be.EqualTo(2);
			result.TimeLine.Count().Should().Be.EqualTo(12);
		}

		[Test]
		public void ShouldSeeMyUnpublishedScheduleWhenIHaveViewUnpublishedSchedulePermission()
		{
			SetUp();
			var personMe = PersonRepository.LoadAll().First(p => p.Name.FirstName == "Unpublish_person");
			PermissionProvider.PermitApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, true);

			LoggedOnUser.SetFakeLoggedOnUser(personMe);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.MySchedule.ScheduleLayers.Should().Not.Be.Null();
			result.MySchedule.ScheduleLayers.Count().Should().Be(1);
		}

		[Test]
		public void ShouldSeeAgentsUnpublishedScheduleWhenLoggedOnUserHasViewUnpublishedSchedulePermission()
		{
			SetUp();
			var personMe = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
			PermissionProvider.PermitApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, true);

			LoggedOnUser.SetFakeLoggedOnUser(personMe);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.Count().Should().Be(2);
			result.AgentSchedules.First().ScheduleLayers.Should().Not.Be.Null();
			result.AgentSchedules.First().ScheduleLayers.Count().Should().Be(1);
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithDateNoReadModel()
		{
			SetUp();

			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2015, 5, 21, 6, 0, 0));
		}

		[Test]
		public void ShouldReturnAgentScheduleWithCorrectColor()
		{
			SetUp();

			var expactedColor = Color.Red;
			var scheduleDate = new DateOnly(2015, 5, 21);
			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
			var scheduleDay = ScheduleProvider.GetScheduleForPersons(scheduleDate, new List<IPerson> {person1});
			scheduleDay.First().PersonAssignment().ShiftCategory.DisplayColor = expactedColor;

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = scheduleDate,
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = "",
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod> { new DateTimePeriod(2015,5,21,10, 2015,5,21,12)},
					EndTimes = new List<DateTimePeriod> { new DateTimePeriod(2015,5,21,0, 2015,5,21,23)},
					IsWorkingDay = true
				}
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.ShiftCategory.DisplayColor.Should().Be.EqualTo(expactedColor.ToHtml());
		}

		[Test]
		public void ShouldReturnEmptyLayersWhenHasEmptyScheduleWithDateNoReadModel()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");

			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p5);

			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == p5.Id.Value);
			agentSchedule.IsFullDayAbsence.Should().Be.EqualTo(false);
			agentSchedule.IsDayOff.Should().Be.EqualTo(false);
			agentSchedule.ScheduleLayers.Count().Should().Be.EqualTo(0);
			result.TimeLine.Count().Should().Be.EqualTo(11);
		}

		[Test]
		public void ShouldIndicateFullDayAbsenceWhenHasMainShiftWithDateNoReadModel()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");

			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p5);

			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p5, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == p5.Id.Value);
			agentSchedule.IsFullDayAbsence.Should().Be.EqualTo(true);
			agentSchedule.ScheduleLayers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIndicateFullDayAbsenceWhenHasDayoffWithDateNoReadModel()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");

			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p5);

			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithDayOff(p5, scenario, new DateOnly(2015, 5, 23), new DayOffTemplate(new Description("dayoff")));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == p5.Id.Value);
			agentSchedule.IsFullDayAbsence.Should().Be.EqualTo(true);
			agentSchedule.ScheduleLayers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnDayoffWhenHasDayoffWithDateNoReadModel()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");

			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p5);

			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithDayOff(p5, scenario, new DateOnly(2015, 5, 23), new DayOffTemplate(new Description("dayoff")));
			p5ScheduleOn23.Add(p5AssOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == p5.Id.Value);
			agentSchedule.IsFullDayAbsence.Should().Be.EqualTo(false);
			agentSchedule.IsDayOff.Should().Be.EqualTo(true);
			agentSchedule.DayOffName.Should().Be.EqualTo("dayoff");
		}

		[Test]
		public void ShouldReturnDayoffWhenHasFullAbsenceOnContractDayOffWithDateNoReadModel()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");

			businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2011, 1, 1), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			p5.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2015, 5, 22));
			p5.AddSchedulePeriod(schedulePeriod);
			p5.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			PersonRepository.Add(p5);

			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreatePersonAssignment(p5, scenario, new DateOnly(2015, 5, 23));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(new DateTime(2015, 5, 23, 0, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 5, 23, 23, 59, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("absence"));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(personAbsence);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(new DateTime(2015, 5, 23, 0, 0, 0, 0, DateTimeKind.Utc)),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == p5.Id.Value);
			agentSchedule.IsFullDayAbsence.Should().Be.EqualTo(true);
			agentSchedule.ScheduleLayers.Count().Should().Be.EqualTo(1);
			agentSchedule.ScheduleLayers[0].TitleHeader.Should().Be.EqualTo("absence");
			agentSchedule.IsDayOff.Should().Be.EqualTo(true);
			agentSchedule.DayOffName.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldIndicateOvertimeWithDateNoReadModelOnlyForMySchedule()
		{
			SetUp();
			var me = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
			var teamate = PersonRepository.LoadAll().First(p => p.Name.LastName == "a");
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 22),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == teamate.Id.Value);

			result.MySchedule.ScheduleLayers[0].IsOvertime.Should().Be.True();
			result.MySchedule.ScheduleLayers[1].IsOvertime.Should().Be.False();
			agentSchedule.ScheduleLayers.First().IsOvertime.Should().Be.True();
		}

		[Test]
		public void ShouldSortAgentScheduleCorrectlyWithDateNoReadModel()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "a");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person1Schedule1On23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person1, scenario);
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p3,
				scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p4, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 11, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p4ScheduleOn23.Add(p4AssOn23);
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p5, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);
			var p6 = PersonFactory.CreatePersonWithGuid("p6", "p6");
			var p6ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p6, scenario);
			p6ScheduleOn23.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("dayoff")));
			ScheduleProvider.AddScheduleDay(p6ScheduleOn23);

			p3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p4.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p6.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p3);
			PersonRepository.Add(p4);
			PersonRepository.Add(p5);
			PersonRepository.Add(p6);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedules = result.AgentSchedules;
			agentSchedules[0].PersonId.Should().Be.EqualTo(person2.Id.GetValueOrDefault());
			agentSchedules[1].PersonId.Should().Be.EqualTo(p3.Id.GetValueOrDefault());
			agentSchedules[2].PersonId.Should().Be.EqualTo(person1.Id.GetValueOrDefault());
			agentSchedules[3].PersonId.Should().Be.EqualTo(p6.Id.GetValueOrDefault());
			agentSchedules[4].PersonId.Should().Be.EqualTo(p5.Id.GetValueOrDefault());
			agentSchedules[5].PersonId.Should().Be.EqualTo(p4.Id.GetValueOrDefault());
			agentSchedules[6].PersonId.Should().Be.EqualTo(person3.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldSortAgentScheduleCorrectlyWhenThereIsMultiDayAbsenceWithDateNoReadModel()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "a");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person1Schedule1On23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person1, scenario);
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p3,
				scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 4, 21, 0, 2015, 5, 24, 23));
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p5, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);
			var p6 = PersonFactory.CreatePersonWithGuid("p6", "p6");
			var p6ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p6, scenario);
			p6ScheduleOn23.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("dayoff")));
			ScheduleProvider.AddScheduleDay(p6ScheduleOn23);

			p3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p4.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p6.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p3);
			PersonRepository.Add(p4);
			PersonRepository.Add(p5);
			PersonRepository.Add(p6);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedules = result.AgentSchedules;
			agentSchedules[0].Name.Should().Be.EqualTo("person@a");
			agentSchedules[1].Name.Should().Be.EqualTo("p3@b");
			agentSchedules[2].Name.Should().Be.EqualTo("person@1");
			agentSchedules[3].Name.Should().Be.EqualTo("p6@p6");
			agentSchedules[4].Name.Should().Be.EqualTo("p4@p4");
			agentSchedules[5].Name.Should().Be.EqualTo("p5@p5");
			agentSchedules[6].Name.Should().Be.EqualTo("Unpublish_person@3");
		}

		[Test]
		public void ShouldSortAgentWithEmptyScheduleButNonNullPersonAssLastWithDateNoReadModel()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "a");
			var person3 = PersonFactory.CreatePersonWithGuid("person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person1Schedule1On23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person1, scenario);
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var person3EmptyScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person3, scenario);
			var person3EmptyAssOn23 = PersonAssignmentFactory.CreatePersonAssignment(person3, scenario, new DateOnly(2015, 5, 23));
			person3EmptyScheduleOn23.Add(person3EmptyAssOn23);
			ScheduleProvider.AddScheduleDay(person3EmptyScheduleOn23);


			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p3,
				scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p4, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 11, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p4ScheduleOn23.Add(p4AssOn23);
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p5, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);
			var p6 = PersonFactory.CreatePersonWithGuid("p6", "p6");
			var p6ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p6, scenario);
			p6ScheduleOn23.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("dayoff")));
			ScheduleProvider.AddScheduleDay(p6ScheduleOn23);

			p3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p4.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p6.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p3);
			PersonRepository.Add(p4);
			PersonRepository.Add(p5);
			PersonRepository.Add(p6);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedules = result.AgentSchedules;
			agentSchedules[0].PersonId.Should().Be.EqualTo(person2.Id.GetValueOrDefault());
			agentSchedules[1].PersonId.Should().Be.EqualTo(p3.Id.GetValueOrDefault());
			agentSchedules[2].PersonId.Should().Be.EqualTo(person1.Id.GetValueOrDefault());
			agentSchedules[3].PersonId.Should().Be.EqualTo(p6.Id.GetValueOrDefault());
			agentSchedules[4].PersonId.Should().Be.EqualTo(p5.Id.GetValueOrDefault());
			agentSchedules[5].PersonId.Should().Be.EqualTo(p4.Id.GetValueOrDefault());
			agentSchedules[6].PersonId.Should().Be.EqualTo(person3.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnAgentSchedulesCorrectlyWithDateForSpecificPageNoReadModel()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "a");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person1Schedule1On23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person1, scenario);
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p3,
				scenario, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p4, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 11, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p4ScheduleOn23.Add(p4AssOn23);
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(p5, scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"));
			var p5AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p5, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p5ScheduleOn23.Add(p5AssOn23);
			p5ScheduleOn23.Add(p5AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p5ScheduleOn23);
			var p6 = PersonFactory.CreatePersonWithGuid("p6", "p6");
			var p6ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p6, scenario);
			p6ScheduleOn23.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("dayoff")));
			ScheduleProvider.AddScheduleDay(p6ScheduleOn23);

			p3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p4.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p5.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			p6.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PersonRepository.Add(p3);
			PersonRepository.Add(p4);
			PersonRepository.Add(p5);
			PersonRepository.Add(p6);

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 2, Skip = 4 },
				SearchNameText = ""
			});

			var agentSchedules = result.AgentSchedules;
			agentSchedules.Count().Should().Be.EqualTo(2);
			agentSchedules[0].PersonId.Should().Be.EqualTo(p5.Id.GetValueOrDefault());
			agentSchedules[1].PersonId.Should().Be.EqualTo(p4.Id.GetValueOrDefault());
			result.PageCount.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldNotSeeScheduleOfUnpublishedAgentNoReadModel()
		{
			SetUp();

			var result = Target.GetTeamScheduleViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.First(x => x.Name.Contains("Unpublish")).ScheduleLayers.Should().Be.Null();
		}

	}
}
