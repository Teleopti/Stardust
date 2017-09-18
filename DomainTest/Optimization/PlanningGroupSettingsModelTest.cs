using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
	public class PlanningGroupSettingsModelTest
	{
		public IFetchPlanningGroupSettingsModel Target;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;

		[Test]
		public void ShouldIncludePersistedDayOffRuleWhenLoadingAll()
		{
			var presentInDb = new PlanningGroupSettings
			{
				DayOffsPerWeek = new MinMax<int>(2, 3),
				ConsecutiveDayOffs = new MinMax<int>(1, 5),
				ConsecutiveWorkdays = new MinMax<int>(2, 3),
				Name = RandomName.Make()
			}.WithId();
			PlanningGroupSettingsRepository.Add(presentInDb);

			var loaded = Target.FetchAllWithoutPlanningGroup().Single(x => !x.Default);

			loaded.MinDayOffsPerWeek.Should().Be.EqualTo(presentInDb.DayOffsPerWeek.Minimum);
			loaded.MaxDayOffsPerWeek.Should().Be.EqualTo(presentInDb.DayOffsPerWeek.Maximum);
			loaded.MinConsecutiveDayOffs.Should().Be.EqualTo(presentInDb.ConsecutiveDayOffs.Minimum);
			loaded.MaxConsecutiveDayOffs.Should().Be.EqualTo(presentInDb.ConsecutiveDayOffs.Maximum);
			loaded.MinConsecutiveWorkdays.Should().Be.EqualTo(presentInDb.ConsecutiveWorkdays.Minimum);
			loaded.MaxConsecutiveWorkdays.Should().Be.EqualTo(presentInDb.ConsecutiveWorkdays.Maximum);
			loaded.Id.Should().Be.EqualTo(presentInDb.Id);
			loaded.Default.Should().Be.False();
			loaded.Name.Should().Be.EqualTo(presentInDb.Name);

		}

		[Test]
		public void ShouldReturnDefaultRuleWhenNotExistsInDb()
		{
			var loaded = Target.FetchAllWithoutPlanningGroup().Single(x => x.Default);
			loaded.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAddExtraDefaultRuleIfPresentInDb()
		{
			var presentDefaultRule = PlanningGroupSettings.CreateDefault().WithId();
      PlanningGroupSettingsRepository.Add(presentDefaultRule);

			var loaded = Target.FetchAllWithoutPlanningGroup().Single();

			loaded.Id.Should().Be.EqualTo(presentDefaultRule.Id);
		}

		[Test]
		public void ShouldIncludeContractFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var contract = new Contract(filterName).WithId();
			var contractFilter = new ContractFilter(contract);
			var dayOffRule = new PlanningGroupSettings().WithId();
			dayOffRule.AddFilter(contractFilter);
			PlanningGroupSettingsRepository.Add(dayOffRule);

			var loaded = Target.FetchAllWithoutPlanningGroup().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.ContractFilterType);
			filter.Id.Should().Be.EqualTo(contract.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeTeamFilterWhenFetching()
		{
			var team = new Team().WithId().WithDescription(new Description(RandomName.Make()));
			team.Site = new Site(RandomName.Make());
			var teamFilter = new TeamFilter(team);
			var dayOffRule = new PlanningGroupSettings().WithId();
			dayOffRule.AddFilter(teamFilter);
			PlanningGroupSettingsRepository.Add(dayOffRule);

			var loaded = Target.FetchAllWithoutPlanningGroup().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.TeamFilterType);
			filter.Id.Should().Be.EqualTo(team.Id.Value);
			filter.Name.Should().Be.EqualTo(team.SiteAndTeam);
		}

		[Test]
		public void ShouldIncludeSiteFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var site = new Site(filterName).WithId();
			var siteFilter = new SiteFilter(site);
			var dayOffRule = new PlanningGroupSettings().WithId();
			dayOffRule.AddFilter(siteFilter);
			PlanningGroupSettingsRepository.Add(dayOffRule);

			var loaded = Target.FetchAllWithoutPlanningGroup().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SiteFilterType);
			filter.Id.Should().Be.EqualTo(site.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}


		[Test]
		public void ShouldFetchDayOffRule()
		{
			var curr = new PlanningGroupSettings().WithId();
			PlanningGroupSettingsRepository.Add(curr);
			curr.ConsecutiveWorkdays = new MinMax<int>(1,1);
			curr.ConsecutiveDayOffs = new MinMax<int>(2,2);
			curr.DayOffsPerWeek = new MinMax<int>(3,3);
			curr.Name = RandomName.Make();

			var dayOffRulesModel = Target.Fetch(curr.Id.Value);
			dayOffRulesModel.MinDayOffsPerWeek.Should().Be.EqualTo(curr.DayOffsPerWeek.Minimum);
			dayOffRulesModel.MaxDayOffsPerWeek.Should().Be.EqualTo(curr.DayOffsPerWeek.Maximum);
			dayOffRulesModel.MinConsecutiveDayOffs.Should().Be.EqualTo(curr.ConsecutiveDayOffs.Minimum);
			dayOffRulesModel.MaxConsecutiveDayOffs.Should().Be.EqualTo(curr.ConsecutiveDayOffs.Maximum);
			dayOffRulesModel.MinConsecutiveWorkdays.Should().Be.EqualTo(curr.ConsecutiveWorkdays.Minimum);
			dayOffRulesModel.MaxConsecutiveWorkdays.Should().Be.EqualTo(curr.ConsecutiveWorkdays.Maximum);
			dayOffRulesModel.Id.Should().Be.EqualTo(curr.Id);
			dayOffRulesModel.Default.Should().Be.False();
			dayOffRulesModel.Name.Should().Be.EqualTo(curr.Name);
		}

		[Test]
		public void ShouldThrowIfFetchNonExisting()
		{
			Assert.Throws<ArgumentException>(() => Target.Fetch(Guid.NewGuid()));
		} 
	}
}