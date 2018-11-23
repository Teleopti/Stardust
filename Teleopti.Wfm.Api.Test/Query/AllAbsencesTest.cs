using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class AllAbsencesTest
	{
		public IApiHttpClient Client;
		public IAbsenceRepository AbsenceRepository;

		[Test]
		public async Task ShouldGetAllAbsences()
		{
			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			AbsenceRepository.Add(absence);

			var result = await Client.PostAsync("/query/Absence/AllAbsences", new StringContent("{}", Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(1);
			resultDto["Result"].First()["Id"].ToObject<Guid>().Should().Be.EqualTo(absence.Id);
		}
	}
}
