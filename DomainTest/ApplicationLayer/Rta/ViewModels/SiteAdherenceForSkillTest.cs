using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class SiteAdherenceForSkillTest
	{
		public AgentsInAlarmForSiteViewModelBuilder Target;
		public FakeSiteInAlarmReader Database;
		public FakeSiteRepository Sites;
		public MutableNow Now;
		
		[Test]
		public void ShouldBuildForSkill()
		{
			Now.Is("2016-06-21 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var skill = Guid.NewGuid();			
			Sites.Has(siteId);
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.OnSkill(skill);

			var viewModel = Target.ForSkills(new[] {skill}).Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldBuildForSkillWihNoAgentsInAlarm()
		{
			var skill = Guid.NewGuid();
			var site = Guid.NewGuid();
			Sites.Has(site);
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					SiteId = site,
				})
				.OnSkill(skill);

			var viewModel = Target.ForSkills(new[] { skill }).Single();

			viewModel.Id.Should().Be(site);
			viewModel.OutOfAdherence.Should().Be(0);
		}


		[Test]
		public void ShouldNotCountSameAgentTwiceForSkillArea()
		{
			Now.Is("2016-06-21 08:30");
			var personId1 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			Sites.Has(siteId);
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.OnSkill(skill1)
				.OnSkill(skill2)
				;

			var viewModel = Target.ForSkills(new[] { skill1, skill2 }).Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.OutOfAdherence.Should().Be(1);
		}
	}
}