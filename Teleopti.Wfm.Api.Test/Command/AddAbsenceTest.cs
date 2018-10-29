using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	public class AddAbsenceTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;

		[Test]
		public void ShouldAddAbsenceLayer()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var addAbsenceDto = new {
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 2)
			};

			var result = Client.PostAsync("/command/AddAbsence",
				new StringContent(JsonConvert.SerializeObject(addAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldValidateStartAndEndDateTime()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var addAbsenceDto = new {
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 1)
			};

			var result = Client.PostAsync("/command/AddAbsence",
				new StringContent(JsonConvert.SerializeObject(addAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(false);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldTrackPersonalAccount()
		{
			Client.Authorize();
			var period = new DateTimePeriod(2018, 1, 1, 8, 2018, 1, 1, 17);
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, period));
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var account = AbsenceAccountFactory.CreateAbsenceAccountDays(person, absence, new DateOnly(2018, 1, 1),
				new TimeSpan(100, 0, 0, 0), new TimeSpan(10, 0, 0, 0), new TimeSpan(0), new TimeSpan(0), new TimeSpan(0),
				new TimeSpan(0)).WithId();
			var personAbsenceAccount =
				PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absence, account).WithId();
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
			var addAbsenceDto = new
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				UtcStartTime = period.StartDateTime,
				UtcEndTime = period.EndDateTime
			};

			var result = Client.PostAsync("/command/AddAbsence",
				new StringContent(JsonConvert.SerializeObject(addAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(1);
			account.LatestCalculatedBalance.Days.Should().Be.EqualTo(1);
		}
	}
}
