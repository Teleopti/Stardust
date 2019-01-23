using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Secrets.DayOffPlanning;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class DaysOffPerWeekSolverTest
    {
        private IDayOffBackToLegalStateSolver _target;
        private IDayOffBackToLegalStateFunctions _functions;
        private LockableBitArray _bitArray;
        private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray);
            _daysOffPreferences = new DaysOffPreferences();
            _target = new DaysOffPerWeekSolver(_bitArray, _functions, _daysOffPreferences, 20);
        }

        [Test]
        public void VerifySwapBits()
        {
            DaysOffPerWeekSolver target = new DaysOffPerWeekSolver(_bitArray, _functions, _daysOffPreferences, 20);
            Assert.IsTrue(target.SwapBits(0, 1));
            Assert.IsFalse(target.SwapBits(-1, 1));
            Assert.IsFalse(target.SwapBits(0, -1));
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(0, 4);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(2, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(1, 3);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(0, 3);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible()
        {
            _bitArray.SetAll(false);
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(-1, -1);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossible2()
        {
            _target = new DaysOffPerWeekSolver(_bitArray, _functions, _daysOffPreferences, 0);
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(0, 1);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenAllDaysAreLocked()
        {
            for (int i = 0; i < _bitArray.Count; i++)
            {
                _bitArray.Lock(i, true);
            }
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(0, 3);
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(1, 4);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible()
        {
            _bitArray.SetAll(false);
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(3, 4);
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossible2()
        {
            _target = new DaysOffPerWeekSolver(_bitArray, _functions, _daysOffPreferences, 0);
            _daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(3, 4);
            Assert.IsTrue(_target.SetToFewBackToLegalState());
        }

        private static LockableBitArray array1()
        {
            LockableBitArray ret = new LockableBitArray(28, true, true);
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