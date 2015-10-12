using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class GlobalAreaControllerTest
	{
		[Test]
		public void ShouldReturnEmptyWithToggleOff()
		{
			var toggleManager = new FakeToggleManager();
			var areaPathProvider = new AreaWithPermissionPathProvider(new FakePermissionProvider(), toggleManager);
			var target = new GlobalAreaController(areaPathProvider, toggleManager);

			var result = target.GetApplicationAreas();
			result.Count().Should().Be(0);
		}
	}
}