using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class GamificationSettingRepositoryTest : RepositoryTest<IGamificationSetting>
	{
		private IGamificationSetting _gamificationSetting;
		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override IGamificationSetting CreateAggregateWithCorrectBusinessUnit()
		{
			IGamificationSetting gamificationSetting = new GamificationSetting("dummyGamificationSetting");

			return gamificationSetting;
		}

		[Test]
		public void VerifyCanPersistProperties()
		{
			_gamificationSetting = new GamificationSetting("MyGamificationSetting")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor,
				AHTBadgeEnabled = true,
				AHTBronzeThreshold = TimeSpan.FromMinutes(10),
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 8
			};

			PersistAndRemoveFromUnitOfWork(_gamificationSetting);
			IList<IGamificationSetting> loadedGamificationSettings = GamificationSettingRepository.DONT_USE_CTOR(UnitOfWork).LoadAll().ToList();
			Assert.AreEqual(1, loadedGamificationSettings.Count);
			Assert.AreEqual(TimeSpan.FromMinutes(10), loadedGamificationSettings[0].AHTBronzeThreshold);
			Assert.AreEqual(2, loadedGamificationSettings[0].GoldToSilverBadgeRate);
			Assert.AreEqual(8, loadedGamificationSettings[0].SilverToBronzeBadgeRate);
			Assert.AreEqual(true, loadedGamificationSettings[0].AHTBadgeEnabled);
		}

		[Test]
		public void VerifyCanLoadGamificationSetting()
		{
			_gamificationSetting = new GamificationSetting("MyGamificationSetting");
			PersistAndRemoveFromUnitOfWork(_gamificationSetting);
			IEnumerable<IGamificationSetting> loadedGamificationSettings = GamificationSettingRepository.DONT_USE_CTOR(UnitOfWork).FindAllGamificationSettingsSortedByDescription();
			Assert.AreEqual(1, loadedGamificationSettings.Count());
		}

		[Test]
		public void ShouldFindGamificationSettingByName()
		{
			const string settingName = "MyGamificationSetting";
			_gamificationSetting = new GamificationSetting(settingName);
			PersistAndRemoveFromUnitOfWork(_gamificationSetting);
			IList<IGamificationSetting> loadedGamificationSettings = GamificationSettingRepository.DONT_USE_CTOR(UnitOfWork).FindSettingByDescriptionName(settingName).ToList();
			Assert.AreEqual(settingName, loadedGamificationSettings.First().Description.Name);
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IGamificationSetting loadedAggregateFromDatabase)
		{
			IGamificationSetting org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
		}

		protected override Repository<IGamificationSetting> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return GamificationSettingRepository.DONT_USE_CTOR(currentUnitOfWork);
		}
	}
}