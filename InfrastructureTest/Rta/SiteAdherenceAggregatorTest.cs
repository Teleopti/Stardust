﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	public class SiteAdherenceAggregatorTest
	{
		[Test]
		public void ShouldAggregateOutOfAdherenceOnPositiveStaffingEffectForOneSite()
		{
			var today = new Now().LocalDateOnly();

			var siteId = Guid.NewGuid();
			var site = new Site(siteId.ToString());
			site.SetId(siteId);
			var team = TeamFactory.CreateTeam("t", "s");
			site.AddTeam(team);

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

			var statisticRepository = MockRepository.GenerateMock<IRtaRepository>();
			statisticRepository.Stub(x => x.LoadLastAgentState(new[] {personId1, personId2, personId3}))
				.Return(new List<AgentStateReadModel> {inAdherence1, inAdherence2, outOfAdherence});
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			siteRepository.Stub(x => x.Get(siteId)).Return(site);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today)))
				.Return(new List<IPerson> {person1, person2, person3});

			var target = new SiteAdherenceAggregator(statisticRepository, siteRepository, personRepository, new Now());

			var result = target.Aggregate(siteId);
			result.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateOutOfAdherenceOnNegativeStaffingEffectForOneSite()
		{
			var today = new Now().LocalDateOnly();

			var siteId = Guid.NewGuid();
			var site = new Site(siteId.ToString());
			site.SetId(siteId);
			var team = TeamFactory.CreateTeam("t", "s");
			site.AddTeam(team);

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

			var statisticRepository = MockRepository.GenerateMock<IRtaRepository>();
			statisticRepository.Stub(x => x.LoadLastAgentState(new[] {personId1, personId2, personId3}))
				.Return(new List<AgentStateReadModel> {inAdherence1, inAdherence2, outOfAdherence});
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			siteRepository.Stub(x => x.Get(siteId)).Return(site);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today)))
				.Return(new List<IPerson> {person1, person2, person3});

			var target = new SiteAdherenceAggregator(statisticRepository, siteRepository, personRepository, new Now());

			var result = target.Aggregate(siteId);
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
