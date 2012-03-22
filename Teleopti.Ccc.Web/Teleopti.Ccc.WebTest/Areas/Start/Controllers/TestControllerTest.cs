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

		private TestController target;

		[SetUp]
		public void Setup()
		{
			target = new TestController(MockRepository.GenerateStub<ISessionSpecificDataProvider>());
		}

		[Test]
		public void PlainStupid()
		{
			target.CorruptMyCookie();
			target.ExpireMyCookie();
			target.NonExistingDatasourceCookie();
		}
	}
}