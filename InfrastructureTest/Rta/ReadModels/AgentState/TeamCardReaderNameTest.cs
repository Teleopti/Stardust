using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderNameTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldIncludeTeamName()
		{
			var team = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
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
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				SiteId = site,
				SiteName = "site"
			});

			Target.Read().Single()
				.SiteName.Should().Be("site");
		}

	}
}