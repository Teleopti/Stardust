using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests for PeriodScaleUnit
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2008-02-27
    /// </remarks>
    [TestFixture]
    public class PeriodScaleUnitTest
    {
        private PeriodScaleUnit target;

        /// <summary>
        /// Determines whether this instance [can create instance].
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        [Test]
        public void CanCreateInstance()
        {
            target = new PeriodScaleUnit(PeriodScaleUnitType.Days, "Minutes");
            Assert.IsNotNull(target);
            Assert.AreEqual("Minutes", target.DisplayName);
            Assert.AreEqual(PeriodScaleUnitType.Days, target.UnitType);
        }

        /// <summary>
        /// Verifies default constructor is not public.
        /// </summary>
        [Test]
        public void VerifyDisabledDefaultConstructor()
        {
            bool ret = ReflectionHelper.HasDefaultConstructor(target.GetType());
            Assert.IsTrue(ret);
        }

        /// <summary>
        /// Verifies the get scale unit date time period.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-27
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyGetScaleUnitDateTimePeriod()
        {
            //633347856000000000
            DateOnly startingDateTime = new DateOnly(2008, 1, 1);
            int numberOf = 10;

            // Test Days
            DateOnly expectedDateTime = new DateOnly(2008, 1, 1 + numberOf);
            DateOnlyPeriod resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Days, numberOf, startingDateTime);
            Assert.AreEqual(expectedDateTime, resultDateTimePeriod.EndDate);

            // Test weeks
            expectedDateTime = new DateOnly(2008, 3, 11);
            resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Weeks, numberOf, startingDateTime);
            Assert.AreEqual(expectedDateTime, resultDateTimePeriod.EndDate);

            // Test weeks
            resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Weeks, 0, startingDateTime);
            Assert.AreEqual(startingDateTime, resultDateTimePeriod.EndDate);

            // Test Months
            expectedDateTime = new DateOnly(2008, 1 + numberOf, 1);
            resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Months, numberOf, startingDateTime);
            Assert.AreEqual(expectedDateTime, resultDateTimePeriod.EndDate);

            // Test Years
            expectedDateTime = new DateOnly(2008 + numberOf, 1, 1);
            resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Years, numberOf, startingDateTime);
            Assert.AreEqual(expectedDateTime, resultDateTimePeriod.EndDate);

            // Test Minus in Number of
            numberOf = -10;
            expectedDateTime = new DateOnly(2008 + numberOf, 1, 1);
            resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Years, numberOf, startingDateTime);

            Assert.AreEqual(expectedDateTime, resultDateTimePeriod.StartDate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ExposingBug11329PreparingWorkloadIntradayTemplatesIncludesToday()
        {
            //If today is 2010, 7, 19 and i ask for last 2 weeks, today should not be included (gives 15 days of data, instead of 14)
            DateOnly today = new DateOnly(2010, 7, 19);
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(2010, 7, 5, 2010, 07, 18);
            DateOnlyPeriod resultDateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                PeriodScaleUnitType.Weeks, -2, today);
            Assert.AreEqual(expectedPeriod, resultDateTimePeriod);
        }
    }
}
