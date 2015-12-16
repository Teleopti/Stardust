using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
    [Category("LongRunning")]
	[RtaDatabaseTest]
	public class RtaRepositoryTest : DatabaseTest
    {
        private IAgentStateReadModelReader target;

		protected override void SetupForRepositoryTest()
		{
			target = new AgentStateReadModelReader(null);
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
			new AgentStateReadModelPersister(new FakeConnectionStrings())
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
			var dbWritter = new AgentStateReadModelPersister(new FakeConnectionStrings());
            dbWritter.PersistActualAgentReadModel(state1);
			dbWritter.PersistActualAgentReadModel(state2);
			dbWritter.PersistActualAgentReadModel(state3);

			var result = target.LoadForTeam(teamId);

			result.Count.Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesBySiteIds()
		{
			var siteId1= Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest { SiteId = siteId1, PersonId = personId1 };
			var state2 = new AgentStateReadModelForTest { SiteId = siteId2, PersonId = personId2};
			var state3 = new AgentStateReadModelForTest { SiteId = Guid.Empty, PersonId = personId3};
			var dbWriter = new AgentStateReadModelPersister(new FakeConnectionStrings());
            dbWriter.PersistActualAgentReadModel(state1);
			dbWriter.PersistActualAgentReadModel(state2);
			dbWriter.PersistActualAgentReadModel(state3);

			var result = target.LoadForSites(new[] {siteId1, siteId2});

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamIds()
		{
			var teamId1= Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest { TeamId = teamId1, PersonId = personId1 };
			var state2 = new AgentStateReadModelForTest { TeamId = teamId2, PersonId = personId2};
			var state3 = new AgentStateReadModelForTest { TeamId = Guid.Empty, PersonId = personId3};
			var dbWriter = new AgentStateReadModelPersister(new FakeConnectionStrings());
            dbWriter.PersistActualAgentReadModel(state1);
			dbWriter.PersistActualAgentReadModel(state2);
			dbWriter.PersistActualAgentReadModel(state3);

			var result = target.LoadForTeams(new[] {teamId1, teamId2});

			result.Count().Should().Be(2);
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
				Adherence = (int) Adherence.Out
			};
			new AgentStateReadModelPersister(new FakeConnectionStrings())
				.PersistActualAgentReadModel(state);

			target.Load(new[] {personId}).Single().Adherence.Should().Be(Adherence.Out);
			target.LoadForTeam(teamId).Single().Adherence.Should().Be(Adherence.Out);
		}
		
		[Test]
		public void ShouldLoadStatesInAlarmOnly()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				IsRuleAlarm = true
			};
			var state2 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				IsRuleAlarm = false
			};
			var persister = new AgentStateReadModelPersister(new FakeConnectionStrings());
			persister.PersistActualAgentReadModel(state1);
			persister.PersistActualAgentReadModel(state2);

			var result = target.LoadForTeams(new[] { teamId }, true);

			result.Single().PersonId.Should().Be(personId1);
		}
		
		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirst()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			};
			var state2 = new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			};
			var persister = new AgentStateReadModelPersister(new FakeConnectionStrings());
			persister.PersistActualAgentReadModel(state1);
			persister.PersistActualAgentReadModel(state2);

			var result = target.LoadForSites(new[] { siteId }, null, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeLast()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			};
			var state2 = new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			};
			var persister = new AgentStateReadModelPersister(new FakeConnectionStrings());
			persister.PersistActualAgentReadModel(state1);
			persister.PersistActualAgentReadModel(state2);

			var result = target.LoadForSites(new[] { siteId }, null, false);

			result.First().PersonId.Should().Be(personId1);
			result.Last().PersonId.Should().Be(personId2);
		}
		[Test]
		public void ShouldLoadTeamStatesOrderByLongestAlarmTimeFirst()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			};
			var state2 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			};
			var persister = new AgentStateReadModelPersister(new FakeConnectionStrings());
			persister.PersistActualAgentReadModel(state1);
			persister.PersistActualAgentReadModel(state2);

			var result = target.LoadForTeams(new[] { teamId }, null, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadTeamStatesOrderByLongestAlarmTimeLast()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var state1 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			};
			var state2 = new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			};
			var persister = new AgentStateReadModelPersister(new FakeConnectionStrings());
			persister.PersistActualAgentReadModel(state1);
			persister.PersistActualAgentReadModel(state2);

			var result = target.LoadForTeams(new[] { teamId }, null, false);

			result.First().PersonId.Should().Be(personId1);
			result.Last().PersonId.Should().Be(personId2);
		}
	}
}
