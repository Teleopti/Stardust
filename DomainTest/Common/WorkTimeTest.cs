using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests WorkTime struct
    /// </summary>
    [TestFixture]
    public class WorkTimeTest
    {
        private TimeSpan _avgWorkTime;

        /// <summary>
        /// Set up the test class
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _avgWorkTime = TimeSpan.FromHours(8);
        }

        /// <summary>
        /// Verifies that average work time is set using create.
        /// </summary>
        [Test]
        public void VerifyPropertiesAreSetUsingCreate()
        {
            WorkTime workTime = new WorkTime(_avgWorkTime);
            Assert.AreEqual(_avgWorkTime, workTime.AvgWorkTimePerDay);
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
            WorkTime testWorkTime = new WorkTime(_avgWorkTime);

            Assert.IsTrue(testWorkTime.Equals(new WorkTime(_avgWorkTime)));
            Assert.IsFalse(
                testWorkTime.Equals(
                    new WorkTime(_avgWorkTime.Add(new TimeSpan(0, 30, 0)))));
            Assert.IsFalse(new WorkTime().Equals(null));
            Assert.AreEqual(testWorkTime, (object) testWorkTime);
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            WorkTime workTime1 = new WorkTime(_avgWorkTime);
            WorkTime workTime2 = new WorkTime();
            Assert.IsTrue(workTime1 == new WorkTime(_avgWorkTime));
            Assert.IsTrue(workTime1 != workTime2);
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            WorkTime workTime = new WorkTime(_avgWorkTime);
            IDictionary<WorkTime, int> dic = new Dictionary<WorkTime, int>();
            dic[workTime] = 5;
            Assert.AreEqual(5, dic[workTime]);
        }

        /// <summary>
        /// Verifies average work time limit works.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyWorkTimeLimitWorks()
        {
            WorkTime workTime = new WorkTime(TimeSpan.FromHours(25));
            Assert.AreEqual(workTime, workTime);
        }
    }
}