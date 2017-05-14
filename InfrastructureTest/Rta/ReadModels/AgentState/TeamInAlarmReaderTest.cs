using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamInAlarmReaderTest
	{
		public IAgentStateReadModelPersister Persister;
		public ITeamInAlarmReader Target;
		public MutableNow Now;

		[Test]
		public void ShouldRead()
		{
			var siteId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
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
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			var result = Target.Read(siteId).Single();
			result.TeamId.Should().Be(teamId);
			result.Count.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyCountAgentsInAlarm()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:06".Utc()
			});

			Target.Read(siteId).Single().Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotCountDeletedAgents()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = personId,
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.UpsertDeleted(personId, "2016-08-18 08:05".Utc());

			Target.Read(siteId).Single().Count.Should().Be(1);
		}
	}
}