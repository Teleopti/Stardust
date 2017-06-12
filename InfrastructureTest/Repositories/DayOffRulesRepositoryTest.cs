﻿using System;
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
	public class DayOffRulesRepositoryTest : RepositoryTest<DayOffRules>
	{
		protected override DayOffRules CreateAggregateWithCorrectBusinessUnit()
		{
			return new DayOffRules
			{
				ConsecutiveDayOffs = new MinMax<int>(2,6),
				DayOffsPerWeek = new MinMax<int>(4,6),
				ConsecutiveWorkdays = new MinMax<int>(5,7)
			};
		}

		protected override void VerifyAggregateGraphProperties(DayOffRules loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.DayOffsPerWeek.Should().Be.EqualTo(expected.DayOffsPerWeek);
			loadedAggregateFromDatabase.ConsecutiveDayOffs.Should().Be.EqualTo(expected.ConsecutiveDayOffs);
			loadedAggregateFromDatabase.ConsecutiveWorkdays.Should().Be.EqualTo(expected.ConsecutiveWorkdays);
		}

		protected override Repository<DayOffRules> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new DayOffRulesRepository(currentUnitOfWork);
		}

		[Test]
		public void CanAddMultipleNonDefaults()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(new DayOffRules());
			UnitOfWork.Flush();
			rep.Add(new DayOffRules());
			UnitOfWork.Flush();
		}

		[Test]
		public void WhenAddingTwoDefaultSettingsLastWins()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			var lastDefault = DayOffRules.CreateDefault();
			rep.Add(DayOffRules.CreateDefault());
			UnitOfWork.Flush();
			rep.Add(lastDefault);
			UnitOfWork.Flush();
			rep.LoadAllWithoutPlanningGroup().SingleOrDefault().Should().Not.Be.Null();
			rep.LoadAllWithoutPlanningGroup().Single().Should().Be.EqualTo(lastDefault);
		}

		[Test]
		public void CanUseAddWhenUpdatingAlreadyPersistedDefault()
		{
			var dayOffSettings = DayOffRules.CreateDefault();
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(dayOffSettings);
			UnitOfWork.Flush();
			Assert.DoesNotThrow(() => rep.Add(dayOffSettings));
		}

		[Test]
		public void CanNotRemoveDefaultSetting()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			var defaultSetting = DayOffRules.CreateDefault();
			Assert.Throws<ArgumentException>(() => rep.Remove(defaultSetting));
		}

		[Test]
		public void ShouldReturnDefaultValuesIfNotPresentInDb()
		{
			var defaultSetting = DayOffRules.CreateDefault();
			var defaultInDb = new DayOffRulesRepository(CurrUnitOfWork).LoadAllWithoutPlanningGroup().SingleOrDefault();

			defaultInDb.Should().Not.Be.Null();
			defaultInDb.DayOffsPerWeek.Should().Be.EqualTo(defaultSetting.DayOffsPerWeek);
			defaultInDb.ConsecutiveDayOffs.Should().Be.EqualTo(defaultSetting.ConsecutiveDayOffs);
			defaultInDb.ConsecutiveWorkdays.Should().Be.EqualTo(defaultSetting.ConsecutiveWorkdays);
		}

		[Test]
		public void ShouldPersistAndFetchContractFilters()
		{
			var contract = new Contract("_");
			var contractFilter = new ContractFilter(contract);
			var dayOffRules = new DayOffRules();
			dayOffRules.AddFilter(contractFilter);

			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new DayOffRulesRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(contractFilter);
		}

		[Test]
		public void ShouldPersistAndFetchTeamFilter()
		{
			var site = new Site("_");
			var team = new Team {Site = site }.WithDescription(new Description("_"));
			var teamFilter = new TeamFilter(team);
			var dayOffRules = new DayOffRules();
			dayOffRules.AddFilter(teamFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new DayOffRulesRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(teamFilter);
		}

		[Test]
		public void ShouldPersistAndFetchSiteFilter()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var dayOffRules = new DayOffRules();
			dayOffRules.AddFilter(siteFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new DayOffRulesRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Filters.Single()
				.Should().Be.EqualTo(siteFilter);
		}

		[Test]
		public void ShouldPersistAndFetchRuleName()
		{
			var dayOffRules = new DayOffRules();
			var name = RandomName.Make();
			dayOffRules.Name = name;
			
			PersistAndRemoveFromUnitOfWork(dayOffRules);

			var dayOffRulesInDb = new DayOffRulesRepository(CurrUnitOfWork).Get(dayOffRules.Id.Value);

			dayOffRulesInDb.Name
				.Should().Be.EqualTo(name);	
		}

		[Test]
		public void ShouldDeleteDayOffRulesWithFilters()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var dayOffRules = new DayOffRules();
			dayOffRules.AddFilter(siteFilter);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(dayOffRules);
			var rep = new DayOffRulesRepository(CurrUnitOfWork);

			rep.Remove(dayOffRules);

			rep.Get(dayOffRules.Id.Value)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldAddDayOffRuleForPlanningGroup()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			PersistAndRemoveFromUnitOfWork(DayOffRules.CreateDefault());

			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(DayOffRules.CreateDefault(planningGroup));

			UnitOfWork.Flush();

			var result = rep.LoadAllByPlanningGroup(planningGroup);

			result.SingleOrDefault().Should().Not.Be.Null();
			result.Single().PlanningGroup.Should().Be.EqualTo(planningGroup);

			var result2 = rep.LoadAllWithoutPlanningGroup();

			result2.SingleOrDefault().Should().Not.Be.Null();
			result2.Single().PlanningGroup.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldBeAbleToRemoveForPlanningGroup()
		{
			var planningGroup = new PlanningGroup("_");
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(DayOffRules.CreateDefault(planningGroup));
			UnitOfWork.Flush();

			rep.RemoveForPlanningGroup(planningGroup);
			UnitOfWork.Flush();

			var result = rep.LoadAllByPlanningGroup(planningGroup);
			result.Should().Be.Empty();
		}
	}
}