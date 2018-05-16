using System.Net.Http;
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
	public class UserByEmailTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldGetUserByEmail()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			person.Email = "a@b.c";
			PersonRepository.Add(person);

			var result = Client.PostAsync("/query/User/UserByEmail", new StringContent(JsonConvert.SerializeObject(new {Email = "a@b.c"})));
			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			obj["Id"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["FirstName"].Value<string>().Should().Be.EqualTo(person.Name.FirstName);
			obj["LastName"].Value<string>().Should().Be.EqualTo(person.Name.LastName);
			obj["Email"].Value<string>().Should().Be.EqualTo(person.Email);
		}
	}
}
