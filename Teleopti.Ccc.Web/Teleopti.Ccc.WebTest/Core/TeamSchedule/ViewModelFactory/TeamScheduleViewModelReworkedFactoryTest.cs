using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{	
	[TestFixture, MyTimeWebTest]
	class TeamScheduleViewModelReworkedFactoryTest
	{
		private readonly ITeamScheduleViewModelReworkedFactory _target;

		private readonly IPersonRepository _personRepository;
		private readonly IPersonForScheduleFinder _personForScheduleFinder;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPermissionProvider _permissionProvider;

		public TeamScheduleViewModelReworkedFactoryTest()
		{
			
		}

		public TeamScheduleViewModelReworkedFactoryTest(ITeamScheduleViewModelReworkedFactory target, IPersonRepository personRepository, IPersonForScheduleFinder personForScheduleFinder, IBusinessUnitRepository businessUnitRepository, ITeamRepository teamRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder, IPersonAssignmentRepository personAssignmentRepository, IPermissionProvider permissionProvider)
		{
			_target = target;
			_personRepository = personRepository;
			_personForScheduleFinder = personForScheduleFinder;
			_businessUnitRepository = businessUnitRepository;
			_teamRepository = teamRepository;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
			_personAssignmentRepository = personAssignmentRepository;
			_permissionProvider = permissionProvider;
		}

		protected void SetUp()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			_businessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			_teamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			_personRepository.Add(person1);
			_personRepository.Add(person2);
			_personRepository.Add(person3);


			var person1Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 14));
			var person1Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 21));
			person1Assignment_2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));


			var person2Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 19));
			person2Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 11, 2015, 5, 19, 20));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2015, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 12, 2015, 5, 19, 15));
			_personAssignmentRepository.Add(person1Assignment_1);
			_personAssignmentRepository.Add(person1Assignment_2);
			_personAssignmentRepository.Add(person2Assignment);
			_personAssignmentRepository.Add(person3Assignment);
		}


		[Test]
		public void TargetControllerNotNull()
		{
			_target.Should().Not.Be.Null();
		}


		[Test]
		public void PermissionProviderShouldBeUsed()
		{
			_permissionProvider.Should().Not.Be.Null();
		}

		[Test]
		public void PersonForScheduleFinderShouldWork()
		{
			SetUp();

			var team = _teamRepository.LoadAll().First();
			var persons = _personForScheduleFinder.GetPersonFor(new DateOnly(2015, 5, 19), new[] { team.Id.Value }, "");

			persons.Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void PersonScheduleDayReadModelFinderShouldWork()
		{
			SetUp();

			var personScheduleReadModels = _personScheduleDayReadModelFinder.ForPersons(new DateOnly(2015, 5, 19),
				_personRepository.LoadAll().Select(x => x.Id.Value), new Paging());

			personScheduleReadModels.Should().Have.Count.EqualTo(3);

		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			SetUp();

			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging {Take = 20, Skip = 0},
				SearchNameText = ""
			});

			result.TimeLine.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 20, 15, 0));
			result.TimeLine.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 7, 45, 0));
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithNameSearch()
		{
			SetUp();
	
			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = "1"
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithDate()
		{
			SetUp();

			var person1 = _personRepository.LoadAll().First(p => p.Name.LastName == "1");
	
			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 21),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.AgentSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.MinStart.Should().Be.EqualTo(new DateTime(2015, 5, 21, 10, 0, 0));
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithTimeFilter()
		{
			SetUp();
			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>{ new DateTimePeriod(2015, 5, 19, 7, 2015, 5, 19, 9)},
					EndTimes = new List<DateTimePeriod> {new DateTimePeriod(2015, 5, 19, 13, 2015, 5, 19, 15)},
					IsDayOff = true,
					IsWorkingDay = true,
					IsEmptyDay = true
				},
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReturnAgentsWithoutScheduleWhenNoTimeFilter()
		{
			SetUp();

			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 20),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.Should().Have.Count.EqualTo(3);
			result.AgentSchedules.ForEach(x => x.ScheduleLayers.Should().Be.Null());
		}




		[Test]
		public void ShouldNotSeeScheduleOfUnpublishedAgent()
		{
			SetUp();

			var result = _target.GetViewModel(new TeamScheduleViewModelData
			{
				ScheduleDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.AgentSchedules.First(x => x.Name.Contains("Unpublish")).ScheduleLayers.Should().Be.Null();
		}

	}
}
