using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.WebTest.Areas.People.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[WebPeopleTest]
	public class RoleControllerAuditBase
	{
		public RoleController Target;
		public FakeApplicationRoleRepository ApplicationRoleRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAccessRepository PersonAccessRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePermissions Permissions;

		private IPerson _person1;
		private IPerson _person2;
		private IApplicationRole _role1;
		private IApplicationRole _role2;

		[SetUp]
		public void SetUp()
		{
			_person1 = PersonFactory.CreatePerson(new Name("Person1F", "Person1L")).WithId();
			_person2 = PersonFactory.CreatePerson(new Name("Person2F", "Person2L")).WithId();
			_role1 = new ApplicationRole().WithId();
			_role2 = new ApplicationRole().WithId();
		}

		[Test]
		public void GrantRoles_ShouldMakeCorrectAuditLogEntry()
		{
			/// Setup
			PersonRepository.Add(_person1);
			ApplicationRoleRepository.Add(_role1);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			var inputModel = new GrantRolesInputModel()
			{
				Persons = new[] {_person1.Id.GetValueOrDefault()},
				Roles = new[] {_role1.Id.GetValueOrDefault()}
			};

			/// Test
			// Check Change, SingleGrant
			var result = Target.GrantRoles(inputModel);
			result.Should().Be.OfType<OkResult>();
			PersonAccessRepository.LoadAll().Count().Should().Be.EqualTo(1);

			var auditRow = PersonAccessRepository.LoadAll().First();
			auditRow.Action.Should().Be.EqualTo(PersonAuditActionType.SingleGrantRole.ToString());
			auditRow.ActionResult.Should().Be.EqualTo(PersonAuditActionResult.Change.ToString());
			auditRow.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			auditRow.ActionPerformedOnId.Should().Be.EqualTo(_person1.Id);

			// Check NoChange, SingleGrant
			var result2 = Target.GrantRoles(inputModel);
			result2.Should().Be.OfType<OkResult>();
			PersonAccessRepository.LoadAll().Count().Should().Be.EqualTo(2);

			auditRow = PersonAccessRepository.LoadAll().Last();
			auditRow.Action.Should().Be.EqualTo(PersonAuditActionType.SingleGrantRole.ToString());
			auditRow.ActionResult.Should().Be.EqualTo(PersonAuditActionResult.NoChange.ToString());
			auditRow.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			auditRow.ActionPerformedOnId.Should().Be.EqualTo(_person1.Id);
		}

		[Test]
		public void GrantRoles_ShouldMakeNotPermittedAuditLogEntry()
		{
			/// Setup
			PersonRepository.Add(_person1);
			ApplicationRoleRepository.Add(_role1);

			var inputModel = new GrantRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault() }
			};

			/// Test
			// Check Change, SingleGrant
			var result = Target.GrantRoles(inputModel);
			result.Should().Be.OfType<OkResult>();
			PersonAccessRepository.LoadAll().Count().Should().Be.EqualTo(1);

			var auditRow = PersonAccessRepository.LoadAll().First();
			auditRow.Action.Should().Be.EqualTo(PersonAuditActionType.SingleGrantRole.ToString());
			auditRow.ActionResult.Should().Be.EqualTo(PersonAuditActionResult.NotPermitted.ToString());
			auditRow.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			auditRow.ActionPerformedOnId.Should().Be.EqualTo(_person1.Id);
		}

		[Test]
		public void GrantMultipleRolesNewAndPresent_ShouldMakeAuditLogChangeAndNoChangeEntries()
		{
			/// Setup
			PersonRepository.Add(_person1);
			PersonRepository.Add(_person2);
			ApplicationRoleRepository.Add(_role1);
			ApplicationRoleRepository.Add(_role2);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			/// Test
			// Check Change, MultiGrant
			var inputModel = new GrantRolesInputModel()
			{
				Persons = new[] {_person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault()},
				Roles = new[] {_role1.Id.GetValueOrDefault()}
			};

			var result = Target.GrantRoles(inputModel);
			result.Should().Be.OfType<OkResult>();

			var auditRows = PersonAccessRepository.LoadAll();
			auditRows.Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.Action == PersonAuditActionType.MultiGrantRole.ToString()).Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.ActionResult == PersonAuditActionResult.Change.ToString()).Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.ActionResult == PersonAuditActionResult.NoChange.ToString()).Count().Should().Be.EqualTo(0);

			var p1AuditRow1 = auditRows.Single(x => x.ActionPerformedOnId == _person1.Id);
			var p2AuditRow1 = auditRows.Single(x => x.ActionPerformedOnId == _person2.Id);
			p1AuditRow1.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			p2AuditRow1.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);

			// Check NoChange, MultiGrant
			var inputModel2 = new GrantRolesInputModel()
			{
				Persons = new[] {_person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault()},
				Roles = new[] {_role1.Id.GetValueOrDefault(), _role2.Id.GetValueOrDefault()}
			};

			var result2 = Target.GrantRoles(inputModel2);
			result.Should().Be.OfType<OkResult>();

			var auditRows2 = PersonAccessRepository.LoadAll();
			auditRows2.Count().Should().Be.EqualTo(6);
			auditRows2.Where(x => x.Action == PersonAuditActionType.MultiGrantRole.ToString()).Count().Should().Be.EqualTo(6);
			auditRows2.Where(x => x.ActionResult == PersonAuditActionResult.Change.ToString()).Count().Should().Be.EqualTo(4);
			auditRows2.Where(x => x.ActionResult == PersonAuditActionResult.NoChange.ToString()).Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void RevokeRole_ShouldMakeCorrectAuditLogEntry()
		{
			/// Setup
			PersonRepository.Add(_person1);
			ApplicationRoleRepository.Add(_role1);
			_person1.PermissionInformation.AddApplicationRole(_role1);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			var inputModel = new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault() }
			};

			/// Test
			// Check Change, SingleRevoke
			var result = Target.RevokeRoles(inputModel);
			result.Should().Be.OfType<OkResult>();
			PersonAccessRepository.LoadAll().Count().Should().Be.EqualTo(1);

			var auditRow = PersonAccessRepository.LoadAll().First();
			auditRow.Action.Should().Be.EqualTo(PersonAuditActionType.SingleRevokeRole.ToString());
			auditRow.ActionResult.Should().Be.EqualTo(PersonAuditActionResult.Change.ToString());
			auditRow.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			auditRow.ActionPerformedOnId.Should().Be.EqualTo(_person1.Id);

			// Check NoChange, SingleRevoke
			var result2 = Target.RevokeRoles(inputModel);
			result2.Should().Be.OfType<OkResult>();
			PersonAccessRepository.LoadAll().Count().Should().Be.EqualTo(2);

			auditRow = PersonAccessRepository.LoadAll().Last();
			auditRow.Action.Should().Be.EqualTo(PersonAuditActionType.SingleRevokeRole.ToString());
			auditRow.ActionResult.Should().Be.EqualTo(PersonAuditActionResult.NoChange.ToString());
			auditRow.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			auditRow.ActionPerformedOnId.Should().Be.EqualTo(_person1.Id);
		}

		[Test]
		public void RevokeMultipleRolesNewAndPresent_ShouldMakeAuditLogChangeAndNoChangeEntries()
		{
			/// Setup
			PersonRepository.Add(_person1);
			PersonRepository.Add(_person2);
			ApplicationRoleRepository.Add(_role1);
			ApplicationRoleRepository.Add(_role2);
			_person1.PermissionInformation.AddApplicationRole(_role1);
			_person1.PermissionInformation.AddApplicationRole(_role2);
			_person2.PermissionInformation.AddApplicationRole(_role1);
			_person2.PermissionInformation.AddApplicationRole(_role2);
			Permissions.HasPermission(DefinedRaptorApplicationFunctionPaths.PeopleAccess);

			/// Test
			// Check Change, MultiRevoke
			var inputModel = new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault() }
			};

			var result = Target.RevokeRoles(inputModel);
			result.Should().Be.OfType<OkResult>();

			var auditRows = PersonAccessRepository.LoadAll();
			auditRows.Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.Action == PersonAuditActionType.MultiRevokeRole.ToString()).Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.ActionResult == PersonAuditActionResult.Change.ToString()).Count().Should().Be.EqualTo(2);
			auditRows.Where(x => x.ActionResult == PersonAuditActionResult.NoChange.ToString()).Count().Should().Be.EqualTo(0);

			var p1AuditRow1 = auditRows.Single(x => x.ActionPerformedOnId == _person1.Id);
			var p2AuditRow1 = auditRows.Single(x => x.ActionPerformedOnId == _person2.Id);
			p1AuditRow1.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
			p2AuditRow1.ActionPerformedById.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);

			// Check NoChange, MultiGrant
			var inputModel2 = new RevokeRolesInputModel()
			{
				Persons = new[] { _person1.Id.GetValueOrDefault(), _person2.Id.GetValueOrDefault() },
				Roles = new[] { _role1.Id.GetValueOrDefault(), _role2.Id.GetValueOrDefault() }
			};

			var result2 = Target.RevokeRoles(inputModel2);
			result.Should().Be.OfType<OkResult>();

			var auditRows2 = PersonAccessRepository.LoadAll();
			auditRows2.Count().Should().Be.EqualTo(6);
			auditRows2.Where(x => x.Action == PersonAuditActionType.MultiRevokeRole.ToString()).Count().Should().Be.EqualTo(6);
			auditRows2.Where(x => x.ActionResult == PersonAuditActionResult.Change.ToString()).Count().Should().Be.EqualTo(4);
			auditRows2.Where(x => x.ActionResult == PersonAuditActionResult.NoChange.ToString()).Count().Should().Be.EqualTo(2);
		}
	}


	public class RoleControllerAuditToggleOffTest : RoleControllerAuditBase
	{

	}

	
	[Toggle(Toggles.Wfm_AuditTrail_GenericAuditTrail_74938)]
	public class RoleControllerAuditToggleOnTest : RoleControllerAuditBase
	{

	}
}

