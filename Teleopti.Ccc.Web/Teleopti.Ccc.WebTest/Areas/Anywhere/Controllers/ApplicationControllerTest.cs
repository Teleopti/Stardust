using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var target = new ApplicationController();
			var result = target.Index();
			result.FileName.Should().Be("~/Areas/Anywhere/Content/Templates/index.html");
			result.ContentType.Should().Be("text/html");
		}

	}
}