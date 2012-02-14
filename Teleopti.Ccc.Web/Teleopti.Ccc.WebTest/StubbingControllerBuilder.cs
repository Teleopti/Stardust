using System.Web.Mvc;
using MvcContrib.TestHelper;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest
{
	public class StubbingControllerBuilder
	{
		private readonly TestControllerBuilder _testControllerBuilder = new TestControllerBuilder(new RhinoMocksStubFactory());

		public T CreateController<T>(params object[] constructorArgs) where T : Controller
		{
			var controller = _testControllerBuilder.CreateController<T>(constructorArgs);
			StubStuff(controller);
			return controller;
		}

		public void InitializeController(Controller controller)
		{
			_testControllerBuilder.InitializeController(controller);
			StubStuff(controller);
		}

		private static void StubStuff(Controller controller)
		{
			controller.ControllerContext.HttpContext.Response.Stub(x => x.StatusCode).PropertyBehavior();
			controller.ControllerContext.HttpContext.Response.Stub(x => x.TrySkipIisCustomErrors).PropertyBehavior();
		}
	}
}