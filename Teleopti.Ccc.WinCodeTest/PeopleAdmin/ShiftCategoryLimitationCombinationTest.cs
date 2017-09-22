using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class ShiftCategoryLimitationCombinationTest
    {
        private ShiftCategoryLimitationCombination _target;
        private IShiftCategory _shiftCategory;

        [SetUp]
        public void Setup()
        {
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");
            _target = new ShiftCategoryLimitationCombination(_shiftCategory);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsFalse(_target.Limit.Value);
            Assert.IsFalse(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(_shiftCategory, _target.ShiftCategory);
            Assert.AreEqual(-1, _target.MaxNumberOf);
        }

        [Test]
        public void VerifyClear()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;
            _target.CombineWithNoLimitation(_shiftCategory);
            _target.CombineLimitations(limitation);
            _target.Clear();
            Assert.IsFalse(_target.Limit.Value);
            Assert.IsFalse(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(_shiftCategory, _target.ShiftCategory);
            Assert.AreEqual(-1, _target.MaxNumberOf);
        }

        [Test]
        public void VerifyEmptyFirst()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;
            _target.CombineWithNoLimitation(_shiftCategory);
            _target.CombineLimitations(limitation);

            Assert.IsFalse(_target.Limit.HasValue);
            Assert.IsTrue(_target.Weekly.Value);
            Assert.AreEqual(2, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifyFirstTimeCombine()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            Assert.IsTrue(_target.Limit.Value);
            Assert.IsTrue(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(2, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifySameShiftCategoryWhenCombine()
        {
            Guid guid = Guid.NewGuid();
            IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("yy");
            category.SetId(guid);
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(category);
            Assert.Throws<ArgumentException>(() => _target.CombineLimitations(limitation));
        }

        [Test]
        public void VerifyInitializedCombineWhenAllSame()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            _target.CombineLimitations(limitation);

            Assert.IsTrue(_target.Limit.Value);
            Assert.IsTrue(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(2, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifyInitializedCombineWhenNumberDiffers()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            limitation.MaxNumberOf = 1;
            _target.CombineLimitations(limitation);

            Assert.IsTrue(_target.Limit.Value);
            Assert.IsTrue(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.IsFalse(_target.MaxNumberOf.HasValue);
        }

        [Test]
        public void VerifyInitializedCombineWhenWeekDiffers()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            limitation.Weekly = false;
            _target.CombineLimitations(limitation);

            Assert.IsTrue(_target.Limit.Value);
            Assert.IsFalse(_target.Weekly.HasValue);
            Assert.IsFalse(_target.Period.HasValue);
            Assert.AreEqual(2, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifyNoLimitation()
        {
            _target.CombineWithNoLimitation(_shiftCategory);

            Assert.IsFalse(_target.Limit.Value);
            Assert.IsFalse(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(-1, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifyNoLimitationWhenInitialized()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            _target.CombineWithNoLimitation(_shiftCategory);

            Assert.IsFalse(_target.Limit.HasValue);
            Assert.IsTrue(_target.Weekly.Value);
            Assert.IsFalse(_target.Period.Value);
            Assert.AreEqual(2, _target.MaxNumberOf.Value);
        }

        [Test]
        public void VerifySameShiftCategoryWhenCombineWithNoLimitation()
        {
            Assert.Throws<ArgumentException>(() => _target.CombineWithNoLimitation(ShiftCategoryFactory.CreateShiftCategory("yy")));
        }

        [Test]
        public void VerifyInitWithPeriod()
        {
            IShiftCategoryLimitation limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = false;

            _target.CombineLimitations(limitation);

            Assert.IsTrue(_target.Period.Value);
            Assert.IsFalse(_target.Weekly.Value);

            limitation = new ShiftCategoryLimitation(_shiftCategory);
            limitation.MaxNumberOf = 2;
            limitation.Weekly = true;

            _target.CombineLimitations(limitation);
            Assert.IsFalse(_target.Period.HasValue);
            Assert.IsFalse(_target.Weekly.HasValue);
        }
    }
}