using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldGetPermittedModules()
		{
			var target = new ApplicationController(new FakePermissionProvider());
			var result = target.GetAreas();

			result.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldNotGetUnpermittedModules()
		{
			var target = new ApplicationController(new FakeNoPermissionProvider());
			var result = target.GetAreas();

			result.Count().Should().Be.EqualTo(0);
		}
	}
}
