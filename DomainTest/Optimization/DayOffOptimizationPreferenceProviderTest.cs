using NUnit.Framework;
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
		public FakeDayOffRulesRepository DayOffRulesRepository;

		[Test]
		public void ShouldReturnRulesForContractFilter()
		{
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings {DayOffsPerWeek = new MinMax<int>(19, 20)};
			dayOffRules.AddFilter(new ContractFilter(contract));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900,1,1));
			agent.Period(new DateOnly(2000,1,1)).PersonContract = new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_"));

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).DaysOffPerWeekValue
				.Should().Be.EqualTo(new MinMax<int>(19, 20));
		}

		[Test]
		public void ShouldNotReturnRulesForContractFilterIfAgentHaveNoContract()
		{
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings { DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldNotReturnRulesForWrongContractFilter()
		{
			var contract = new Contract("_");
			var dayOffRules = new PlanningGroupSettings { DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));


			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveWorkdays);
		}

		[Test]
		public void ShouldReturnRulesForSiteFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(5, 7) };
			dayOffRules.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000,1,1)).Team.Site));
			DayOffRulesRepository.Add(dayOffRules);
		
			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(5, 7));
		}

		[Test]
		public void ShouldNotReturnRulesForSiteFilterIfAgentHaveNoPersonPeriod()
		{
			var dayOffRules = new PlanningGroupSettings { DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new SiteFilter(new Site("_")));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldReturnRulesForTeamFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			DayOffRulesRepository.Add(dayOffRules);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(6, 7));
		}

		[Test]
		public void ShouldNotReturnRulesForTeamFilterIfAgentHaveNoPersonPeriod()
		{
			var dayOffRules = new PlanningGroupSettings { DayOffsPerWeek = new MinMax<int>(1, 1) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldUseExplicitFilterWhenDefaultFilterExists()
		{
			DayOffRulesRepository.Add(PlanningGroupSettings.CreateDefault());
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			DayOffRulesRepository.Add(dayOffRules);
			
			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(6, 7));
		}

		[Test]
		public void ShouldDoAndOperationBetweenDifferentFilters()
		{
			var contractNotOnAgent = new Contract("_");
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(6, 7) };
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			dayOffRules.AddFilter(new ContractFilter(contractNotOnAgent));

			DayOffRulesRepository.Add(dayOffRules);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().ConsecutiveWorkdays);	
		}

		[Test]
		public void ShouldDoOrOperationWithinSameFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(1, 2) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			dayOffRules.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			DayOffRulesRepository.Add(dayOffRules);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(1, 2));
		}

		[Test]
		public void ShouldDoOrOperationBetweenTeamAndSite()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var dayOffRules = new PlanningGroupSettings { ConsecutiveWorkdays = new MinMax<int>(1, 2) };
			dayOffRules.AddFilter(new TeamFilter(new Team()));
			dayOffRules.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			DayOffRulesRepository.Add(dayOffRules);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(new MinMax<int>(1, 2));
		}
	}
}