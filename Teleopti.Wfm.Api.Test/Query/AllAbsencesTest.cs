using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Wfm.Api.Query;

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

			var result = Client.PostAsync("/query/Absence/AllAbsences", new StringContent("{}"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result).ToObject<QueryResultDto<AbsenceDto>>();
			resultDto.Successful.Should().Be.EqualTo(true);
			resultDto.Result.Count().Should().Be.EqualTo(1);
			resultDto.Result.FirstOrDefault().Id.Should().Be.EqualTo(absence.Id);
		}
	}
}
