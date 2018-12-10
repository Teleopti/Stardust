using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class ViewModelBuilderTest2
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

			Target.Build(new AgentStateFilter {SiteIds = new[] {siteId1, siteId2}})
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

			Target.Build(new AgentStateFilter {TeamIds = new[] {teamId1, teamId2}})
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
					PersonId = personId1
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
			Target.Build(new AgentStateFilter {SkillIds = new[] {skillId1, skillId2}})
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

			Target.Build(new AgentStateFilter
			{
				SiteIds = new[] {siteId},
				SkillIds = new[] {skillId1}
			})
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

			Target.Build(new AgentStateFilter
			{
				TeamIds = new[] {teamId},
				SkillIds = new[] {skillId1}
			})
				.States.Single()
				.PersonId.Should().Be(personId1);
		}
	}
}