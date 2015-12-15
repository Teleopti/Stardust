using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{	
	[TestFixture, MyTimeWebTest]
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

		protected void SetUp()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
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
			person1Assignment1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 6, 2015, 5, 21, 8));
			person1Assignment1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 11, 2015, 5, 21, 12));
			person1Schedule.Add(person1Assignment1);
			ScheduleProvider.AddScheduleDay(person1Schedule);
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
			agentSchedule.MinStart.Should().Be.EqualTo(new DateTime(2015, 5, 21, 6, 0, 0));
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
