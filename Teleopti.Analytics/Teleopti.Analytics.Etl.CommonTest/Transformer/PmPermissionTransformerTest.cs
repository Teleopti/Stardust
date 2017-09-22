using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class PmPermissionTransformerTest
	{
		private PmPermissionTransformer _target;
		private IList<IPerson> _personCollection;
		private MockRepository _mocks;
		private IPmPermissionExtractor _permissionExtractor;
		private IUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_target = new PmPermissionTransformer();
			_personCollection = PersonFactory.CreatePersonGraphCollection();

			_mocks = new MockRepository();
			_permissionExtractor = _mocks.StrictMock<IPmPermissionExtractor>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
		}

		[Test]
		public void ShouldGetUserWithPermission()
		{
			IPerson person = _personCollection[4];
			
			var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];
			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
			_mocks.ReplayAll();

			var users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, _permissionExtractor, _unitOfWorkFactory);
			_mocks.VerifyAll();

			Assert.AreEqual(1, users.Count);
			users[0].AccessLevel.Should().Be.EqualTo(PmPermissionType.ReportDesigner);
		}


		[Test]
		public void ShouldNotGetUsersThatLacksPermissionToPm()
		{
			IPerson person = _personCollection[7]; // Does not belong to a ApplicationRole
			Assert.AreEqual(0, person.PermissionInformation.ApplicationRoleCollection.Count);

			var users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, _permissionExtractor, _unitOfWorkFactory);
			Assert.AreEqual(0, users.Count);
		}

		[Test]
		public void ShouldCheckThatUserHasViewPermission()
		{
			IPerson person = _personCollection[3];
			var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];

			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.GeneralUser);
			_mocks.ReplayAll();

			var userDtoCollection = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, _permissionExtractor, _unitOfWorkFactory);
			_mocks.VerifyAll();
			Assert.AreEqual(81, userDtoCollection[0].AccessLevel);
		}

		[Test]
		public void ShouldCheckThatUserHasCreatePermission()
		{
			IPerson person = _personCollection[4];
			var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];

			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
			_mocks.ReplayAll();

			var userDtoCollection = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, _permissionExtractor, _unitOfWorkFactory);
			_mocks.VerifyAll();

			Assert.AreEqual(85, userDtoCollection[0].AccessLevel);
		}

		[Test]
		public void VerifyTransform()
		{
			using (var table = new DataTable())
			{
				var userWithViewPermission = new PmUser { PersonId = Guid.NewGuid(), AccessLevel = 1 };
				var userWithCreatePermission = new PmUser { PersonId = Guid.NewGuid(), AccessLevel = 2 };

				var users = new List<PmUser> { userWithViewPermission, userWithCreatePermission };

				table.Locale = Thread.CurrentThread.CurrentCulture;
				PmUserInfrastructure.AddColumnsToDataTable(table);
				_target.Transform(users, table);

				Assert.IsNotNull(table);
				Assert.AreEqual(2, table.Rows.Count);
				Assert.AreEqual(userWithViewPermission.PersonId, table.Rows[0]["person_id"]);
				Assert.AreEqual(userWithCreatePermission.PersonId, table.Rows[1]["person_id"]);

			}
		}

		[Test]
		public void ShouldGetTheMostGenerousPermissionOutOfThreeRoles()
		{
			// Person has 3 roles with following PM permissions. 1st create, 2nd view, 3rd none 
			IPerson person = _personCollection[6];
			var applicationRole1 = person.PermissionInformation.ApplicationRoleCollection[0];
			var applicationRole2 = person.PermissionInformation.ApplicationRoleCollection[1];
			var applicationRole3 = person.PermissionInformation.ApplicationRoleCollection[2];

			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole1.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole2.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.GeneralUser);
			Expect.Call(_permissionExtractor.ExtractPermission(applicationRole3.ApplicationFunctionCollection, _unitOfWorkFactory)).Return(PmPermissionType.None);
			_mocks.ReplayAll();

			var users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, _permissionExtractor, _unitOfWorkFactory);
			_mocks.VerifyAll();

			Assert.AreEqual(1, users.Count);
			Assert.AreEqual(85, users[0].AccessLevel); // AccessLevel = 2 is Create
		}
	}
}
