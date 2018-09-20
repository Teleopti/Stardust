using System.IO;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;

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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(DisabledFeatureShouldBe);
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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(EnabledFeatureShouldBe);
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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});

				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(UndefinedFeatureShouldBe);
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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(RcFeatureShouldBe);
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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});

				toggleManager.IsEnabled(Toggles.TestToggle)
						.Should().Be.EqualTo(RcFeatureShouldBe);
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
				var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()){FeatureToggle = tempFile, ToggleMode = ToggleMode});
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(devFeatureShouldBe);
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

		private bool devFeatureShouldBe => UndefinedFeatureShouldBe;

		protected abstract string ToggleMode { get; }
	}
}