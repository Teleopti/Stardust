using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;

namespace Teleopti.Wfm.Api.Test
{
	public class AuthorizationTest : ApiTest
	{
		[Test]
		public async Task ShouldRequireAuthorization()
		{
			var result = await Client.GetAsync("/api/wfm/command");
			result.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
		}
	}
}