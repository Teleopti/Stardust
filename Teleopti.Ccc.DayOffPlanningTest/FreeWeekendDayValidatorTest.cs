﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class FreeWeekendDayValidatorTest
    {
        #region Variables

        private FreeWeekendDayValidator _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private MinMax<int> _periodRange;
        private BitArray _periodDays;

        #endregion

        [SetUp]
        public void Setup()
        {
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));
            _periodDays = createBitArrayForTest();
        }

        [Test]
        public void VerifyValidation()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

            _periodRange = new MinMax<int>(12, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(3, 3), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(_periodDays, 13));

        }

        [Test]
        public void VerifyIsValidWithBitArray()
        {
            BitArray bitArray = createBitArrayForTest();

            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 1), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(bitArray, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(bitArray, 13));

            _periodRange = new MinMax<int>(12, 19);
            _target = new FreeWeekendDayValidator(new MinMax<int>(2, 2), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(bitArray, 13));

            _target = new FreeWeekendDayValidator(new MinMax<int>(3, 3), _officialWeekendDays, _periodRange);
            Assert.IsFalse(_target.IsValid(bitArray, 13));
        }

        #region Week before and after

        [Test]
        public void VerifyFirstValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 13));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyLastValidDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 19));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyInvalidDayBeforeFirstDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 12));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyInvalidDayAfterLastDayInPeriod()
        {
            _periodRange = new MinMax<int>(13, 19);

            _target = new FreeWeekendDayValidator(new MinMax<int>(1, 7), _officialWeekendDays, _periodRange);
            Assert.IsTrue(_target.IsValid(_periodDays, 20));
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
}