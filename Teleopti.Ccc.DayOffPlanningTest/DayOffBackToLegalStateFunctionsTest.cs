using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class DayOffBackToLegalStateFunctionsTest
    {
        private IDayOffBackToLegalStateFunctions _target;
        private LockableBitArray _bitArray;
        private CultureInfo _culture;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _culture = CultureInfo.CreateSpecificCulture("en-GB");
            _target = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
        }

        [Test]
        public void VerifyWeekendListMondayFirstDayOfWeek()
        {
            IList<Point> weekendList = _target.WeekendList();
            Assert.AreEqual(4, weekendList.Count);
            Assert.AreEqual(new Point(5, 6), weekendList[0]);
            Assert.AreEqual(new Point(26, 27), weekendList[3]);
        }

        [Test]
        public void VerifyFindFirstIndexOfFirstWeekendWithNoDayOff()
        {
            int index = _target.FindFirstIndexOfFirstWeekendWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(12, index);
            _bitArray.Set(12, true);
            _bitArray.Set(19, true);
            index = _target.FindFirstIndexOfFirstWeekendWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindFirstIndexOfFirstWeekendWithNoDayOffWhenFirstDayIsLocked()
        {
            _bitArray.Lock(12, true);
            int index = _target.FindFirstIndexOfFirstWeekendWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(13, index);
        }

        [Test]
        public void VerifyIsWeekendDay()
        {
            Assert.IsTrue(_target.IsWeekendDay(6, _target.WeekendList()));
            Assert.IsFalse(_target.IsWeekendDay(7, _target.WeekendList()));
        }

        [Test]
        public void VerifyFindFirstNonWeekendDayWithNoDayOff()
        {
            int index = _target.FindFirstNonWeekendDayWithNoDayOff(_target.WeekendList());
            Assert.AreEqual(2, index);
            _bitArray.SetAll(true);
            index = _target.FindFirstNonWeekendDayWithNoDayOff(_target.WeekendList());
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindFirstNonWeekendDayWithDayOff()
        {
            _bitArray.Set(0, false);
            int index = _target.FindFirstNonWeekendDayWithDayOff(_target.WeekendList(), true);
            Assert.AreEqual(1, index);
            _bitArray.SetAll(false);
            index = _target.FindFirstNonWeekendDayWithDayOff(_target.WeekendList(), true);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindFirstWeekendDayWithNoDayOff()
        {
            int index = _target.FindFirstWeekendDayWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(12, index);
            _bitArray.Set(12, true);
            index = _target.FindFirstWeekendDayWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(13, index);
            _bitArray.SetAll(true);
            index = _target.FindFirstWeekendDayWithNoDayOff(_target.WeekendList(), true);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindFirstWeekendDayDayOff()
        {
            int index = _target.FindFirstWeekendDayDayOff(_target.WeekendList(), true);
            Assert.AreEqual(5, index);
            _bitArray.Set(5, false);
            index = _target.FindFirstWeekendDayDayOff(_target.WeekendList(), true);
            Assert.AreEqual(6, index);
            _bitArray.Set(6, false);
            index = _target.FindFirstWeekendDayDayOff(_target.WeekendList(), true);
            Assert.AreEqual(27, index);
            _bitArray.SetAll(false);
            index = _target.FindFirstWeekendDayDayOff(_target.WeekendList(), true);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindFirstSingleDayWeekendDayOff()
        {
            int index = _target.FindFirstNonLockedSingleDayWeekendDayOff(_target.WeekendList(), true);
            Assert.AreEqual(27, index);
            _bitArray.Set(6, false);
            index = _target.FindFirstNonLockedSingleDayWeekendDayOff(_target.WeekendList(), true);
            Assert.AreEqual(5, index);
        }

        [Test]
        public void VerifyFindFirstNoneLockedAccompanyingSingleDayWeekendDayOff()
        {
            int index = _target.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(_target.WeekendList());
            Assert.AreEqual(26, index);
            _bitArray.Set(6, false);
            index = _target.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(_target.WeekendList());
            Assert.AreEqual(6, index);
			_bitArray.Lock(6, true);
			index = _target.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(_target.WeekendList());
			Assert.AreEqual(5, index);
            _bitArray.Lock(5, true);
            index = _target.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(_target.WeekendList());
            Assert.AreEqual(26, index);
            _bitArray.Lock(26, true);
            index = _target.FindFirstNoneLockedAccompanyingSingleDayWeekendDayOff(_target.WeekendList());
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void VerifyFindLongestConsecutiveWorkdayBlock()
        {
            Point point = _target.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
            Assert.AreEqual(11, point.X);
            Assert.AreEqual(26, point.Y);
        }

        [Test]
        public void VerifyFindShortestConsecutiveWorkdayBlock()
        {
            Point point = _target.FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
            Assert.AreEqual(8, point.X);
            Assert.AreEqual(8, point.Y);
        }

        [Test]
        public void VerifyFindShortestConsecutiveWorkdayWhenBothBeforeAndAfterIsLockedWithoutSolution()
        {
            _bitArray = array4();
            _target = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            Point point = _target.FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
            Assert.AreEqual(-1, point.X);
            Assert.AreEqual(-1, point.Y);
        }

        [Test]
        public void VerifyFindNextIndexWithNoDayOff()
        {
            int index = _target.FindNextIndexWithNoDayOff(5, true);
            Assert.AreEqual(8, index);
            index = _target.FindNextIndexWithNoDayOff(27, true);
            Assert.AreEqual(2, index);
        }

        [Test]
        public void VerifyWeekNumberOfDaysOffList()
        {
            IDictionary<int, int> numberOfDaysOffPerWeek = _target.CreateWeeklyDaysOffsDictionary(false, false);
            Assert.AreEqual(4, numberOfDaysOffPerWeek[0]);
        }

        [Test]
        public void VerifyFindWeekWithLeastNumberOfDays()
        {
            int weekIndex =
                DayOffBackToLegalStateFunctions.FindWeekWithLeastNumberOfDaysOff(_target.CreateWeeklyDaysOffsDictionary(false, false));
            Assert.AreEqual(2, weekIndex);
        }

        [Test]
        public void VerifyFindWeekWithMostNumberOfDays()
        {
            int weekIndex =
                DayOffBackToLegalStateFunctions.FindWeekWithMostNumberOfDays(_target.CreateWeeklyDaysOffsDictionary(false, false));
            Assert.AreEqual(0, weekIndex);
        }

        [Test]
        public void VerifyFindFirstWeekdayIndexWithDayOffButRatherNotWeekend()
        {
            int index = _target.FindFirstWeekdayIndexWithDayOffButRatherNotWeekend(0, _target.WeekendList(), true);
            Assert.AreEqual(0, index);
            index = _target.FindFirstWeekdayIndexWithDayOffButRatherNotWeekend(3, _target.WeekendList(), true);
            Assert.AreEqual(27, index);
        }

        [Test]
        public void VerifyFindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend()
        {
            _bitArray.Set(3, true);
            _bitArray.Set(4, true);
            _bitArray.Set(5, false);
            int index = _target.FindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend(0, _target.WeekendList(), true);
            Assert.AreEqual(2, index);
            _bitArray.Set(2, true);
            index = _target.FindFirstWeekdayIndexWithNoDayOffButRatherNotWeekend(0, _target.WeekendList(), true);
            Assert.AreEqual(5, index);
        }

        [Test]
        public void VerifyFindRandomNonWeekendDayWithNoDayOff()
        {
            int index = _target.FindRandomNonLockedNonWeekendDayWithNoDayOff(_target.WeekendList(), 0);
            Assert.AreEqual(-1, index);
            index = _target.FindRandomNonLockedNonWeekendDayWithNoDayOff(_target.WeekendList(), 1000);
            Assert.AreNotEqual(-1, index);

        }

        [Test]
        public void VerifyFindFirstIndexOfWeek()
        {
            Assert.AreEqual(0, _target.FindFirstIndexOfWeek(3));
            Assert.AreEqual(0, _target.FindFirstIndexOfWeek(6));
            Assert.AreEqual(7, _target.FindFirstIndexOfWeek(7));
            Assert.AreEqual(7, _target.FindFirstIndexOfWeek(9));
            Assert.AreEqual(7, _target.FindFirstIndexOfWeek(13));
            Assert.AreEqual(14, _target.FindFirstIndexOfWeek(14));
            Assert.AreEqual(21, _target.FindFirstIndexOfWeek(27));
        }

        [Test]
        public void VerifyFindFirstDayOffBlockWithUnlockedDayOff()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            Point ret = _target.FindFirstDayOffBlockWithUnlockedDayOff(2, int.MaxValue);
            Assert.AreEqual(5, ret.X);
            Assert.AreEqual(7, ret.Y);
            _bitArray.Lock(6, true);
            ret = _target.FindFirstDayOffBlockWithUnlockedDayOff(2, int.MaxValue);
            Assert.AreEqual(5, ret.X);
            Assert.AreEqual(7, ret.Y);
            _bitArray.Lock(5, true);
            _bitArray.Lock(7, true);
            ret = _target.FindFirstDayOffBlockWithUnlockedDayOff(2, int.MaxValue);
            Assert.AreEqual(9, ret.X);
            Assert.AreEqual(10, ret.Y);
            _bitArray.Lock(9, true);
            _bitArray.Lock(10, true);
            ret = _target.FindFirstDayOffBlockWithUnlockedDayOff(2, int.MaxValue);
            Assert.AreEqual(26, ret.X);
            Assert.AreEqual(27, ret.Y);
        }

        [Test]
        public void VerifyFindShortestWorkdayBlockDoesNotConsiderTheEndsOfTheArray()
        {
            _bitArray = array2();
            _target = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            Point result = _target.FindShortestConsecutiveWorkdayBlockWithAtLeastOneMovableBitBeforeOrAfter();
            Assert.AreEqual(8, result.X);
            Assert.AreEqual(9, result.Y);
        }

		[Test]
		public void WeekNumberOfDaysOffListShouldCalculateCorrect()
		{
			_bitArray = array3();
			_target = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
			IDictionary<int, int> result = _target.CreateWeeklyDaysOffsDictionary(true, true);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(2, result[0]);
			result = _target.CreateWeeklyDaysOffsDictionary(false, true);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(2, result[1]);
			result = _target.CreateWeeklyDaysOffsDictionary(true, false);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(2, result[1]);
			result = _target.CreateWeeklyDaysOffsDictionary(false, false);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(2, result[1]);
			Assert.AreEqual(2, result[2]);
		}

        private static LockableBitArray array1()
        {

            LockableBitArray ret = new LockableBitArray(28, true, true, null);
            ret.SetAll(false);
            ret.Set(0, true);
            ret.Set(1, true);
            ret.Set(5, true);//sa
            ret.Set(6, true);//su
            ret.Set(7, true);
            ret.Set(9, true);
            ret.Set(10, true);
            ret.Set(27, true);//su

            return ret;
        }

        private static LockableBitArray array2()
        {

            LockableBitArray ret = new LockableBitArray(28, true, true, null);
            ret.SetAll(false);
            ret.Set(0, false);
            ret.Set(1, true);
            ret.Set(5, true);//sa
            ret.Set(6, true);//su
            ret.Set(7, true);
            ret.Set(10, true);
            ret.Set(26, true);
            ret.Set(27, false);//su

            return ret;
        }

		private static LockableBitArray array3()
		{

            LockableBitArray ret = new LockableBitArray(21, true, true, null);
			ret.SetAll(false);
			ret.Set(5, true);//sa
			ret.Set(6, true);//su
			ret.Set(11, true);//fr
			ret.Set(12, true);//sa
			ret.Set(19, true);//sa
			ret.Set(20, true);//su

			return ret;
		}

        private static LockableBitArray array4()
        {

            LockableBitArray ret = new LockableBitArray(35, true, true, null);
            ret.SetAll(false);
            ret.Set(5, true);
            ret.Set(8, true);
            ret.Set(14, true);
            ret.Set(20, true);
            ret.Set(26, true);
            ret.Set(32, true);

            ret.Lock(0, true);
            ret.Lock(1, true);
            ret.Lock(2, true);
            ret.Lock(3, true);
            ret.Lock(4, true);
            ret.Lock(5, true);
            ret.Lock(8, true);
            ret.Lock(14, true);
            ret.Lock(20, true);
            ret.Lock(26, true);
            ret.Lock(29, true);
            ret.Lock(32, true);
            ret.Lock(34, true); ;

            return ret;
        }
    }
}