using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class GetAllActivitiesTest
	{
		public IApiHttpClient Client;
		public IActivityRepository ActivityRepository;

		[Test]
		public void ShouldGetAllActivities()
		{
			Client.Authorize();

			ActivityRepository.Add(new Activity("Activity1"));
			ActivityRepository.Add(new Activity("Activity2"));

			var result = Client.PostAsync("/query/Activity/AllActivities", new StringContent("{}"));
			var activitesObj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"];
			activitesObj.Count().Should().Be.EqualTo(2);
		}
	}
}
