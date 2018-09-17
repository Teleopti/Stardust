using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class OverrideToggleTest
	{
		[Test]
		public void ShouldNotOverrideIfFeatureIsNotEnabled()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "TestToggle=false" });
				var toggleOverride = new FakeFetchToggleOverride();
				toggleOverride.SetValue(Toggles.TestToggle, true);
				var iocArgs = new IocArgs(new ConfigReader())
				{
					FetchToggleOverride = toggleOverride, 
					FeatureToggle = tempFile
				};

				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.False();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
		
		[Test]
		public void CanOverrideWithTrue()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "TestToggle=false" });
				var configReader = new FakeConfigReader("PBI77584", "true");
				var toggleOverride = new FakeFetchToggleOverride();
				toggleOverride.SetValue(Toggles.TestToggle, true);
				var iocArgs = new IocArgs(configReader)
				{
					FetchToggleOverride = toggleOverride, FeatureToggle = tempFile
				};

				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.True();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
		
		[Test]
		public void CanOverrideWithFalse()
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { "TestToggle=true" });
				var configReader = new FakeConfigReader("PBI77584", "true");
				var toggleOverride = new FakeFetchToggleOverride();
				toggleOverride.SetValue(Toggles.TestToggle, false);
				var iocArgs = new IocArgs(configReader)
				{
					FetchToggleOverride = toggleOverride, FeatureToggle = tempFile
				};
				
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.False();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}