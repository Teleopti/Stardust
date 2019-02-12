using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	[NoDefaultData]
	public class ScheduleByPersonIdTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;

		[Test]
		public async Task ShouldGetBasicScheduleInformation()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Phone").WithId();
			var personAssignment = new PersonAssignment(person,scenario,new DateOnly(2001,1,1)).WithId();
			personAssignment.AddActivity(activity,new TimePeriod(8,17));
			PersonAssignmentRepository.Add(personAssignment);

			var result = await Client.PostAsync("/query/Schedule/ScheduleByPersonId",
				new StringContent(JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.GetValueOrDefault(),
					StartDate = new DateTime(2001, 1, 1),
					EndDate = new DateTime(2001, 1, 1)
				}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];

			obj["PersonId"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["Date"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1));
			obj["Shift"][0]["PayloadId"].Value<string>().Should().Be.EqualTo(activity.Id.ToString());
			obj["Shift"][0]["Name"].Value<string>().Should().Be.EqualTo("Phone");
			obj["Shift"][0]["StartTime"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1,8,0,0));
			obj["Shift"][0]["EndTime"].Value<DateTime>().Should().Be.EqualTo(new DateTime(2001,1,1,17,0,0));
		}

		[Test]
		public async Task ShouldGetIndicationForAbsenceLayer()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Phone").WithId();
			var absence = AbsenceFactory.CreateAbsence("PTO").WithId();
			AbsenceRepository.Has(absence);
			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2001, 1, 1)).WithId();
			personAssignment.AddActivity(activity, new TimePeriod(8, 17));
			PersonAssignmentRepository.Add(personAssignment);

			var personAbsence = new PersonAbsence(person, scenario, new AbsenceLayer(absence,new DateTimePeriod(2001,1,1, 8,2001, 1, 1, 17))).WithId();
			PersonAbsenceRepository.Add(personAbsence);

			var result = await Client.PostAsync("/query/Schedule/ScheduleByPersonId",
				new StringContent(JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.GetValueOrDefault(),
					StartDate = new DateTime(2001, 1, 1),
					EndDate = new DateTime(2001, 1, 1)
				}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];
			
			obj["Shift"][0]["PayloadId"].Value<string>().Should().Be.EqualTo(absence.Id.ToString());
			obj["Shift"][0]["Name"].Value<string>().Should().Be.EqualTo("PTO");
			obj["Shift"][0]["IsAbsence"].Value<bool>().Should().Be.True();
		}
		
		[Test]
		public async Task ShouldGetAgentTimezoneInformation()
		{
			Client.Authorize();

			var timezone = TimeZoneInfoFactory.MountainTimeZoneInfo();
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(timezone);
			PersonRepository.Add(person);

			ScenarioRepository.Has("Default");

			var result = await Client.PostAsync("/query/Schedule/ScheduleByPersonId",
				new StringContent(JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.GetValueOrDefault(),
					StartDate = new DateTime(2001, 1, 1),
					EndDate = new DateTime(2001, 1, 1)
				}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];
			
			obj["TimeZoneId"].Value<string>().Should().Be.EqualTo(timezone.Id);
		}

		[Test]
		public async Task ShouldGetActivityDisplayColor()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Phone").WithId();
			activity.DisplayColor = Color.FromName("blue");
			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2001, 1, 1)).WithId();
			personAssignment.AddActivity(activity, new TimePeriod(8, 17));
			PersonAssignmentRepository.Add(personAssignment);

			var result = await Client.PostAsync("/query/Schedule/ScheduleByPersonId",
				new StringContent(JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.GetValueOrDefault(),
					StartDate = new DateTime(2001, 1, 1),
					EndDate = new DateTime(2001, 1, 1)
				}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];

			obj["Shift"][0]["DisplayColor"].Value<int>().Should().Be.EqualTo(Color.FromName("blue").ToArgb());
		}
	}
}
