using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTestAttribute]
	public class ToggleOverrideControllerTest
	{
		public ToggleOverrideController Target;

		[Test]
		public void ShouldSaveAndGetOverride()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			
			Target.SaveOverride(new SaveOverrideInput{Toggle = Toggles.TestToggle, Value = true});
			var allOverrides = Target.GetAllOverrides().Content;
			var overrideModel = allOverrides.Single();
			overrideModel.Toggle.Should().Be.EqualTo(Toggles.TestToggle.ToString());
			overrideModel.Enabled.Should().Be.True();
		}

		[Test]
		public void ShouldDeleteOverride()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			Target.SaveOverride(new SaveOverrideInput{Toggle = Toggles.TestToggle, Value = true});
			
			Target.DeleteOverride(Toggles.TestToggle.ToString());
			
			Target.GetAllOverrides().Content.Should().Be.Empty();
		}
	}
}