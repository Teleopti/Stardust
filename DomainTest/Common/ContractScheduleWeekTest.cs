using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

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

            Assert.AreEqual(true, testContractScheduleWeek[DayOfWeek.Saturday]);
			Assert.AreEqual(true, testContractScheduleWeek[DayOfWeek.Thursday]);
		}


        /// <summary>
        /// Verifies IsWorkday method gives expected result
        /// </summary>
        [Test]
        public void VerifyIsWorkdayGiveExpectedResult()
        {
            testContractScheduleWeek.Add(DayOfWeek.Monday, true);
            testContractScheduleWeek.Add(DayOfWeek.Friday, true);

            Assert.IsFalse(testContractScheduleWeek.IsWorkday(DayOfWeek.Saturday));
            Assert.IsFalse(testContractScheduleWeek.IsWorkday(DayOfWeek.Thursday));
            Assert.IsTrue(testContractScheduleWeek.IsWorkday(DayOfWeek.Friday));
            Assert.IsTrue(testContractScheduleWeek.IsWorkday(DayOfWeek.Monday));
            Assert.IsTrue(testContractScheduleWeek[DayOfWeek.Friday]);
            Assert.IsTrue(testContractScheduleWeek[DayOfWeek.Monday]);
        }

    }
}