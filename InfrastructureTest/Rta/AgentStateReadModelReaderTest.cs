using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class AgentStateReadModelReaderTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;
		public WithAnalyticsUnitOfWork WithAnalytics;

		[Test]
		public void VerifyLoadActualAgentState()
		{
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = Target.Load(new List<IPerson> {person});
			Assert.IsNotNull(result);
		}

		[Test]
		public void ShouldLoadLastAgentState()
		{
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = Target.Load(new List<Guid> {person.Id.GetValueOrDefault()});
			Assert.IsNotNull(result);
		}

		[Test]
		public void ShouldLoadAgentStateByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId
				});
			});
			var result = Target.LoadForTeam(teamId);

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamId()
		{
			var teamId = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest {TeamId = teamId, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {TeamId = teamId, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {TeamId = Guid.Empty, PersonId = Guid.NewGuid()});
			});

			var result = Target.LoadForTeam(teamId);

			result.Count.Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesBySiteIds()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest {SiteId = siteId1, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {SiteId = siteId2, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {SiteId = Guid.Empty, PersonId = Guid.NewGuid()});
			});

			var result = Target.LoadForSites(new[] {siteId1, siteId2}, null, null);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamIds()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest {TeamId = teamId1, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {TeamId = teamId2, PersonId = Guid.NewGuid()});
				Persister.Persist(new AgentStateReadModelForTest {TeamId = Guid.Empty, PersonId = Guid.NewGuid()});
			});

			var result = Target.LoadForTeams(new[] {teamId1, teamId2}, null, null);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadStatesWithAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId,
					Adherence = (int) Adherence.Out
				});
			});

			Target.Load(new[] {personId}).Single().Adherence.Should().Be(Adherence.Out);
			Target.LoadForTeam(teamId).Single().Adherence.Should().Be(Adherence.Out);
		}

		[Test]
		public void ShouldLoadStatesInAlarmOnly()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId1,
					IsAlarm = true
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = Guid.NewGuid(),
					IsAlarm = false
				});
			});

			var result = Target.LoadForTeams(new[] {teamId}, true, null);

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirst()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					SiteId = siteId,
					PersonId = personId1,
					AlarmStartTime = "2015-12-16 8:30".Utc()
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					SiteId = siteId,
					PersonId = personId2,
					AlarmStartTime = "2015-12-16 8:00".Utc()
				});
			});

			var result = Target.LoadForSites(new[] {siteId}, null, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeLast()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					SiteId = siteId,
					PersonId = personId1,
					AlarmStartTime = "2015-12-16 8:30".Utc()
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					SiteId = siteId,
					PersonId = personId2,
					AlarmStartTime = "2015-12-16 8:00".Utc()
				});
			});

			var result = Target.LoadForSites(new[] {siteId}, null, false);

			result.First().PersonId.Should().Be(personId1);
			result.Last().PersonId.Should().Be(personId2);
		}

		[Test]
		public void ShouldLoadTeamStatesOrderByLongestAlarmTimeFirst()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId1,
					AlarmStartTime = "2015-12-16 8:30".Utc()
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId2,
					AlarmStartTime = "2015-12-16 8:00".Utc()
				});
			});

			var result = Target.LoadForTeams(new[] {teamId}, null, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadTeamStatesOrderByLongestAlarmTimeLast()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			WithAnalytics.Do(() =>
			{
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId1,
					AlarmStartTime = "2015-12-16 8:30".Utc()
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					TeamId = teamId,
					PersonId = personId2,
					AlarmStartTime = "2015-12-16 8:00".Utc()
				});
			});

			var result = Target.LoadForTeams(new[] {teamId}, null, false);

			result.First().PersonId.Should().Be(personId1);
			result.Last().PersonId.Should().Be(personId2);
		}
	}
}
