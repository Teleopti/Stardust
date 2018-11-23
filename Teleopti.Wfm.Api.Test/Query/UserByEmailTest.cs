using System;
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
	public class UserByEmailTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;

		[Test]
		public async Task ShouldGetUserByEmail()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePerson().WithId();
			person.Email = "a@b.c";
			PersonRepository.Add(person);

			var result = await Client.PostAsync("/query/User/UserByEmail", new StringContent(JsonConvert.SerializeObject(new {Email = "a@b.c"}), Encoding.UTF8, "application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];

			obj["Id"].ToObject<Guid>().Should().Be.EqualTo(person.Id.Value);
			obj["FirstName"].Value<string>().Should().Be.EqualTo(person.Name.FirstName);
			obj["LastName"].Value<string>().Should().Be.EqualTo(person.Name.LastName);
			obj["Email"].Value<string>().Should().Be.EqualTo(person.Email);
		}
	}
}
