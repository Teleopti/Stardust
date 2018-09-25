﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class PersonalAccountBalanceTest
	{
		public IApiHttpClient Client;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;

		[Test, Explicit("Working on this one...")]
		public void ShouldGetAccountBalance()
		{
			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			AbsenceRepository.Add(absence);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1)).WithId();
			PersonRepository.Has(person);

			var result = Client.PostAsync("/query/Absence/Account", new StringContent(JsonConvert.SerializeObject(new
			{
				PersonId = person.Id.Value,
				AbsenceId = absence.Id.Value,
				Date = new DateTime(2018,2,1)
			}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(1);
			resultDto["Result"].First()["Id"].ToObject<Guid>().Should().Be.EqualTo(absence.Id);
		}
	}
}