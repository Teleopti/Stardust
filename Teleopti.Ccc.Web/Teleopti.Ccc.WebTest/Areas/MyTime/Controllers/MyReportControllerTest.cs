using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MyReportControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForMyReport_ShouldReturnPartialView()
		{
			var target = new MyReportController();
			
			var result = target.Index();

			result.ViewName.Should().Be.EqualTo("MyReportPartial");
		}
	}
}