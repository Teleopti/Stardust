﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class DayOffOptimizationPreferenceProviderTest
	{
		public DayOffOptimizationPreferenceProviderUsingFiltersFactory Target;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;

		[Test]
		public void ShouldReturnRulesForContractFilter()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings(planningGroup) { DayOffsPerWeek = new MinMax<int>(19, 20)};
			dayOffRules.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900,1,1));
			agent.Period(new DateOnly(2000,1,1)).PersonContract = new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_"));

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).DaysOffPerWeekValue
				.Should().Be.EqualTo(new MinMax<int>(19, 20));
		}

		[Test]
		public void ShouldNotReturnRulesForContractFilterIfAgentHaveNoContract()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldNotReturnRulesForWrongContractFilter()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));


			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveWorkdays);
		}

		[Test]
		public void ShouldReturnRulesForSiteFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(5, 7) };
			dayOffRules.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000,1,1)).Team.Site));
			PlanningGroupSettingsRepository.Add(dayOffRules);
		
			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(5, 7));
		}

		[Test]
		public void ShouldNotReturnRulesForSiteFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroup = new PlanningGroup();
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new SiteFilter(new Site("_")));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldReturnRulesForTeamFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(dayOffRules);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(6, 7));
		}

		[Test]
		public void ShouldNotReturnRulesForTeamFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroup = new PlanningGroup();
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  DayOffsPerWeek = new MinMax<int>(1, 1) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldUseExplicitFilterWhenDefaultFilterExists()
		{
			var planningGroup = new PlanningGroup();
			PlanningGroupSettingsRepository.Add(PlanningGroupSettings.CreateDefault());
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(dayOffRules);
			
			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(6, 7));
		}

		[Test]
		public void ShouldDoAndOperationBetweenDifferentFilters()
		{
			var planningGroup = new PlanningGroup();
			var contractNotOnAgent = new Contract("_");
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			dayOffRules.AddFilter(new ContractFilter(contractNotOnAgent));

			PlanningGroupSettingsRepository.Add(dayOffRules);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveWorkdays);	
		}

		[Test]
		public void ShouldDoOrOperationWithinSameFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(1, 2) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(dayOffRules);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(1, 2));
		}

		[Test]
		public void ShouldDoOrOperationBetweenTeamAndSite()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(1, 2) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			dayOffRules.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			PlanningGroupSettingsRepository.Add(dayOffRules);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(1, 2));
		}

		[Test]
		public void ShouldSelectDayOfffRuleWithHighestPriority()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(1, 2) , Priority = 1};
			var dayOffRules2 = new PlanningGroupSettings(planningGroup) {  ConsecutiveWorkdays = new MinMax<int>(1, 3), Priority = 2 };


			dayOffRules.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			dayOffRules2.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));

			PlanningGroupSettingsRepository.Add(dayOffRules);
			PlanningGroupSettingsRepository.Add(dayOffRules2);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(1, 3));
		}

		[Test]
		public void ShouldMapPropertiesForDayOffRules()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings(planningGroup)
			{
				ConsecutiveWorkdays = new MinMax<int>(1, 2),
				DayOffsPerWeek = new MinMax<int>(3, 4),
				ConsecutiveDayOffs = new MinMax<int>(5, 6),
				FullWeekendsOff = new MinMax<int>(7, 8),
				WeekendDaysOff = new MinMax<int>(9, 10)
			};
			PlanningGroupSettingsRepository.Add(dayOffRules);

			var daysOffPreferences = Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1));
			daysOffPreferences.ConsecutiveWorkdaysValue.Should().Be.EqualTo(new MinMax<int>(1, 2));
			daysOffPreferences.DaysOffPerWeekValue.Should().Be.EqualTo(new MinMax<int>(3, 4));
			daysOffPreferences.ConsecutiveDaysOffValue.Should().Be.EqualTo(new MinMax<int>(5, 6));
			daysOffPreferences.FullWeekendsOffValue.Should().Be.EqualTo(new MinMax<int>(7, 8));
			daysOffPreferences.WeekEndDaysOffValue.Should().Be.EqualTo(new MinMax<int>(9, 10));
			daysOffPreferences.UseConsecutiveDaysOff.Should().Be.True();
			daysOffPreferences.UseConsecutiveWorkdays.Should().Be.True();
			daysOffPreferences.ConsiderWeekAfter.Should().Be.True();
			daysOffPreferences.ConsiderWeekBefore.Should().Be.True();
			daysOffPreferences.UseDaysOffPerWeek.Should().Be.True();
			daysOffPreferences.UseFullWeekendsOff.Should().Be.True();
			daysOffPreferences.UseWeekEndDaysOff.Should().Be.True();
		}
	}
}