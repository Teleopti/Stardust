using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ShiftCategoryLimitationTest
    {
        private IShiftCategoryLimitation _target;
        private IShiftCategory _shiftCategory;

        [SetUp]
        public void Setup()
        {
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");
            _target = new ShiftCategoryLimitation(_shiftCategory);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_target.ShiftCategory, _shiftCategory);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShiftCategoryCannotBeNull()
        {
            _target.ShiftCategory = null;
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShiftCategoryCannotBeNull2()
        {
            _target = new ShiftCategoryLimitation(null);
        }
    }
}