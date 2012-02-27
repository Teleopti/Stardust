﻿using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tui"), TestFixture]
    public class TuiCaseSolverTest
    {
        private TuiCaseSolver _target;
        private CultureInfo _culture;
        private IDayOffBackToLegalStateFunctions _functions;
        private ILockableBitArray _bitArray;
        private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _bitArray = array1();
            _culture = CultureInfo.CreateSpecificCulture("en-GB");
            _functions = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            _daysOffPreferences = new DaysOffPreferences();
            _target = new TuiCaseSolver(_bitArray, _functions, _daysOffPreferences, 20);
        }

        [Test]
        public void VerifyResolverDescriptionKey()
        {
            Assert.AreEqual("TUI Case solver", _target.ResolverDescriptionKey);
        }

        [Test]
        public void VerifyIsInLegalState()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 16);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifyIsInLegalStateWhenPeriodEndsWithLongestBlock()
        {
            _bitArray.Set(27, false);
            _bitArray.Set(11, true);
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState2()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState3()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalState4()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
            Assert.IsFalse(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenMiddleBitOfLongestBlockIsLocked()
        {
            _bitArray.Lock(18, true);
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateWhenNoDaysOff()
        {
            _bitArray.SetAll(false);
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToManyBackToLegalStateOutOfIterations()
        {
            _target = new TuiCaseSolver(_bitArray, _functions, _daysOffPreferences, 0);
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
            Assert.IsTrue(_target.SetToManyBackToLegalState());
        }

        [Test]
        public void VerifySetToFewBackToLegalState()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }


        [Test]
        public void MinConsecutiveWorkdaysShouldNotConsiderLastBlockInArray()
        {
            _daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 6);
            _bitArray = array3();
            _functions = new DayOffBackToLegalStateFunctions(_bitArray, _culture);
            _target = new TuiCaseSolver(_bitArray, _functions, _daysOffPreferences, 20);
            Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
            Assert.IsFalse(_target.SetToFewBackToLegalState());
        }

        private static ILockableBitArray array1()
        {
            ILockableBitArray ret = new LockableBitArray(28, true, true, null);
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

        private static ILockableBitArray array3()
        {
            ILockableBitArray ret = new LockableBitArray(7, false, false, null);
            ret.SetAll(false);
            ret.Set(4, true);//fr
            ret.Set(5, true);//sa
            return ret;
        }
    }
}