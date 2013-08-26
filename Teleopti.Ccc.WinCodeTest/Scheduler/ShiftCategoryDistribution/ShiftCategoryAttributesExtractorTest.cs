using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftCategoryAttributesExtractorTest
    {
        private IShiftCategoryAttributesExtractor _target;
        private ShiftCategoryStructure _shiftCategoryStructure1;
        private ShiftCategoryStructure _shiftCategoryStructure2;
        private ShiftCategoryStructure _shiftCategoryStructure3;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private ShiftCategory _sc1;
        private ShiftCategory _sc2;

        [SetUp]
        public void Setup()
        {
            _target = new ShiftCategoryAttributesExtractor();
            _person1 = PersonFactory.CreatePerson("person1");
            _person2 = PersonFactory.CreatePerson("person2");
            _person3 = PersonFactory.CreatePerson("person3");

            _sc1 = new ShiftCategory("sc1");
            _sc2 = new ShiftCategory("sc2");
        }

        [Test]
        public void CheckIfCorrectListShiftCategoryCountIsMapped()
        {
            
            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1,DateOnly.Today,_person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1,DateOnly.Today,_person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2,DateOnly.Today,_person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.ShiftCategories.Count, 2);
        }

        [Test]
        public void CheckIfCorrectListWithShiftCategoryIsMapped()
        {

            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2, DateOnly.Today, _person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.ShiftCategories[0], _sc1);
            Assert.AreEqual(_target.ShiftCategories[1], _sc2  );
        }

        [Test]
        public void CheckIfCorrectListWithPersonCountIsMapped()
        {
            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2, DateOnly.Today, _person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.PersonInvolved.Count, 3);
        }

        [Test]
        public void CheckIfCorrectListWithPersonIsMapped()
        {
            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2, DateOnly.Today, _person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.PersonInvolved[0].Name , _person1.Name );
            Assert.AreEqual(_target.PersonInvolved[1].Name , _person2.Name );
            Assert.AreEqual(_target.PersonInvolved[2].Name , _person3.Name );
        }

        [Test]
        public void CheckIfCorrectListWithDateCountIsMapped()
        {
            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2, DateOnly.Today, _person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.Dates .Count, 1);
        }

        [Test]
        public void CheckIfCorrectListWithDateIsMapped()
        {
            _shiftCategoryStructure1 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(_sc1, DateOnly.Today, _person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(_sc2, DateOnly.Today, _person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.Dates[0], DateOnly.Today);
        }
    }

   
}
