using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentStatesViewModelBuilderTest2
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;

		[Test]
		public void ShouldGetForSites()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId2
				})
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					SiteId = Guid.NewGuid()
				})
				;

			Target.For(new[] {siteId1, siteId2}, null, null)
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1, personId2);
		}

		[Test]
		public void ShouldGetForTeams()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					TeamId = teamId1
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					TeamId = teamId2
				})
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					TeamId = Guid.NewGuid()
				})
				;

			Target.For(null, new[] {teamId1, teamId2}, null)
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1, personId2);
		}

		[Test]
		public void ShouldGetForSkills()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var skillId1 = Guid.NewGuid();
			var skillId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
				})
				.WithPersonSkill(personId1, skillId1)
				.Has(new AgentStateReadModel
				{
					PersonId = personId2
				})
				.WithPersonSkill(personId2, skillId2)
				.Has(new AgentStateReadModel
				{
					PersonId = personId3
				})
				.WithPersonSkill(personId3, Guid.NewGuid())
				;

			Target.For(null, null, new[] {skillId1, skillId2})
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1, personId2);
		}

		[Test]
		public void ShouldGetForSiteAndSkill()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var skillId1 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId
				})
				.WithPersonSkill(personId1, skillId1)
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId
				})
				.WithPersonSkill(personId2, Guid.NewGuid())
				;

			Target.For(new[] {siteId}, null, new[] {skillId1})
				.States.Single()
				.PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldGetForTeamAndSkill()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skillId1 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					TeamId = teamId
				})
				.WithPersonSkill(personId1, skillId1)
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					TeamId = teamId
				})
				.WithPersonSkill(personId2, Guid.NewGuid())
				;

			Target.For(null, new[] { teamId }, new[] { skillId1 })
				.States.Single()
				.PersonId.Should().Be(personId1);
		}

	}
}