using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http.Controllers;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Mart.Controllers;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

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

			var controller = setupControllerForTests(handler, "Teleopti WFM", 1);

			controller.PostIntervals(queueStatsModels);
			handler.AssertWasCalled(x => x.Handle(queueStatsModels, "Teleopti WFM", 1, 0));
		}

		private static QueueStatsController setupControllerForTests(IQueueStatHandler handler, string nhibName, int sourceId)
		{
			var controller = new QueueStatsController(handler)
			{
				RequestContext = new HttpRequestContext {Principal = createPrincipal(nhibName)},
				Request = new HttpRequestMessage()
			};

			controller.Request.Headers.Add("sourceId", sourceId.ToString(CultureInfo.InvariantCulture));
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
