using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
	public class PeopleAdminTerminalDateComparerTest
	{
		private PersonGeneralModel _target;
		private PersonGeneralModel _personGeneralModel;
		private PeopleAdminTerminalDateComparer peopleAdminTerminalDateComparer;
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
			// By deafult terminal date ara null therefore we ont need to set the terminal 
			// date explicitly

			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

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
			// Sets the language of the object
			_personGeneralModel.TerminalDate = DateOnly.Today;

			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

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
            _target.TerminalDate = DateOnly.Today;

			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

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
			// Sets the date erlier than the _personGeneralModel's terminal date
            _target.TerminalDate = DateOnly.Today;

			// Sets the date comes after the _target's terminal date
			_personGeneralModel.TerminalDate = DateOnly.Today.AddDays(1);

			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

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
			// Sets the date comes the _personGeneralModel's terminal date
            _target.TerminalDate = DateOnly.Today.AddDays(1);

			// Sets the date erlier than after the_target's terminal date
            _personGeneralModel.TerminalDate = DateOnly.Today;

			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

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
			// Sets the terminal date same as the _personGeneralModel's terminal date
            _target.TerminalDate = DateOnly.Today;

			// Sets the terminal date same as the_target's terminal date
            _personGeneralModel.TerminalDate = DateOnly.Today;


			// Calls the compares method
			peopleAdminTerminalDateComparer = new PeopleAdminTerminalDateComparer();
			result = peopleAdminTerminalDateComparer.Compare(_target, _personGeneralModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}
	}
}
