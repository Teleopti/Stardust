using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest.Filters
{
	public class FilterTester
	{
		private readonly NameValueCollection _headers = new NameValueCollection();
		private readonly Dictionary<string, string> _items = new Dictionary<string, string>();
		private Func<ActionResult> _controllerAction = () => null;
		private bool _isCustomErrorEnabled;
		private ITestController _controller;
		private IPrincipal _user = Thread.CurrentPrincipal;
		private readonly IDictionary<string, string> _routeDataTokens = new Dictionary<string, string>();

		public ActionResult InvokeFilter(IAuthorizationFilter authorizationFilter) { return InvokeFilter(new FilterTestActionInvoker(authorizationFilter)); }
		public ActionResult InvokeFilter(AuthorizeAttribute authorizationFilter) { return InvokeFilter(new FilterTestActionInvoker(authorizationFilter)); }
		public ActionResult InvokeFilter(IExceptionFilter exceptionFilter) { return InvokeFilter(new FilterTestActionInvoker(exceptionFilter)); }
		public ActionResult InvokeFilter(IResultFilter resultFilter) { return InvokeFilter(new FilterTestActionInvoker(resultFilter)); }
		public ActionResult InvokeFilter(IActionFilter actionFilter) { return InvokeFilter(new FilterTestActionInvoker(actionFilter)); }
		public ActionResult InvokeFilter(ActionFilterAttribute actionFilter) { return InvokeFilter(new FilterTestActionInvoker(actionFilter)); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public FilterTester()
		{
			HttpContext.Current = new HttpContext(new HttpRequest("foo", "http://tempuri.org/foo", ""), new HttpResponse(new StringWriter()));
		}

		private ActionResult InvokeFilter(FilterTestActionInvoker invoker)
		{
			SetupRoutes();
			_controller = CreateTestController(_controllerAction);
			invoker.InvokeAction(_controller.ControllerContext, "DummyAction");
			return invoker.ActionResult;
		}

		private void SetupRoutes()
		{
			RouteTable.Routes.Clear();
			RouteTable.Routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
				);
		}

		public void ActionMethod(Func<ActionResult> controllerAction) { _controllerAction = controllerAction; }

		public void IsAjaxRequest()
		{
			_headers.Add("X-Requested-With", "XMLHttpRequest");
			_items.Add("X-Requested-With", "XMLHttpRequest");
		}

		public void IsCustomErrorEnabled() { _isCustomErrorEnabled = true; }

		public void AcceptJson() { _headers.Add("Accept", "application/json, text/javascript"); }

		public void IsUser(IPrincipal principal) { _user = principal; }

		public void UseController(ITestController controller) { _controller = controller; }

		public void AddRouteDataToken(string key, string value) { _routeDataTokens.Add(key, value); }

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private ITestController CreateTestController(Func<ActionResult> controllerAction)
		{
			ITestController controller = new TestController(controllerAction);
			if (_controller != null)
				controller = _controller;

			new StubbingControllerBuilder().InitializeController((Controller) controller);

			controller.ControllerContext.RouteData.Values.Add("controller", "TestController");
			controller.ControllerContext.RouteData.Values.Add("action", "DummyAction");
			_routeDataTokens.ToList().ForEach(kv => controller.ControllerContext.RouteData.DataTokens.Add(kv.Key, kv.Value));

			controller.ControllerContext.HttpContext.Stub(x => x.IsCustomErrorEnabled).Return(_isCustomErrorEnabled);
			controller.ControllerContext.HttpContext.Stub(x => x.Items).Return(_items);
			controller.ControllerContext.HttpContext.Stub(x => x.User).Return(_user);
			controller.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(_headers);
			controller.ControllerContext.HttpContext.Request.Stub(x => x.RequestContext)
				.Return(new RequestContext(controller.ControllerContext.HttpContext, controller.ControllerContext.RouteData));

			var cache = MockRepository.GenerateMock<HttpCachePolicyBase>();
			controller.ControllerContext.HttpContext.Response.Stub(x => x.Cache).Return(cache);

			return controller;
		}

		public ControllerContext ControllerContext { get { return _controller.ControllerContext; } }

		#region Nested type: FilterTestActionInvoker

		public class FilterTestActionInvoker : ControllerActionInvoker
		{
			private readonly IActionFilter _actionFilter;
			private readonly IAuthorizationFilter _authorizeFilter;
			private readonly IExceptionFilter _exceptionFilter;
			private readonly IResultFilter _resultFilter;

			public FilterTestActionInvoker(IAuthorizationFilter authorizeFilter) { _authorizeFilter = authorizeFilter; }
			public FilterTestActionInvoker(AuthorizeAttribute authorizeFilter) { _authorizeFilter = authorizeFilter; }
			public FilterTestActionInvoker(IExceptionFilter exceptionFilter) { _exceptionFilter = exceptionFilter; }
			public FilterTestActionInvoker(IResultFilter resultFilter) { _resultFilter = resultFilter; }
			public FilterTestActionInvoker(IActionFilter actionFilter) { _actionFilter = actionFilter; }

			public FilterTestActionInvoker(ActionFilterAttribute actionFilter)
			{
				_actionFilter = actionFilter;
				_resultFilter = actionFilter;
			}

			public ActionResult ActionResult { get; set; }

			protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
			{
				var filterInfo = base.GetFilters(controllerContext, actionDescriptor);
				if (_authorizeFilter != null)
					filterInfo.AuthorizationFilters.Add(_authorizeFilter);
				if (_exceptionFilter != null)
					filterInfo.ExceptionFilters.Add(_exceptionFilter);
				if (_resultFilter != null)
					filterInfo.ResultFilters.Add(_resultFilter);
				if (_actionFilter != null)
					filterInfo.ActionFilters.Add(_actionFilter);
				return filterInfo;
			}

			protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult) { ActionResult = actionResult; }
		}

		#endregion

		#region Nested type: TestController

		public interface ITestController
		{
			ActionResult DummyAction();
			ControllerContext ControllerContext { get; }
		}

		public class TestController : Controller, ITestController
		{
			private readonly Func<ActionResult> _controllerAction;

			public TestController(Func<ActionResult> controllerAction) { _controllerAction = controllerAction; }

			public ActionResult DummyAction() { return _controllerAction.Invoke(); }
		}

		#endregion
	}
}