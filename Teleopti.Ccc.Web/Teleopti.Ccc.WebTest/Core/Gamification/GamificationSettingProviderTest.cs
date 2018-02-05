using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Gamification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Gamification
{
	[TestFixture, GamificationTest]
	public class GamificationSettingProviderTest
	{
		public IGamificationSettingProvider Target;
		public IGamificationSettingRepository GamificationSettingRepository;		

		[Test]
		public void ShouldGetGamificationSettingByIdAndSortExternalBadgeSettingsById()
		{
			var settingId = createGamificationSettingWithId();

			var setting = Target.GetGamificationSetting(settingId);
			setting.Should().Not.Be.Null();
			setting.ExternalBadgeSettings[0].QualityId.Should().Be.EqualTo(1);
			setting.ExternalBadgeSettings[1].QualityId.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetSettingDescriptionList()
		{
			var setting1 = createGamificationSettingWithId("1");
			var setting2 = createGamificationSettingWithId("2");

			var list = Target.GetGamificationList();

			list.Count().Should().Be.EqualTo(2);
			list[0].Value.Name.Should().Be.EqualTo("1");
			list[1].Value.Name.Should().Be.EqualTo("2");
		}


		private Guid createGamificationSettingWithId(string description = "Default Gamification Setting")
		{
			var id = Guid.NewGuid();
			var aSetting = new GamificationSetting(description)
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AdherenceBadgeEnabled = true,

				AnsweredCallsThreshold = 30,
				AnsweredCallsBronzeThreshold = 30,
				AnsweredCallsSilverThreshold = 40,
				AnsweredCallsGoldThreshold = 50,

				AHTThreshold = TimeSpan.FromSeconds(60),
				AHTBronzeThreshold = TimeSpan.FromSeconds(60),
				AHTSilverThreshold = TimeSpan.FromSeconds(50),
				AHTGoldThreshold = TimeSpan.FromSeconds(40),

				AdherenceThreshold = new Percent(0.70),
				AdherenceBronzeThreshold = new Percent(0.70),
				AdherenceSilverThreshold = new Percent(0.85),
				AdherenceGoldThreshold = new Percent(0.95),

				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2,
				
			};

			aSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric });
			aSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 1, DataType = ExternalPerformanceDataType.Percent });

			aSetting.WithId(id);

			GamificationSettingRepository.Add(aSetting);

			return id;
		}
	}
}
