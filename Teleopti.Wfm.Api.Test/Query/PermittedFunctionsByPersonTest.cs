using System;
using System.Linq;
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
	public class PermittedFunctionsByPersonTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeApplicationRoleRepository ApplicationRoleRepository;

		[Test]
		public async Task ShouldGetPermissionByPerson()
		{
			Client.Authorize();

			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test").WithId();
			var applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("test", ApplicationFunctionFactory.CreateApplicationFunction("testMain").WithId()).WithId();
			applicationRole.AddApplicationFunction(applicationFunction);
			ApplicationRoleRepository.Has(applicationRole);
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(person);

			var result = await Client.PostAsync("/query/ApplicationFunction/PermissionByPerson",
				new StringContent(JsonConvert.SerializeObject(new {PersonId = person.Id}), Encoding.UTF8,
					"application/json"));
			var obj = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"].Single();

			obj["Id"].ToObject<Guid>().Should().Be.EqualTo(applicationFunction.Id.Value);
			obj["FunctionCode"].Value<string>().Should().Be.EqualTo(applicationFunction.FunctionCode);
			obj["FunctionPath"].Value<string>().Should().Be.EqualTo(applicationFunction.FunctionPath);
		}
	}
}