using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DateOnlyDtoTest
    {
        private DateOnlyDto _target;
        private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _dateOnly = DateOnly.Today;
			_target = new DateOnlyDto { DateTime = _dateOnly.Date };
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_dateOnly.Date.Ticks, _target.DateTime.Ticks);

            DateOnly dateOnly = new DateOnly(2010, 8, 1);
            _target = new DateOnlyDto(dateOnly.Year, dateOnly.Month, dateOnly.Day);
            Assert.AreEqual(dateOnly.Date.Ticks, _target.DateTime.Ticks);
        }
    }
}