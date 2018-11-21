using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class GetAllActivitiesTest
	{
		public IApiHttpClient Client;
		public IActivityRepository ActivityRepository;

		[Test]
		public async Task ShouldGetAllActivities()
		{
			Client.Authorize();

			ActivityRepository.Add(new Activity("Activity1"));
			ActivityRepository.Add(new Activity("Activity2"));

			var result = await Client.PostAsync("/query/Activity/AllActivities", new StringContent("{}", Encoding.UTF8, "application/json"));
			var activitesObj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"];
			activitesObj.Count().Should().Be.EqualTo(2);
		}
	}
}
