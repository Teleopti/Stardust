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
			var dayOffRules = new DayOffRules {DayOffsPerWeek = new MinMax<int>(19, 20)};
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
			var dayOffRules = new DayOffRules { DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveDaysOffValue
				.Should().Be.EqualTo(DayOffRules.CreateDefault().ConsecutiveDayOffs);
		}

		[Test]
		public void ShouldNotReturnRulesForWrongContractFilter()
		{
			var contract = new Contract("_");
			var dayOffRules = new DayOffRules { DayOffsPerWeek = new MinMax<int>(19, 20) };
			dayOffRules.AddFilter(new ContractFilter(contract));
			DayOffRulesRepository.Add(dayOffRules);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));


			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).ConsecutiveWorkdaysValue
				.Should().Be.EqualTo(DayOffRules.CreateDefault().ConsecutiveWorkdays);
		}
	}
}