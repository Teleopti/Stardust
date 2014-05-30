using System.IO;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public abstract class ToggleBaseTest
	{
		[Test]
		public void DisabledFeatureInFile()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "EnabledFeature=false" });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(DisabledFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void EnabledFeatureInFile()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "EnabledFeature=true" });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(EnabledFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void UndefinedFeatureInFile()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new string[0]);
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(UndefinedFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void RcFeatureInFile()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "EnabledFeature.license.name=" + ToggleNetModule.RcLicenseName });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(RcFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void RcFeatureInFileWithStrangeCasing()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "EnabledFeature.license.name=" + ToggleNetModule.RcLicenseName.ToUpper()});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(RcFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void RcFeatureInFileUntrimmed()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "EnabledFeature.license.name  =	 " + ToggleNetModule.RcLicenseName.ToUpper() + "		"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new ToggleNetModule(tempFile));
				containerBuilder.Register(_ => createLicenseActivitor());
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.EnabledFeature)
						.Should().Be.EqualTo(RcFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		protected abstract bool UndefinedFeatureShouldBe { get; }
		protected abstract bool EnabledFeatureShouldBe { get; }
		protected abstract bool DisabledFeatureShouldBe { get; }
		protected abstract bool RcFeatureShouldBe { get; }

		private ILicenseActivator createLicenseActivitor()
		{
			var act = MockRepository.GenerateMock<ILicenseActivator>();
			act.Expect(x => x.CustomerName).Return(LicenseCustomerName);
			return act;
		}

		protected abstract string LicenseCustomerName { get; }
	}
}