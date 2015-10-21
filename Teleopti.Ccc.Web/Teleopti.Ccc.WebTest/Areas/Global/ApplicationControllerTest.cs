using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldNotGetUnpermittedModules()
		{
			var toggleManager = new FakeToggleManager(Toggles.Wfm_ResourcePlanner_32892);
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager);
			var target = new ApplicationController(areaPathProvider);

			var result = target.GetAreas();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetEnabledModules()
		{
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakeNoPermissionProvider(), toggleManager);
			var target = new ApplicationController(areaPathProvider);

			var result = target.GetAreas();
			result.Any(x=>((dynamic)(x)).InternalName == "resourceplanner" ).Should().Be.False();
		}
	}
}
