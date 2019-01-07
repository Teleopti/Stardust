using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	[LoggedOnAppDomain]
	[UseRealPersonRequestPermissionCheck]
	public class AddIntradayAbsenceRequestTest
	{
		public IApiHttpClient Client;
		public FakePersonRepository PersonRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FullPermission Permission;

		[Test]
		public async Task ShouldAddIntradayAbsenceRequest()
		{
			Client.Authorize();

			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var person = new Person().WithId();
			var role = ApplicationRoleFactory.CreateRole("Agent", "Agent");
			role.AvailableData = new AvailableData {AvailableDataRange = AvailableDataRangeOption.MyOwn};
			role.AddApplicationFunction(ApplicationFunction.FindByPath(
				new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb));
			person.PermissionInformation.AddApplicationRole(role);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Add(person);

			var result = await Client.PostAsync("/command/AddIntradayAbsenceRequest", new StringContent(
				JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.Value,
					AbsenceId = absence.Id.Value,
					Subject = "from the bot",
					Message = "",
					UtcStartTime = new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc),
					UtcEndTime = new DateTime(2018, 8, 2, 15, 0, 0, DateTimeKind.Utc)
				}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Id"].ToObject<Guid>().Should().Be.EqualTo(PersonRequestRepository.LoadAll().Single().Id);
		}
		
		[Test]
		public async Task ShouldAddIntradayAbsenceRequestWithDenyResult()
		{
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove);
			Client.Authorize();

			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var person = new Person().WithId();
			var role = ApplicationRoleFactory.CreateRole("Agent", "Agent");
			role.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.MyOwn };
			role.AddApplicationFunction(ApplicationFunction.FindByPath(
				new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb));
			person.PermissionInformation.AddApplicationRole(role);
			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence, AbsenceRequestProcess = new GrantAbsenceRequest(),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 1, 1, 2029, 12, 31),
				Period = new DateOnlyPeriod(2018, 1, 1, 2029, 12, 31),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			});
			person.WorkflowControlSet = workflowControlSet;
			LoggedOnUser.SetFakeLoggedOnUser(person);
			PersonRepository.Add(person);

			var result = await Client.PostAsync("/command/AddIntradayAbsenceRequest", new StringContent(
				JsonConvert.SerializeObject(new
				{
					PersonId = person.Id.Value,
					AbsenceId = absence.Id.Value,
					Subject = "from the bot",
					Message = "",
					UtcStartTime = new DateTime(2018, 8, 2, 13, 0, 0, DateTimeKind.Utc),
					UtcEndTime = new DateTime(2018, 8, 2, 15, 0, 0, DateTimeKind.Utc)
				}), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Id"].ToObject<Guid>().Should().Be.EqualTo(PersonRequestRepository.LoadAll().Single().Id);
		}
	}
}