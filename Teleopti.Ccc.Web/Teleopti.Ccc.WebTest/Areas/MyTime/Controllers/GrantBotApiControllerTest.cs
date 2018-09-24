using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SharpTestsEx;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class GrantBotApiControllerTest
	{
		[Test]
		public async Task ShouldGetTokenFromBotService()
		{
			var configReader = new FakeConfigReader();
			configReader.FakeSetting("GrantBotDirectLineSecret", "Secret123");
			configReader.FakeSetting("GrantBotTokenGenerateUrl",
				"https://directline.botframework.com/v3/directline/tokens/generate");

			var expectedToken = new DirectLineToken
			{
				token = "token456"
			};
			var httpRequestHandler = new FakeHttpRequestHandler();
			httpRequestHandler.SetFakeResponse(new HttpResponseMessage(HttpStatusCode.OK)
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(expectedToken), System.Text.Encoding.UTF8,
					"application/json")
			});

			var target = new GrantBotApiController(configReader, httpRequestHandler, new FakeCurrentHttpContext());

			var result = await target.GetGrantBotConfig();
			result.Token.Should().Be(expectedToken.token);
		}
	}
}