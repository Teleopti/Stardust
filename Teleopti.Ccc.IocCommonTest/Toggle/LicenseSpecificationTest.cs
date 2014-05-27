using System.IO;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class LicenseSpecificationTest
	{
		[Test, Ignore("not yet fixed")]
		public void NoneDeclaredFeatureShouldBeEnabledIfUsingDeveloperLicense()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.RegisterModule<UnitOfWorkModule>();
				containerBuilder.RegisterModule<AuthenticationModule>();
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
	}
}