using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Tests.Controllers
{
	[TestFixture]
	public class HomeControllerTest
	{
		[Test]
		public void GetAllTenantsShouldNoBeNull()
		{
			var controller = new HomeController(new TenantListFake());

			controller.GetAllTenants().Should().Not.Be.Null();
		}

		
	}
}
