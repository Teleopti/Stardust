using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	class BadgeSettingProviderTest
	{
		[Test]
		public void ShouldGetAgentBadgeSettings()
		{
			var settings = new AgentBadgeSettings
			{
				BadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				AnsweredCallsThreshold = 99,
				AdherenceBadgeEnabled = true,
				AdherenceThreshold = new Percent(0.55),
				AHTBadgeEnabled = true,
				AHTThreshold = new TimeSpan(0, 3, 30),
				GoldToSilverBadgeRate = 3,
				SilverToBronzeBadgeRate = 2
			};

			var repository = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			repository.Stub(x => x.GetSettings()).IgnoreArguments().Return(settings);

			var target = new BadgeSettingProvider(repository);

			var result = target.GetBadgeSettings();

			result.BadgeEnabled.Should().Be(settings.BadgeEnabled);
			
			result.AnsweredCallsBadgeEnabled.Should().Be(settings.AnsweredCallsBadgeEnabled);
			result.AnsweredCallsThreshold.Should().Be(settings.AnsweredCallsThreshold);

			result.AdherenceBadgeEnabled.Should().Be(settings.AdherenceBadgeEnabled);
			result.AdherenceThreshold.ValueAsPercent().Should().Be(settings.AdherenceThreshold.ValueAsPercent());

			result.AHTBadgeEnabled.Should().Be(settings.AHTBadgeEnabled);
			result.AHTThreshold.Ticks.Should().Be(settings.AHTThreshold.Ticks);

			result.SilverToBronzeBadgeRate.Should().Be(result.SilverToBronzeBadgeRate);
			result.GoldToSilverBadgeRate.Should().Be(result.GoldToSilverBadgeRate);
		}
	}
}
