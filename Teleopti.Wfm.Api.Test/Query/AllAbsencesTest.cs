using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class AllAbsencesTest
	{
		public IApiHttpClient Client;
		public IAbsenceRepository AbsenceRepository;

		[Test]
		public void ShouldGetAllAbsences()
		{
			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			AbsenceRepository.Add(absence);

			var result = Client.PostAsync("/query/Absence/AllAbsences", new StringContent("{}", Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(1);
			resultDto["Result"].First()["Id"].ToObject<Guid>().Should().Be.EqualTo(absence.Id);
		}
	}
}
