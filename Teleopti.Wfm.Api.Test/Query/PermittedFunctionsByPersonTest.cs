using System;
using System.Linq;
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
	public class PermittedFunctionsByPersonTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeApplicationRoleRepository ApplicationRoleRepository;

		[Test]
		public void ShouldGetPermissionByPerson()
		{
			Client.Authorize();

			var applicationRole = ApplicationRoleFactory.CreateRole("test", "test").WithId();
			var applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("test", ApplicationFunctionFactory.CreateApplicationFunction("testMain").WithId()).WithId();
			applicationRole.AddApplicationFunction(applicationFunction);
			ApplicationRoleRepository.Has(applicationRole);
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(person);

			var result = Client.PostAsync("/query/ApplicationFunction/PermissionByPerson",
				new StringContent(JsonConvert.SerializeObject(new {PersonId = person.Id}), Encoding.UTF8,
					"application/json"));
			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"].Single();

			obj["Id"].ToObject<Guid>().Should().Be.EqualTo(applicationFunction.Id.Value);
			obj["FunctionCode"].Value<string>().Should().Be.EqualTo(applicationFunction.FunctionCode);
			obj["FunctionPath"].Value<string>().Should().Be.EqualTo(applicationFunction.FunctionPath);
		}
	}
}