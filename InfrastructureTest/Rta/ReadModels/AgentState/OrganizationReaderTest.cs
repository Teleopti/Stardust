using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[DatabaseTest]
	[ExtendScope(typeof(PersonAssociationChangedEventPublisher))]
	[ExtendScope(typeof(AgentStateReadModelMaintainer))]
	[ExtendScope(typeof(UpdateGroupingReadModelHandler))]
	public class OrganizationReaderSkillsTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public IOrganizationReader Target;
		public Database Database;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldReadTeamWithSkill()
		{
			Database
				.WithSite()
				.WithTeam()
				.WithAgent()
				.WithTeam()
				.WithAgent()
				.WithSkill("phone")
				;
			var team2 = Database.CurrentTeamId();
			var phone = Database.SkillIdFor("phone");

			var result = UnitOfWork.Get(() => Target.Read(phone.AsArray()));

			result.Single().Teams.Single().Id.Should().Be(team2);
		}

	}

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

			result.Id.Should().Be(site);
			result.Name.Should().Be("Site");
			result.Teams.Single().Id.Should().Be(team);
			result.Teams.Single().Name.Should().Be("team");
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

			result.Teams.Select(x => x.Id).Should().Have.SameValuesAs(team1, team2);
			result.Teams.Select(x => x.Name).Should().Have.SameValuesAs("team1", "team2");
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
			Persister.UpsertDeleted(person, DateTime.UtcNow);

			var result = Target.Read();

			result.Should().Be.Empty();
		}

	}

}