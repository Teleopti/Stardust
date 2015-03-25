using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class GeneralTest
	{
		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttp()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = "http://tralala" }, null)));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.Should().Be.OfType<ToggleQuerier>();
			}
		}

		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttps()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = "https://hejsan" }, null)));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.Should().Be.OfType<ToggleQuerier>();
			}
		}

		[Test]
		public void ShouldRegisterToggleFillerIfToggleQuerierIsUsed()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = "https://hejsan" }, null)));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				var toggleFiller = container.Resolve<IToggleFiller>();
				toggleChecker.Should().Be.SameInstanceAs(toggleFiller);
			}
		}

		[Test]
		public void ShouldResolveTogglesActive()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = "http://something" }, null)));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<ITogglesActive>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldSetAllTogglesToFalseIfPathIsEmpty()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = "" }, null)));
			using (var container = containerBuilder.Build())
			{
				var toggleManager = container.Resolve<IToggleManager>();
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.False();
				toggleManager.Should().Be.OfType<FalseToggleManager>();
			}
		}

		[Test]
		public void ShouldSetAllTogglesToFalseIfPathIsNull()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = null }, null)));
			using (var container = containerBuilder.Build())
			{
				var toggleManager = container.Resolve<IToggleManager>();
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.False();
				toggleManager.Should().Be.OfType<FalseToggleManager>();
			}
		}
	}
}