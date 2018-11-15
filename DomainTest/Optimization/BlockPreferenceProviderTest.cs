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

		[Test]
		public void ShouldReturnRulesForContractFilter()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings() { BlockFinderType = BlockFinderType.BetweenDayOff};
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			planningGroup.AddSetting(planningGroupSettings);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_"));

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForContractFilterIfAgentHaveNoContract()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			planningGroup.AddSetting(planningGroupSettings);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldNotReturnRulesForWrongContractFilter()
		{
			var planningGroup = new PlanningGroup();
			var contract = new Contract("_");
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new ContractFilter(contract));
			planningGroup.AddSetting(planningGroupSettings);
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));


			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldReturnRulesForSiteFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForSiteFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroup = new PlanningGroup();
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new SiteFilter(new Site("_")));
			planningGroup.AddSetting(planningGroupSettings);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldReturnRulesForTeamFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldNotReturnRulesForTeamFilterIfAgentHaveNoPersonPeriod()
		{
			var planningGroup = new PlanningGroup();
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			planningGroup.AddSetting(planningGroupSettings);
			var agent = new Person();

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldUseExplicitFilterWhenDefaultFilterExists()
		{
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(PlanningGroupSettings.CreateDefault());
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldDoAndOperationBetweenDifferentFilters()
		{
			var planningGroup = new PlanningGroup();
			var contractNotOnAgent = new Contract("_");
			var agent = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1900, 1, 1));
			agent.Period(new DateOnly(2000, 1, 1)).PersonContract = new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_"));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			planningGroupSettings.AddFilter(new ContractFilter(contractNotOnAgent));

			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(PlanningGroupSettings.CreateDefault().BlockFinderType);
		}

		[Test]
		public void ShouldDoOrOperationWithinSameFilter()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			planningGroupSettings.AddFilter(new TeamFilter(agent.Period(new DateOnly(2000, 1, 1)).Team));
			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldDoOrOperationBetweenTeamAndSite()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff };
			planningGroupSettings.AddFilter(new TeamFilter(new Team()));
			planningGroupSettings.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			planningGroup.AddSetting(planningGroupSettings);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}

		[Test]
		public void ShouldSelectBlockSettingsWithHighestPriority()
		{
			var planningGroup = new PlanningGroup();
			var agent = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(1900, 1, 1));
			var planningGroupSettings = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.BetweenDayOff, Priority = 2, };
			var planningGroupSettings2 = new PlanningGroupSettings() {  BlockFinderType = BlockFinderType.SchedulePeriod, Priority = 1 };
			planningGroupSettings.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			planningGroupSettings2.AddFilter(new SiteFilter(agent.Period(new DateOnly(2000, 1, 1)).Team.Site));
			planningGroup.AddSetting(planningGroupSettings);
			planningGroup.AddSetting(planningGroupSettings2);

			Target.Create(planningGroup).ForAgent(agent, new DateOnly(2000, 1, 1)).BlockTypeValue
				.Should().Be.EqualTo(BlockFinderType.BetweenDayOff);
		}
	}
}