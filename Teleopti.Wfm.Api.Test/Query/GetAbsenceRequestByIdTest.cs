using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[FullPermissions]
	public class GetAbsenceRequestByIdTest
	{
		public IApiHttpClient Client;
		public FakeAbsenceRepository AbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FullPermission FullPermission;

		[Test]
		public void ShouldGetAbsenceRequestById()
		{
			Client.Authorize();

			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2018, 8, 1));
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateRequestableAbsence("Time in lieu", "TIL", Color.BlueViolet);
			AbsenceRepository.Has(absence);

			var personRequest = new PersonRequest(person,
				new AbsenceRequest(absence, new DateTimePeriod(2018, 8, 1, 13, 2018, 8, 1, 15))).WithId();
			PersonRequestRepository.Add(personRequest);

			var result = Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = personRequest.Id }), Encoding.UTF8, "application/json"));
			var request = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];
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
		public void ShouldNotGetAbsenceRequestByIdWhenNotPermitted()
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

			var result = Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = personRequest.Id }), Encoding.UTF8, "application/json"));
			var response = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			response["Successful"].Value<bool>().Should().Be.False();
		}

		[Test]
		public void ShouldHandleWhenAbsenceRequestByIdNotExists()
		{
			Client.Authorize();
			
			var result = Client.PostAsync("/query/AbsenceRequest/AbsenceRequestById", new StringContent(JsonConvert.SerializeObject(new { RequestId = Guid.NewGuid() }), Encoding.UTF8, "application/json"));
			var response = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			response["Successful"].Value<bool>().Should().Be.False();
		}
	}
}