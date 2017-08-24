using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class OrganizationReaderTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public IOrganizationReader Target;
		public Database Database;

		[Test]
		public void ShouldReadOrganization()
		{
			var team = Guid.NewGuid();
			var site = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				TeamId = team,
				TeamName = "team",
				SiteId = site,
				SiteName = "Site"
			});

			var result = Target.Read().Single();

			result.SiteId.Should().Be(site);
			result.SiteName.Should().Be("Site");
			result.Teams.Single().TeamId.Should().Be(team);
			result.Teams.Single().TeamName.Should().Be("team");
		}

		[Test]
		public void ShouldReadOrganizationWithMultipleTeams()
		{
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			var site = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				TeamId = team1,
				TeamName = "team1",
				SiteId = site,
				SiteName = "Site"
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				TeamId = team2,
				TeamName = "team2",
				SiteId = site,
				SiteName = "Site"
			});

			var result = Target.Read().Single();

			result.Teams.Select(x => x.TeamId).Should().Have.SameValuesAs(team1, team2);
			result.Teams.Select(x => x.TeamName).Should().Have.SameValuesAs("team1", "team2");
		}

		[Test]
		public void ShouldNotReadDeleted()
		{
			var team = Guid.NewGuid();
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = person,
				TeamId = team,
				SiteId = site,
			});
			Persister.UpsertDeleted(person);

			var result = Target.Read();

			result.Should().Be.Empty();
		}

	}

}