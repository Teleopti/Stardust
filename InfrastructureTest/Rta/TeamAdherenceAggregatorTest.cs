﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class TeamAdherenceAggregatorTest
	{
		[Test]
		public void ShouldAggregateOutOfAdherenceOnPositiveStaffingEffectForOneTeam()
		{
			var today = new Now().LocalDateOnly();

			var teamId = Guid.NewGuid();
			var team = TeamFactory.CreateTeam("t", "s");

			var personId1 = Guid.NewGuid();
			var person1 = createPerson(team);
			person1.SetId(personId1);
			var personId2 = Guid.NewGuid();
			var person2 = createPerson(team);
			person2.SetId(personId2);
			var personId3 = Guid.NewGuid();
			var person3 = createPerson(team);
			person3.SetId(personId3);

			var inAdherence1 = new AgentStateReadModel {StaffingEffect = 0};
			var inAdherence2 = new AgentStateReadModel {StaffingEffect = 0};
			var outOfAdherence = new AgentStateReadModel {StaffingEffect = 1};

			var statisticRepository = MockRepository.GenerateMock<IAgentStateReadModelReader>();
			statisticRepository.Stub(x => x.Load(new[] {personId1, personId2, personId3}))
				.Return(new List<AgentStateReadModel> {inAdherence1, inAdherence2, outOfAdherence});
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(teamId)).Return(team);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today)))
				.Return(new List<IPerson> {person1, person2, person3});

			var target = new TeamAdherenceAggregator(statisticRepository, teamRepository, personRepository, new Now());

			var result = target.Aggregate(teamId);
			result.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateOutOfAdherenceOnNegativeStaffingEffectForOneTeam()
		{
			var today = new Now().LocalDateOnly();

			var teamId = Guid.NewGuid();
			var team = TeamFactory.CreateTeam("t", "s");

			var personId1 = Guid.NewGuid();
			var person1 = createPerson(team);
			person1.SetId(personId1);
			var personId2 = Guid.NewGuid();
			var person2 = createPerson(team);
			person2.SetId(personId2);
			var personId3 = Guid.NewGuid();
			var person3 = createPerson(team);
			person3.SetId(personId3);

			var inAdherence1 = new AgentStateReadModel {StaffingEffect = 0};
			var inAdherence2 = new AgentStateReadModel {StaffingEffect = 0};
			var outOfAdherence = new AgentStateReadModel {StaffingEffect = -1};

			var statisticRepository = MockRepository.GenerateMock<IAgentStateReadModelReader>();
			statisticRepository.Stub(x => x.Load(new[] {personId1, personId2, personId3}))
				.Return(new List<AgentStateReadModel> {inAdherence1, inAdherence2, outOfAdherence});
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(teamId)).Return(team);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today)))
				.Return(new List<IPerson> {person1, person2, person3});

			var target = new TeamAdherenceAggregator(statisticRepository, teamRepository, personRepository, new Now());

			var result = target.Aggregate(teamId);
			result.Should().Be(1);
		}

		private static IPerson createPerson(ITeam team)
		{
			var ptp = new PartTimePercentage("ptp");
			var contract = new Contract("c");
			var contractSchedule = new ContractSchedule("cs");
			var personPeriod = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(contract, ptp, contractSchedule),
				team);
			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			return person;
		}
	}
}
