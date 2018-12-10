using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for ContractSchedule class.
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class ContractScheduleTest
    {
        private ContractSchedule testContractSchedule;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            testContractSchedule = new ContractSchedule("test");
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(testContractSchedule.IsDeleted);
        }


        /// <summary>
        /// Verifies that default properties are set
        /// </summary>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.AreEqual("test", testContractSchedule.Description.Name);
            Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, testContractSchedule.BusinessUnit);
        }

        /// <summary>
        /// Verifies that name can be set
        /// </summary>
        [Test]
        public void VerifyNameCanBeSet()
        {
            testContractSchedule.Description = new Description("new");
            Assert.AreEqual("new", testContractSchedule.Description.Name);
        }

        /// <summary>
        /// Verifies a new ContractScheduleWeek can be added
        /// </summary>
        [Test]
        public void VerifyContractScheduleWeekCanBeAdded()
        {
            ContractScheduleWeek contractScheduleWeek = new ContractScheduleWeek();
            contractScheduleWeek.Add(DayOfWeek.Saturday, false);
            contractScheduleWeek.Add(DayOfWeek.Sunday, false);

            testContractSchedule.AddContractScheduleWeek(contractScheduleWeek);

            Assert.AreEqual(1, testContractSchedule.ContractScheduleWeeks.Count());
        }

        /// <summary>
        /// Verifies a new ContractScheduleWeek collection can be cleared
        /// </summary>
        [Test]
        public void VerifyContractScheduleWeekCanBeCleared()
        {
            ContractScheduleWeek contractScheduleWeek = new ContractScheduleWeek();
            contractScheduleWeek.Add(DayOfWeek.Saturday, false);
            contractScheduleWeek.Add(DayOfWeek.Sunday, false);

            testContractSchedule.AddContractScheduleWeek(contractScheduleWeek);

            testContractSchedule.ClearContractScheduleWeeks();

            Assert.AreEqual(0, testContractSchedule.ContractScheduleWeeks.Count());
        }

        /// <summary>
        /// Verifies a new ContractScheduleWeek collection can be cleared
        /// </summary>
        [Test]
        public void VerifyContractScheduleWeekCanBeRemoved()
        {
            ContractScheduleWeek contractScheduleWeek = new ContractScheduleWeek();
            contractScheduleWeek.Add(DayOfWeek.Saturday, false);
            contractScheduleWeek.Add(DayOfWeek.Sunday, false);

            ContractScheduleWeek contractScheduleWeek2 = new ContractScheduleWeek();
            contractScheduleWeek2.Add(DayOfWeek.Saturday, false);
            contractScheduleWeek2.Add(DayOfWeek.Sunday, false);

            testContractSchedule.AddContractScheduleWeek(contractScheduleWeek);
            testContractSchedule.AddContractScheduleWeek(contractScheduleWeek2);

            testContractSchedule.RemoveContractScheduleWeek(contractScheduleWeek2);

            Assert.AreEqual(1, testContractSchedule.ContractScheduleWeeks.Count());
        }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(testContractSchedule.GetType()));
        }
        /// <summary>
        /// Determines whether this instance [can add ContractSchedule Week].
        /// </summary>
        /// <remarks>
        /// Created by:Dumithu Dharmasena
        /// Created date: 2008-04-08
        /// </remarks>
        [Test]
        public void CheckAddContractScheduleWeek()
        {
            ContractScheduleWeek _ContractScheduleWeek = new ContractScheduleWeek();
            testContractSchedule.AddContractScheduleWeek(_ContractScheduleWeek);

            Assert.IsTrue(testContractSchedule.ContractScheduleWeeks.Contains(_ContractScheduleWeek));
        }

		[Test]
		public void VerifyIsWorkday()
		{

			testContractSchedule = new ContractSchedule("test");

			// contract schedule - workday = 1
			// 0000000 1111100

			IContractScheduleWeek week = new ContractScheduleWeek();
			testContractSchedule.AddContractScheduleWeek(week); // first week all days off
			week = new ContractScheduleWeek();
			week.Add(DayOfWeek.Monday, true);
			week.Add(DayOfWeek.Tuesday, true);
			week.Add(DayOfWeek.Wednesday, true);
			week.Add(DayOfWeek.Thursday, true);
			week.Add(DayOfWeek.Friday, true);
			testContractSchedule.AddContractScheduleWeek(week); // second week sat and sun days off

			DateOnly personPeriodStartDate = new DateOnly(2012, 06, 01); // this is the first date of the scheduling period
			DateOnly requestedDate = new DateOnly(2012, 06, 01); //friday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 02); //saturday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 03); //sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 04); //monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 01); // sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 02); // monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
		}

		[Test]
		public void VerifyIsWorkdayFoundsCorrectWeekNumber()
		{

			testContractSchedule = new ContractSchedule("test");

			// contract schedule - workday = 1
			// 0000000 1111111

			IContractScheduleWeek week = new ContractScheduleWeek();
			testContractSchedule.AddContractScheduleWeek(week); // first week all days off
			week = new ContractScheduleWeek();
			week.Add(DayOfWeek.Monday, true);
			week.Add(DayOfWeek.Tuesday, true);
			week.Add(DayOfWeek.Wednesday, true);
			week.Add(DayOfWeek.Thursday, true);
			week.Add(DayOfWeek.Friday, true);
			week.Add(DayOfWeek.Saturday, true);
			week.Add(DayOfWeek.Sunday, true);
			testContractSchedule.AddContractScheduleWeek(week); // second week sat and sun days off

			DateOnly personPeriodStartDate = new DateOnly(2012, 06, 01); // this is the first date of the scheduling period
			DateOnly requestedDate = new DateOnly(2012, 06, 01); //odd friday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 02); //odd saturday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 03); //odd sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 04); //even monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 09); //even saturday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 01); // odd sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 02); // even monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 08); // even sunday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));

			personPeriodStartDate = new DateOnly(2012, 06, 03); // this is the first date of the scheduling period

			requestedDate = new DateOnly(2012, 06, 03); //odd sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 04); //even monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 09); //even saturday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 01); // odd sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 02); // even monday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 07, 08); // even sunday
			Assert.IsTrue(testContractSchedule.IsWorkday(personPeriodStartDate, requestedDate, DayOfWeek.Monday));
		}

        [Test]
        public void VerifyIsWorkdayReturnsCorrectContractScheduleDayForDate()
        {
            testContractSchedule = new ContractSchedule("test");
            IContractScheduleWeek week = new ContractScheduleWeek();
            week.Add(DayOfWeek.Monday, true);
            week.Add(DayOfWeek.Tuesday, true);
            testContractSchedule.AddContractScheduleWeek(week);
            week = new ContractScheduleWeek();
            week.Add(DayOfWeek.Wednesday, true);
            week.Add(DayOfWeek.Thursday, true);
            week.Add(DayOfWeek.Friday, true);
            testContractSchedule.AddContractScheduleWeek(week);
            DateOnly owningPeriodStartDate = new DateOnly(2009, 1, 1);
            DateOnly requestedDate = new DateOnly(2009,1,19);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);

            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));

            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);

            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));

            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
            requestedDate = requestedDate.AddDays(1);
            Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
        }

        [Test]
        public void VerifyIsWorkdayUnresolvedRequestedDateThrowsException()
        {
	        Assert.Throws<ArgumentException>(() =>
	        {
				testContractSchedule = new ContractSchedule("test");
				IContractScheduleWeek week = new ContractScheduleWeek();
				week.Add(DayOfWeek.Monday, true);
				week.Add(DayOfWeek.Tuesday, true);
				testContractSchedule.AddContractScheduleWeek(week);
				week = new ContractScheduleWeek();
				week.Add(DayOfWeek.Wednesday, true);
				week.Add(DayOfWeek.Thursday, true);
				week.Add(DayOfWeek.Friday, true);
				testContractSchedule.AddContractScheduleWeek(week);

				DateOnly owningPeriodStartDate = new DateOnly(2009, 1, 1);
				DateOnly requestedDate = new DateOnly(2008, 1, 19);
				Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
			});
        }

        
        [Test]
        public void VerifyIsWorkdayReturnsTrueWhenContractScheduleWeeksCountIsZero()
        {
            testContractSchedule = new ContractSchedule("test");
            DateOnly owningPeriodStartDate = new DateOnly(2009, 1, 1);
            DateOnly requestedDate = new DateOnly(2008, 1, 19);
            Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
        }


		/// <summary>
		/// Expose the bug 19581]
		/// </summary>
		[Test]
		public void ShouldReturnCorrectIsWorkdayForDate()
		{
			testContractSchedule = new ContractSchedule("test");
			IContractScheduleWeek week = new ContractScheduleWeek();
			testContractSchedule.AddContractScheduleWeek(week); // first week all days off
			week = new ContractScheduleWeek();
			week.Add(DayOfWeek.Monday, true);
			week.Add(DayOfWeek.Tuesday, true);
			week.Add(DayOfWeek.Wednesday, true);
			week.Add(DayOfWeek.Thursday, true);
			week.Add(DayOfWeek.Friday, true);
			testContractSchedule.AddContractScheduleWeek(week); // second week sat and sun days off

			DateOnly owningPeriodStartDate = new DateOnly(2012, 06, 01); // this is the first date of the scheduling period
			DateOnly requestedDate = new DateOnly(2012, 06, 01); //friday
			Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 02); //saturday
			Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 03); //sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
			requestedDate = new DateOnly(2012, 06, 04); //monday
			Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));

			owningPeriodStartDate = new DateOnly(2012, 07, 01); // this is the first date of the scheduling period
			requestedDate = new DateOnly(2012, 07, 01); // odd sunday
			Assert.IsFalse(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));

			// this day is false before the bugfix
			requestedDate = new DateOnly(2012, 07, 02); // even monday
			Assert.IsTrue(testContractSchedule.IsWorkday(owningPeriodStartDate, requestedDate, DayOfWeek.Monday));
		}


    }
}