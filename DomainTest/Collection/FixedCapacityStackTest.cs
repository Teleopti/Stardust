using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.DomainTest.Collection
{
    /// <summary>
    /// Tests for FixedCapacityStack
    /// </summary>
    [TestFixture]
    public class FixedCapacityStackTest
    {
        private FixedCapacityStack<int> target;
        private const int stackSize = 2;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new FixedCapacityStack<int>(stackSize);
        }

        /// <summary>
        /// Determines whether this instance [can push and pop].
        /// </summary>
        [Test]
        public void CanPushAndPop()
        {
            target.Push(1);
            target.Push(2);
            Assert.AreEqual(2, target.Pop());
            Assert.AreEqual(1, target.Pop());
        }

        /// <summary>
        /// Verifies pop from empty stack is forbidden.
        /// </summary>
        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void VerifyCannotPopFromEmptyStack()
        {
            target.Pop();
        }

        /// <summary>
        /// Verifies the count property.
        /// </summary>
        [Test]
        public void VerifyCount()
        {
            target.Push(1);
            target.Push(1);
            Assert.AreEqual(2, target.Count);
        }

        /// <summary>
        /// Verifies if exceeding stack length, the oldest will be remove.
        /// </summary>
        [Test]
        public void VerifyExceededStackLengthWillRemoveOldest()
        {
            target.Push(1);
            target.Push(2);
            target.Push(3);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(3, target.Pop());
            Assert.AreEqual(2, target.Pop());
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void VerifyEnumerator()
        {
            target.Push(1);
            target.Push(2);
            target.Push(3);

            IList<int> list = target.ToList();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(3, list[0]);
            Assert.AreEqual(2, list[1]);
        }

        /// <summary>
        /// Verifies the stack length not set to zero.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyStackLengthNotSetToZero()
        {
            target = new FixedCapacityStack<int>(0);
        }

        /// <summary>
        /// Verifies the stack length is not negative.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyStackLengthNotNegative()
        {
            target = new FixedCapacityStack<int>(-17);
        }

        /// <summary>
        /// Verifies the clear method works.
        /// </summary>
        [Test]
        public void VerifyClearMethodWorks()
        {
            target.Push(1);
            target.Clear();
            Assert.AreEqual(0, target.Count);
        }
    }
}