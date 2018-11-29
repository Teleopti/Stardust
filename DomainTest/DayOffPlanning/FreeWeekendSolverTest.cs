using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Secrets.DayOffPlanning;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class FreeWeekendSolverTest
    {
        private FreeWeekendSolver _target;
        private IDayOffBackToLegalStateFunctions _functions;
        private LockableBitArray _bitArray;
        private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray);
            _daysOffPreferences = new DaysOffPreferences();
            _target = new FreeWeekendSolver(_bitArray, _functions, _daysOffPreferences, 20);
        }

        [Test]
        public void VerifySwapBits()
        {
            FreeWeekendSolver target = new FreeWeekendSolver(_bitArray, _functions, _daysOffPreferences, 20);
            Assert.IsTrue(target.SwapBits(0, 1));
            Assert.IsFalse(target.SwapBits(-1, 1));
            Assert.IsFalse(target.SwapBits(0, -1));
        }
      
        [Test]
        public void VerifyIsWeekendsInLegalState()
        {
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(1, 3);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(2, 4);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(0, 0);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _bitArray.Lock(6, true);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(1, 1);
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
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(1, 1);
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
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(-1, -1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToManyBackToLegalStateImpossibleRule2()
        {
            _target = new FreeWeekendSolver(_bitArray, _functions, _daysOffPreferences, 0);
            _bitArray.Set(0, false);
            _bitArray.Set(26, true);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(-1, -1);
            bool result = _target.SetToManyBackToLegalState();
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(2, 2);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalState1()
        {
            _bitArray.Set(26, true);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(3, 3);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
            result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossibleRule()
        {
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(6, 6);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifySetToFewBackToLegalStateImpossibleRule2()
        {
            _target = new FreeWeekendSolver(_bitArray, _functions, _daysOffPreferences, 0);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(6, 6);
            bool result = _target.SetToFewBackToLegalState();
            Assert.IsTrue(result);
        }

	    [Test]
	    public void ShouldRandomizeWorkingWeekEnd()
	    {
		    LockableBitArray ret = array1();
		    _target = new FreeWeekendSolver(ret, _functions, _daysOffPreferences, 20);
			_daysOffPreferences.UseFullWeekendsOff = true;
		    _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(1, 1);
		    var first = false;
		    var second = false;
		    for (int i = 0; i < 50; i++)
		    {
			    ret.SetAll(false);
			    ret.Set(12, true);
			    ret.Set(13, true);
			    ret.Set(19, true);
			    ret.Set(20, true);
			    _target.SetToManyBackToLegalState();
			    if (!ret[12] || !ret[13])
				    first = true;

			    if (!ret[19] || !ret[20])
				    second = true;

				if(first && second)
					break;
			}

			(first && second).Should().Be.True();
		}

        [Test]
        public void VerifySolvableStateOnlyLooksInsidePeriodArea()
        {
            _bitArray.SetAll(false);
            _bitArray.Set(5, true);
            _bitArray.Set(6, true);
            _bitArray.Set(12, true);
            _bitArray.Set(13, true);
            _daysOffPreferences.UseFullWeekendsOff = true;
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(2, 2);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _bitArray.PeriodArea = new MinMax<int>(6, 12);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(2, 2);
            Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
            _bitArray.PeriodArea = new MinMax<int>(6, 13);
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(1, 1);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(0, 0);
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