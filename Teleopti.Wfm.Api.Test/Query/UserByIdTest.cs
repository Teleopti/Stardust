using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class UserByIdTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldGetUserById()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);

			var result = Client.PostAsync("/query/User/UserById", new StringContent(JsonConvert.SerializeObject(new { PersonId = person.Id}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			obj["Id"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["FirstName"].Value<string>().Should().Be.EqualTo(person.Name.FirstName);
			obj["LastName"].Value<string>().Should().Be.EqualTo(person.Name.LastName);
			obj["Email"].Value<string>().Should().Be.EqualTo(person.Email);
		}
	}
}
