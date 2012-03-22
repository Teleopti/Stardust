using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class TestControllerTest
	{
		/*
		 * just dummy tests here - impl used only from behaviour tests  
		 * (and they are not part of ncover)
		 */

		[Test]
		public void PlainStupid()
		{
			using (var target = new TestController(MockRepository.GenerateStub<ISessionSpecificDataProvider>()))
			{
				target.CorruptMyCookie();
				target.ExpireMyCookie();
				target.NonExistingDatasourceCookie();	
			}
		}
	}
}