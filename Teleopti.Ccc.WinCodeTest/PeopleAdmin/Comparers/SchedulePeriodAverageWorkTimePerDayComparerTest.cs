﻿#region Imports

using System;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCodeTest.FakeData;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Test class for the SchedulePeriodAverageWorkTimePerDayComparer class 
    /// of the wincode.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 30-07-2008
    /// </remarks>
    [TestFixture]
    public class SchedulePeriodAverageWorkTimePerDayComparerTest
    {
        #region Variables

        private SchedulePeriodModel _target, _schedulePeriodModel;
        private SchedulePeriodAverageWorkTimePerDayComparer schedulePeriodAverageWorkTimePerDayComparerTest;
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
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, null);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the first parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 30-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod5);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod5);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime1.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with null value for the second parameter.
        /// </summary>
        /// <remarks>
        /// Created By: madhurangap
        /// Created Date: 30-07-2008
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
//            helper.person.AddPersonPeriod(helper._personPeriod5);
            helper.person.AddSchedulePeriod(helper._schedulePeriod5);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime1.Date), helper.person, null);

            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
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
//            helper.person.AddPersonPeriod(helper._personPeriod1);
			helper._schedulePeriod1.AverageWorkTimePerDayOverride = new TimeSpan(1, 1, 1);
            helper.person.AddSchedulePeriod(helper._schedulePeriod1);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime3.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod5);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod5);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

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
//            helper.person.AddPersonPeriod(helper._personPeriod2);
            helper.person.AddSchedulePeriod(helper._schedulePeriod2);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod4);
			helper._schedulePeriod4.AverageWorkTimePerDayOverride = new TimeSpan(1, 1, 1);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod4);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

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
//            helper.person.AddPersonPeriod(helper._personPeriod1);
            helper.person.AddSchedulePeriod(helper._schedulePeriod1);
            _target = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person, null);

//            helper.person1.AddPersonPeriod(helper._personPeriod4);
            helper.person1.AddSchedulePeriod(helper._schedulePeriod4);
            _schedulePeriodModel = new SchedulePeriodModel(new DateOnly(helper.universalTime2.Date), helper.person1, null);

            // Calls the compares method
            schedulePeriodAverageWorkTimePerDayComparerTest = new SchedulePeriodAverageWorkTimePerDayComparer();
            result = schedulePeriodAverageWorkTimePerDayComparerTest.Compare(_target, _schedulePeriodModel);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}
