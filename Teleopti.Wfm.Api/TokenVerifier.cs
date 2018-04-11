using System.Net.Http;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Api
{
	public class TokenVerifier : ITokenVerifier
	{
		private readonly HttpClient client = new HttpClient();
		private readonly string _url;

		public TokenVerifier(IConfigReader configReader)
		{
			_url = configReader.AppConfig("Web") + "api/token/verify";
		}

		public bool TryGetUser(string token, out string user)
		{
			var result = client.PostAsJsonAsync(_url, token);
			var content = result.Result.EnsureSuccessStatusCode()
				.Content
				.ReadAsStringAsync().Result;
			if (string.IsNullOrWhiteSpace(content))
			{
				user = null;
				return false;
			}

			user = content;
			return true;
		}
	}
}