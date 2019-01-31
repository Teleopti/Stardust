using System;
using System.Net.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettingsTenantClient : ISharedSettingsTenantClient
	{
		private readonly string _urlToServer;
		private readonly HttpClient _httpClient;

		public SharedSettingsTenantClient(string urlToServer)
		{
			_urlToServer = urlToServer;
			_httpClient = new HttpClient {BaseAddress = new Uri(_urlToServer)};
		}

		public SharedSettings GetSharedSettings()
		{
			var response = _httpClient.GetAsync("Start/Config/SharedSettings").GetAwaiter().GetResult();
			if (!response.IsSuccessStatusCode)
				return new SharedSettings();

			var jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var ret = JsonConvert.DeserializeObject<SharedSettings>(jsonString);
			ret.Hangfire =
				Encryption.DecryptStringFromBase64(ret.Hangfire, EncryptionConstants.Image1,
					EncryptionConstants.Image2);
			return ret;
		}
	}
}