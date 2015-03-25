using System.IO;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;

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
				File.WriteAllLines(tempFile, new[] { "TestToggle=false" });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
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
				File.WriteAllLines(tempFile, new[] { "TestToggle=true" });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
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
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
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
				File.WriteAllLines(tempFile, new[] {"TestToggle=rc"});
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
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
				File.WriteAllLines(tempFile, new[] { "TestToggle= Rc	 " });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
						.Should().Be.EqualTo(RcFeatureShouldBe);
				}
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void DevFeatureInFile()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "TestToggle= Dev " });
				var containerBuilder = new ContainerBuilder();
				containerBuilder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new AppConfigReader()) { FeatureToggle = tempFile, ToggleMode = ToggleMode }, null)));
				using (var container = containerBuilder.Build())
				{
					var toggleChecker = container.Resolve<IToggleManager>();
					toggleChecker.IsEnabled(Toggles.TestToggle)
						.Should().Be.EqualTo(devFeatureShouldBe);
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

		private bool devFeatureShouldBe
		{
			get { return UndefinedFeatureShouldBe; }
		}

		protected abstract string ToggleMode { get; }
	}
}