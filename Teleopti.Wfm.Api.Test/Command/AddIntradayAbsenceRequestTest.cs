using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	public class AddIntradayAbsenceRequestTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonRequestRepository PersonRequestRepository;

		[Test]
		public void ShouldAddIntradayAbsenceRequest()
		{
			Client.Authorize();

			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var person = new Person().WithId();
			PersonRepository.Add(person);

			var result = Client.PostAsync("/command/AddIntradayAbsenceRequest", new StringContent(
				JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.Value,
					AbsenceId = absence.Id.Value,
					Subject = "from the bot",
					Message = "",
					UtcStartTime = new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc),
					UtcEndTime = new DateTime(2018, 8, 2, 15, 0, 0, DateTimeKind.Utc)
				}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Id"].ToObject<Guid>().Should().Be.EqualTo(PersonRequestRepository.LoadAll().Single().Id);
		}
	}
}