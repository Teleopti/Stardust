using System.Globalization;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core
{
	[TestFixture]
	public class HtmlHelperExtensionTest
	{
		[Test]
		public void ShouldGetSubHelpers()
		{
			var target = new TestHtmlHelperBuilder().CreateHtmlHelper();
			target.Schedule().Should().Not.Be.Null();
			target.Portal().Should().Not.Be.Null();
			target.LayoutBase().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnHtmlStyleLeft()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");
			var target = new TestHtmlHelperBuilder().CreateHtmlHelper();
			target.HtmlStyleLeft().Should().Be("left");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ar-SA"); // right-to-left = true;
			target.HtmlStyleLeft().Should().Be("right");
		}

	}
}