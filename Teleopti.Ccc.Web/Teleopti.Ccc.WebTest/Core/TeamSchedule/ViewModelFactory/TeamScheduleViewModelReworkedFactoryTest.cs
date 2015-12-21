using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	public class MyTimeWebTeamScheduleViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}

	[TestFixture, MyTimeWebTeamScheduleViewModelFactoryTest]
	public class TeamScheduleViewModelReworkedFactoryTest
	{
		public ITeamScheduleViewModelReworkedFactory Target;
		public IPersonRepository PersonRepository;
		public IPersonForScheduleFinder PersonForScheduleFinder;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ITeamRepository TeamRepository;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPermissionProvider PermissionProvider;
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

			team = TeamFactory.CreateTeamWithId("team1");
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
			var person1Assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person1,new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21 ,16));
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
			var person1Assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person1,new DateTimePeriod(2015, 5, 22, 10, 2015, 5, 22 ,16));
			person1Assignment2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 22, 8, 2015, 5, 22, 16));
			person1Assignment2.AddOvertimeActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 22, 6, 2015, 5, 22, 8), new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			person1Schedule2.Add(person1Assignment2);
			ScheduleProvider.AddScheduleDay(person1Schedule2);

			
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
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging {Take = 20, Skip = 0},
				SearchNameText = ""
			});

			result.TimeLine.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 20, 15, 0));
			result.TimeLine.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 7, 45, 0));
		}
		
		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLineNoReadModel()
		{
			SetUp();

			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging {Take = 20, Skip = 0},
				SearchNameText = ""
			});

			result.TimeLine.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 21, 16, 15, 0));
			result.TimeLine.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 21, 5, 45, 0));
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithNameSearch()
		{
			SetUp();
	
			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = "1"
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithDate()
		{
			SetUp();

			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
	
			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.MinStart.Should().Be.EqualTo(new DateTime(2015, 5, 21, 10, 0, 0));
		}
		
		[Test]
		public void ShouldReturnMyScheduleWithDateNoReadModel()
		{
			SetUp();
			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
			LoggedOnUser.SetFakeLoggedOnUser(person1);
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
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
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
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
		public void ShouldReturnCorrectAgentSchedulesWithDateNoReadModel()
		{
			SetUp();

			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
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
		public void ShouldIndicateOvertimeWithDateNoReadModel()
		{
			SetUp();

			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 22),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.ScheduleLayers.First().IsOvertime.Should().Be.EqualTo(true);
			agentSchedule.ScheduleLayers.Second().IsOvertime.Should().Be.EqualTo(false);
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
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person1, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person2, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, p3,
				new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("Phone"), p4,
				new DateTimePeriod(2015, 5, 23, 11, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"), scenario);
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p4ScheduleOn23.Add(p4AssOn23);
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("Phone"), p5,
				new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"), scenario);
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
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
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
			agentSchedules[3].PersonId.Should().Be.EqualTo(p5.Id.GetValueOrDefault());
			agentSchedules[4].PersonId.Should().Be.EqualTo(p4.Id.GetValueOrDefault());
			agentSchedules[5].PersonId.Should().Be.EqualTo(p6.Id.GetValueOrDefault());
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
			var person1Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person1, new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 10, 2015, 5, 23, 16));
			person1Schedule1On23.Add(person1Assignment1On23);
			ScheduleProvider.AddScheduleDay(person1Schedule1On23);
			var person2ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), person2, scenario);
			var person2Assignment1On23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person2, new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2Assignment1On23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person2ScheduleOn23.Add(person2Assignment1On23);
			ScheduleProvider.AddScheduleDay(person2ScheduleOn23);
			var p3 = PersonFactory.CreatePersonWithGuid("p3", "b");
			var person3ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p3,
				scenario);
			var person3AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, p3,
				new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3AssOn23.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 23, 9, 2015, 5, 23, 16));
			person3ScheduleOn23.Add(person3AssOn23);
			ScheduleProvider.AddScheduleDay(person3ScheduleOn23);
			var p4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			var p4ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p4, scenario);
			var p4AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("Phone"), p4,
				new DateTimePeriod(2015, 5, 23, 11, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"), scenario);
			var p4AbsenceOn23 = PersonAbsenceFactory.CreatePersonAbsence(p4, scenario,
				new DateTimePeriod(2015, 5, 23, 0, 2015, 5, 23, 23));
			p4ScheduleOn23.Add(p4AssOn23);
			p4ScheduleOn23.Add(p4AbsenceOn23);
			ScheduleProvider.AddScheduleDay(p4ScheduleOn23);
			var p5 = PersonFactory.CreatePersonWithGuid("p5", "p5");
			var p5ScheduleOn23 = ScheduleDayFactory.Create(new DateOnly(2015, 5, 23), p5, scenario);
			var p5AssOn23 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("Phone"), p5,
				new DateTimePeriod(2015, 5, 23, 7, 2015, 5, 23, 16), ShiftCategoryFactory.CreateShiftCategory("test"), scenario);
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
	
			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 2, Skip = 4 },
				SearchNameText = ""
			});

			var agentSchedules = result.AgentSchedules;
			agentSchedules.Count().Should().Be.EqualTo(2);
			agentSchedules[0].PersonId.Should().Be.EqualTo(p4.Id.GetValueOrDefault());
			agentSchedules[1].PersonId.Should().Be.EqualTo(p6.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithTimeFilter()
		{
			SetUp();
			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>{ new DateTimePeriod(2015, 5, 19, 7, 2015, 5, 19, 9)},
					EndTimes = new List<DateTimePeriod> {new DateTimePeriod(2015, 5, 19, 13, 2015, 5, 19, 15)},
					IsDayOff = false,
					IsWorkingDay = true,
					IsEmptyDay = false
				},
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReturnAllAgentsOfTeamWhenNoTimeFilter()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 20),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void ShouldNotSeeScheduleOfUnpublishedAgent()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.First(x => x.Name.Contains("Unpublish")).ScheduleLayers.Should().Be.Null();
		}

		[Test]
		public void ShouldNotSeeScheduleOfUnpublishedAgentNoReadModel()
		{
			SetUp();

			var result = Target.GetViewModelNoReadModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.First(x => x.Name.Contains("Unpublish")).ScheduleLayers.Should().Be.Null();
		}


		[Test]
		public void ShouldSeeDayOffAgentScheduleWhenDayOffFilterEnabled()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>(),
					EndTimes = new List<DateTimePeriod>(),
					IsDayOff = true,
					IsWorkingDay = false,
					IsEmptyDay = false
				},
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);			
		}

		[Test]
		[Ignore("Ideally this should be included. But with the current implementation the permission check in application is incompatible with the pagingation from repository. So ignored. Discussion welcomed!")]
		public void ShouldSeeEmptyDayAgentButNotUnpulishedAgentWhenEmptyDayFilterEnabled()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>(),
					EndTimes = new List<DateTimePeriod>(),
					IsDayOff = false,
					IsWorkingDay = false,
					IsEmptyDay = true
				},
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);	
		}

		[Test]
		public void ShouldSeeCorrectAgentSchedulesWhenBothDayOffAndEmptyDayFilterEnabled()
		{
			SetUp();

			var result = Target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>(),
					EndTimes = new List<DateTimePeriod>(),
					IsDayOff = true,
					IsWorkingDay = false,
					IsEmptyDay = true
				},
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(3);	
		}

	}
}
