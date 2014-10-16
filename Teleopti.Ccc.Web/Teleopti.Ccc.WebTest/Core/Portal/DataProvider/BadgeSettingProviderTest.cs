using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	class BadgeSettingProviderTest
	{
		[Test]
		public void ShouldGetAgentBadgeSettingsByDefaultValue()
		{
			var repository = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			repository.Stub(x => x.GetSettings()).IgnoreArguments().Return(new AgentBadgeSettings());

			var target = new BadgeSettingProvider(repository);

			var result = target.GetBadgeSettings();

			result.BadgeEnabled.Should().Be(false);
			result.AnsweredCallsThreshold.Should().Be(100);
			result.AHTThreshold.Minutes.Should().Be(5);
			result.AdherenceThreshold.ValueAsPercent().Should().Be(75);
			result.SilverToBronzeBadgeRate.Should().Be(2);
			result.GoldToSilverBadgeRate.Should().Be(5);
		}
	}
}
