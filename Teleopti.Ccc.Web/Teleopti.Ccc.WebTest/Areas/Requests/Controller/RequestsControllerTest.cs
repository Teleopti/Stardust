using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Controller;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Controller
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class RequestsControllerTest: IIsolateSystem
	{
		public RequestsController Target;
		public FakeToggleManager ToggleManager;
		public FakePermissions Permissions;
		public ICurrentScenario CurrentScenario;
		public FakeDatabase Database;
		public FakePersonRepository PersonRepository;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("test") { DefaultScenario = true })).For<IScenarioRepository>();
		}

		[Test]
		public void ShouldGetRequests()
		{
			var input = setupData();

			var result = Target.GetRequests(input);
			result.Requests.Count().Should().Be.EqualTo(1);
		}

		private AllRequestsFormData setupData()
		{
			var scenarioId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			CurrentScenario.Current().WithId(scenarioId);
			Database.WithPerson(personId)
				.WithTeam(teamId, "myTeam")
				.WithPeriod(DateOnly.MinValue.ToString())
				.WithAbsenceRequest(personId, new DateTime(2018, 11, 21).ToString());
			var person = PersonRepository.Get(personId);
			PersonFinderReadOnlyRepository.Has(person);
			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = personId });
			setPermissions(person, DefinedRaptorApplicationFunctionPaths.WebRequests);

			return new AllRequestsFormData
			{
				StartDate = new DateOnly(2018, 11, 20),
				EndDate = new DateOnly(2018, 11, 30),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { teamId.ToString() }
			};
		}

		private void setPermissions(IPerson person, params string[] functionPaths)
		{
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebRequests);
			var role = ApplicationRoleFactory.CreateRole("test", "test");
			role.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			person.PermissionInformation.AddApplicationRole(role);

			var factory = new DefinedRaptorApplicationFunctionFactory();
			foreach (var functionPath in functionPaths)
			{
				role.AddApplicationFunction(ApplicationFunction.FindByPath(factory.ApplicationFunctions, functionPath));
			}
		}

		[Test]
		public void ShouldHasPermissionsWhenToggleOff()
		{
			ToggleManager.Disable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();
			
			result.Content.HasApproveOrDenyPermission.Should().Be.True();
			result.Content.HasCancelPermission.Should().Be.True();
			result.Content.HasEditSiteOpenHoursPermission.Should().Be.True();
			result.Content.HasReplyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldHasApproveOrDenyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebApproveOrDenyRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasApproveOrDenyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasApproveOrDenyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasApproveOrDenyPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasCancelPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebCancelRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasCancelPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasCancelPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasCancelPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasReplyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebReplyRequest);

			var result = Target.GetRequestsPermissions();

			result.Content.HasReplyPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasReplyPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasReplyPermission.Should().Be.False();
		}

		[Test]
		public void ShouldHasEditSiteOpenHoursPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.WebEditSiteOpenHours);

			var result = Target.GetRequestsPermissions();

			result.Content.HasEditSiteOpenHoursPermission.Should().Be.True();
		}

		[Test]
		public void ShouldNotHasEditSiteOpenHoursPermission()
		{
			ToggleManager.Enable(Toggles.WFM_Request_View_Permissions_77731);

			var result = Target.GetRequestsPermissions();

			result.Content.HasEditSiteOpenHoursPermission.Should().Be.False();
		}
	}
}
