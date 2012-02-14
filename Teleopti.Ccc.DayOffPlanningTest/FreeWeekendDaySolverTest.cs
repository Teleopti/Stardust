using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class FreeWeekendDaySolverTest
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
            _target = new FreeWeekendDaySolver(_bitArray, _functions, _sessionRuleSet, 20);
        }

        [Test]
        public void VerifyResolverDescriptionKey()
        {
            Assert.AreEqual("FreeWeekendDayRule", _target.ResolverDescriptionKey);
        }

        [Test]
        public void VerifySwapBits()
        {
            FreeWeekendDaySolver target = new FreeWeekendDaySolver(_bitArray, _functions, _sessionRuleSet, 20);
            Assert.IsTrue(target.SwapBits(0, 1));
            Assert.IsFalse(target.SwapBits(-1, 1));
            Assert.IsFalse(target.SwapBits(0, -1));
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(3, 3);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(4, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(0, 0);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _bitArray.Set(26, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(2, 2);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible()
        {
            _bitArray.Set(26, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(-1, -1);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible2()
        {
            _target = new FreeWeekendDaySolver(_bitArray, _functions, _sessionRuleSet, 0);
            _bitArray.Set(26, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(-1, -1);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _bitArray.Set(13, true);
            _bitArray.Set(20, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(6, 6);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible()
        {
            _bitArray.Set(13, true);
            _bitArray.Set(20, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(66, 66);
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible2()
        {
            _target = new FreeWeekendDaySolver(_bitArray, _functions, _sessionRuleSet, 0);
            _bitArray.Set(13, true);
            _bitArray.Set(20, true);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(66, 66);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySolvableStateOnlyLooksInsidePeriodArea()
        {
            _bitArray.SetAll(false);
            _bitArray.Set(5, true);
            _bitArray.Set(6, true);
            _bitArray.Set(12, true);
            _bitArray.Set(13, true);
            _sessionRuleSet.UseFreeWeekendDays = true;
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(4, 4);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _bitArray.PeriodArea = new MinMax<int>(6, 12);
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(4, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(2, 2);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.FreeWeekendDays = new MinMax<int>(1, 1);
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