﻿using System;
using System.Collections;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class FreeWeekendValidatorSwedenTest
    {
        #region Variables

        private FreeWeekendValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyIsValidWithBitArray()
        {            
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendValidator(new MinMax<int>(0, 0), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 13));

            _target = new FreeWeekendValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));

            _periodRange = new MinMax<int>(12, 19);

            _target = new FreeWeekendValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 13));

            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));
        }


        #region Week before and after

        [Test]
        public void VerifyFirstValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 13));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyLastValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 19));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 11));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 20));

        }

        #endregion

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,

                                    false,
                                    false,
                                    true,
                                    false,
                                    false,
                                    false,
                                    true,

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }

    }

    [TestFixture]
    public class FreeWeekendValidatorUSTest
    {
        #region Variables

        private IDayOffLegalStateValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("en-US"));
            _periodRange = new MinMax<int>(7, 13);
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidationWeekends()
        {
            _target = new FreeWeekendValidator(new MinMax<int>(0, 0), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 10));
        }

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    true,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,
                                    false,

                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true
                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }

    [TestFixture]
    public class FreeWeekendValidatorArabicTest
    {
        #region Variables

        private IDayOffLegalStateValidator _target;
        private MinMax<int> _periodRange;
        private BitArray _bitArray;
        private IOfficialWeekendDays _officialWeekendDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("ar-AE"));
            _periodRange = new MinMax<int>(7, 13);
            _bitArray = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidationWeekends()
        {
            _target = new FreeWeekendValidator(
                new MinMax<int>(1, 1),
                _officialWeekendDays,
                _periodRange);
            Assert.IsFalse(_target.IsValid(_bitArray, 11));

            _target = new FreeWeekendValidator(
                new MinMax<int>(0, 0),
                _officialWeekendDays,
                _periodRange);
            Assert.IsTrue(_target.IsValid(_bitArray, 11));
        }

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true,

                                    true,
                                    false,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,

                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false
                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }
}