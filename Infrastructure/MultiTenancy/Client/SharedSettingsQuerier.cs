using System.Net.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettingsQuerier : ISharedSettingsQuerier
	{
		private readonly string _urlToServer;

		public SharedSettingsQuerier(string urlToServer)
		{
			_urlToServer = urlToServer;
		}

		public SharedSettings GetSharedSettings()
		{
			using (var httpClient = new HttpClient())
			{
				var response = httpClient.GetAsync(_urlToServer + "Start/Config/SharedSettings").GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
					return new SharedSettings();

				var jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var ret = JsonConvert.DeserializeObject<SharedSettings>(jsonString);
				ret.Hangfire = Encryption.DecryptStringFromBase64(ret.Hangfire, EncryptionConstants.Image1, EncryptionConstants.Image2);
				return ret;
			}
		}
	}
}