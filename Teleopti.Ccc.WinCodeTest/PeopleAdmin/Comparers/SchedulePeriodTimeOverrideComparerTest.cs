using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
	/// <summary>
	/// Test class for the SchedulePeriodTargetComparer class of the wincode.
	/// </summary>
	/// <remarks>
	/// Created By: tamasb
	/// Created Date: 15-06-2012
	/// </remarks>
	[TestFixture]
	public class SchedulePeriodTimeOverrideComparerTest
	{
		#region Variables

		private SchedulePeriodModel _schedulePeriodModel1, _schedulePeriodModel2;
		private SchedulePeriodTimeOverrideComparer _comparer;
		private int result;
		private SchedulePeriodComparerTestHelper helper = new SchedulePeriodComparerTestHelper();

		#endregion

		#region SetUp

		[SetUp]
		public void Setup()
		{
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
		/// Created Date: 30-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithAllNull()
		{
			_schedulePeriodModel1 = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, null);
			_schedulePeriodModel2 = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person1, null);

			// Calls the compares method
			_comparer = new SchedulePeriodTimeOverrideComparer();
			result = _comparer.Compare(_schedulePeriodModel1, _schedulePeriodModel2);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		/// <summary>
		/// Verifies the compare method with a for the first parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 30-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodAscending()
		{
			((SchedulePeriod)helper._schedulePeriod1).ResetAverageWorkTimePerDay();
			helper.person.AddSchedulePeriod(helper._schedulePeriod1);
			_schedulePeriodModel1 = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, null);
			_schedulePeriodModel1.PeriodTime = new TimeSpan(0, 1, 0);

			helper.person1.AddSchedulePeriod(helper._schedulePeriod5);

			_schedulePeriodModel2 = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);
			_schedulePeriodModel2.PeriodTime = new TimeSpan(0, 2, 0);

			// Calls the compares method
			_comparer = new SchedulePeriodTimeOverrideComparer();
			result = _comparer.Compare(_schedulePeriodModel1, _schedulePeriodModel2);

			// Checks whether the roles are equal
			Assert.AreEqual(-1, result);
		}

		/// <summary>
		/// Verifies the compare method with a for teh second parameter.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 30-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodDescending()
		{
			helper.person.AddSchedulePeriod(helper._schedulePeriod2);
			_schedulePeriodModel1 = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);
			_schedulePeriodModel1.PeriodTime = new TimeSpan(0, 2, 0);

			helper.person1.AddSchedulePeriod(helper._schedulePeriod4);
			_schedulePeriodModel2 = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);
			_schedulePeriodModel2.PeriodTime = new TimeSpan(0, 1, 0);

			// Calls the compares method
			_comparer = new SchedulePeriodTimeOverrideComparer();
			result = _comparer.Compare(_schedulePeriodModel1, _schedulePeriodModel2);

			// Checks whether the roles are equal
			Assert.AreEqual(1, result);
		}

		/// <summary>
		/// Verifies the compare method with same role for both parameters.
		/// </summary>
		/// <remarks>
		/// Created By: madhurangap
		/// Created Date: 30-07-2008
		/// </remarks>
		[Test]
		public void VerifyCompareMethodWithSecondWithSame()
		{
			helper.person.AddSchedulePeriod(helper._schedulePeriod1);
			_schedulePeriodModel1 = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);
			_schedulePeriodModel1.DaysOff = 1;

			helper.person1.AddSchedulePeriod(helper._schedulePeriod4);
			_schedulePeriodModel2 = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);
			_schedulePeriodModel2.DaysOff = 1;

			// Calls the compares method
			_comparer = new SchedulePeriodTimeOverrideComparer();
			result = _comparer.Compare(_schedulePeriodModel1, _schedulePeriodModel2);

			// Checks whether the roles are equal
			Assert.AreEqual(0, result);
		}

		#endregion
	}
}
