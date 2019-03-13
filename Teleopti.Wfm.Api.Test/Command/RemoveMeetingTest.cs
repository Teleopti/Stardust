using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	[LoggedOnAppDomain]
	[UseRealPersonRequestPermissionCheck]
	public class RemoveMeetingTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeActivityRepository ActivityRepository;
		
		[Test]
		public async Task ShouldRemoveMeeting()
		{
			var start = new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2018, 8, 2, 15, 0, 0, DateTimeKind.Utc);
			var person = new Person().WithId();
			PersonRepository.Has(person);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var dateOnly = new DateOnly(start);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnly.ToDateOnlyPeriod(), scenario);
			var range = scheduleDictionary[person];
			var day = range.ScheduledDay(dateOnly);
			var assignment = day.PersonAssignment(true);
			var activity = new Activity().WithId();
			ActivityRepository.Has(activity);
			assignment.AddMeeting(activity, new DateTimePeriod(start, end), Guid.NewGuid());
			ScheduleStorage.Add(assignment);
			
			Client.Authorize();
			var result = await Client.PostAsync("/command/RemoveMeeting", new StringContent(
				JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.Value,
					UtcStartTime = start,
					UtcEndTime = end
				}), Encoding.UTF8, "application/json"));
			
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnly.ToDateOnlyPeriod(), scenario)[person]
				.ScheduledDay(dateOnly).PersonAssignment().Meetings().Should().Be.Empty();
		}
	}
}