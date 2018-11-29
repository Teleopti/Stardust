using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class GetAbsenceRequestByIdTest
	{
		public IApiHttpClient Client;
		public FakeAbsenceRepository AbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FullPermission FullPermission;

		[Test]
		public async Task ShouldGetAbsenceRequestById()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2018, 8, 1));
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateRequestableAbsence("Time in lieu", "TIL", Color.BlueViolet);
			AbsenceRepository.Has(absence);

			var personRequest = new PersonRequest(person,
				new AbsenceRequest(absence, new DateTimePeriod(2018, 8, 1, 13, 2018, 8, 1, 15))).WithId();
			PersonRequestRepository.Add(personRequest);

			var result = await Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = personRequest.Id }), Encoding.UTF8, "application/json"));
			var request = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Result"][0];
			request["Id"].ToObject<Guid>().Should().Be.EqualTo(personRequest.Id);
			request["IsNew"].Value<bool>().Should().Be.EqualTo(true);
			request["IsPending"].Value<bool>().Should().Be.EqualTo(false);
			request["IsAlreadyAbsent"].Value<bool>().Should().Be.EqualTo(false);
			request["IsApproved"].Value<bool>().Should().Be.EqualTo(false);
			request["IsAutoAproved"].Value<bool>().Should().Be.EqualTo(false);
			request["IsAutoDenied"].Value<bool>().Should().Be.EqualTo(false);
			request["IsCancelled"].Value<bool>().Should().Be.EqualTo(false);
			request["IsDeleted"].Value<bool>().Should().Be.EqualTo(false);
			request["IsDenied"].Value<bool>().Should().Be.EqualTo(false);
			request["IsExpired"].Value<bool>().Should().Be.EqualTo(false);
		}

		[Test]
		public async Task ShouldNotGetAbsenceRequestByIdWhenNotPermitted()
		{
			FullPermission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			Client.Authorize();

			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2018, 8, 1));
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateRequestableAbsence("Time in lieu", "TIL", Color.BlueViolet);
			AbsenceRepository.Has(absence);

			var personRequest = new PersonRequest(person,
				new AbsenceRequest(absence, new DateTimePeriod(2018, 8, 1, 13, 2018, 8, 1, 15))).WithId();
			PersonRequestRepository.Add(personRequest);

			var result = await Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = personRequest.Id }), Encoding.UTF8, "application/json"));
			var response = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			response["Successful"].Value<bool>().Should().Be.False();
		}

		[Test]
		public async Task ShouldHandleWhenAbsenceRequestByIdNotExists()
		{
			Client.Authorize();
			
			var result = await Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = Guid.NewGuid() }), Encoding.UTF8, "application/json"));
			var response = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			response["Successful"].Value<bool>().Should().Be.False();
		}
	}
}