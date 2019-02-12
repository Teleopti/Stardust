using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Auth;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class GrantBotApiController : ApiController
	{
		private readonly IConfigReader _configReader;
		private readonly IHttpServer _httpRequestHandler;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IServerConfigurationRepository _serverConfigurationRepository;
		private readonly SignatureCreator _signatureCreator;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;

		public GrantBotApiController(IConfigReader configReader, IHttpServer httpRequestHandler,
			ICurrentHttpContext currentHttpContext, IServerConfigurationRepository serverConfigurationRepository,
			SignatureCreator signatureCreator, INow now, ILoggedOnUser loggedOnUser)
		{
			_configReader = configReader;
			_httpRequestHandler = httpRequestHandler;
			_currentHttpContext = currentHttpContext;
			_serverConfigurationRepository = serverConfigurationRepository;
			_signatureCreator = signatureCreator;
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		[HttpGet, Route("api/GrantBot/Config"), TenantUnitOfWork]
		public virtual async Task<GrantBotConfig> GetGrantBotConfig()
		{
			var secret = _serverConfigurationRepository.Get(ServerConfigurationKey.GrantBotDirectLineSecret);
			if (string.IsNullOrEmpty(secret))
			{
				secret = _configReader.AppConfig(ServerConfigurationKey.GrantBotDirectLineSecret.ToString());
			}
			var url =  _configReader.AppConfig("GrantBotTokenGenerateUrl");
			if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(url))
			{
				return null;
			}
			
			var response = await _httpRequestHandler.Post(url, new
			{
				TrustedOrigins = new[]
				{
					_currentHttpContext.Current().Request.UrlConsideringLoadBalancerHeaders()
				}
			}, _ => new NameValueCollection{{"Authorization", $"Bearer {secret}"}});

			var token = string.Empty;
			if (response.IsSuccessStatusCode)
			{
				var body = await response.Content.ReadAsStringAsync();
				token = JsonConvert.DeserializeObject<DirectLineToken>(body).token;
			}

			var timestamp = _now.UtcDateTime().Ticks;
			var config = new GrantBotConfig
			{
				Timestamp = timestamp,
				Signature = _signatureCreator.Create($"{_loggedOnUser.CurrentUser().Id.ToString().ToLowerInvariant()}{timestamp}"),
				Token = token,
			};
			return config;
		}
	}
}
