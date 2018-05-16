using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Teleopti.Wfm.Api.Test
{
	[ApiTest]
	public class CommandListingTest
	{
		public IApiHttpClient Client;

		[Test]
		public async Task ShouldListCommands()
		{
			Client.Authorize();

			var result = await Client.GetAsync("/command");
			result.EnsureSuccessStatusCode();

			var content = await result.Content.ReadAsStringAsync();
			var obj = JToken.Parse(content);
			obj["Result"].Children().Should().Be.Empty();
		}
	}
}