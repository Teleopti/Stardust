using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ChatConfigurationController : ApiController
	{
		private readonly IHttpServer _httpServer;
		private readonly IServerConfigurationRepository _serverConfigurationRepository;

		public ChatConfigurationController(IHttpServer httpServer,
			IServerConfigurationRepository serverConfigurationRepository)
		{
			_httpServer = httpServer;
			_serverConfigurationRepository = serverConfigurationRepository;
		}

		[Route("api/chatconfiguration/exists"), HttpPost, TenantUnitOfWork]
		public virtual async Task<bool> Exists([FromBody] TenantCredential tenantCredential)
		{
			var baseUri = getBaseUri();
			var result = await _httpServer.Post($"{baseUri}/exists", tenantCredential);
			result.EnsureSuccessStatusCode();
			var content = await result.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<bool>(content);
		}

		private string getBaseUri()
		{
			return _serverConfigurationRepository.Get(ServerConfigurationKey.GrantBotApiUrl);
		}

		[Route("api/chatconfiguration"), HttpPost, TenantUnitOfWork]
		public virtual async Task Configure([FromBody] TenantCredential tenantCredential)
		{
			var baseUri = getBaseUri();
			var result = await _httpServer.Post($"{baseUri}", tenantCredential);
			result.EnsureSuccessStatusCode();
		}
	}
}