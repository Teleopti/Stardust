using System;
using System.Net.Http;
using System.Threading.Tasks;
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

		public async Task<ValueTuple<bool,UserIdWithTenant>> TryGetUserAsync(string token)
		{
			var result = await client.PostAsJsonAsync(_url, token);
			var content = await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
			if (string.IsNullOrWhiteSpace(content))
			{
				return (false, new UserIdWithTenant());
			}

			var x = JObject.Parse(content);
			return (true, new UserIdWithTenant
			{
				UserId = Guid.Parse(x["PersonId"].Value<string>()),
				Tenant = x["Tenant"].Value<string>()
			});
		}
	}
}