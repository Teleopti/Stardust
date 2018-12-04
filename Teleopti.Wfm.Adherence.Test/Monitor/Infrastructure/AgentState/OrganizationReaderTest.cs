using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class OrganizationReaderTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public IOrganizationReader Target;

		[Test]
		public void ShouldReadOrganization()
		{
			var team = Guid.NewGuid();
			var site = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
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
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				TeamId = team1,
				TeamName = "team1",
				SiteId = site,
				SiteName = "Site"
			});
			Persister.Upsert(new AgentStateReadModelForTest
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
		public void ShouldNotReadAgentsWithoutAssociation()
		{
			var team = Guid.NewGuid();
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = site,
				TeamId = team,
			});
			Persister.UpsertNoAssociation(person);

			var result = Target.Read();

			result.Should().Be.Empty();
		}
		
	}

}