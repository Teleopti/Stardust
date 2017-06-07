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
	public class TeamsInAlarmReaderSitesTest
	{
		public IAgentStateReadModelPersister Persister;
		public ITeamsInAlarmReader Target;
		public MutableNow Now;

		[Test]
		public void ShouldRead()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			var result = Target.Read().Single();

			result.BusinessUnitId.Should().Be(businessUnitId);
			result.SiteId.Should().Be(siteId);
			result.TeamId.Should().Be(teamId);
			result.InAlarmCount.Should().Be(1);
		}
		
	}
}