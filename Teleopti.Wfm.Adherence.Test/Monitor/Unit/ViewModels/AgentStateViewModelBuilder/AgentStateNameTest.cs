using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	public class AgentStateNameTest
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Persister;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		[TestCaseSource(typeof(SelectionFactory), nameof(SelectionFactory.Permutations))]
		public void ShouldGetNamesForEverythingForEverySelection(AgentStateFilter p)
		{
			var personId = Guid.NewGuid();
			var siteId = SelectionFactory.SiteId;
			var teamId = SelectionFactory.TeamId;
			var skillId = SelectionFactory.SkillId;
			Now.Is("2017-01-20 08:00".Utc());
			Database
				.WithAgentNameDisplayedAs("{EmployeeNumber} - {FirstName} {LastName}");

			Persister
				.Has(new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = "Bill",
					LastName = "Gates",
					EmploymentNumber = "123",
					SiteId = siteId,
					SiteName = "London",
					TeamId = teamId,
					TeamName = "Team Perferences",
					IsRuleAlarm = true,
					AlarmStartTime = "2017-01-20 07:50".Utc(),
					StateGroupId = Guid.NewGuid()
				})
				.WithPersonSkill(personId, skillId);

			assert(Target.Build(p).States);
			p.InAlarm = true;
			assert(Target.Build(p).States);
			p.ExcludedStateIds = new Guid?[] {Guid.NewGuid()};
			assert(Target.Build(p).States);
		}

		private static void assert(IEnumerable<AgentStateViewModel> result)
		{
			var model = result.Single();
			model.Name.Should().Be("123 - Bill Gates");
			model.TeamName.Should().Be("Team Perferences");
			model.SiteName.Should().Be("London");
		}
	}

	public static class SelectionFactory
	{
		public static Guid SiteId => new Guid("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
		public static Guid TeamId => new Guid("34590a63-6331-4921-bc9f-9b5e015ab495");
		public static Guid SkillId => new Guid("3f15b334-22d1-4bc1-8e41-72359805d30c");

		public static IEnumerable Permutations
		{
			get
			{
				var permutations = new[]
				{
					new AgentStateFilter {SiteIds = new[] {SiteId}},
					new AgentStateFilter {TeamIds = new[] {TeamId}},
					new AgentStateFilter {SiteIds = new[] {SiteId}, TeamIds = new[] {TeamId}},
					new AgentStateFilter {SkillIds = new[] {SkillId}},
					new AgentStateFilter {SiteIds = new[] {SiteId}, SkillIds = new[] {SkillId}},
					new AgentStateFilter {TeamIds = new[] {TeamId}, SkillIds = new[] {SkillId}},
					new AgentStateFilter {SiteIds = new[] {SiteId}, TeamIds = new[] {TeamId}, SkillIds = new[] {SkillId}}
				};

				return from p in permutations
					   let site = p.SiteIds != null ? "site, " : null
					   let team = p.TeamIds != null ? "team, " : null
					   let skill = p.SkillIds != null ? "skill" : null
					   let name = string.Join(site, team, skill) 
					   select new TestCaseData(p).SetName(name);
			}
		}

		
	}
}