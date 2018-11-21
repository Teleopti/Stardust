using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class UserByIdTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;

		[Test]
		public async Task ShouldGetUserById()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);

			var result = await Client.PostAsync("/query/User/UserById", new StringContent(JsonConvert.SerializeObject(new { PersonId = person.Id}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];

			obj["Id"].Value<string>().Should().Be.EqualTo(person.Id.Value.ToString());
			obj["FirstName"].Value<string>().Should().Be.EqualTo(person.Name.FirstName);
			obj["LastName"].Value<string>().Should().Be.EqualTo(person.Name.LastName);
			obj["Email"].Value<string>().Should().Be.EqualTo(person.Email);
		}
	}
}
