using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SharpTestsEx;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
			var configReader = createFakeConfigReader();

			var expectedToken = new DirectLineToken
			{
				token = "token456"
			};
			var httpRequestHandler = new FakeHttpServer();
			httpRequestHandler.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(expectedToken), System.Text.Encoding.UTF8,
					"application/json")
			});

			var target = new GrantBotApiController(configReader, httpRequestHandler, new FakeCurrentHttpContext(),
				new FakeServerConfigurationRepository(), new SignatureCreator(configReader), new MutableNow(),
				new FakeLoggedOnUser());

			var result = await target.GetGrantBotConfig();
			result.Token.Should().Be(expectedToken.token);
			httpRequestHandler.Requests[0].Headers["Authorization"].Should().Be.EqualTo("Bearer Secret123");
		}

		[Test]
		public async Task ShouldGetTokenFromBotServiceUsingServerSettings()
		{
			var configReader = createFakeConfigReader();
			var expectedToken = new DirectLineToken
			{
				token = "token456"
			};
			var httpRequestHandler = new FakeHttpServer();
			httpRequestHandler.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(expectedToken), System.Text.Encoding.UTF8,
					"application/json")
			});

			var configurationRepository = new FakeServerConfigurationRepository();
			configurationRepository.Update(ServerConfigurationKey.GrantBotApiUrl.ToString(),
				"https://teleoptibottest.azurewebsites.net/t/api/tenant");
			configurationRepository.Update(ServerConfigurationKey.GrantBotDirectLineSecret.ToString(), "Secret1234");
			var target = new GrantBotApiController(configReader, httpRequestHandler, new FakeCurrentHttpContext(),
				configurationRepository, new SignatureCreator(configReader), new MutableNow(), new FakeLoggedOnUser());

			var result = await target.GetGrantBotConfig();
			result.Token.Should().Be(expectedToken.token);
			httpRequestHandler.Requests[0].Headers["Authorization"].Should().Be.EqualTo("Bearer Secret1234");
		}

		[Test]
		public async Task ShouldCreateSignature()
		{
			var configReader = createFakeConfigReader();

			var expectedToken = new DirectLineToken
			{
				token = "token456"
			};
			var httpRequestHandler = new FakeHttpServer();
			httpRequestHandler.FakeResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(expectedToken), System.Text.Encoding.UTF8,
					"application/json")
			});

			var now = new DateTime(2019, 02, 12, 01, 13, 45, DateTimeKind.Utc);
			var personId = new Guid("D4A1B6A4-7EA3-48DB-BA61-DE9DD6FFB389");
			var fakeLoggedOnUser = new FakeLoggedOnUser();
			fakeLoggedOnUser.CurrentUser().SetId(personId);
			var target = new GrantBotApiController(configReader, httpRequestHandler, new FakeCurrentHttpContext(),
				new FakeServerConfigurationRepository(), new SignatureCreator(configReader), new MutableNow(now),
				fakeLoggedOnUser);

			var result = await target.GetGrantBotConfig();
			result.Timestamp.Should().Be(now.Ticks);
			result.Signature.Should().Be(
				"sD5jNVCGUqbvsmABXkjvwC3raRKCUVYT0Q5HeoV5Kv6w3sYYwi8CZAi1qxbtVInw366vIGUdFg653mCJufK0yTmdxXhqBcrRV9EiGtOzhnmo8dadDTVMnb6VH7TwjhmSv7B2E+qjA7Vv0HdGmc5lFoSlJgn5WP6QWNdt+p4epDTGVxBgZvTGqg58MEioOr3JlKMFPrAqcYHDtFrHYtNnNio52wXKq+11UeHJQ8HIgSfTU51bOYGgIZdJmIR5QJj2Y2PNzHctGrfRQTFL5h7OWW1m+km/k11tr10ZsxbOZwDGXSe9a5/a2KbaC4qaDnjHtUeE+pS8YaN95L7XFOegDw==");
		}

		private IConfigReader createFakeConfigReader()
		{
			return new FakeConfigReader(new Dictionary<string, string>
			{
				{
					"GrantBotDirectLineSecret", "Secret123"
				},
				{
					"GrantBotTokenGenerateUrl",
					"https://directline.botframework.com/v3/directline/tokens/generate"
				},
				{
					"CertificateModulus",
					"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w=="
				},
				{"CertificateExponent", "AQAB"},
				{
					"CertificateP",
					"8r2FFhgc78WZf/uKEjHPyiLL9FkcjbPsdLB/Dd6AEOVuzpVFlBJsai31gyLIUU3zY6gE/NdMZzQ7ejsjhbpC4/ptbJguTpIOGB+7dX+/DEdwZkx8rIlNG32VDIdP6kqpwPzhtGVfNiq8xHaS+SHTQf6JSWQtNKgVbilWgYyEZ9k="
				},
				{
					"CertificateQ",
					"v7Hm0iP49ReGhVvKdAsDUgcK0olmVGAKwsxsFnXUGkAWydh+T3QaChYkYBS+h5cX4UBlil5FaJKtGq4wduKDkMCN8TNHh3n2k05rh4DxxPmLvhCqkQgvMB/22E+z10VAmjKPq7BnAq/lc2rGJWa1lq3qaeSkcF6agCPQVYd6vKs="
				},
				{
					"CertificateDP",
					"7mr7dvIEKfVZiX0U5j4Kq611yfBkvUHFs+9PO94Yx3+yUDIJfyCBX+D4Te8x9bmsn2t+SqFlJ9EDwlCn2UdTP/zO0WS/xuhp84PnaccpbPQWEER8CDNrit7UMNQOyD7BcQ5w2fDfjaJ4ejdEsHJqv100luNQC3I0alkr4F6WBjE="
				},
				{
					"CertificateDQ",
					"nZ2rSlGlm/BR/Ujx9+QuQL3lmiK7btjhQDZREU6krUjQ8/n8MVwnJO/7zLyBxH7pdZ47X0AQFeG0T2G2G6o3v0dz7kTZpX0Uzx4FsA7Hu8vrqMWPWVy/X/SIRGeUWYZpjd/Q3bxXlpAGO5Ypggsnd9NcEOGci4BdzMqlvA1/T60="
				},
				{
					"CertificateInverseQ",
					"4OVuc98gImxH9yLmvMimnr9zFxw6pwRd++/A7q1Uj8BrCKhzD8rY6QosNZCKlMjWdHNYtAWrDDqxwCRPUYhmdOm+JEkxBL7yelodmongNXUS0J9kf9c+k8fjgEsB02J15QqryyuRvw0Z638BheD9Ry3NN/REO0pdnj2LHxa17V0="
				},
				{
					"CertificateD",
					"YQOGoS8pc9rSE1OUoOJlN0+bI57kOlD0uO3/NYrN3Lkyx51tMtALGTMFt7d4SNsVwgQ+EGQaM5IJcd/ylx9kgESQBvSjEhJLLiKWukQG2BH+rpOjN2Fq3qU4mqmHpQn6tbNox98Af1aDNnO+Coi652jQxbv4Kh0ot9zJHddK5wuTxIQDLAxyb50f/ReG3UekxZoEOZEtj1oEd+QB/py+hP7Xp010Wfzy82g5Ec3ELjzeNPtijxmO+WExoF5zALIUYd8ClH+ayr2Ab3rD0Dv8VM1Y08npzSs6d5OOAAnG+245koiwkgJoXvZg0EVkcdJxZsMxIGL/OWEl82VnqC6mUQ=="
				}
			});
		}
	}
}