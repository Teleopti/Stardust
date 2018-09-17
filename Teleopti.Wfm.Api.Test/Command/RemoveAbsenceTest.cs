using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Api.Command;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	public class RemoveAbsenceTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeAbsenceRepository AbsenceRepository;

		[Test]
		public void ShouldValidateThatEndDateIsGreatedThatStartDate()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var personAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 1, 2018, 1, 2));
			PersonAbsenceRepository.Add(personAbsence);

			var removeAbsenceDto = new RemoveAbsenceDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartUtc = new DateTime(2018, 1, 2),
				PeriodEndUtc = new DateTime(2018, 1, 1),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = Client.PostAsync("/command/RemoveAbsence",
				new StringContent(JsonConvert.SerializeObject(removeAbsenceDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldRemoveAbsence()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var personAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 1, 2018, 1, 2));
			PersonAbsenceRepository.Add(personAbsence);

			var removeAbsenceDto = new RemoveAbsenceDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartUtc = new DateTime(2018,1,1),
				PeriodEndUtc = new DateTime(2018,1,2),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = Client.PostAsync("/command/RemoveAbsence",
				new StringContent(JsonConvert.SerializeObject(removeAbsenceDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShoulSplitAbsenceIfBiggerThanPeriod()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var absence = AbsenceFactory.CreateAbsence("absence").WithId();
			AbsenceRepository.Has(absence);
			var personAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2018, 1, 1, 2018, 1, 10));
			PersonAbsenceRepository.Add(personAbsence);

			var removeAbsenceDto = new RemoveAbsenceDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				PeriodStartUtc = new DateTime(2018, 1, 3),
				PeriodEndUtc = new DateTime(2018, 1, 4),
				ScenarioId = scenario.Id.GetValueOrDefault()
			};

			var result = Client.PostAsync("/command/RemoveAbsence",
				new StringContent(JsonConvert.SerializeObject(removeAbsenceDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(2);
			PersonAbsenceRepository.LoadAll().Any(x => x.Period == new DateTimePeriod(2018,1,1,2018,1,3)).Should().Be.True();
			PersonAbsenceRepository.LoadAll().Any(x => x.Period == new DateTimePeriod(2018,1,4,2018,1,10)).Should().Be.True();
			
		}
	}
}
