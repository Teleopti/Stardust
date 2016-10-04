using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class SiteInAlarmFromAgentStatesReadModelTest
	{
		public IAgentStateReadModelPersister Persister;
		public ISiteInAlarmReader Target;
		public MutableNow Now;

		[Test]
		public void ShouldRead()
		{
			Now.Is("2016-08-18 08:05".Utc());
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			Target.Read().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			var siteId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			var result = Target.Read().Single();
			result.SiteId.Should().Be(siteId);
			result.Count.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyCountAgentsInAlarm()
		{
			var siteId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:06".Utc()
			});

			Target.Read().Single().Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotCountDeletedAgents()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:05".Utc()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId,
				SiteId = siteId,
				AlarmStartTime = "2016-08-18 08:05".Utc(),
			});
			Persister.SetDeleted(personId, "2016-08-18 08:05".Utc());

			Target.Read().Single().Count.Should().Be(1);
		}
	}
}