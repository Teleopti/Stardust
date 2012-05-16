using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using SharpTestsEx;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.LayoutBase
{
	[TestFixture]
	public class SplitButtonSplitterTest
	{
		[Test]
		public void ShoudReturnCorrectProperty()
		{
			var splitButtonSplitter=new SplitButtonSplitter();
			splitButtonSplitter.Value.Should().Be.EqualTo("-");
			splitButtonSplitter.Text.Should().Be.EqualTo("-");
		}
	}
}
