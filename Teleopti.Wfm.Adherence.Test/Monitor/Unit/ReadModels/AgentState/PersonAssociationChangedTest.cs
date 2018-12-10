using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ReadModels.AgentState
{
	[TestFixture]
	[DomainTest]
	public class PersonAssociationChangedTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;
		
		[Test]
		public void ShouldInsertReadModel()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				Timestamp = "2016-10-04 08:10".Utc(),
				ExternalLogons = new[] { new ExternalLogon() }
			});

			Persister.Load(personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMovePersonToNewTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = teamId,
				ExternalLogons = new[] {new ExternalLogon()}
			});

			Persister.Models.Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldMovePersonToNewTeamOnDifferentSite()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				SiteId = siteId,
				ExternalLogons = new[] {new ExternalLogon()}
			});

			Persister.Models.Single().SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldMovePersonTonewTeamOnDifferentBusinessUnit()
		{
			var personId = Guid.NewGuid();
			var businessUnit = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
				BusinessUnitId = businessUnit,
				ExternalLogons = new[] {new ExternalLogon()}
			});

			Persister.Models.Single().BusinessUnitId.Should().Be(businessUnit);
		}

		[Test]
		public void ShouldUpdateWithNames()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				TeamName = "team",
				SiteId = Guid.NewGuid(),
				SiteName = "site",
				ExternalLogons = new[] {new ExternalLogon()}
			});

			Persister.Models.Single().TeamName.Should().Be("team");
			Persister.Models.Single().SiteName.Should().Be("site");
		}
	}
}
