using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class FreeWeekendSolverTest
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
            _target = new FreeWeekendSolver(_bitArray, _functions, _sessionRuleSet, 20);
        }

        [Test]
        public void VerifyResolverDescriptionKey()
        {
            Assert.AreEqual("FreeWeekendRule", _target.ResolverDescriptionKey);
        }

        [Test]
        public void VerifySwapBits()
        {
            FreeWeekendSolver target = new FreeWeekendSolver(_bitArray, _functions, _sessionRuleSet, 20);
            Assert.IsTrue(target.SwapBits(0, 1));
            Assert.IsFalse(target.SwapBits(-1, 1));
            Assert.IsFalse(target.SwapBits(0, -1));
        }
      
        [Test]
        public void VerifyIsWeekendsInLegalState()
        {
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.FreeWeekends = new MinMax<int>(2, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _sessionRuleSet.FreeWeekends = new MinMax<int>(0, 0);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _bitArray.Lock(6, true);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(1, 1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToManyBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenAllNoneWeekendIsLocked()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _bitArray.Lock(6, true);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(1, 1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToManyBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossibleRule()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _bitArray.Lock(21, true);
            _bitArray.Lock(22, true);
            _bitArray.Lock(23, true);
            _bitArray.Lock(24, true);
            _bitArray.Lock(25, true);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(-1, -1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossibleRule2()
        {
            _target = new FreeWeekendSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(-1, -1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _sessionRuleSet.FreeWeekends = new MinMax<int>(2, 2);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalState1()
        {
            _bitArray.Set(26, true);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(3, 3);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossibleRule()
        {
            _sessionRuleSet.FreeWeekends = new MinMax<int>(6, 6);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossibleRule2()
        {
            _target = new FreeWeekendSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(6, 6);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifySolvableStateOnlyLooksInsidePeriodArea()
        {
            _bitArray.SetAll(false);
            _bitArray.Set(5, true);
            _bitArray.Set(6, true);
            _bitArray.Set(12, true);
            _bitArray.Set(13, true);
            _sessionRuleSet.UseFreeWeekends = true;
            _sessionRuleSet.FreeWeekends = new MinMax<int>(2, 2);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _bitArray.PeriodArea = new MinMax<int>(6, 12);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(2, 2);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _bitArray.PeriodArea = new MinMax<int>(6, 13);
            _sessionRuleSet.FreeWeekends = new MinMax<int>(1, 1);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.FreeWeekends = new MinMax<int>(0, 0);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
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
    }
}