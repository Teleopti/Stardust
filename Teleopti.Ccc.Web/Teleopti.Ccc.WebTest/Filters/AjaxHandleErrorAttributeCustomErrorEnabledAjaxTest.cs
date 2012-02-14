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
	public class AjaxHandleErrorAttributeCustomErrorEnabledAjaxTest
	{
		[Test]
		public void ShouldReturnErrorView()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => { throw new Exception(); });

			var result = filterTester.InvokeFilter(target) as ViewResult;

			result.ViewName.Should().Be("ErrorPartial");
		}

		[Test]
		public void ShouldReturnHttp200()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => { throw new Exception(); });

			filterTester.InvokeFilter(target);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be(200);
		}

		[Test]
		public void ShouldNotThrow()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
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
			filterTester.ActionMethod(() => { throw new HttpException(500, ""); });

			filterTester.InvokeFilter(target);
		}

		[Test]
		public void ShouldThrowOnNonHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.IsCustomErrorEnabled();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => { throw new HttpException(404, ""); });

			Assert.Throws<HttpException>(() => filterTester.InvokeFilter(target));
		}
	}
}