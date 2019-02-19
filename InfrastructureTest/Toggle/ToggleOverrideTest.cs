using System.IO;
using Autofac;
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
				var configReader = new FakeConfigReader();
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString());
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
				var configReader = new FakeConfigReader();
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString());
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				new PersistToggleOverride(configReader).Save(Toggles.TestToggle, dbValue);
				
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.EqualTo(dbValue);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		[Test]
		public void ShouldCacheResultWhenCreatingContainer()
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { $"TestToggle={false}" });
				var configReader = new FakeConfigReader();
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString());
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				new PersistToggleOverride(configReader).Save(Toggles.TestToggle, true);
				var toggleManager = CommonModule.ToggleManagerForIoc(iocArgs);
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.True();
				new PersistToggleOverride(configReader).Delete(Toggles.TestToggle.ToString());
				
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.True();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
		
		[Test]
		public void ShouldCacheResultWhenUsingContainerInRuntime()
		{
			SetupFixtureForAssembly.RestoreCcc7Database();
			var tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllLines(tempFile, new[] { $"TestToggle={false}" });
				var configReader = new FakeConfigReader();
				configReader.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString());
				var iocArgs = new IocArgs(configReader) { FeatureToggle = tempFile };
				new PersistToggleOverride(configReader).Save(Toggles.TestToggle, true);
				var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
				var builder = new ContainerBuilder();
				builder.RegisterModule(new CommonModule(configuration));
				var runtimeContainer = builder.Build();

				var toggleManager = runtimeContainer.Resolve<IToggleManager>();
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.True();
				new PersistToggleOverride(configReader).Delete(Toggles.TestToggle.ToString());
				
				
				toggleManager.IsEnabled(Toggles.TestToggle)
					.Should().Be.True();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}
	}
}