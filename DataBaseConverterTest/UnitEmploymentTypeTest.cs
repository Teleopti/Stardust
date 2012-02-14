using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    [TestFixture]
    public class UnitEmploymentTypeTest
    {
        private UnitEmploymentType _target;
        private Unit _unit;
        private EmploymentType _empType;

        [SetUp]
        public void Setup()
        {
            _unit = new Unit(7, "", false, false, null, null, false);
            _empType = new EmploymentType(9, "", 1, new TimeSpan(), new TimeSpan(), new TimeSpan());
            _target = new UnitEmploymentType(_unit, _empType);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(7, _target.Unit.Id);
            Assert.AreEqual(9, _target.EmploymentType.Id);
        }

        [Test]
        public void VerifyEqualsOther()
        {
            IList<UnitEmploymentType> list = new List<UnitEmploymentType>();
            list.Add(_target);

            Unit unit = new Unit(7, "", false, false, null, null, false);
            EmploymentType empType = new EmploymentType(9, "", 1, new TimeSpan(), new TimeSpan(), new TimeSpan());
            UnitEmploymentType t2 = new UnitEmploymentType(unit, empType);

            Assert.IsTrue(list.Contains(t2));
        }

        [Test]
        public void VerifyEqualsFalse()
        {
            IList<UnitEmploymentType> list = new List<UnitEmploymentType>();
            list.Add(_target);

            Unit unit2 = new Unit(8, "", false, false, null, null, false);
            EmploymentType empType = new EmploymentType(9, "", 1, new TimeSpan(), new TimeSpan(), new TimeSpan());
            UnitEmploymentType t2 = new UnitEmploymentType(unit2, empType);

            Assert.IsFalse(t2.Equals(_target));
        }

        [Test]
        public void VerifyEqualsObject()
        {
            Assert.IsFalse(_target.Equals((object)_unit));
            Assert.IsTrue(_target.Equals((object)_target));
        }

        [Test]
        public void VerifyGetHashCode()
        {
            Assert.AreEqual(_unit.GetHashCode() ^ _empType.GetHashCode(), _target.GetHashCode());
        }
    }
}
