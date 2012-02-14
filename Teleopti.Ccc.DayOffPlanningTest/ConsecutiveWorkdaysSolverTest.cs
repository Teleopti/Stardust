using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class ConsecutiveWorkdaysSolverTest
    {
        private IDayOffBackToLegalStateSolver _target;
        private CultureInfo _culture;
        private IDayOffBackToLegalStateFunctions _functions;
        private LockableBitArray _bitArray;
        private DayOffPlannerSessionRuleSet _sessionRuleSet;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _culture = CultureInfo.CreateSpecificCulture("en-GB");
            _functions = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            _sessionRuleSet = new DayOffPlannerSessionRuleSet();
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _sessionRuleSet, 20);
        }

        [Test]
        public void VerifyResolverDescriptionKey()
        {
            Assert.AreEqual("ConsecutiveWorkdaysRule", _target.ResolverDescriptionKey);
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 16);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 16);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifyIsInLegalStateWhenPeriodEndsWithLongestBlock()
        {
            _bitArray.Set(27, false);
            _bitArray.Set(11, true);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenMiddleBitOfLongestBlockIsLocked()
        {
            _bitArray.Lock(18, true);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenNoDaysOff()
        {
            _bitArray.SetAll(false);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateOutOfIterations()
        {
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateOutOfIterations()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(17, true);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateWhenDayOffBeforeShortestBlockIsWeekendDay()
        {
            _bitArray.Set(6, false);
            _bitArray.Set(17, true);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateWhenShortestIsTheFirstBlock()
        {   _bitArray.Set(0, false);
            _bitArray.Set(17, true);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 16);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackWhenConsideringWeekAfter()
        {
            _bitArray = array2();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _sessionRuleSet, 20);
            _bitArray.PeriodArea = new MinMax<int>(0, 6);
            _bitArray.Lock(7, true);
            _bitArray.Lock(8, true);
            _bitArray.Lock(9, true);
            _bitArray.Lock(10, true);
            _bitArray.Lock(11, true);
            _bitArray.Lock(12, true);
            _bitArray.Lock(13, true);
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 6);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_bitArray[6]);
        }

        [Test]
        public void MinConsecutiveWorkdaysShouldNotConsiderLastBlockInArray()
        {
            _sessionRuleSet.ConsecutiveWorkdays = new MinMax<int>(2, 6);
            _bitArray = array3();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            _target = new ConsecutiveWorkdaysSolver(_bitArray, _functions, _sessionRuleSet, 20);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
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