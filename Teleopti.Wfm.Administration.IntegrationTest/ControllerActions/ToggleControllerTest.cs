using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminToggleTest]
	public class ToggleControllerTest
	{
		public ToggleController Target;

		[Test]
		public void ShouldSaveAndGetOverride()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			
			Target.SaveOverride(Toggles.TestToggle, true);
			var allOverrides = Target.GetAllOverrides().Content;
			var overrideModel = allOverrides.Single();
			overrideModel.Toggle.Should().Be.EqualTo(Toggles.TestToggle.ToString());
			overrideModel.Enabled.Should().Be.True();
		}

		[Test]
		public void ShouldDeleteOverride()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Target.SaveOverride(Toggles.TestToggle, true);
			
			Target.DeleteOverride(Toggles.TestToggle);
			
			Target.GetAllOverrides().Content.Should().Be.Empty();
		}
	}
	
	public class WfmAdminToggleTestAttribute : WfmAdminTestAttribute
	{ 
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeSetting("PBI77584", "true");
			config.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
			return config;
		}
	}
}