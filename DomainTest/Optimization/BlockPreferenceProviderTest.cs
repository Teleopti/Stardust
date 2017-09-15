using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class BlockPreferenceProviderTest
	{
		public BlockPreferenceProviderUsingFiltersFactory Target;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;

		[Test]
		public void ShouldReturnRulesForContractFilter()
		{
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings {BlockFinderType = BlockFinderType.BetweenDayOff};
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_"));

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForContractFilterIfAgentHaveNoContract()
		{
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldNotReturnRulesForWrongContractFilter()
		{
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));


			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldReturnRulesForSiteFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForSiteFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new SiteFilter(new Site("_")));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldReturnRulesForTeamFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForTeamFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);
			var agent = new Person();

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldUseExplicitFilterWhenDefaultFilterExists()
		{
			PlanningGroupSettingsRepository.Add(PlanningGroupSettings.CreateDefault());
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldDoAndOperationBetweenDifferentFilters()
		{
			var contractNotOnAgent = new Contract("_");
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			planningGroupSettings.AddFilter(new ContractFilter(contractNotOnAgent));

			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldDoOrOperationWithinSameFilter()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldDoOrOperationBetweenTeamAndSite()
		{
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings { BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			planningGroupSettings.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			PlanningGroupSettingsRepository.Add(planningGroupSettings);

			Target.Create().ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}
	}
}