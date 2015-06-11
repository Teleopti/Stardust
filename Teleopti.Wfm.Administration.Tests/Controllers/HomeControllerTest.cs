using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Tests.Controllers
{
	[TestClass]
	public class HomeControllerTest
	{
		[TestMethod]
		public void Index()
		{
			var controller = new HomeController(new TenantListFake());
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			
		}

		
	}
}
