using System.Net.Http;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class ScheduleByPersonIdTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		public void ShouldGetBasicScheduleInformation()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);
			var scenario = ScenarioRepository.Has("Default");
			PersonAssignmentRepository.Add(new PersonAssignment(person,scenario,new DateOnly(2001,1,1)));

			var result = Client.PostAsync("/query/User/UserByEmail", new StringContent(JsonConvert.SerializeObject(new { Email = "a@b.c" })));
			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			obj["Id"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["FirstName"].Value<string>().Should().Be.EqualTo(person.Name.FirstName);
			obj["LastName"].Value<string>().Should().Be.EqualTo(person.Name.LastName);
			obj["Email"].Value<string>().Should().Be.EqualTo(person.Email);
		}
	}
}
