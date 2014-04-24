using System.IO;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class ToggleNetModuleTest
	{
		[Test]
		public void DefinedShouldBeEnabledIfAll()
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
		public void DefinedShouldBeEnabledIfEndWithAll()
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
				File.WriteAllLines(tempFile, new []{"EnabledFeature=true"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
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
				File.WriteAllLines(tempFile, new[] { "EnabledFeature=false" });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
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
	}
}