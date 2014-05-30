using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class GeneralTest
	{
		[Test]
		public void ShouldUseToggleQuerierIfStartsWithHttp()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ToggleNetModule("http://tralala"));
			containerBuilder.Register(c => MockRepository.GenerateMock<ILicenseActivator>());
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
			containerBuilder.Register(c => MockRepository.GenerateMock<ILicenseActivator>());
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
			containerBuilder.RegisterModule(new ToggleNetModule("http://something"));
			containerBuilder.Register(c => MockRepository.GenerateMock<ILicenseActivator>());
			using (var container = containerBuilder.Build())
			{
				container.Resolve<ITogglesActive>()
					.Should().Not.Be.Null();
			}
		} 
	}
}