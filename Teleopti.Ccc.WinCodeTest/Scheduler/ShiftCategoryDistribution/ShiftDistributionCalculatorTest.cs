using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftDistributionCalculatorTest
    {
        private List<ShiftCategoryStructure> _mappedShiftCategoriesList;
        private IShiftCategory _morning;
        private DateOnly _today;
        private IPerson _person1;
        private IPerson _person2;
        private DateOnly _tomorrow;
        private IPerson _person3;
        private IShiftCategory _day;
        private IShiftCategory _late;

        [SetUp]
        public void Setup()
        {
            _morning = new ShiftCategory("Morning");
            _day  = new ShiftCategory("Day");
            _late  = new ShiftCategory("Late");

            _today = DateOnly.Today;
            _tomorrow = DateOnly.Today.AddDays(1);

            _person1 = PersonFactory.CreatePerson("Person1");
            _person2 = PersonFactory.CreatePerson("Person2");
            _person3 = PersonFactory.CreatePerson("Person3");

            _mappedShiftCategoriesList = new List<ShiftCategoryStructure>();
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning,_today,_person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning,_today,_person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning,_tomorrow,_person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning,_tomorrow,_person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning,_tomorrow,_person3));
            
            
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day,_today,_person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day,_tomorrow ,_person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day,_tomorrow ,_person2));
            
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_late,_today,_person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_late,_tomorrow ,_person1));


        }

        [Test]
        public void TestReturnListCount()
        {
            var shiftDistributionList = ShiftDistributionCalculator.Extract(_mappedShiftCategoriesList);
            Assert.AreEqual(shiftDistributionList.Count,6 );
        }

        [Test]
        public void TestIfMorningShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftDistributionCalculator.Extract(_mappedShiftCategoriesList);
            Assert.AreEqual(shiftDistributionList[0].ShiftCategory,_morning);
            Assert.AreEqual(shiftDistributionList[1].ShiftCategory,_morning);
            Assert.AreEqual(shiftDistributionList[0].DateOnly,_today  );
            Assert.AreEqual(shiftDistributionList[1].DateOnly,_tomorrow   );
            Assert.AreEqual(shiftDistributionList[0].Count,2 );
            Assert.AreEqual(shiftDistributionList[1].Count,3 );
        }

        [Test]
        public void TestIfDayShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftDistributionCalculator.Extract(_mappedShiftCategoriesList);
            Assert.AreEqual(shiftDistributionList[2].ShiftCategory, _day );
            Assert.AreEqual(shiftDistributionList[3].ShiftCategory, _day);
            Assert.AreEqual(shiftDistributionList[2].DateOnly, _today);
            Assert.AreEqual(shiftDistributionList[3].DateOnly, _tomorrow);
            Assert.AreEqual(shiftDistributionList[2].Count, 1);
            Assert.AreEqual(shiftDistributionList[3].Count, 2);
        }

        [Test]
        public void TestIfLateShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftDistributionCalculator.Extract(_mappedShiftCategoriesList);
            Assert.AreEqual(shiftDistributionList[4].ShiftCategory, _late );
            Assert.AreEqual(shiftDistributionList[5].ShiftCategory, _late );
            Assert.AreEqual(shiftDistributionList[4].DateOnly, _today);
            Assert.AreEqual(shiftDistributionList[5].DateOnly, _tomorrow);
            Assert.AreEqual(shiftDistributionList[4].Count, 1);
            Assert.AreEqual(shiftDistributionList[5].Count, 1);
        }
    }
}
