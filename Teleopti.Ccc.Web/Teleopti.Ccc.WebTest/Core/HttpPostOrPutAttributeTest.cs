using System.Web.Mvc;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class HttpPostOrPutAttributeTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldHaveCoverage()
		{
			var target = new HttpPostOrPutAttribute();
			var controller = new TestController();
			var controllerBuilder = new TestControllerBuilder();
			controllerBuilder.InitializeController(controller);
			target.IsValidForRequest(controller.ControllerContext, typeof (TestController).GetMethod("Action"));
		}

		class TestController : Controller
		{
			[HttpPostOrPut]
			public void Action()
			{
				
			}
		}

	}
}
