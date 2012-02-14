using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Rhino.Mocks;
using Teleopti.Ccc.DomainTest.Helper;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for ContractScheduleWeek class.
    /// </summary>
    [TestFixture]
    public class ContractScheduleWeekTest
    {
        private ContractScheduleWeek testContractScheduleWeek;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            testContractScheduleWeek = new ContractScheduleWeek();
        }

        /// <summary>
        /// Verifies that default properties are set
        /// </summary>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.AreEqual(0, testContractScheduleWeek.WeekOrder);
            Assert.AreEqual(0, testContractScheduleWeek.Count);
        }

        /// <summary>
        /// Verifies that order can be set
        /// </summary>
        [Test]
        public void VerifyOrderCanBeSet()
        {
            testContractScheduleWeek.WeekOrder = 2;
            Assert.AreEqual(2, testContractScheduleWeek.WeekOrder);
        }

        /// <summary>
        /// Verifies workdays can be added to collection
        /// </summary>
        [Test]
        public void VerifyWorkdaysCanBeAdded()
        {
            testContractScheduleWeek.Add(DayOfWeek.Saturday, true);
            testContractScheduleWeek.Add(DayOfWeek.Thursday, true);
            testContractScheduleWeek.Add(DayOfWeek.Thursday, true);

            Assert.AreEqual(2, testContractScheduleWeek.Count);
        }

        /// <summary>
        /// Verifies workdays can be removed from collection
        /// </summary>
        [Test]
        public void VerifyWorkdaysCanBeRemoved()
        {
            testContractScheduleWeek.Add(DayOfWeek.Saturday, true);
            testContractScheduleWeek.Add(DayOfWeek.Thursday, true);

            Assert.AreEqual(2, testContractScheduleWeek.Count);

            testContractScheduleWeek.Remove(DayOfWeek.Thursday);
            testContractScheduleWeek.Remove(DayOfWeek.Friday);

            Assert.AreEqual(1, testContractScheduleWeek.Count);
        }

        /// <summary>
        /// Verifies IsWorkday method gives expected result
        /// </summary>
        [Test]
        public void VerifyIsWorkdayGiveExpectedResult()
        {
            testContractScheduleWeek.Add(DayOfWeek.Monday, true);
            testContractScheduleWeek.Add(DayOfWeek.Friday, true);

            Assert.AreEqual(2, testContractScheduleWeek.Count);
            Assert.IsFalse(testContractScheduleWeek.IsWorkday(DayOfWeek.Saturday));
            Assert.IsFalse(testContractScheduleWeek.IsWorkday(DayOfWeek.Thursday));
            Assert.IsTrue(testContractScheduleWeek.IsWorkday(DayOfWeek.Friday));
            Assert.IsTrue(testContractScheduleWeek.IsWorkday(DayOfWeek.Monday));
            Assert.IsTrue(testContractScheduleWeek[DayOfWeek.Friday]);
            Assert.IsTrue(testContractScheduleWeek[DayOfWeek.Monday]);
        }

    }
}