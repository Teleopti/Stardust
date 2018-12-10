#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the SchedulePeriodStartDateComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 25-07-2008
	/// </remarks>
	[TestFixture]
	public class SchedulePeriodUnitComparerTest
	{
		#region Variables

		private SchedulePeriodModel _target, _schedulePeriodModel;
		private SchedulePeriodUnitComparer schedulePeriodUnitComparer;
		private int result;
        private SchedulePeriodComparerTestHelper helper = new SchedulePeriodComparerTestHelper();

        private DateOnly dateTime1;

		#endregion

		#region SetUp

		[SetUp]
		public void Setup()
		{
            dateTime1 = new DateOnly(2009, 1, 3);

            helper.SetFirstTarget();
            helper.SetSecondtarget();
		}

		#endregion

		#region Test

		/// <summary>
		/// Verifies the compare method with null values for all parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 25-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithAllNull()
		{
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 25-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithFirstNull()
		{
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod1);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod1);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with null value for the second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 25-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondNull()
		{
//            helper.person.AddPersonPeriod(helper._personPeriod1);
            helper.person.AddSchedulePeriod(helper._schedulePeriod1);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 23-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodAscending()
		{
//            helper.person.AddPersonPeriod(helper._personPeriod1);
		    helper.person.AddSchedulePeriod(
                new SchedulePeriod(dateTime1,
		                           SchedulePeriodType.Day, 1));
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod5);
		    helper.person1.AddSchedulePeriod(
                new SchedulePeriod(dateTime1,
		                           SchedulePeriodType.Month, 1));
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for teh second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 25-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodDescending()
		{
//            helper.person.AddPersonPeriod(helper._personPeriod2);
            helper.person.AddSchedulePeriod(new SchedulePeriod(dateTime1, SchedulePeriodType.Week, 1));
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod5);
            helper.person1.AddSchedulePeriod(new SchedulePeriod(dateTime1, SchedulePeriodType.Month, 1));
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

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
//            helper.person.AddPersonPeriod(helper._personPeriod1);
            helper.person.AddSchedulePeriod(helper._schedulePeriod1);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod4);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod4);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

			// Calls the compares method
			schedulePeriodUnitComparer = new SchedulePeriodUnitComparer();
			result = schedulePeriodUnitComparer.Compare(_target, _schedulePeriodModel);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		#endregion
	}
}
