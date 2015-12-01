using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var target = new ApplicationController();
			var result = target.Index();
			result.ViewName.Should().Be.Empty();
		}
	}
}