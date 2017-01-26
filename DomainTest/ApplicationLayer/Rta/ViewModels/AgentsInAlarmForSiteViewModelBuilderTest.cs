using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentsInAlarmForSiteViewModelBuilderTest:ISetup
	{
		public AgentsInAlarmForSiteViewModelBuilder Target;
		public FakeSiteInAlarmReader Database;
		public FakeSiteRepository Sites;
		public MutableNow Now;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

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
		public void ShouldBuildForMultipleSitesForSkill()
		{
			Now.Is("2016-06-21 08:30");
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var skill = Guid.NewGuid();
			Sites.Has(siteId1);
			Sites.Has(siteId2);
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId1,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.OnSkill(skill)
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId2,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.OnSkill(skill)
				.Has(new AgentStateReadModel
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId2,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.OnSkill(skill);

			var result = Target.ForSkills(new[] {skill});

			result.Single(x => x.Id == siteId1).OutOfAdherence.Should().Be(1);
			result.Single(x => x.Id == siteId2).OutOfAdherence.Should().Be(2);
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


		[Test]
		public void ShouldBuildForMultipleSitesForSkillOrderSitesName()
		{
		
			var site1 = new Site("B").WithId();
			var site2 = new Site("C").WithId();
			var site3 = new Site("A").WithId();
			var skill = Guid.NewGuid();

			Sites.Has(site1);
			Sites.Has(site2);
			Sites.Has(site3);

			var result = Target.ForSkills(new[] { skill });
			result.Select(x => x.Id)
				.Should().Have.SameSequenceAs(new[] 
				{
					site3.Id.GetValueOrDefault(),
					site1.Id.GetValueOrDefault(),
					site2.Id.GetValueOrDefault()
				});
		
		}

		[Test]
		public void ShouldBuildForMultipleSitesForSkillOrderSitesNameAccordingToSwedishName()
		{
	
			var site1 = new Site("�").WithId(); 
			var site2 = new Site("�").WithId(); 
			var site3 = new Site("A").WithId(); 
			var skill = Guid.NewGuid();

			Sites.Has(site2);
			Sites.Has(site1);
			Sites.Has(site3);

			UiCulture.IsSwedish();

			var result = Target.ForSkills(new[] { skill });
			result.Select(x => x.Id)
				.Should().Have.SameSequenceAs(new[]
				{
					site3.Id.GetValueOrDefault(),
					site1.Id.GetValueOrDefault(),
					site2.Id.GetValueOrDefault()
				});

		}
	}

}