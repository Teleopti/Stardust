using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the PeopleAdminApplicationLanguageComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 22-07-2008
	/// </remarks>
	[TestFixture]
	public class PeopleAdminLanguageComparerTest
	{
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private PeopleAdminLanguageComparer peopleAdminAppliactionLanguageComparer;
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
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

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
			// Sets the null language for the target
			_target.LanguageInfo = new Culture(0, "No Language");
			// Sets the language of the object
			Culture culture = new Culture(1034, "Test");
			_personGeneralModel.LanguageInfo = culture;

			// Calls the compares method
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

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
			// Sets the null language for the target
			_target.LanguageInfo = new Culture(1034, "Test");
			// Sets the culture of the object
			Culture culture = new Culture(0, "No Language");
			_personGeneralModel.LanguageInfo = culture;

			// Calls the compares method
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

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
			// Sets the null language for the target - language display name - Finnish
			_target.LanguageInfo = new Culture(1035, "Basque (Basque)");

			// Sets the culture of the object - language display name - spanish
			Culture culture = new Culture(1034, "Arabic (Saudi Arabia)");
			_personGeneralModel.LanguageInfo = culture;

			// Calls the compares method
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

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
			// Sets the null language for the target
			_target.LanguageInfo = new Culture(1034, "Basque (Basque)");

			// Sets the culture of the object
			Culture culture = new Culture(1035, "Arabic (Saudi Arabia)");
			_personGeneralModel.LanguageInfo = culture;

			// Calls the compares method
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

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
			// Sets the null language for the target
			_target.LanguageInfo = new Culture(1034, "Basque (Basque)");

			// Sets the culture of the object
			Culture culture = new Culture(1034, "Basque (Basque)");
			_personGeneralModel.LanguageInfo = culture;

			// Calls the compares method
			peopleAdminAppliactionLanguageComparer = new PeopleAdminLanguageComparer();
			result = peopleAdminAppliactionLanguageComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
