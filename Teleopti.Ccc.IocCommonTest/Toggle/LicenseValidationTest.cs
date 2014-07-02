using System.IO;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class LicenseValidationTest
	{
		[Test]
		public void MustSetName()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "TestToggle=license"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile, string.Empty));
				ToggleNetModule.RegisterDependingModules(containerBuilder);
				containerBuilder.Register(_ => MockRepository.GenerateStub<ILicenseActivatorProvider>()).Named<ILicenseActivatorProvider>("querystring");
				using (var container = containerBuilder.Build())
				{
					Assert.Throws<DependencyResolutionException>(() => container.Resolve<IToggleManager>());
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}