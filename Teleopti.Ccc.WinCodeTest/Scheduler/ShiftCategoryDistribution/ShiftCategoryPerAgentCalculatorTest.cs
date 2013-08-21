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
    public class ShiftCategoryPerAgentCalculatorTest
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
            _day = new ShiftCategory("Day");
            _late = new ShiftCategory("Late");

            _today = DateOnly.Today;
            _tomorrow = DateOnly.Today.AddDays(1);

            _person1 = PersonFactory.CreatePerson("John");
            _person2 = PersonFactory.CreatePerson("Ashley");
            _person3 = PersonFactory.CreatePerson("Bill");

            _mappedShiftCategoriesList = new List<ShiftCategoryStructure>();
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _today, _person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _tomorrow , _person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day, _tomorrow.AddDays(1), _person1));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_late, _tomorrow.AddDays(2), _person1));

            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _today, _person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _tomorrow.AddDays(1), _person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _tomorrow.AddDays(2), _person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day, _tomorrow.AddDays(3), _person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day, _tomorrow.AddDays(4), _person2));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_late, _tomorrow.AddDays(5), _person2));

            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _today, _person3));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_morning, _tomorrow.AddDays(1), _person3));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day, _tomorrow.AddDays(3), _person3));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_day, _tomorrow.AddDays(4), _person3));
            _mappedShiftCategoriesList.Add(new ShiftCategoryStructure(_late, _tomorrow.AddDays(5), _person3));


        }

        [Test]
        public void TestReturnListCount()
        {
            var shiftDistributionList = ShiftCategoryPerAgentCalculator.Extract(_mappedShiftCategoriesList);
            Assert.AreEqual(shiftDistributionList.Count, 9);
        }

        [Test]
        public void TestIfMorningShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftCategoryPerAgentCalculator.Extract(_mappedShiftCategoriesList);
            Assert.IsTrue(shiftDistributionList[0].ShiftCategoryName.Equals(_morning.Description.Name));
            Assert.IsTrue(shiftDistributionList[1].ShiftCategoryName.Equals(_day.Description.Name));
            Assert.IsTrue(shiftDistributionList[2].ShiftCategoryName.Equals(_late.Description.Name));
            Assert.AreEqual(shiftDistributionList[0].Person, _person1);
            Assert.AreEqual(shiftDistributionList[1].Person, _person1);
            Assert.AreEqual(shiftDistributionList[2].Person, _person1);
            Assert.AreEqual(shiftDistributionList[0].Count, 2);
            Assert.AreEqual(shiftDistributionList[1].Count, 1);
            Assert.AreEqual(shiftDistributionList[2].Count, 1);
        }

        [Test]
        public void TestIfDayShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftCategoryPerAgentCalculator.Extract(_mappedShiftCategoriesList);
            Assert.IsTrue(shiftDistributionList[3].ShiftCategoryName.Equals(_morning.Description.Name));
            Assert.IsTrue(shiftDistributionList[4].ShiftCategoryName.Equals(_day.Description.Name));
            Assert.IsTrue(shiftDistributionList[5].ShiftCategoryName.Equals(_late.Description.Name));
            Assert.AreEqual(shiftDistributionList[3].Person, _person2);
            Assert.AreEqual(shiftDistributionList[4].Person, _person2);
            Assert.AreEqual(shiftDistributionList[5].Person, _person2);
            Assert.AreEqual(shiftDistributionList[3].Count, 3);
            Assert.AreEqual(shiftDistributionList[4].Count, 2);
            Assert.AreEqual(shiftDistributionList[5].Count, 1);
        }

        [Test]
        public void TestIfLateShiftsAreCorrect()
        {
            var shiftDistributionList = ShiftCategoryPerAgentCalculator.Extract(_mappedShiftCategoriesList);
            Assert.IsTrue(shiftDistributionList[6].ShiftCategoryName.Equals(_morning.Description.Name));
            Assert.IsTrue(shiftDistributionList[7].ShiftCategoryName.Equals(_day.Description.Name));
            Assert.IsTrue(shiftDistributionList[8].ShiftCategoryName.Equals(_late.Description.Name));
            Assert.AreEqual(shiftDistributionList[6].Person, _person3);
            Assert.AreEqual(shiftDistributionList[7].Person, _person3);
            Assert.AreEqual(shiftDistributionList[8].Person, _person3);
            Assert.AreEqual(shiftDistributionList[6].Count, 2);
            Assert.AreEqual(shiftDistributionList[7].Count, 2);
            Assert.AreEqual(shiftDistributionList[8].Count, 1);
        }
    }
}
