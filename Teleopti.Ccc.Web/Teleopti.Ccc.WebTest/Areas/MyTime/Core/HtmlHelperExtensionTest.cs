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

	}
}