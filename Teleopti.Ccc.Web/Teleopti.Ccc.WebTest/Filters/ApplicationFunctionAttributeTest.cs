using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class ApplicationFunctionAttributeTest
	{

		[Test]
		public void ShouldSetErrorPartialViewWithAjaxRequestWhenNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionAttribute("Test") {PermissionProvider = permissionProvider};

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(false);

			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
			var viewResult = result as ViewResult;
			viewResult.ViewName.Should().Be("ErrorPartial");
		}

		[Test]
		public void ShouldSetErrorViewWhenNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(false);

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
			var viewResult = result as ViewResult;
			viewResult.ViewName.Should().Be("Error");
		}

		[Test]
		public void ShouldPassThroughWhenPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(true);

			var result = new FilterTester().InvokeFilter(target);

			result.Should().Be.OfType<EmptyResult>();
		}

	}
}
