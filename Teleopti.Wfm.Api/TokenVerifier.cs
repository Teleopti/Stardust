using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Api
{
	public class TokenVerifier : ITokenVerifier
	{
		private readonly HttpClient client = new HttpClient();
		private readonly string _url;

		public TokenVerifier(IConfigReader configReader)
		{
			_url = configReader.AppConfig("TenantServer") + "api/token/verify";
		}

		public bool TryGetUser(string token, out UserIdWithTenant user)
		{
			var result = client.PostAsJsonAsync(_url, token);
			var content = result.GetAwaiter().GetResult().EnsureSuccessStatusCode()
				.Content
				.ReadAsStringAsync().GetAwaiter().GetResult();
			if (string.IsNullOrWhiteSpace(content))
			{
				user = new UserIdWithTenant();
				return false;
			}

			var x = JObject.Parse(content);
			user = new UserIdWithTenant
			{
				UserId = Guid.Parse(x["PersonId"].Value<string>()),
				Tenant = x["Tenant"].Value<string>()
			};
			return true;
		}
	}
}