using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class InAlarmTest
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetForSites()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1,
					AlarmStartTime = "2016-11-07 08:00".Utc()
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

			Target.Build(new AgentStateFilter {SiteIds = new []{siteId1}, InAlarm = true})
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldGetForTeams()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					TeamId = teamId1,
					AlarmStartTime = "2016-11-07 08:00".Utc()
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

			Target.Build(new AgentStateFilter { TeamIds = new[] { teamId1 }, InAlarm = true})
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldGetForSkills()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var skillId1 = Guid.NewGuid();
			var skillId2 = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					AlarmStartTime = "2016-11-07 08:00".Utc()
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
			Target.Build(new AgentStateFilter { SkillIds = new[] { skillId1 }, InAlarm = true})
				.States.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldGetForSiteAndSkill()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(personId1, skillId)
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(personId2, Guid.NewGuid())
				;

			Target.Build(new AgentStateFilter {SiteIds = new[] {siteId}, SkillIds = new[] {skillId}, InAlarm = true})
				.States.Single()
				.PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldGetForTeamAndSkill()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(personId1, skillId)
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(personId2, Guid.NewGuid())
				;

			Target.Build(new AgentStateFilter { TeamIds = new[] { teamId }, SkillIds = new[] { skillId }, InAlarm = true})
				.States.Single()
				.PersonId.Should().Be(personId1);
		}

	}
}