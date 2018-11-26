using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class AbsenceRequestRulesByPersonIdTest
	{
		public IApiHttpClient Client;
		public IAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;
		public MutableNow Now;

		[Test]
		public async Task ShouldGetSingleRuleForAbsenceRequest()
		{
			Now.Is(new DateTime(2018,8,2,13,0,0,DateTimeKind.Utc));

			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			absence.Requestable = true;
			AbsenceRepository.Add(absence);
			var person = PersonFactory.CreatePersonWithSchedulePublishedToDate(new DateOnly(2018, 8, 31)).WithId();
			person.WorkflowControlSet.WithId().AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				BetweenDays = new MinMax<int>(0, 3),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018,1,1,2020,12,31),
				AbsenceRequestProcess = new ApproveAbsenceRequestWithValidators(), PersonAccountValidator = new AbsenceRequestNoneValidator(), 
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			});
			PersonRepository.Has(person);

			var result = await Client.PostAsync("/query/AbsenceRequestRule/AbsenceRequestRulesByPersonId",
				new StringContent(
					JsonConvert.SerializeObject(new
					{
						PersonId = person.Id, StartDate = new DateTime(2018, 8, 2), EndDate = new DateTime(2018, 8, 3)
					}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(1);
			resultDto["Result"].First()["AbsenceId"].ToObject<Guid>().Should().Be.EqualTo(absence.Id);
			resultDto["Result"].First()["Projection"].First()["StartDate"].ToObject<DateTime>().Should().Be.EqualTo(new DateTime(2018,8,2));
			resultDto["Result"].First()["Projection"].First()["EndDate"].ToObject<DateTime>().Should().Be.EqualTo(new DateTime(2018,8,5));
			resultDto["Result"].First()["Projection"].First()["RequestProcess"].Value<string>().Should().Be.EqualTo("ApproveAbsenceRequestWithValidators");
			resultDto["Result"].First()["Projection"].First()["PersonAccountValidator"].Value<string>().Should().Be.EqualTo("AbsenceRequestNoneValidator");
			resultDto["Result"].First()["Projection"].First()["StaffingThresholdValidator"].Value<string>().Should().Be.EqualTo("StaffingThresholdWithShrinkageValidator");
		}

		[Test]
		public async Task ShouldHandleNoPerson()
		{
			Now.Is(new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc));

			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			absence.Requestable = true;
			AbsenceRepository.Add(absence);
			
			var result = await Client.PostAsync("/query/AbsenceRequestRule/AbsenceRequestRulesByPersonId",
				new StringContent(
					JsonConvert.SerializeObject(new
					{
						PersonId = Guid.NewGuid(),
						StartDate = new DateTime(2018, 8, 2),
						EndDate = new DateTime(2018, 8, 3)
					}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(0);
		}

		[Test]
		public async Task ShouldHandleNoWorkflowControlSet()
		{
			Now.Is(new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc));

			Client.Authorize();
			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			absence.Requestable = true;
			AbsenceRepository.Add(absence);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var result = await Client.PostAsync("/query/AbsenceRequestRule/AbsenceRequestRulesByPersonId",
				new StringContent(
					JsonConvert.SerializeObject(new
					{
						PersonId = person.Id,
						StartDate = new DateTime(2018, 8, 2),
						EndDate = new DateTime(2018, 8, 3)
					}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo(0);
		}
	}
}