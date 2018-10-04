using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class GeneralTest
	{
		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttp()
		{
			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = "http://tralala"});
			
			toggleManager.Should().Be.OfType<ToggleQuerier>();
		}

		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttps()
		{
			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = "https://hejsan"});

			toggleManager.Should().Be.OfType<ToggleQuerier>();
		}

		[Test]
		public void ShouldRegisterToggleFillerIfToggleQuerierIsUsed()
		{
			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = "https://hejsan"});

			(toggleManager is IToggleFiller).Should().Be.True();
		}

		[Test]
		public void ShouldSetAllTogglesToFalseIfPathIsEmpty()
		{
			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = ""});
			
			toggleManager.IsEnabled(Toggles.TestToggle)
				.Should().Be.False();
			toggleManager.Should().Be.OfType<FalseToggleManager>();
		}

		[Test]
		public void ShouldSetAllTogglesToFalseIfPathIsNull()
		{
			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = null});
			
			toggleManager.IsEnabled(Toggles.TestToggle)
				.Should().Be.False();
			toggleManager.Should().Be.OfType<FalseToggleManager>();
		}
	}
}