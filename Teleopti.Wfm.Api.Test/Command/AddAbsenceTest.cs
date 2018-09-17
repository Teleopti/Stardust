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
using Teleopti.Wfm.Api.Command;

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
			var addAbsenceDto = new AddAbsenceDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 2)
			};

			var result = Client.PostAsync("/command/AddAbsence",
				new StringContent(JsonConvert.SerializeObject(addAbsenceDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);
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
			var addAbsenceDto = new AddAbsenceDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				UtcStartTime = new DateTime(2018, 1, 1),
				UtcEndTime = new DateTime(2018, 1, 1)
			};

			var result = Client.PostAsync("/command/AddAbsence",
				new StringContent(JsonConvert.SerializeObject(addAbsenceDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(false);
			PersonAbsenceRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}
	}
}
