﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class SiteCardViewModelBuilderTest : ISetup
	{
		public SiteCardViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeSiteRepository Sites;
		public FakeNumberOfAgentsInSiteReader AgentsInSite;
		public MutableNow Now;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldBuild()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId =Guid.NewGuid();
			
			Database
				.WithSite(siteId, "London")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					SiteName = "London",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc(),
				})
				.WithAgent(personId);

			var viewModel = Target.Build().Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Name.Should().Be("London");
			viewModel.AgentsCount.Should().Be(1);
			viewModel.InAlarmCount.Should().Be(1);
			viewModel.Color.Should().Be("danger");
		}
		[Test]
		public void ShouldBuildForSkill()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var  siteId = Guid.NewGuid();
			Database
				.WithSite(siteId, "London")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					SiteName = "London",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc(),
				})
				.WithAgent(personId)
				.WithSkill(skill);

			var viewModel = Target.Build(new[] { skill }).Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Name.Should().Be("London");
			viewModel.AgentsCount.Should().Be(1);
			viewModel.InAlarmCount.Should().Be(1);
			viewModel.Color.Should().Be("danger");

		}

		[Test]
		public void ShouldBuildForMultipleSitesForSkill()
		{
			Now.Is("2016-06-21 08:30");
			var businessUnitId = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var skill = Guid.NewGuid();


			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			Database
				.WithSite(siteId1)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId1,
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId)
				.WithSkill(skill)
				.WithSite(siteId2)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = businessUnitId,
					SiteId = siteId2,
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId2)
				.WithSkill(skill)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = businessUnitId,
					SiteId = siteId2,
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId3)
				.WithSkill(skill);

			var result = Target.Build(new[] { skill });

			result.Single(x => x.Id == siteId1).InAlarmCount.Should().Be(1);
			result.Single(x => x.Id == siteId2).InAlarmCount.Should().Be(2);
		}

		[Test]
		public void ShouldBuildForSkillWihNoAgentsInAlarm()
		{

			var skill = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var site = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					TeamId = Guid.NewGuid(),
					SiteId = site,
				})
				.WithAgent(personId)
				.WithSkill(skill);

			var viewModel = Target.Build(new[] { skill }).Single();

			viewModel.Id.Should().Be(site);
			viewModel.InAlarmCount.Should().Be(0);
		}

		[Test]
		public void ShouldNotCountSameAgentTwiceForSkillArea()
		{
			Now.Is("2016-06-21 08:30");
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId)
				.WithSkill(skill1)
				.WithSkill(skill2);

			var viewModel = Target.Build(new[] { skill1, skill2 }).Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.InAlarmCount.Should().Be(1);
		}

		[Test]
		public void ShouldCountAgentsForSkill()
		{
			Now.Is("2016-06-21 08:30");
			var personWithSkill = Guid.NewGuid();
			var personWithoutSkill = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personWithSkill,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personWithSkill)
				.WithSkill(skill1)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personWithoutSkill,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personWithoutSkill)
				.WithSkill(skill2);

			var viewModel = Target.Build(new[] { skill1 }).Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.AgentsCount.Should().Be(1);
		}

		[Test]
		public void ShouldBuildForMultipleSitesForSkillOrderSitesName()
		{

			Now.Is("2016-06-21 08:30");
			var businessUnitId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var siteId3 = Guid.NewGuid();
			var skill = Guid.NewGuid();


			Database
				.WithSite(siteId1, "B")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId1,
					BusinessUnitId = businessUnitId,
					SiteId = siteId1,
					SiteName = "B",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId1)
				.WithSkill(skill)
				.WithSite(siteId2, "C")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = businessUnitId,
					SiteId = siteId2,
					SiteName = "C",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId2)
				.WithSkill(skill)
				.WithSite(siteId3, "A")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = businessUnitId,
					SiteId = siteId3,
					SiteName = "A",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId3)
				.WithSkill(skill);

			var result = Target.Build(new[] { skill });
			result.Select(x => x.Id)
				.Should().Have.SameSequenceAs(new[]
				{
					siteId3,
					siteId1,
					siteId2
				});
		}

		[Test]
		public void ShouldBuildForMultipleSitesForSkillOrderSitesNameAccordingToSwedishName()
		{


			Now.Is("2016-06-21 08:30");
			var businessUnitId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var siteId3 = Guid.NewGuid();
			var skill = Guid.NewGuid();


			Database
				.WithSite(siteId1, "Å")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId1,
					BusinessUnitId = businessUnitId,
					SiteId = siteId1,
					SiteName = "Å",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId1)
				.WithSkill(skill)
				.WithSite(siteId2, "Ä")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = businessUnitId,
					SiteId = siteId2,
					SiteName = "Ä",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId2)
				.WithSkill(skill)
				.WithSite(siteId3, "A")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = businessUnitId,
					SiteId = siteId3,
					SiteName = "A",
					TeamId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc(),
				})
				.WithAgent(personId3)
				.WithSkill(skill);

			var result = Target.Build(new[] { skill });
			result.Select(x => x.Id)
				.Should().Have.SameSequenceAs(new[]
				{
					siteId3,
					siteId1,
					siteId2
				});

		}
	}
}