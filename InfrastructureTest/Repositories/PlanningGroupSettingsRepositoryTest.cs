using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PlanningGroupSettingsRepositoryTest : RepositoryTest<PlanningGroupSettings>
	{
		protected override PlanningGroupSettings CreateAggregateWithCorrectBusinessUnit()
		{
			return new PlanningGroupSettings
			{
				ConsecutiveDayOffs = new MinMax<int>(2, 6),
				DayOffsPerWeek = new MinMax<int>(4, 6),
				ConsecutiveWorkdays = new MinMax<int>(5, 7),
				FullWeekendsOff = new MinMax<int>(2, 7),
				WeekendDaysOff = new MinMax<int>(3, 15),
				Priority = 5
			};
		}

		protected override void VerifyAggregateGraphProperties(PlanningGroupSettings loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.DayOffsPerWeek.Should().Be.EqualTo(expected.DayOffsPerWeek);
			loadedAggregateFromDatabase.ConsecutiveDayOffs.Should().Be.EqualTo(expected.ConsecutiveDayOffs);
			loadedAggregateFromDatabase.ConsecutiveWorkdays.Should().Be.EqualTo(expected.ConsecutiveWorkdays);
			loadedAggregateFromDatabase.Priority.Should().Be.EqualTo(expected.Priority);
			loadedAggregateFromDatabase.FullWeekendsOff.Should().Be.EqualTo(expected.FullWeekendsOff);
			loadedAggregateFromDatabase.WeekendDaysOff.Should().Be.EqualTo(expected.WeekendDaysOff);
		}

		protected override Repository<PlanningGroupSettings> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PlanningGroupSettingsRepository(currentUnitOfWork);
		}

		[Test]
		public void CanAddMultipleNonDefaults()
		{
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			rep.Add(new PlanningGroupSettings());
			UnitOfWork.Flush();
			rep.Add(new PlanningGroupSettings());
			UnitOfWork.Flush();
		}

		[Test]
		public void ShouldBeOrdered()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			var planningGroupSetting3 = new PlanningGroupSettings(planningGroup) {Priority = 3};
			var planningGroupSetting1 = new PlanningGroupSettings(planningGroup) {Priority = 1};
			var planningGroupSetting2 = new PlanningGroupSettings(planningGroup) {Priority = 2};
			rep.Add(planningGroupSetting3);
			rep.Add(planningGroupSetting1);
			rep.Add(planningGroupSetting2);
			UnitOfWork.Flush();

			rep.LoadAllByPlanningGroup(planningGroup)
				.Should().Have.SameSequenceAs(planningGroupSetting1, planningGroupSetting2, planningGroupSetting3);
		}
		
		[Test]
		public void ShouldPlaceDefaultSettingLast()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			var defaultPlanningGroupSetting = PlanningGroupSettings.CreateDefault(planningGroup);
			var planningGroupSetting = new PlanningGroupSettings(planningGroup);
			rep.Add(defaultPlanningGroupSetting);
			rep.Add(planningGroupSetting);
			UnitOfWork.Flush();

			rep.LoadAllByPlanningGroup(planningGroup)
				.Should().Have.SameSequenceAs(planningGroupSetting, defaultPlanningGroupSetting);
		}

		[Test]
		public void CanUseAddWhenUpdatingAlreadyPersistedDefault()
		{
			var dayOffSettings = PlanningGroupSettings.CreateDefault();
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			rep.Add(dayOffSettings);
			UnitOfWork.Flush();
			Assert.DoesNotThrow(() => rep.Add(dayOffSettings));
		}

		[Test]
		public void CanNotRemoveDefaultSetting()
		{
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			var defaultSetting = PlanningGroupSettings.CreateDefault();
			Assert.Throws<ArgumentException>(() => rep.Remove(defaultSetting));
		}

		[Test]
		public void ShouldPersistAndFetchContractFilters()
		{
			var contract = new Contract("_");
			var contractFilter = new ContractFilter(contract);
			var dayOffRules = new PlanningGroupSettings();
			dayOffRules.AddFilter(contractFilter);

			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new PlanningGroupSettingsRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(contractFilter);
		}

		[Test]
		public void ShouldPersistAndFetchTeamFilter()
		{
			var site = new Site("_");
			var team = new Team {Site = site }.WithDescription(new Description("_"));
			var teamFilter = new TeamFilter(team);
			var dayOffRules = new PlanningGroupSettings();
			dayOffRules.AddFilter(teamFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new PlanningGroupSettingsRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(teamFilter);
		}

		[Test]
		public void ShouldPersistAndFetchSiteFilter()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var dayOffRules = new PlanningGroupSettings();
			dayOffRules.AddFilter(siteFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new PlanningGroupSettingsRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(siteFilter);
		}

		[Test]
		public void ShouldPersistAndFetchRuleName()
		{
			var dayOffRules = new PlanningGroupSettings();
			var name = RandomName.Make();
			dayOffRules.Name = name;
			
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new PlanningGroupSettingsRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Name
				.Should().Be.EqualTo(name);	
		}

		[Test]
		public void ShouldDeleteDayOffRulesWithFilters()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var dayOffRules = new PlanningGroupSettings();
			dayOffRules.AddFilter(siteFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(dayOffRules);
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);

			rep.Remove(dayOffRules);

			rep.Get(dayOffRules.Id.Value)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldAddDayOffRuleForPlanningGroup()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			PersistAndRemoveFromUnitOfWork(PlanningGroupSettings.CreateDefault());

			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			rep.Add(PlanningGroupSettings.CreateDefault(planningGroup));

			UnitOfWork.Flush();

			var result = rep.LoadAllByPlanningGroup(planningGroup);

			result.SingleOrDefault().Should().Not.Be.Null();
			result.Single().PlanningGroup.Should().Be.EqualTo(planningGroup);
		}

		[Test]
		public void ShouldBeAbleToRemoveForPlanningGroup()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new PlanningGroupSettingsRepository(CurrUnitOfWork);
			rep.Add(PlanningGroupSettings.CreateDefault(planningGroup));
			UnitOfWork.Flush();

			rep.RemoveForPlanningGroup(planningGroup);
			UnitOfWork.Flush();

			var result = rep.LoadAllByPlanningGroup(planningGroup);
			result.Should().Be.Empty();
		}
	}
}