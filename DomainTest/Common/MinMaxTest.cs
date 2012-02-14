﻿using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class MinMaxTest
    {
        private MinMax<int> target;

        [SetUp]
        public void Setup()
        {
            target = new MinMax<int>(3, 5);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(3, target.Minimum);
            Assert.AreEqual(5, target.Maximum);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyMinimumValueIsNotTooHigh()
        {
            new MinMax<int>(7, 6);
        }

        [Test]
        public void VerifyEqualsWorks()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
            Assert.IsTrue(target == target2);
            Assert.IsFalse(target != target2);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsTrue(target.Equals((object)target2));
            Assert.IsFalse(target.Equals(4));
            Assert.IsFalse(target.Equals(null));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
            Assert.AreEqual(target.GetHashCode(), target2.GetHashCode());
        }


        [Test]
        public void VerifyContainsWork()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
            Assert.IsTrue(target.Contains(3));
            Assert.IsTrue(target2.Contains(5));
            Assert.IsFalse(target2.Contains(7));
        }
    }
}
