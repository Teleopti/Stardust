using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http.Controllers;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Mart.Controllers;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Controllers
{
	[TestFixture]
	class QueueStatsControllerTest
	{

		[Test]
		public void ShouldCallHandler()
		{
			var queueStatsModels = new List<QueueStatsModel> { new QueueStatsModel() };
			var handler = MockRepository.GenerateMock<IQueueStatHandler>();
			var publisher = MockRepository.GenerateMock<IMessagePopulatingServiceBusSender>();

			var controller = setupControllerForTests(handler, "Teleopti WFM", 1, publisher);

			controller.PostIntervals(queueStatsModels);
			handler.AssertWasCalled(x => x.Handle(queueStatsModels, "Teleopti WFM", 1, 0));
		}

		private static QueueStatsController setupControllerForTests(IQueueStatHandler handler, string nhibName, int sourceId, IMessagePopulatingServiceBusSender populatingPublisher)
		{
			//other properties could be set if needed later
			//var config = new HttpConfiguration();
			//var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/queuestat");
			//var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
			//var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "queuestat" } });
			var controller = new QueueStatsController(handler, populatingPublisher)
			{
				//ControllerContext = new HttpControllerContext(config, routeData, request),
				//Request = request,
				RequestContext = new HttpRequestContext {Principal = createPrincipal(nhibName)},
				Request = new HttpRequestMessage()
			};

			controller.Request.Headers.Add("sourceId", sourceId.ToString(CultureInfo.InvariantCulture));
			//controller.Request.Properties.Add(HttpPropertyKeys.HttpRouteDataKey, routeData);
			//controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
			return controller;
		}

		private static ClaimsPrincipal createPrincipal(string name)
		{
			var claims = new List<Claim>
			{
				new Claim(System.IdentityModel.Claims.ClaimTypes.NameIdentifier, name ),
				new Claim(System.IdentityModel.Claims.ClaimTypes.Locality, name )
			};
			var claimsIdentity = new ClaimsIdentity(claims, "Basic", System.IdentityModel.Claims.ClaimTypes.NameIdentifier,"");
			
			return new ClaimsPrincipal(claimsIdentity);
		}
	}
}
