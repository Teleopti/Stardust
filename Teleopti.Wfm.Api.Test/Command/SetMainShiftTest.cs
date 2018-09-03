using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Api.Command;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	public class SetMainShiftTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeShiftCategoryRepository ShiftCategoryRepository;

		[Test]
		public void ShouldSetMainShift()
		{
			Client.Authorize();
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("TestScenario", true, false).WithId());
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");


			var setMainShiftDto = new SetMainShiftDto
			{
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new List<ActivityLayerDto>
				{
					new ActivityLayerDto
					{
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSetNewMainshiftIfAlreadyExists()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			var activityEmail = ActivityRepository.Has("Email");

			PersonAssignmentRepository.Has(person, scenario, activityEmail, new DateOnlyPeriod(2018, 1, 1, 2018, 1, 1),
				new TimePeriod(10, 19));
			var setMainShiftDto = new SetMainShiftDto
			{
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new List<ActivityLayerDto>
				{
					new ActivityLayerDto
					{
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
			PersonAssignmentRepository.LoadAll().First().ShiftLayers.First().Payload.Should().Be.EqualTo(activity);
		}

		[Test]
		public void ShouldNotChangePersonalActivities()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			var activityEmail = ActivityRepository.Has("Email");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person, scenario,
				activityEmail, new DateTimePeriod(2018, 1, 1, 10, 2018, 1, 1, 19), new ShiftCategory("_")));

			var setMainShiftDto = new SetMainShiftDto
			{
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new List<ActivityLayerDto>
				{
					new ActivityLayerDto
					{
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
			PersonAssignmentRepository.LoadAll().First().ShiftLayers.Any(x => x.GetType() == typeof(PersonalShiftLayer)).Should().Be.True();
			PersonAssignmentRepository.LoadAll().First().ShiftLayers.Where(x => x.GetType() == typeof(PersonalShiftLayer)).First().Payload.Should().Be.EqualTo(activityEmail);
			PersonAssignmentRepository.LoadAll().First().ShiftLayers.Where(x => x.GetType() == typeof(MainShiftLayer)).First().Payload.Should().Be.EqualTo(activity);
		}

		[Test]
		public void ShouldKeepPersonAssignmentsForOtherDays()
		{
			Client.Authorize();
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false).WithId();
			ScenarioRepository.Has(scenario);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			var activityEmail = ActivityRepository.Has("Email");

			PersonAssignmentRepository.Has(person, scenario, activityEmail, new DateOnlyPeriod(2018, 1, 1, 2018, 1, 1),
				new TimePeriod(10, 19));
			PersonAssignmentRepository.Has(person, scenario, activityEmail, new DateOnlyPeriod(2018, 1, 2, 2018, 1, 2),
				new TimePeriod(10, 19));
			PersonAssignmentRepository.Has(person, scenario, activityEmail, new DateOnlyPeriod(2018, 1, 3, 2018, 1, 3),
				new TimePeriod(10, 19));
			var setMainShiftDto = new SetMainShiftDto
			{
				Date = new DateTime(2018, 1, 2),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new List<ActivityLayerDto>
				{
					new ActivityLayerDto
					{
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 2, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 2, 15, 0, 0)
					}
				}
			};

			var result = Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(3);
			PersonAssignmentRepository.LoadAll().First(x => x.Date == new DateOnly(2018, 1, 1)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activityEmail);
			PersonAssignmentRepository.LoadAll().First(x => x.Date == new DateOnly(2018, 1, 2)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activity);
			PersonAssignmentRepository.LoadAll().First(x => x.Date == new DateOnly(2018, 1, 3)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activityEmail);

		}

		[Test]
		public void ShouldSetShiftCategory()
		{
			Client.Authorize();
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("TestScenario", true, false).WithId());
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory").WithId();
			ShiftCategoryRepository.Add(shiftCategory);


			var setMainShiftDto = new SetMainShiftDto
			{
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new List<ActivityLayerDto>
				{
					new ActivityLayerDto
					{
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				},
				ShiftCategory = shiftCategory.Id.GetValueOrDefault()
			};

			var result = Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto)));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)
				.ToObject<ResultDto>();
			resultDto.Successful.Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}
	}
}
