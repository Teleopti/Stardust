using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class GrantBotApiController : ApiController
	{
		private readonly IConfigReader _configReader;
		private readonly IHttpRequestHandler _httpRequestHandler;

		public GrantBotApiController(IConfigReader configReader,
			IHttpRequestHandler httpRequestHandler)
		{
			_configReader = configReader;
			_httpRequestHandler = httpRequestHandler;
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
