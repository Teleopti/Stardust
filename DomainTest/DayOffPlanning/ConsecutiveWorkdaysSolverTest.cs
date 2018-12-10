using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Secrets.DayOffPlanning;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class ConsecutiveWorkdaysSolverTest
    {
        private IDayOffBackToLegalStateSolver _target;
        private IDayOffBackToLegalStateFunctions _functions;
        private LockableBitArray _bitArray;
        private IDaysOffPreferences _datDaysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray);
            _datDaysOffPreferences = new DaysOffPreferences();
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 20);
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 16);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifyIsInLegalStateWhenPeriodEndsWithLongestBlock()
        {
            _bitArray.Set(27, false);
            _bitArray.Set(11, true);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenMiddleBitOfLongestBlockIsLocked()
        {
            _bitArray.Lock(18, true);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenNoDaysOff()
        {
            _bitArray.SetAll(false);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateOutOfIterations()
        {
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 0);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateOutOfIterations()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(17, true);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 0);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateWhenDayOffBeforeShortestBlockIsWeekendDay()
        {
            _bitArray.Set(6, false);
            _bitArray.Set(17, true);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateWhenShortestIsTheFirstBlock()
        {   _bitArray.Set(0, false);
            _bitArray.Set(17, true);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackWhenConsideringWeekAfter()
        {
            _bitArray = array2();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 20);
            _bitArray.PeriodArea = new MinMax<int>(0, 6);
            _bitArray.Lock(7, true);
            _bitArray.Lock(8, true);
            _bitArray.Lock(9, true);
            _bitArray.Lock(10, true);
            _bitArray.Lock(11, true);
            _bitArray.Lock(12, true);
            _bitArray.Lock(13, true);
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 6);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_bitArray[6]);
        }

        [Test]
        public void MinConsecutiveWorkdaysShouldNotConsiderLastBlockInArray()
        {
            _datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 6);
            _bitArray = array3();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 20);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

		[Test]
		public void FixForBug20501()
		{
			_datDaysOffPreferences.ConsecutiveDaysOffValue = new MinMax<int>(2, 2);
			_datDaysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(2, 2);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(5, 5);
			_datDaysOffPreferences.ConsiderWeekBefore = true;
			_datDaysOffPreferences.ConsiderWeekAfter = true;
			_bitArray = new LockableBitArray(42, true, true, null);
			_bitArray.SetAll(false);
			_bitArray.Set(2, true);
			_bitArray.Set(3, true);
			_bitArray.Set(5, true);
			_bitArray.Set(6, true);
			_bitArray.Set(12, true);
			_bitArray.Set(13, true);
			_bitArray.Set(19, true);
			_bitArray.Set(20, true);
			_bitArray.Set(26, true);
			_bitArray.Set(27, true);
			_bitArray.Set(33, true);
			_bitArray.Set(34, true);
			_bitArray.Set(37, true); //this one was selected and no check if it was locked

			for (int i = 0; i < _bitArray.Count; i++)
			{
				_bitArray.Lock(i, true);
			}

			_bitArray.Lock(12, false);
			_bitArray.Lock(13, false);
			_bitArray.Lock(17, false);
			_bitArray.Lock(19, false);
			_bitArray.Lock(20, false);
			_bitArray.Lock(26, false);
			_bitArray.Lock(27, false);
			_bitArray.Lock(33, false);
			_bitArray.Lock(34, false);
			
			_functions = new DayOffBackToLegalStateFunctions(_bitArray);
			_target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences, 20);
			Assert.IsTrue(_target.SetToFewBackToLegalState());
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
            LockableBitArray ret = new LockableBitArray(14, true, true, null);
            ret.SetAll(false);
            ret.Set(5, true);//sa
            ret.Set(6, true);//su

            ret.Set(8, true);//tu
            ret.Set(9, true);//we


            return ret;
        }

        private static LockableBitArray array3()
        {
            LockableBitArray ret = new LockableBitArray(7, false, false, null);
            ret.SetAll(false);
            ret.Set(4, true);//fr
            ret.Set(5, true);//sa

            return ret;
        }
    }
}