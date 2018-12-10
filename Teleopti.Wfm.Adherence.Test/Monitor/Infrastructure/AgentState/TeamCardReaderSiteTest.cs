using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderSiteTest
	{
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public MutableNow Now;

		[Test]
		public void ShouldRead()
		{
			var siteId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			Target.Read(siteId).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			var result = Target.Read(siteId).Single();
			result.TeamId.Should().Be(teamId);
			result.InAlarmCount.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyCountAgentsInAlarm()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:06".Utc()
			});

			Target.Read(siteId).Single().InAlarmCount.Should().Be(1);
		}

		[Test]
		public void ShouldIncludeTeamsWithNoAgentsInAlarm()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId
			});

			Target.Read(siteId).Single().InAlarmCount.Should().Be(0);
		}

		[Test]
		public void ShouldNotCountDeletedAgents()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = personId,
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.UpsertNoAssociation(personId);

			Target.Read(siteId).Single().InAlarmCount.Should().Be(1);
		}
	}
}