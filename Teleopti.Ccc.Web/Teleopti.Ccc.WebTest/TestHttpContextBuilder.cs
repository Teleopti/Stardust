using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest
{
	// a stubbing variation of this
	// http://www.hanselman.com/blog/ASPNETMVCSessionAtMix08TDDAndMvcMockHelpers.aspx
	public class TestHttpContextBuilder
	{
		public HttpContextBase StubHttpContext()
		{
			var context = MockRepository.GenerateMock<HttpContextBase>();
			var request = MockRepository.GenerateMock<HttpRequestBase>();
			var response = MockRepository.GenerateMock<HttpResponseBase>();
			var session = MockRepository.GenerateMock<HttpSessionStateBase>();
			var server = MockRepository.GenerateMock<HttpServerUtilityBase>();

			context.Stub(x => x.Request).Return(request);
			context.Stub(x => x.Response).Return(response);
			context.Stub(x => x.Session).Return(session);
			context.Stub(x => x.Server).Return(server);

			request.Stub(x => x.ApplicationPath).Return("/");
			request.Stub(x => x.ServerVariables).Return(new NameValueCollection());
			request.Stub(x => x.PathInfo).Return(string.Empty);

			response.Stub(x => x.ApplyAppPathModifier(Arg<string>.Is.Anything))
				.Return(null)
				.WhenCalled(x => x.ReturnValue = x.Arguments[0]);

			return context;
		}

		public HttpContextBase StubHttpContext(string url)
		{
			var context = StubHttpContext();

			context.Request.Stub(x => x.Url).Return(new Uri(url));
			context.Request.Stub(x => x.QueryString).Return(GetQueryStringParameters(url));
			context.Request.Stub(x => x.AppRelativeCurrentExecutionFilePath).Return(GetUrlFileName(url));

			return context;
		}

		public void SetFakeControllerContext(Controller controller)
		{
			var httpContext = StubHttpContext();
			var context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
			controller.ControllerContext = context;
		}

		private string GetUrlFileName(string url) {
			return new Uri(url).AbsolutePath;
		}

		private NameValueCollection GetQueryStringParameters(string url)
		{
			var query = new Uri(url).Query;
			if (string.IsNullOrEmpty(query))
			{
				return null;
			}

			var parameters = new NameValueCollection();

			var keys = query.Split("&".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);

			foreach (var key in keys)
			{
				var part = key.Split("=".ToCharArray());
				parameters.Add(part[0], part[1]);
			}

			return parameters;
		}
	}
}