using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PeopleAdminTimeZoneInfoComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 22-07-2008
	/// </remarks>
	[TestFixture]
	public class PeopleAdminTimeZoneInfoComparerTest
	{
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private PeopleAdminTimeZoneInfoComparer peopleAdminAppliactionTImeZoneInfoComparer;
		private int result;

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
			_target.TimeZoneInformation = TimeZoneInfo.Local;
			_personGeneralModel.TimeZoneInformation = TimeZoneInfo.Local;

			// Calls the compares method
			peopleAdminAppliactionTImeZoneInfoComparer = new PeopleAdminTimeZoneInfoComparer();
			result = peopleAdminAppliactionTImeZoneInfoComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
