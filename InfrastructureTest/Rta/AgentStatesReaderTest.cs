using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class AgentStatesReaderTest
	{
		
		[Test]
		public void GetLatestStatesForTeam_WhenThereAreMembersOfTheTeam_ShouldReadLastStatesForAllMembersOfTheTeam2()
		{
			var now = new Now();
			var today = now.LocalDateOnly();
			var teamPeriod = new DateOnlyPeriod(today, today);
			var team = createTeam();

			var person1 = createPersonInTeam(team);
			var person2 = createPersonInTeam(team);

			var agentState1 = new AgentStateReadModel { PersonId = person1.Id.GetValueOrDefault() };
			var agentState2 = new AgentStateReadModel { PersonId = person2.Id.GetValueOrDefault() };

			var teamRepository = createTeamRepositoryForTeam(team);
			var personRepository = createPersonRepositoryWithPeopleInTeam(team, teamPeriod, new[] {person1,person2});
			var statisticsRepository = createStatisticRepositoryWithAgentStates(new[] {agentState1,agentState2});

			var target = new AgentStatesReader(statisticsRepository, teamRepository, personRepository, now);

			var result = target.GetLatestStatesForTeam(team.Id.GetValueOrDefault());

			result.Count().Should().Be(2);
		}

		[Test]
		public void GetLatestStatesFromTeam_Always_ShouldSetTheInformationFromLatestStates2()
		{

			var now = new Now();
			var today = now.LocalDateOnly();
			var team = createTeam();
			var teamPeriod = new DateOnlyPeriod(today, today);

			var person = createPersonInTeam(team);

			person.Name = new Name("Ashley", "Andeen");

			var utcDateTime = now.UtcDateTime();
			var agentState1 = new AgentStateReadModel
			                  {
				                  PersonId = person.Id.GetValueOrDefault(),
				                  State = "Ready",
								  StateStart = utcDateTime,
				                  Scheduled = "Phone",
				                  ScheduledNext = "Lunch",
								  NextStart = utcDateTime,
				                  AlarmName = "Out of adherence",
								  AlarmStart = utcDateTime,
								  Color = Color.Red.ToArgb()
			                  };

			var teamRepository = createTeamRepositoryForTeam(team);
			var personRepository = createPersonRepositoryWithPeopleInTeam(team, teamPeriod, new[] { person });
			var statisticsRepository = createStatisticRepositoryWithAgentStates(new[] { agentState1 });

			var target = new AgentStatesReader(statisticsRepository, teamRepository, personRepository, new Now());

			var agentStateResult = target.GetLatestStatesForTeam(team.Id.GetValueOrDefault()).Single();

			agentStateResult.PersonId.Should().Be(person.Id);
			agentStateResult.State.Should().Be(agentState1.State);
			agentStateResult.AlarmStart.Should().Be(agentState1.StateStart);
			agentStateResult.Activity.Should().Be(agentState1.Scheduled);
			agentStateResult.NextActivity.Should().Be(agentState1.ScheduledNext);
			agentStateResult.NextActivityStartTime.Should().Be(agentState1.NextStart);
			agentStateResult.Alarm.Should().Be(agentState1.AlarmName);
			agentStateResult.AlarmStart.Should().Be(agentState1.AlarmStart);
			agentStateResult.AlarmColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(agentState1.Color.Value)));
		}

		[Test]
		public void GetLatestState_WhenNoPeopleInTeam_ShouldReturnEmptyList2()
		{
			var now = new Now();
			var today = now.LocalDateOnly();
			var teamPeriod = new DateOnlyPeriod(today, today);
			var teamWithoutPeople = createTeam();


			var teamRepository = createTeamRepositoryForTeam(teamWithoutPeople);
			var statisticsRepository = createStatisticRepositoryWithAgentStates(new[] {new AgentStateReadModel() });
			var personRepository = createPersonRepositoryWithPeopleInTeam(teamWithoutPeople, teamPeriod, new List<IPerson>());

			var target = new AgentStatesReader(statisticsRepository, teamRepository, personRepository, now);

			CollectionAssert.IsEmpty(target.GetLatestStatesForTeam(teamWithoutPeople.Id.GetValueOrDefault()));

		}

		#region helpers
		private static ITeam createTeam()
		{
			var teamId = Guid.NewGuid();
			var team = new Team();
			team.SetId(teamId);
			return team;
		}

		private static ITeamRepository createTeamRepositoryForTeam(ITeam team)
		{
			var teamRepository = MockRepository.GenerateStub<ITeamRepository>();
			teamRepository.Stub(t => t.Get(team.Id.GetValueOrDefault())).Return(team);
			return teamRepository;
		}

		private static IPersonRepository createPersonRepositoryWithPeopleInTeam(ITeam team,DateOnlyPeriod period, ICollection<IPerson> people)
		{
			var personRepository = MockRepository.GenerateStub<IPersonRepository>();
			personRepository.Stub(t => t.FindPeopleBelongTeam(team,period)).Return(people);
			return personRepository;
		}

		private static IRtaRepository createStatisticRepositoryWithAgentStates(IEnumerable<AgentStateReadModel> actualAgentStates)
		{
			return fakeStatisticRepo.WithAddAgentStates(actualAgentStates);
		}

		private class fakeStatisticRepo : IRtaRepository
		{
			private IEnumerable<AgentStateReadModel> _actualAgentStates;

			public static IRtaRepository WithAddAgentStates(IEnumerable<AgentStateReadModel> actualAgentStates)
			{
				return new fakeStatisticRepo{_actualAgentStates = actualAgentStates};
			}

			public IList<AgentStateReadModel> LoadActualAgentState(IEnumerable<IPerson> persons)
			{
				throw new NotImplementedException();
			}

			public IList<AgentStateReadModel> LoadLastAgentState(IEnumerable<Guid> personGuids)
			{
				var states = from a in _actualAgentStates
					where personGuids.Contains(a.PersonId)
					select a;
				return states.ToList();
			}

			public AgentStateReadModel LoadOneActualAgentState(Guid value)
			{
				throw new NotImplementedException();
			}

			public void AddOrUpdateActualAgentState(AgentStateReadModel agentStateReadModel)
			{
				throw new NotImplementedException();
			}

		}

		private static IPerson createPersonInTeam(ITeam team)
		{
			var ptp = new PartTimePercentage(" ");
			var contract = new Contract(" ");
			var contractSchedule = new ContractSchedule(" ");
			var personPeriod = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(contract, ptp, contractSchedule),
				team);
			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.SetId(Guid.NewGuid());
			return person;
		}
		#endregion 
	}
}