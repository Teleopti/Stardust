using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class DayOffInitialDistributionTest
    {
        private DayOffInitialDistribution _target;
        private IOfficialWeekendDays _weekEndDays;
        private LockableBitArray _testArray;

        [SetUp]
        public void Setup()
        {
            _testArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyDistributeDayOffsEvenlySwedishCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("se-SE");
            _weekEndDays = new OfficialWeekendDays(cultureInfo);
            _target = new DayOffInitialDistribution(_weekEndDays);
            _target.DistributeDayOffsEvenly(_testArray);

            Assert.IsTrue(_testArray.DaysOffBitArray[6]);
            Assert.IsTrue(_testArray.DaysOffBitArray[12]);
            Assert.IsTrue(_testArray.DaysOffBitArray[13]);
            Assert.IsTrue(_testArray.DaysOffBitArray[17]);
            Assert.IsTrue(_testArray.DaysOffBitArray[18]);
        }

        [Test]
        public void VerifyDistributeDayOffsEvenlyUSCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            _weekEndDays = new OfficialWeekendDays(cultureInfo);
            _target = new DayOffInitialDistribution(_weekEndDays);
            _target.DistributeDayOffsEvenly(_testArray);

            Assert.IsTrue(_testArray.DaysOffBitArray[6]);
            Assert.IsTrue(_testArray.DaysOffBitArray[7]);
            Assert.IsTrue(_testArray.DaysOffBitArray[13]);
            Assert.IsTrue(_testArray.DaysOffBitArray[14]);
            Assert.IsTrue(_testArray.DaysOffBitArray[18]);
        }

        [Test]
        public void VerifyDistributeDayOffsEvenlyWithLockedValues()
        {
            _testArray = createFullyLockedBitArrayForTest();
            CultureInfo cultureInfo = new CultureInfo("en-US");
            _weekEndDays = new OfficialWeekendDays(cultureInfo);
            _target = new DayOffInitialDistribution(_weekEndDays);
            _target.DistributeDayOffsEvenly(_testArray);

            Assert.IsTrue(_testArray.DaysOffBitArray[6]);
            Assert.IsTrue(_testArray.DaysOffBitArray[7]);
            Assert.IsTrue(_testArray.DaysOffBitArray[8]);
            Assert.IsTrue(_testArray.DaysOffBitArray[17]);
            Assert.IsTrue(_testArray.DaysOffBitArray[18]);
        }

        [Test]
        public void VerifyTestArray()
        {
            Assert.IsFalse(_testArray.IsLocked(0, true));
            Assert.IsFalse(_testArray.IsLocked(9, true));
            Assert.IsTrue(_testArray.IsLocked(10, true));
            Assert.IsTrue(_testArray.IsLocked(11, true));
            Assert.IsFalse(_testArray.IsLocked(12, true));
        }

        private static LockableBitArray createBitArrayForTest()
        {
            LockableBitArray bitArray = new LockableBitArray(21, false, false, null);
            bitArray.PeriodArea = new MinMax<int>(6, 19);

            bitArray.Set(6, true);
            bitArray.Set(7, true);
            bitArray.Set(8, true);
            bitArray.Set(17, true);
            bitArray.Set(18, true);

            bitArray.Lock(10, true);
            bitArray.Lock(11, true);
            bitArray.Lock(18, true);
            bitArray.Lock(19, true);

            return bitArray;
        }

        private static LockableBitArray createFullyLockedBitArrayForTest()
        {
            LockableBitArray bitArray = new LockableBitArray(21, false, false, null);
            bitArray.PeriodArea = new MinMax<int>(6, 19);

            bitArray.Set(6, true);
            bitArray.Set(7, true);
            bitArray.Set(8, true);
            bitArray.Set(17, true);
            bitArray.Set(18, true);

            bitArray.Lock(6, true);
            bitArray.Lock(7, true);
            bitArray.Lock(8, true);
            bitArray.Lock(17, true);
            bitArray.Lock(18, true);

            return bitArray;
        }

    }
}
