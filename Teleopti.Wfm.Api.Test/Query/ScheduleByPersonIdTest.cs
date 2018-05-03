using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class ScheduleByPersonIdTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test, Ignore("Work in progress")]
		public void ShouldGetBasicScheduleInformation()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Phone");
			var personAssignment = new PersonAssignment(person,scenario,new DateOnly(2001,1,1)).WithId();
			personAssignment.AddActivity(activity,new TimePeriod(8,17));
			PersonAssignmentRepository.Add(personAssignment);

			var result = Client.PostAsync("/query/Schedule/ScheduleByPersonId",
				new StringContent(JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.GetValueOrDefault(),
					StartDate = new DateTime(2001, 1, 1),
					EndDate = new DateTime(2001, 1, 1)
				})));
			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			obj["PersonId"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["Date"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1));
			obj["Shift"][0]["PayloadId"].Value<string>().Should().Be.EqualTo(activity.Id.ToString());
			obj["Shift"][0]["Name"].Value<string>().Should().Be.EqualTo("Phone");
			obj["Shift"][0]["StartTime"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1,8,0,0));
			obj["Shift"][0]["EndTime"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1,17,0,0));
		}
	}
}
