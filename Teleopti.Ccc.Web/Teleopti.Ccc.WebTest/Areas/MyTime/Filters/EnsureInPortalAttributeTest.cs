using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.WebTest.Filters;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Filters
{
	[TestFixture]
	public class EnsureInPortalAttributeTest
	{

		[Test]
		public void ShouldRedirectWithPortalPrefixIfNotLoadedWithAjax()
		{
			var target = new EnsureInPortalAttribute();

			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ContentResult());
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectResult>();
		}
		
		[Test]
		public void ShouldNotInterceptWhenLoadedWithAjax()
		{
			var target = new EnsureInPortalAttribute();
			var actionResult = new ContentResult();

			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();
			filterTester.ActionMethod(() => actionResult);
			var result = filterTester.InvokeFilter(target);

			result.Should().Be.SameInstanceAs(actionResult);
		}

	}
}