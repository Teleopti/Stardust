using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ChatConfigurationController : ApiController
	{
		private readonly IHttpServer _httpServer;
		private readonly string _baseUri;

		public ChatConfigurationController(IHttpServer httpServer, IConfigReader configReader)
		{
			_httpServer = httpServer;
			_baseUri = configReader.AppConfig("GrantBotApiUrl");
		}

		[Route("api/chatconfiguration/exists"), HttpPost]
		public async Task<bool> Exists([FromBody]TenantCredential tenantCredential)
		{
			var result = await _httpServer.Post($"{_baseUri}/exists", tenantCredential);
			result.EnsureSuccessStatusCode();
			var content = await result.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<bool>(content);
		}

		[Route("api/chatconfiguration"), HttpPost]
		public async Task Configure([FromBody]TenantCredential tenantCredential)
		{
			var result = await _httpServer.Post($"{_baseUri}", tenantCredential);
			result.EnsureSuccessStatusCode();
		}
	}
}