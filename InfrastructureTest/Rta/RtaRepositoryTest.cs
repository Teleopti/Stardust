﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
    [Category("LongRunning")]
    public class RtaRepositoryTest : DatabaseTest
    {
        private IAgentStateReadModelReader target;

		protected override void SetupForRepositoryTest()
		{
			target = new AgentStateReadModelReader(null, null);
		}

		[Test]
		public void VerifyLoadActualAgentState()
		{
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = target.Load(new List<IPerson> {person});
			Assert.IsNotNull(result);
		}
		
	    [Test]
	    public void ShouldLoadLastAgentState()
	    {
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = target.Load(new List<Guid> { person.Id.GetValueOrDefault() });
			Assert.IsNotNull(result);
	    }

		[Test]
		public void ShouldLoadAgentStateByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest
			{
				TeamId = teamId, 
				PersonId = personId
			};
			new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler())
				.PersistActualAgentReadModel(state);
			var result = target.LoadForTeam(teamId);

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest { TeamId =teamId, PersonId = personId1};
			var state2 = new AgentStateReadModelForTest { TeamId =teamId, PersonId = personId2};
			var state3 = new AgentStateReadModelForTest { TeamId =Guid.Empty, PersonId = personId3};
			var dbWritter = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
			dbWritter.PersistActualAgentReadModel(state1);
			dbWritter.PersistActualAgentReadModel(state2);
			dbWritter.PersistActualAgentReadModel(state3);

			var result = target.LoadForTeam(teamId);

			result.Count.Should().Be(2);
		}

		[Test]
		public void ShouldLoadStatesWithAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var state = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId,
				Adherence = (int) AdherenceState.Out
			};
			new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler())
				.PersistActualAgentReadModel(state);

			target.Load(new[] {personId}).Single().Adherence.Should().Be(AdherenceState.Out);
			target.LoadForTeam(teamId).Single().Adherence.Should().Be(AdherenceState.Out);
		}
    }
}
