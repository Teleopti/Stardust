using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Toggle
{
	public class ToggleOverrideTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldGetFileValueIfNotPresentInDb(bool fileValue)
		{
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { $"TestToggle={fileValue.ToString()}" });
				var configReader = new FakeConfigReader("PBI77584", "true");
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(fileValue);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
		
		
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ShouldUseOverridenValue(bool fileValue, bool dbValue)
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { $"TestToggle={fileValue.ToString()}" });
				var configReader = new FakeConfigReader("PBI77584", "true");
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				new SaveToggleOverride(configReader).Save(Toggles.TestToggle, dbValue);
				
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(dbValue);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
		
		//REMOVE ME WHEN FEATURE IS ENABLED! Only to verify old behavior
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ShouldNotUseOverridenValueIfWebConfigValueMissing(bool fileValue, bool dbValue)
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { $"TestToggle={fileValue.ToString()}" });
				var configReader = new FakeConfigReader();
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				new SaveToggleOverride(configReader).Save(Toggles.TestToggle, dbValue);
				
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(fileValue);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}