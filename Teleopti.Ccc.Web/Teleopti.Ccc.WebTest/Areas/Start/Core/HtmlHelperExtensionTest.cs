using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core
{
	[TestFixture]
	public class HtmlHelperExtensionTest
	{
		[Test]
		public void ShouldGetSubHelpers()
		{
			var target = new TestHtmlHelperBuilder().CreateHtmlHelper();
			target.LayoutBase().Should().Not.Be.Null();
		}

	}
}