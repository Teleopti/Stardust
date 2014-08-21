using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests WorkTimeDirective struct
    /// </summary>
    [TestFixture]
    public class WorkTimeDirectiveTest
    {
        private TimeSpan _maxTimePerWeek;
        private TimeSpan _nightlyRest;
        private TimeSpan _weeklyRest;
	    private TimeSpan _minTimePerWeek;

	    /// <summary>
        /// Set up test class
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _maxTimePerWeek = TimeSpan.FromHours(48);
	        _minTimePerWeek = TimeSpan.FromHours(0);
            _nightlyRest = TimeSpan.FromHours(11);
            _weeklyRest = TimeSpan.FromHours(36);
        }

        /// <summary>
        /// Verifies that average work time is set using create.
        /// </summary>
        [Test]
        public void VerifyPropertiesAreSetUsingCreate()
        {
            WorkTimeDirective workTimeDirective = new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest);
            Assert.AreEqual(_maxTimePerWeek, workTimeDirective.MaxTimePerWeek);
            Assert.AreEqual(_nightlyRest, workTimeDirective.NightlyRest);
            Assert.AreEqual(_weeklyRest, workTimeDirective.WeeklyRest);
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
			WorkTimeDirective testWorkTimeDirective = new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest);

            Assert.IsTrue(
				testWorkTimeDirective.Equals(new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest)));
            Assert.IsFalse(
                testWorkTimeDirective.Equals(
					new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek.Add(new TimeSpan(-1, 0, 0)), _nightlyRest, _weeklyRest)));
            Assert.IsFalse(new WorkTimeDirective().Equals(null));
            Assert.AreEqual(testWorkTimeDirective, testWorkTimeDirective);
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
			WorkTimeDirective workTimeDirective1 = new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest);
            WorkTimeDirective workTimeDirective2 = new WorkTimeDirective();
			Assert.IsTrue(workTimeDirective1 == new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest));
            Assert.IsTrue(workTimeDirective1 != workTimeDirective2);
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
			WorkTimeDirective workTimeDirective = new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, _weeklyRest);
            IDictionary<WorkTimeDirective, int> dic = new Dictionary<WorkTimeDirective, int>();
            dic[workTimeDirective] = 5;
            Assert.AreEqual(5, dic[workTimeDirective]);
        }

        /// <summary>
        /// Verifies MaxTimePerWeek limit works.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyMaxTimePerWeekLimitWorks()
        {
            WorkTimeDirective workTimeDirective =
				new WorkTimeDirective(_minTimePerWeek, new TimeSpan(7 * 24, 1, 0), _nightlyRest, _weeklyRest);
            Assert.AreEqual(workTimeDirective, workTimeDirective);
        }

        /// <summary>
        /// Verifies MinTimePerWeek limit works.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyMinTimePerWeekLimitWorks()
        {
            WorkTimeDirective workTimeDirective =
				new WorkTimeDirective(new TimeSpan(7 * 24, 1, 0), _maxTimePerWeek, _nightlyRest, _weeklyRest);
            Assert.AreEqual(workTimeDirective, workTimeDirective);
        }

        /// <summary>
        /// Verifies NightlyRest limit works.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyNightlyRestLimitWorks()
        {
            WorkTimeDirective workTimeDirective =
				new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, new TimeSpan(24, 1, 0), _weeklyRest);
            Assert.AreEqual(workTimeDirective, workTimeDirective);
        }

        /// <summary>
        /// Verifies WeeklyRest limit works.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyWeeklyRestLimitWorks()
        {
            WorkTimeDirective workTimeDirective =
				new WorkTimeDirective(_minTimePerWeek, _maxTimePerWeek, _nightlyRest, new TimeSpan(7 * 24, 1, 0));
            Assert.AreEqual(workTimeDirective, workTimeDirective);
        }
    }
}