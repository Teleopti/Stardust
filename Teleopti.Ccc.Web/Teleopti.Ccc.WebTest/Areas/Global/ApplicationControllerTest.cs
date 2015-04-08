using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldGetPermittedModules()
		{
			var toggleManager = new FakeToggleManager(Toggles.Wfm_ResourcePlanner_32892);
			var target = new ApplicationController(new FakePermissionProvider(), toggleManager);
			
			var result = target.GetAreas();

			result.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldNotGetUnpermittedModules()
		{
			var toggleManager = new FakeToggleManager(Toggles.Wfm_ResourcePlanner_32892);
			var target = new ApplicationController(new FakeNoPermissionProvider(), toggleManager);
			var result = target.GetAreas();

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetEnabledModules()
		{
			var toggleManager = new FakeToggleManager();
			var target = new ApplicationController(new FakePermissionProvider(), toggleManager);

			var result = target.GetAreas();

			result.Count().Should().Be.EqualTo(4);
		}
	}
}
