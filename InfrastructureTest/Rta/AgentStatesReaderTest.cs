using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

			var agentState1 = new ActualAgentState() { PersonId = person1.Id.GetValueOrDefault() };
			var agentState2 = new ActualAgentState() { PersonId = person2.Id.GetValueOrDefault() };

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

			var agentState1 = new ActualAgentState()
			                  {
				                  PersonId = person.Id.GetValueOrDefault(),
				                  State = "Ready",
								  StateStart = now.UtcDateTime(),
				                  Scheduled = "Phone",
				                  ScheduledNext = "Lunch",
								  NextStart = now.UtcDateTime(),
				                  AlarmName = "Out of adherence",
								  AlarmStart = now.UtcDateTime(),
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
			agentStateResult.AlarmColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(agentState1.Color)));
		}

		[Test]
		public void GetLatestState_WhenNoPeopleInTeam_ShouldReturnEmptyList2()
		{
			var now = new Now();
			var today = now.LocalDateOnly();
			var teamPeriod = new DateOnlyPeriod(today, today);
			var teamWithoutPeople = createTeam();


			var teamRepository = createTeamRepositoryForTeam(teamWithoutPeople);
			var statisticsRepository = createStatisticRepositoryWithAgentStates(new[] {new ActualAgentState() });
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

		private static IStatisticRepository createStatisticRepositoryWithAgentStates(IEnumerable<IActualAgentState> actualAgentStates)
		{
			return fakeStatisticRepo.WithAddAgentStates(actualAgentStates);
		}

		private class fakeStatisticRepo : IStatisticRepository
		{
			private IEnumerable<IActualAgentState> _actualAgentStates;

			public static IStatisticRepository WithAddAgentStates(IEnumerable<IActualAgentState> actualAgentStates)
			{
				return new fakeStatisticRepo(){_actualAgentStates = actualAgentStates};
			}

			public ICollection<IStatisticTask> LoadSpecificDates(ICollection<IQueueSource> sources, DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public ICollection<MatrixReportInfo> LoadReports()
			{
				throw new NotImplementedException();
			}

			public ICollection<IQueueSource> LoadQueues()
			{
				throw new NotImplementedException();
			}

			public ICollection<IActiveAgentCount> LoadActiveAgentCount(ISkill skill, DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public void PersistFactQueues(DataTable queueDataTable)
			{
				throw new NotImplementedException();
			}

			public void DeleteStgQueues()
			{
				throw new NotImplementedException();
			}

			public void LoadFactQueues()
			{
				throw new NotImplementedException();
			}

			public void LoadDimQueues()
			{
				throw new NotImplementedException();
			}

			public IList LoadAdherenceData(DateTime dateTime, string timeZoneId, Guid personCode, Guid agentPersonCode, int languageId,
				int adherenceId)
			{
				throw new NotImplementedException();
			}

			public IList LoadAgentStat(Guid scenarioCode, DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
			{
				throw new NotImplementedException();
			}

			public IList LoadAgentQueueStat(DateTime startDate, DateTime endDate, string timeZoneId, Guid personCode)
			{
				throw new NotImplementedException();
			}

			public IList<IActualAgentState> LoadActualAgentState(IEnumerable<IPerson> persons)
			{
				throw new NotImplementedException();
			}

			public IList<IActualAgentState> LoadLastAgentState(IEnumerable<Guid> personGuids)
			{
				var states = from a in _actualAgentStates
					where personGuids.Contains(a.PersonId)
					select a;
				return states.ToList();
			}

			public IActualAgentState LoadOneActualAgentState(Guid value)
			{
				throw new NotImplementedException();
			}

			public void AddOrUpdateActualAgentState(IActualAgentState actualAgentState)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<Guid> LoadAgentsOverThresholdForAnsweredCalls(IUnitOfWork uow)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<Guid> LoadAgentsOverThresholdForAdherence(IUnitOfWork uow)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<Guid> LoadAgentsUnderThresholdForAHT(IUnitOfWork uow)
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