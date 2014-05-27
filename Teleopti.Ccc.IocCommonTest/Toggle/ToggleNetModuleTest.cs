using System.IO;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class ToggleNetModuleTest
	{
		[Test]
		public void DisabledFeatureShouldBeEnabledIfAll()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("ALL"));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.IsEnabled(Toggles.DisabledFeature)
					.Should().Be.False();
			}
		}

		[Test]
		public void EnabledFeatureShouldBeEnabledIfAll()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("ALL"));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.IsEnabled(Toggles.EnabledFeature)
					.Should().Be.True();
			}
		}

		[Test]
		public void PathNameCanEndWithAll()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule(@"c:\blablablab\aLl  "));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.IsEnabled(Toggles.EnabledFeature)
					.Should().Be.True();
			}
		}

		[Test]
		public void EnabledFeatureInFileShouldBeEnabled()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] {"EnabledFeature=true"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => MockRepository.GenerateStub<ILoggedOnUser>()).As<ILoggedOnUser>();
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.True();
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void DisabledFeatureInFileShouldBeDisabled()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] {"EnabledFeature=false"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => MockRepository.GenerateStub<ILoggedOnUser>()).As<ILoggedOnUser>();
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.False();
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttp()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("http://tralala"));
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
			containerBuilder.RegisterModule(new ToggleNetModule("https://hejsan"));
			using (var container = containerBuilder.Build())
			{
				var toggleChecker = container.Resolve<IToggleManager>();
				toggleChecker.Should().Be.OfType<ToggleQuerier>();
			}
		}

		[Test]
		public void ShouldResolveTogglesActive()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("all"));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<ITogglesActive>()
					.Should().Not.Be.Null();
			}
		}
	}
}