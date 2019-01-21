using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class LockableBitArrayTest
    {
        private ILockableBitArray _target;

        [SetUp]
        public void Setup()
        {
            _target = new LockableBitArray(7, false, false);
        }

        [Test]
        public void VerifySetAll()
        {
            Assert.IsFalse(_target[0]);
            Assert.IsFalse(_target[1]);
            Assert.IsFalse(_target[2]);
            Assert.IsFalse(_target[3]);
            Assert.IsFalse(_target[4]);
            Assert.IsFalse(_target[5]);
            Assert.IsFalse(_target[6]);
            _target.SetAll(true);
            Assert.IsTrue(_target[0]);
            Assert.IsTrue(_target[1]);
            Assert.IsTrue(_target[2]);
            Assert.IsTrue(_target[3]);
            Assert.IsTrue(_target[4]);
            Assert.IsTrue(_target[5]);
            Assert.IsTrue(_target[6]);
            _target.SetAll(false);
            Assert.IsFalse(_target[0]);
            Assert.IsFalse(_target[1]);
            Assert.IsFalse(_target[2]);
            Assert.IsFalse(_target[3]);
            Assert.IsFalse(_target[4]);
            Assert.IsFalse(_target[5]);
            Assert.IsFalse(_target[6]);
        }

        [Test]
        public void VerifySet()
        {
            Assert.IsFalse(_target[0]);
            _target.Set(0, true);
            Assert.IsTrue(_target[0]);
            _target.Set(0, false);
            Assert.IsFalse(_target[0]);
        }

        [Test]
        public void VerifyLockedIndexCannotBeChanged()
        {
			
            _target.Lock(0, true);
			Assert.Throws<ArgumentException>(() => _target.Set(0, true));
        }

        [Test]
        public void VerifyLockCanBeReleased()
        {
            _target.Lock(0, true);
            _target.Lock(0, false);
            _target.Set(0, true);
            Assert.IsTrue(_target[0]);
        }

        [Test]
        public void VerifyFindRandomUnlockedIndex()
        {
            _target.Lock(1, true);
            _target.Lock(2, true);
            _target.Lock(3, true);
            _target.Lock(4, true);
            _target.Lock(5, true);
            int randomIndex = _target.FindRandomUnlockedIndex();
            Assert.IsTrue((randomIndex == 0 || randomIndex == 6));
        }

        [Test]
        public void VerifyFindRandomUnlockedIndexWhenAllIsLocked()
        {
            _target.Lock(0, true);
            _target.Lock(1, true);
            _target.Lock(2, true);
            _target.Lock(3, true);
            _target.Lock(4, true);
            _target.Lock(5, true);
            _target.Lock(6, true);
            int randomIndex = _target.FindRandomUnlockedIndex();
            Assert.AreEqual(-1, randomIndex);
        }

        [Test]
        public void VerifyCount()
        {
            Assert.AreEqual(7, _target.Count);
        }

        [Test]
        public void VerifyGet()
        {
            Assert.IsFalse(_target.Get(0));
            _target.Set(0, true);
            Assert.IsTrue(_target.Get(0));
        }

        [Test]
        public void VerifyIsLocked()
        {
            Assert.IsFalse(_target.IsLocked(0, true));
            _target.Lock(0, true);
            Assert.IsTrue(_target.IsLocked(0, true));
            Assert.IsFalse(_target.IsLocked(0, false));
        }

        [Test]
        public void VerifyToString()
        {
            _target.Set(2, true);
			string expected = "Teleopti.Ccc.Domain.DayOffPlanning.LockableBitArray 0010000";
            Assert.AreEqual(expected, _target.ToString());
        }

        [Test]
        public void VerifyPeriodArea()
        {
            Assert.AreEqual(new MinMax<int>(0, 6), _target.PeriodArea);
            _target.PeriodArea = new MinMax<int>(3,3);
            Assert.AreEqual(new MinMax<int>(3, 3), _target.PeriodArea);
        }

        [Test]
        public void VerifyClone()
        {
            ILockableBitArray clone = (LockableBitArray) _target.Clone();
            Assert.AreEqual(_target.PeriodArea, clone.PeriodArea);
            clone.PeriodArea = new MinMax<int>(1, 1);
            Assert.AreNotEqual(_target.PeriodArea, clone.PeriodArea);
        }

        [Test]
        public void VerifyDaysOffBitArray()
        {
            BitArray bitArray = _target.DaysOffBitArray;
            Assert.AreEqual(_target.Count, bitArray.Count);
        }

        [Test]
        public void VerifyUnlockedIndexes()
        {
            IList<int> ret = _target.UnlockedIndexes;
            Assert.AreEqual(7, ret.Count);
            _target.Lock(0, true);
            ret = _target.UnlockedIndexes;
            Assert.AreEqual(6, ret.Count);
        }

        [Test]
        public void VerifyToLongBitArray()
        {
            _target.Set(0, true);
            _target.Set(6, true);
            BitArray longBitArray = _target.ToLongBitArray();
            Assert.AreEqual(21, longBitArray.Count);
            Assert.IsTrue(longBitArray[7]);
            Assert.IsTrue(longBitArray[13]);
            _target = new LockableBitArray(7, true, true);
            _target.Set(0, true);
            _target.Set(6, true);
            longBitArray = _target.ToLongBitArray();
            Assert.AreEqual(7, longBitArray.Count);
            Assert.IsTrue(longBitArray[0]);
            Assert.IsTrue(longBitArray[6]);
            _target = new LockableBitArray(7, false, false);
            longBitArray = _target.ToLongBitArray();
            Assert.AreEqual(21, longBitArray.Count);
            _target = new LockableBitArray(14, true, false);
            longBitArray = _target.ToLongBitArray();
            Assert.AreEqual(21, longBitArray.Count);
            _target = new LockableBitArray(14, false, true);
            longBitArray = _target.ToLongBitArray();
            Assert.AreEqual(21, longBitArray.Count);
        }

		[Test]
		public void ShouldNotHasSameDayOffsDifferentElements()
		{
			var target1 = new LockableBitArray(1, false, false);
			var target2 = new LockableBitArray(1, false, false);
			target1.SetAll(true);
			target2.SetAll(false);
			target1.HasSameDayOffs(target2).Should().Be.False();
		}

		[Test]
		public void ShouldHasSameDayOffsSameElements()
		{
			var target1 = new LockableBitArray(1, false, false);
			var target2 = new LockableBitArray(1, false, false);
			target1.SetAll(true);
			target2.SetAll(true);
			target1.HasSameDayOffs(target2).Should().Be.True();
		}

		[Test]
		public void ShouldNotHasSameDayOffsIfDifferentLengths()
		{
			var target1 = new LockableBitArray(2, false, false);
			var target2 = new LockableBitArray(1, false, false);
			target1.HasSameDayOffs(target2).Should().Be.False();
		}
	}
}