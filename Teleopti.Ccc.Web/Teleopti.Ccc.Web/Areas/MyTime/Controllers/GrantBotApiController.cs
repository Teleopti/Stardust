using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Auth;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class GrantBotApiController : ApiController
	{
		private readonly IConfigReader _configReader;
		private readonly IHttpRequestHandler _httpRequestHandler;
		private readonly ICurrentHttpContext _currentHttpContext;

		public GrantBotApiController(IConfigReader configReader,
			IHttpRequestHandler httpRequestHandler, ICurrentHttpContext currentHttpContext)
		{
			_configReader = configReader;
			_httpRequestHandler = httpRequestHandler;
			_currentHttpContext = currentHttpContext;
		}

		[HttpGet, Route("api/GrantBot/Config")]
		public async Task<GrantBotConfig> GetGrantBotConfig()
		{
			var secret = _configReader.AppConfig("GrantBotDirectLineSecret");
			var url = _configReader.AppConfig("GrantBotTokenGenerateUrl");
			if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(url))
			{
				return null;
			}

			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
			request.Content = new StringContent(JsonConvert.SerializeObject(new { TrustedOrigins = new []
			{
				_currentHttpContext.Current().Request.UrlConsideringLoadBalancerHeaders()
			}}), Encoding.UTF8, "application/json");

			var response = await _httpRequestHandler.SendAsync(request);

			var token = string.Empty;
			if (response.IsSuccessStatusCode)
			{
				var body = await response.Content.ReadAsStringAsync();
				token = JsonConvert.DeserializeObject<DirectLineToken>(body).token;
			}

			var config = new GrantBotConfig
			{
				Token = token,
			};
			return config;
		}
	}
}
