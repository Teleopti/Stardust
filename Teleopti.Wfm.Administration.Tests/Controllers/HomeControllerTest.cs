using System.Web.Mvc;
using NUnit.Framework;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Tests.Controllers
{
	[TestFixture]
	public class HomeControllerTest
	{
		[Test]
		public void Index()
		{
			var controller = new HomeController(new TenantListFake());
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			
		}

		
	}
}
