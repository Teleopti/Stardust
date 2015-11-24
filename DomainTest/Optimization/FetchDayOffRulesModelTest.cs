using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
	public class FetchDayOffRulesModelTest
	{
		public IFetchDayOffRulesModel Target;
		public FakeDayOffRulesRepository DayOffRulesRepository;

		[Test]
		public void ShouldFetchDefaultSettingsIfNotExists()
		{
			var defaultSettings = Target.FetchDefaultRules();
			defaultSettings.MinDayOffsPerWeek.Should().Be.EqualTo(1);
			defaultSettings.MaxDayOffsPerWeek.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveDayOffs.Should().Be.EqualTo(1);
			defaultSettings.MaxConsecutiveDayOffs.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveWorkdays.Should().Be.EqualTo(2);
			defaultSettings.MaxConsecutiveWorkdays.Should().Be.EqualTo(6);
			defaultSettings.Id.Should().Be.EqualTo(Guid.Empty);
			defaultSettings.Default.Should().Be.True();
			defaultSettings.Name.Should().Be.EqualTo(UserTexts.Resources.Default);
		}

		[Test]
		public void ShouldFetchCurrentDefaultSettings()
		{
			var curr = DayOffRules.CreateDefault().WithId();
			DayOffRulesRepository.Add(curr);
			
			var defaultSettings = Target.FetchDefaultRules();
			defaultSettings.MinDayOffsPerWeek.Should().Be.EqualTo(1);
			defaultSettings.MaxDayOffsPerWeek.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveDayOffs.Should().Be.EqualTo(1);
			defaultSettings.MaxConsecutiveDayOffs.Should().Be.EqualTo(3);
			defaultSettings.MinConsecutiveWorkdays.Should().Be.EqualTo(2);
			defaultSettings.MaxConsecutiveWorkdays.Should().Be.EqualTo(6);
			defaultSettings.Id.Should().Be.EqualTo(curr.Id);
			defaultSettings.Default.Should().Be.True();
			defaultSettings.Name.Should().Be.EqualTo(UserTexts.Resources.Default);
		}

		[Test]
		public void ShouldIncludePersistedDayOffRuleWhenLoadingAll()
		{
			var presentInDb = new DayOffRules
			{
				DayOffsPerWeek = new MinMax<int>(2, 3),
				ConsecutiveDayOffs = new MinMax<int>(1, 5),
				ConsecutiveWorkdays = new MinMax<int>(2, 3),
				Name = RandomName.Make()
			}.WithId();
			DayOffRulesRepository.Add(presentInDb);

			var loaded = Target.FetchAll().Single(x => !x.Default);

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
			var loaded = Target.FetchAll().Single(x => x.Default);
			loaded.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAddExtraDefaultRuleIfPresentInDb()
		{
			var presentDefaultRule = DayOffRules.CreateDefault().WithId();
      DayOffRulesRepository.Add(presentDefaultRule);

			var loaded = Target.FetchAll().Single();

			loaded.Id.Should().Be.EqualTo(presentDefaultRule.Id);
		}

		[Test]
		public void ShouldIncludeContractFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var contract = new Contract(filterName).WithId();
			var contractFilter = new ContractFilter(contract);
			var dayOffRule = new DayOffRules().WithId();
			dayOffRule.AddFilter(contractFilter);
			DayOffRulesRepository.Add(dayOffRule);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.ContractFilterType);
			filter.Id.Should().Be.EqualTo(contract.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeTeamFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var team = new Team {Description = new Description(filterName)}.WithId();
			var teamFilter = new TeamFilter(team);
			var dayOffRule = new DayOffRules().WithId();
			dayOffRule.AddFilter(teamFilter);
			DayOffRulesRepository.Add(dayOffRule);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.TeamFilterType);
			filter.Id.Should().Be.EqualTo(team.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeSiteFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var site = new Site(filterName).WithId();
			var siteFilter = new SiteFilter(site);
			var dayOffRule = new DayOffRules().WithId();
			dayOffRule.AddFilter(siteFilter);
			DayOffRulesRepository.Add(dayOffRule);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(dayOffRule.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SiteFilterType);
			filter.Id.Should().Be.EqualTo(site.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}


		[Test]
		public void ShouldFetchDayOffRule()
		{
			var curr = new DayOffRules().WithId();
			DayOffRulesRepository.Add(curr);
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