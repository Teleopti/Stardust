using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PeopleAdminApplicationRoleComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 22-07-2008
	/// </remarks>
	[TestFixture]
	public class PeopleAdminApplicationRoleComparerTest
	{
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private PeopleAdminApplicationRoleComparer peopleAdminAppliactionRoleComparer;
		private int result;
	    private IApplicationRole roleA;
	    private IApplicationRole roleB;

	    /// <summary>
		/// Tests the init.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[SetUp]
		public void TestInit()
		{
			// Instantiates the entities
            _target = PeopleAdminGridDataFactory.GetPeopleAdminGridData("testContract", "TestSchedule", "TestPartTimePercentage");
            _personGeneralModel = PeopleAdminGridDataFactory.GetPeopleAdminGridData("testContract1", "TestSchedule1", "TestPartTimePercentage1");

            SetAvailableRoles();
		}

	    private void SetAvailableRoles()
	    {
	        IList<IApplicationRole> roles = new List<IApplicationRole>();
	        roleA = new ApplicationRole();
	        roleA.DescriptionText = "Role A";
	        roles.Add(roleA);

	        roleB = new ApplicationRole();
	        roleB.DescriptionText = "Role B";
	        roles.Add(roleB);

	        _target.SetAvailableRoles(roles);
	        _personGeneralModel.SetAvailableRoles(roles);
	    }

	    /// <summary>
		/// Verifies the compare method with null values for all parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithAllNull()
		{
			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithFirstNull()
		{
			_personGeneralModel.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondNull()
		{
		    _target.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodAscending()
		{
            _target.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);
            _personGeneralModel.ContainedEntity.PermissionInformation.AddApplicationRole(roleB);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for teh second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodDescending()
		{
			_target.ContainedEntity.PermissionInformation.AddApplicationRole(roleB);
            _personGeneralModel.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with same role for both parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
            _target.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);
		    _personGeneralModel.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		/// <summary>
		/// Verifies the compare method with same role for both parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 22-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithRoleDescriptionAppend()
		{
            _target.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);
            _target.ContainedEntity.PermissionInformation.AddApplicationRole(roleB);

			_personGeneralModel.ContainedEntity.PermissionInformation.AddApplicationRole(roleA);

			// Calls the compares method
			peopleAdminAppliactionRoleComparer = new PeopleAdminApplicationRoleComparer();
			result = peopleAdminAppliactionRoleComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}
	}
}
