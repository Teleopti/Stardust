using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class AjaxJavaScriptRedirectAttributeTest
	{

		[Test]
		public void ShouldAlterRedirectResultToJavaScriptResultWhenInAjax()
		{
			var target = new AjaxJavaScriptRedirectAttribute();

			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => new RedirectResult("a/url"));
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<JavaScriptResult>();
		}

		[Test] 
		public void ShouldAlterRedirectToRouteResultToJavaScriptResultWhenInAjax()
		{
			var target = new AjaxJavaScriptRedirectAttribute();

			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index" })));
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<JavaScriptResult>();
		}


		[Test]
		public void ShouldNotAlterRedirectResultWhenNotInAjax()
		{
			var target = new AjaxJavaScriptRedirectAttribute();

			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new RedirectResult("a/url"));
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectResult>();
		}

		[Test]
		public void ShouldNotAlterRedirectToRouteResultWhenNotInAjax()
		{
			var target = new AjaxJavaScriptRedirectAttribute();

			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index" })));
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectToRouteResult>();
		}
	}
}