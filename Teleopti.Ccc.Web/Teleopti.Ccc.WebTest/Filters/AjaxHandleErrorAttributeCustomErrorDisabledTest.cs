using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class AjaxHandleErrorAttributeCustomErrorDisabledTest
	{
		[Test]
		public void ShouldAlwaysThrow()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => { throw new Exception(); });

			Assert.Throws<Exception>(() => filterTester.InvokeFilter(target));
		}

		[Test]
		public void ShouldAlwaysThrowOnHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => { throw new HttpException(500, ""); });

			Assert.Throws<HttpException>(() => filterTester.InvokeFilter(target));
		}

		[Test]
		public void ShouldAlwaysThrowOnNonHttp500()
		{
			var target = new AjaxHandleErrorAttribute(MockRepository.GenerateMock<IErrorMessageProvider>());
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => { throw new HttpException(404, ""); });

			Assert.Throws<HttpException>(() => filterTester.InvokeFilter(target));
		}
	}
}