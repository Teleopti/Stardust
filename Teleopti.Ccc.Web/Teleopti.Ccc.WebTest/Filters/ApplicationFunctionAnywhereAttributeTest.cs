using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class ApplicationFunctionAnywhereAttributeTest
	{
		[Test]
		public void ShouldSetErrorViewWhenNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionAnywhereAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(false);

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<JsonResult>();
		}

		[Test]
		public void ShouldPassThroughWhenPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionAnywhereAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(true);

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<EmptyResult>();
		}

	}
}