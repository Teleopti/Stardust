using System;
using System.Linq;
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
	public class AddMeetingTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FullPermission Permission;
		public IScheduleStorage ScheduleStorage;

		[Test]
		public async Task ShouldAddMeeting()
		{
			Client.Authorize();

			var activity = new Activity().WithId();
			ActivityRepository.Has(activity);
			var person = new Person().WithId();
			PersonRepository.Has(person);

			var start = new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2018, 8, 2, 15, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var dateOnly = new DateOnly(start);
			var meetingId = Guid.NewGuid();
			var result = await Client.PostAsync("/command/AddMeeting", new StringContent(
				JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.Value,
					ActivityId = activity.Id.Value,
					UtcStartTime = start,
					UtcEndTime = end,
					MeetingId = meetingId
				}), Encoding.UTF8, "application/json"));

			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), dateOnly.ToDateOnlyPeriod(), scenario);
			var range = scheduleDictionary[person];
			var day = range.ScheduledDay(dateOnly);
			var assignment = day.PersonAssignment();
			var meetingShiftLayer = assignment.Meetings().First();
			meetingShiftLayer.Period.StartDateTime.Should().Be.EqualTo(start);
			meetingShiftLayer.Period.EndDateTime.Should().Be.EqualTo(end);
			meetingShiftLayer.Meeting.Id.Should().Be.EqualTo(meetingId);
			meetingShiftLayer.Payload.Id.Value.Should().Be.EqualTo(activity.Id.Value);
		}
	}
}