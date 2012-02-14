using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class ShiftCategoryLimitationCombinerTest
    {
        private ShiftCategoryLimitationCombiner _target;
        private IShiftCategory _shiftCatDay;
        private IShiftCategory _shiftCatNight;

        [SetUp]
        public void Setup()
        {
            _shiftCatDay = ShiftCategoryFactory.CreateShiftCategory("Day");
            _shiftCatNight = ShiftCategoryFactory.CreateShiftCategory("Night");
            _target = new ShiftCategoryLimitationCombiner();
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanCombineSameCategoryWithSamePeriodAndSameMax()
        {
            IShiftCategoryLimitation limitation1 = new ShiftCategoryLimitation(_shiftCatDay);
            IShiftCategoryLimitation limitation2 = new ShiftCategoryLimitation(_shiftCatDay);
            limitation1.Weekly = true;
            limitation2.Weekly = true;
            limitation1.MaxNumberOf = 2;
            limitation2.MaxNumberOf = 2;
            IList<IShiftCategoryLimitation> shiftCategoryLimitations = new List<IShiftCategoryLimitation>{limitation1, limitation2};
            IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> returnLimitations = _target.Combine(shiftCategoryLimitations);

            Assert.AreEqual(1, returnLimitations.Count);
            Assert.AreEqual(_shiftCatDay, returnLimitations[_shiftCatDay].ShiftCategory);
            Assert.AreEqual(true, returnLimitations[_shiftCatDay].Weekly);
            Assert.AreEqual(2, returnLimitations[_shiftCatDay].MaxNumberOf);
        }
        [Test]
        public void CanCombineSameCategoryWithDifferentPeriodAndDifferentMax()
        {
            IShiftCategoryLimitation limitation1 = new ShiftCategoryLimitation(_shiftCatDay);
            IShiftCategoryLimitation limitation2 = new ShiftCategoryLimitation(_shiftCatDay);
            limitation1.Weekly = true;
            limitation2.Weekly = false;
            limitation1.MaxNumberOf = 2;
            limitation2.MaxNumberOf = 4;
            IList<IShiftCategoryLimitation> shiftCategoryLimitations = new List<IShiftCategoryLimitation> { limitation1, limitation2 };
            IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> returnLimitations = _target.Combine(shiftCategoryLimitations);

            Assert.AreEqual(1, returnLimitations.Count);
            Assert.AreEqual(_shiftCatDay, returnLimitations[_shiftCatDay].ShiftCategory);
            Assert.AreEqual(null, returnLimitations[_shiftCatDay].Weekly);
            Assert.AreEqual(null, returnLimitations[_shiftCatDay].MaxNumberOf);
        }
        [Test]
        public void CanCombineDifferentCategoryWithDifferentPeriodAndDifferentMax()
        {
            IShiftCategoryLimitation limitation1 = new ShiftCategoryLimitation(_shiftCatDay);
            IShiftCategoryLimitation limitation2 = new ShiftCategoryLimitation(_shiftCatNight);
            limitation1.Weekly = true;
            limitation2.Weekly = false;
            limitation1.MaxNumberOf = 2;
            limitation2.MaxNumberOf = 4;
            IList<IShiftCategoryLimitation> shiftCategoryLimitations = new List<IShiftCategoryLimitation> { limitation1, limitation2 };
            IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> returnLimitations = _target.Combine(shiftCategoryLimitations);

            Assert.AreEqual(2, returnLimitations.Count);
            Assert.AreEqual(_shiftCatDay, returnLimitations[_shiftCatDay].ShiftCategory);
            Assert.AreEqual(_shiftCatNight, returnLimitations[_shiftCatNight].ShiftCategory);
            Assert.AreEqual(true, returnLimitations[_shiftCatDay].Weekly);
            Assert.AreEqual(false, returnLimitations[_shiftCatNight].Weekly);
            Assert.AreEqual(2, returnLimitations[_shiftCatDay].MaxNumberOf);
            Assert.AreEqual(4, returnLimitations[_shiftCatNight].MaxNumberOf);
        }
    }
}
