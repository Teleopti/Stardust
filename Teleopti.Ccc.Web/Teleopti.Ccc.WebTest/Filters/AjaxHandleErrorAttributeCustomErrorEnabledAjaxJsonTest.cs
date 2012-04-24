using System;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class AjaxHandleErrorAttributeCustomErrorEnabledAjaxJsonTest
	{
		[Test]
		public void ShouldReturnJsonResult()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new Exception(); });

			var result = filterTester.InvokeFilter(target) as JsonResult;

			result.Should().Not.Be.Null();
			filterTester.ControllerContext.HttpContext.Response.TrySkipIisCustomErrors.Should().Be.True();
		}

		[Test]
		public void ShouldReturnHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new Exception(); });

			filterTester.InvokeFilter(target);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be(500);
			filterTester.ControllerContext.HttpContext.Response.TrySkipIisCustomErrors.Should().Be.True();
		}

		[Test]
		public void ShouldReturnHttpErrorCodeFromHttpException()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new HttpException(404, ""); });

			filterTester.InvokeFilter(target);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be(404);
			filterTester.ControllerContext.HttpContext.Response.TrySkipIisCustomErrors.Should().Be.True();
		}

		[Test]
		public void ShouldNotThrow()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new Exception(); });

			filterTester.InvokeFilter(target);
		}

		[Test]
		public void ShouldNotThrowOnHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new HttpException(500, ""); });

			filterTester.InvokeFilter(target);
		}

		[Test]
		public void ShouldNotThrowOnNonHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.AcceptJson();
			filterTester.ActionMethod(() => { throw new HttpException(404, ""); });

			filterTester.InvokeFilter(target);
		}
	}	
}