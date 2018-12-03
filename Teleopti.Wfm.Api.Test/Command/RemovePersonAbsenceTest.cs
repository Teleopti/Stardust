using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class RemovePersonAbsenceTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		public async Task ShouldValidateThatEndDateIsGreatedThatStartDate()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 1, 2018, 1, 2), absence));

			var removePersonAbsenceDto = new {
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 2),
				UtcEndTime = new DateTime(2018, 1, 1),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(false);
		}

		[Test]
		public async Task ShouldRemoveAbsence()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var period = new DateTimePeriod(2018, 1, 1, 2018, 1, 2);
			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, period).WithId());
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period, absence).WithId());

			var removePersonAbsenceDto = new {
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018,1,1),
				UtcEndTime = new DateTime(2018,1,2),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public async Task ShouldSplitAbsenceIfBiggerThanPeriod()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			for (var i = 1; i < 10; i++)
			{
				PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018,1,i,2018,1,i+1)).WithId());
			}
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 1, 2018, 1, 10), absence).WithId());

			var removePersonAbsenceDto = new {
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 3),
				UtcEndTime = new DateTime(2018, 1, 4),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(2);
			PersonAbsenceRepository.LoadAll().Any(x => x.Period == new DateTimePeriod(2018,1,1,2018,1,3)).Should().Be.True();
			PersonAbsenceRepository.LoadAll().Any(x => x.Period == new DateTimePeriod(2018,1,4,2018,1,10)).Should().Be.True();
		}

		[Test]
		public async Task ShouldRemoveAbsenceWhenPeriodIsLargerThanAbsencePeriod()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, 4, 2018, 1, 5)).WithId());
			PersonAbsenceRepository.Has(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 4, 2018, 1, 5), absence).WithId());

			var removePersonAbsenceDto = new
			{
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 31),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}


		[Test]
		public async Task ShouldRemoveAllAbsencesWithingPeriodAndSplitTheOnesThatGoesOutside()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			for (var i = 1; i < 20; i++)
			{
				PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2018, 1, i, 2018, 1, i + 1)).WithId());
			}
			PersonAbsenceRepository.Has(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 4, 2018, 1, 5), absence).WithId());
			PersonAbsenceRepository.Has(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 8, 2018, 1, 12), absence).WithId());
			PersonAbsenceRepository.Has(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 10, 2018, 1, 11), absence).WithId());

			var removePersonAbsenceDto = new
			{
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 9),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(2);
			PersonAbsenceRepository.LoadAll().First(x => x.Period.StartDateTime == new DateTime(2018,1,9)).Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018,1,12));
		}

		[Test]
		public async Task ShouldTrackPersonalAccount()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var period = new DateTimePeriod(2018, 1, 1, 2018, 1, 2);
			var account = AbsenceAccountFactory.CreateAbsenceAccountDays(person, absence, new DateOnly(2018, 1, 1),
				new TimeSpan(100, 0, 0, 0), new TimeSpan(10, 0, 0, 0), new TimeSpan(0), new TimeSpan(0), new TimeSpan(0),
				new TimeSpan(5, 0, 0, 0)).WithId();
			PersonAbsenceAccountRepository.Add(PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absence, account).WithId());

			PersonAssignmentRepository.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, period));
			PersonAbsenceRepository.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period, absence).WithId());
				
			var removePersonAbsenceDto = new
			{
				PersonId = person.Id.GetValueOrDefault(),
				UtcStartTime = period.StartDateTime,
				UtcEndTime = period.EndDateTime,
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/RemovePersonAbsence",
				new StringContent(JsonConvert.SerializeObject(removePersonAbsenceDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
			account.LatestCalculatedBalance.Days.Should().Be.EqualTo(4);
		}
	}
}
