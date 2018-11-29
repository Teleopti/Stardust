using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData;

using AggregateException = Teleopti.Ccc.Domain.Common.AggregateException;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PlanningGroupRepositoryTest : RepositoryTest<PlanningGroup>
	{
		protected override PlanningGroup CreateAggregateWithCorrectBusinessUnit()
		{
			return new PlanningGroup();
		}

		protected override void VerifyAggregateGraphProperties(PlanningGroup loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo(expected.Name);
		}

		protected override Repository<PlanningGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PlanningGroupRepository(currentUnitOfWork);
		}
		
		[Test]
		public void ShouldBeOrdered()
		{
			var planningGroup = new PlanningGroup();
			var planningGroupSettings1 = new PlanningGroupSettings {Priority = 1};
			var planningGroupSettings2 = new PlanningGroupSettings {Priority = 2};
			var planningGroupSettings3 = new PlanningGroupSettings {Priority = 3};
			var planningGroupSettings4 = new PlanningGroupSettings {Priority = 4};
			planningGroup.AddSetting(planningGroupSettings4);
			planningGroup.AddSetting(planningGroupSettings1);
			planningGroup.AddSetting(planningGroupSettings3);
			planningGroup.AddSetting(planningGroupSettings2);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			
			TestRepository(CurrUnitOfWork).Get(planningGroup.Id.Value).Settings
				.Should().Have.SameSequenceAs(planningGroupSettings4, planningGroupSettings3, planningGroupSettings2, planningGroupSettings1, planningGroup.Settings.Single(x=>x.Default));
		}

		[Test]
		public void PlanningGroupSettingsMustBelongToPlanningGroup()
		{
			Assert.Throws<AggregateException>(() => { PersistAndRemoveFromUnitOfWork(new PlanningGroupSettings()); });
		}
		
		[Test]
		public void ShouldPlaceDefaultSettingLast()
		{
			var planningGroup = new PlanningGroup();
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new PlanningGroupRepository(CurrUnitOfWork);
			var planningGroupSetting = new PlanningGroupSettings();
			planningGroup.AddSetting(planningGroupSetting);
			PersistAndRemoveFromUnitOfWork(planningGroup);

			rep.Load(planningGroup.Id.Value).Settings
				.Should().Have.SameSequenceAs(planningGroupSetting, planningGroup.Settings.Single(x=>x.Default));
		}
		
		[Test]
		public void CanAddMultipleNonDefaults()
		{
			var rep = new PlanningGroupRepository(CurrUnitOfWork);
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(new PlanningGroupSettings());
			planningGroup.AddSetting(new PlanningGroupSettings());
			rep.Add(planningGroup);
			UnitOfWork.Flush();
			UnitOfWork.Clear();
			
			TestRepository(CurrUnitOfWork).Get(planningGroup.Id.Value).Settings.Count(x => !x.Default)
				.Should().Be.EqualTo(2);
		}
		
		[Test]
		public void ShouldPersistAndFetchContractFilters()
		{
			var contract = new Contract("_");
			var contractFilter = new ContractFilter(contract);
			var planningGroupSettings = new PlanningGroupSettings();
			planningGroupSettings.AddFilter(contractFilter);
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(planningGroupSettings);

			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(planningGroup);

			var planningGroupInDb = new PlanningGroupRepository(CurrUnitOfWork).Get(planningGroup.Id.Value);

			planningGroupInDb.Settings.Single(x=>!x.Default).Filters.Single().Should().Be.EqualTo(contractFilter);
		}
		
		[Test]
		public void ShouldPersistAndFetchSiteFilter()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var planningGroupSettings = new PlanningGroupSettings();
			planningGroupSettings.AddFilter(siteFilter);
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(planningGroupSettings);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(planningGroup);

			var planningGroupInDb = new PlanningGroupRepository(CurrUnitOfWork).Get(planningGroup.Id.Value);

			planningGroupInDb.Settings.Single(x=>!x.Default).Filters.Single()
				.Should().Be.EqualTo(siteFilter);
		}
		
		[Test]
		public void ShouldPersistAndFetchRuleName()
		{
			var planningGroupSettings = new PlanningGroupSettings();
			var name = RandomName.Make();
			planningGroupSettings.Name = name;
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(planningGroupSettings);
			
			PersistAndRemoveFromUnitOfWork(planningGroup);

			var planningGroupInDb = new PlanningGroupRepository(CurrUnitOfWork).Get(planningGroup.Id.Value);

			planningGroupInDb.Settings.Single(x=>!x.Default).Name.Should().Be.EqualTo(name);	
		}

		[Test]
		public void ShouldDeletePlanningGroupSettingWithFilters()
		{
			var site = new Site("_");
			var siteFilter = new SiteFilter(site);
			var planningGroupSettings = new PlanningGroupSettings();
			planningGroupSettings.AddFilter(siteFilter);
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(planningGroupSettings);

			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var rep = new PlanningGroupRepository(CurrUnitOfWork);

			var planningGroupInDb = new PlanningGroupRepository(CurrUnitOfWork).Get(planningGroup.Id.Value);
			planningGroupInDb.RemoveSetting(planningGroupSettings);
			PersistAndRemoveFromUnitOfWork(planningGroupInDb);

			rep.Get(planningGroup.Id.Value).Settings.Should().Not.Contain(planningGroupSettings);
		}

		[Test]
		public void ShouldFindPlanningGroupContainingPlanningGroupSettingId()
		{
			var setting1 = new PlanningGroupSettings();
			var setting2 = new PlanningGroupSettings();
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(setting1);
			planningGroup.AddSetting(setting2);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var target = new PlanningGroupRepository(CurrUnitOfWork);

			target.FindPlanningGroupBySettingId(setting2.Id.Value).Settings
				.Should().Have.SameValuesAs(setting1, setting2, planningGroup.Settings.Single(x=>x.Default));
		}
		
		[Test]
		public void ShouldFindPlanningGroupShouldIncludeSettings()
		{
			var settings = new PlanningGroupSettings();
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(settings);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var target = new PlanningGroupRepository(CurrUnitOfWork);
			
			var result = target.FindPlanningGroupBySettingId(settings.Id.Value);
				
			Session.Close();
			result.Settings.Count(x=>!x.Default).Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldNotIncludePlanningGroupNotContainingPlanningGroupSettingId()
		{
			var settings = new PlanningGroupSettings();
			var planningGroup = new PlanningGroup();
			planningGroup.AddSetting(settings);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			
			var target = new PlanningGroupRepository(CurrUnitOfWork);

			target.FindPlanningGroupBySettingId(Guid.NewGuid())
				.Should().Be.Null();
		}

		[TestCase(0.22)]
		[TestCase(0.87)]
		public void ShouldPersistPreferenceValue(double preferenceValue)
		{
			var percentValue = new Percent(preferenceValue);
			var planningGroup = new PlanningGroup();
			planningGroup.SetGlobalValues(percentValue);
			PersistAndRemoveFromUnitOfWork(planningGroup);
			var target = new PlanningGroupRepository(CurrUnitOfWork);
			
			target.Get(planningGroup.Id.Value).Settings.PreferenceValue
				.Should().Be.EqualTo(percentValue);
		}
	}
}