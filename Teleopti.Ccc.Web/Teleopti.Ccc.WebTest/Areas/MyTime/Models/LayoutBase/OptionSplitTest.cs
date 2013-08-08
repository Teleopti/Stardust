using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using SharpTestsEx;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.LayoutBase
{
	[TestFixture]
	public class OptionSplitTest
	{
		[Test]
		public void ShouldBeCreatedWithCorrectValues()
		{
			var splitButtonSplitter = new OptionSplit();
			splitButtonSplitter.Value.Should().Be.EqualTo("-");
			splitButtonSplitter.Text.Should().Be.EqualTo(string.Empty);
		}
	}
}
