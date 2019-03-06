using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderNameTest
	{
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldIncludeTeamName()
		{
			var team = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = Guid.NewGuid(),
				TeamId = team,
				TeamName = "team"
			});

			Target.Read().Single()
				.TeamName.Should().Be("team");
		}

		[Test]
		public void ShouldIncludeSiteName()
		{
			var site = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = Guid.NewGuid(),
				SiteId = site,
				SiteName = "site"
			});

			Target.Read().Single()
				.SiteName.Should().Be("site");
		}

	}
}