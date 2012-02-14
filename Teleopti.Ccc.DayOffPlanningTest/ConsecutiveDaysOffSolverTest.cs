using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class ConsecutiveDaysOffSolverTest
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
            _sessionRuleSet = ruleSetForTest();
            _target = new ConsecutiveDaysOffSolver(_bitArray, _functions, _sessionRuleSet, 20);
        }

        [Test]
        public void VerifyResolverDescriptionKey()
        {
            Assert.AreEqual("ConsecutiveDaysOffRule", _target.ResolverDescriptionKey);
        }

        [Test]
        public void VerifySwapBits()
        {
            ConsecutiveDaysOffSolver target = new ConsecutiveDaysOffSolver(_bitArray, _functions, _sessionRuleSet, 20);
            Assert.IsTrue(target.SwapBits(0, 1));
            Assert.IsFalse(target.SwapBits(-1, 1));
            Assert.IsFalse(target.SwapBits(0, -1));
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(0, 4);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(2, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(1, 2);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(1, 2);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState2()
        {
            _bitArray.Set(7, false);
            _bitArray.Set(26, true);
            _bitArray.Set(9, false);
            _bitArray.Set(25, true);
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(1, 2);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible()
        {
            _bitArray.SetAll(false);
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(-1, -1);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible2()
        {
            _target = new ConsecutiveDaysOffSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(1, 1);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(2, 4);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible()
        {
            _bitArray.SetAll(false);
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(9, 9);
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible2()
        {
            _target = new ConsecutiveDaysOffSolver(_bitArray, _functions, _sessionRuleSet, 0);
            _sessionRuleSet.ConsecutiveDaysOff = new MinMax<int>(9, 9);
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

        private static DayOffPlannerSessionRuleSet ruleSetForTest()
        {
            DayOffPlannerSessionRuleSet ruleSet = new DayOffPlannerSessionRuleSet();
            ruleSet.DaysOffPerWeek = new MinMax<int>(1, 3);
            ruleSet.ConsecutiveDaysOff = new MinMax<int>(1, 3);
            ruleSet.ConsecutiveWorkdays = new MinMax<int>(2, 6);
            ruleSet.FreeWeekends = new MinMax<int>(1, 3);
            ruleSet.FreeWeekendDays = new MinMax<int>(4, 6);
            ruleSet.UseDaysOffPerWeek = true;
            ruleSet.UseConsecutiveDaysOff = true;
            ruleSet.UseConsecutiveWorkdays = true;
            ruleSet.UseFreeWeekends = true;
            ruleSet.UseFreeWeekendDays = true;

            return ruleSet;
        }
    }
}