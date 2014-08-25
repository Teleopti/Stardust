using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class GeneralTest
	{
		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttp()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("http://tralala", string.Empty));
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
			containerBuilder.RegisterModule(new ToggleNetModule("https://hejsan", string.Empty));
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
			containerBuilder.RegisterModule(new ToggleNetModule("https://hejsan", string.Empty));
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
			containerBuilder.RegisterModule(new ToggleNetModule("http://something", string.Empty));
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
			containerBuilder.RegisterModule(new ToggleNetModule(string.Empty, string.Empty));
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
			containerBuilder.RegisterModule(new ToggleNetModule(null, string.Empty));
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