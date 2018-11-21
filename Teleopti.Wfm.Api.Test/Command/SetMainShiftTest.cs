using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class SetMainShiftTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeShiftCategoryRepository ShiftCategoryRepository;

		[Test]
		public async Task ShouldSetMainShift()
		{
			Client.Authorize();
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("TestScenario", true, false).WithId());
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			
			var setMainShiftDto = new {
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new [] {
					new {
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = await Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public async Task ShouldSetNewMainshiftIfAlreadyExists()
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
			var setMainShiftDto = new {
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new []  {
					new {
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = await Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
			PersonAssignmentRepository.LoadAll().First().ShiftLayers.First().Payload.Should().Be.EqualTo(activity);
		}

		[Test]
		public async Task ShouldNotChangePersonalActivities()
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

			var setMainShiftDto = new {
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new [] {
					new {
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				}
			};

			var result = await Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);

			var shift = PersonAssignmentRepository.LoadAll().Single();
			shift.PersonalActivities().Should().Not.Be.Empty();
			shift.PersonalActivities().First().Payload.Should().Be.EqualTo(activityEmail);
			shift.MainActivities().First().Payload.Should().Be.EqualTo(activity);
		}

		[Test]
		public async Task ShouldKeepPersonAssignmentsForOtherDays()
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
			var setMainShiftDto = new {
				Date = new DateTime(2018, 1, 2),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new [] {
					new {
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 2, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 2, 15, 0, 0)
					}
				}
			};

			var result = await Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);

			var personAssignments = PersonAssignmentRepository.LoadAll().ToList();
			personAssignments.Count.Should().Be.EqualTo(3);
			personAssignments.First(x => x.Date == new DateOnly(2018, 1, 1)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activityEmail);
			personAssignments.First(x => x.Date == new DateOnly(2018, 1, 2)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activity);
			personAssignments.First(x => x.Date == new DateOnly(2018, 1, 3)).ShiftLayers.First().Payload
				.Should().Be.EqualTo(activityEmail);

		}

		[Test]
		public async Task ShouldSetShiftCategory()
		{
			Client.Authorize();
			ScenarioRepository.Has(ScenarioFactory.CreateScenario("TestScenario", true, false).WithId());
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var activity = ActivityRepository.Has("Phone");
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory").WithId();
			ShiftCategoryRepository.Add(shiftCategory);
			
			var setMainShiftDto = new {
				Date = new DateTime(2018, 1, 1),
				PersonId = person.Id.GetValueOrDefault(),
				LayerCollection = new [] {
					new {
						ActivityId = activity.Id.GetValueOrDefault(),
						UtcStartDateTime = new DateTime(2018, 1, 1, 8, 0, 0),
						UtcEndDateTime = new DateTime(2018, 1, 1, 15, 0, 0)
					}
				},
				ShiftCategory = shiftCategory.Id.GetValueOrDefault()
			};

			var result = await Client.PostAsync("/command/SetMainShift",
				new StringContent(JsonConvert.SerializeObject(setMainShiftDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);

			PersonAssignmentRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}
	}
}
