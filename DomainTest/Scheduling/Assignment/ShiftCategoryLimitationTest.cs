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

        [Test]
        public void ShiftCategoryCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.ShiftCategory = null);
        }

        [Test]
        public void ShiftCategoryCannotBeNull2()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new ShiftCategoryLimitation(null));
        }
    }
}