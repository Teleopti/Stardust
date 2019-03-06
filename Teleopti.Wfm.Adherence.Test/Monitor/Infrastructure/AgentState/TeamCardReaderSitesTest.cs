using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderSitesTest
	{
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public MutableNow Now;

		[Test]
		public void ShouldRead()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Now.Is("2016-08-18 08:05".Utc());
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				TeamId = teamId,
				AlarmStartTime = "2016-08-18 08:00".Utc()
			});

			var result = Target.Read().Single();

			result.BusinessUnitId.Should().Be(CurrentBusinessUnit.CurrentId());
			result.SiteId.Should().Be(siteId);
			result.TeamId.Should().Be(teamId);
			result.InAlarmCount.Should().Be(1);
		}
		
	}
}