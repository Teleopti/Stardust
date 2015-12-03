using System.Net.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

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
				var response = httpClient.GetAsync(_urlToServer + "Start/Config/SharedSettings").Result;
				if (!response.IsSuccessStatusCode)
					return new SharedSettings();

				var jsonString = response.Content.ReadAsStringAsync().Result;
				var ret = JsonConvert.DeserializeObject<SharedSettings>(jsonString);
				ret.Queue = Encryption.DecryptStringFromBase64(ret.Queue, EncryptionConstants.Image1, EncryptionConstants.Image2);
				ret.Hangfire = Encryption.DecryptStringFromBase64(ret.Hangfire, EncryptionConstants.Image1, EncryptionConstants.Image2);

				return ret;
			}
		}
	}
}