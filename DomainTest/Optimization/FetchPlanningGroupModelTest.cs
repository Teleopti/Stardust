using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class FetchPlanningGroupModelTest
	{
		public IFetchPlanningGroupModel Target;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldIncludePersistedPlanningGroupWhenLoadingAll()
		{
			var presentInDb = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(presentInDb);

			var loaded = Target.FetchAll().Single();

			loaded.Id.Should().Be.EqualTo(presentInDb.Id);
			loaded.Name.Should().Be.EqualTo(presentInDb.Name);

		}

		[Test]
		public void ShouldIncludeContractFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var contract = new Contract(filterName).WithId();
			var planningGroup = new PlanningGroup()
				.WithId()
				.AddFilter(new ContractFilter(contract));
			PlanningGroupRepository.Add(planningGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(planningGroup.Id.GetValueOrDefault()));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.ContractFilterType);
			filter.Id.Should().Be.EqualTo(contract.Id.GetValueOrDefault());
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeTeamFilterWhenFetching()
		{
			var team = new Team().WithId().WithDescription(new Description(RandomName.Make()));
			team.Site = new Site(RandomName.Make());
			var planningGroup = new PlanningGroup()
				.WithId()
				.AddFilter(new TeamFilter(team));
			PlanningGroupRepository.Add(planningGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(planningGroup.Id.GetValueOrDefault()));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.TeamFilterType);
			filter.Id.Should().Be.EqualTo(team.Id.GetValueOrDefault());
			filter.Name.Should().Be.EqualTo(team.SiteAndTeam);
		}

		[Test]
		public void ShouldIncludeSiteFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var site = new Site(filterName).WithId();
			var planningGroup = new PlanningGroup().WithId().AddFilter(new SiteFilter(site));
			PlanningGroupRepository.Add(planningGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(planningGroup.Id.GetValueOrDefault()));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SiteFilterType);
			filter.Id.Should().Be.EqualTo(site.Id.GetValueOrDefault());
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeSkillFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var skill = new Skill(filterName).WithId();
			var planningGroup = new PlanningGroup()
				.WithId()
				.AddFilter(new SkillFilter(skill));
			PlanningGroupRepository.Add(planningGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(planningGroup.Id.GetValueOrDefault()));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SkillFilterType);
			filter.Id.Should().Be.EqualTo(skill.Id.GetValueOrDefault());
			filter.Name.Should().Be.EqualTo(filterName);
		}


		[Test]
		public void ShouldFetchPlanningGroup()
		{
			var planningGroup = new PlanningGroup().WithId();
			PlanningGroupRepository.Add(planningGroup);

			var planningGroupModel = Target.Fetch(planningGroup.Id.GetValueOrDefault());
			planningGroupModel.Id.Should().Be.EqualTo(planningGroup.Id);
			planningGroupModel.Name.Should().Be.EqualTo(planningGroup.Name);
		}
		
		[Test]
		public void ShouldFetchPreferenceValue()
		{
			var planningGroup = new PlanningGroup().WithId();
			planningGroup.SetGlobalValues(new Percent(0.22));
			PlanningGroupRepository.Add(planningGroup);

			var planningGroupModel = Target.Fetch(planningGroup.Id.GetValueOrDefault());
			planningGroupModel.PreferenceValue.Should().Be.EqualTo(planningGroup.Settings.PreferenceValue.Value);
		}

		[Test]
		public void ShouldThrowIfFetchNonExisting()
		{
			Assert.Throws<ArgumentException>(() => Target.Fetch(Guid.NewGuid()));
		}
	}
}