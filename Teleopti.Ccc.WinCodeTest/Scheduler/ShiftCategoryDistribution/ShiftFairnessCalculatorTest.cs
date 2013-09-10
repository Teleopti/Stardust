using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftFairnessCalculatorTest
    {
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IPerson _person4;
        private List<ShiftCategoryPerAgent> _shiftCategoryPerAgentList;
        private ShiftCategory _morning;
        private ShiftCategory _day;
        private ShiftCategory _late;

        [SetUp]
        public void Setup()
        {
            _person1 = PersonFactory.CreatePerson("John");
            _person2 = PersonFactory.CreatePerson("Ashley");
            _person3 = PersonFactory.CreatePerson("Bill");
            _person4 = PersonFactory.CreatePerson("Stan");

            _morning = new ShiftCategory("Morning");
            _day = new ShiftCategory("Day");
            _late = new ShiftCategory("Late");

            _shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent>();
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, _morning, 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, _morning, 3));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, _morning, 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, _morning, 4));

            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, _day, 7));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, _day, 6));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, _day, 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, _day, 5));

            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, _late, 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, _late, 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, _late, 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, _late, 1));
        }

        [Test]
        public void VerifyThatShiftCategoryCountIsPopulated()
        {
            Assert.AreEqual(ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList).Count(), 3);
        }

        [Test]
        public void VerifyThatShiftCategoryIsPopulated()
        {
            var shiftFairnessList = ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList);
            Assert.AreEqual(shiftFairnessList[0].ShiftCategory,_morning  );
            Assert.AreEqual(shiftFairnessList[1].ShiftCategory,_day  );
            Assert.AreEqual(shiftFairnessList[2].ShiftCategory,_late  );
        }

        [Test]
        public void VerifyThatMinValueIsPopulated()
        {
            var shiftFairnessList = ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList);
            Assert.AreEqual(shiftFairnessList[0].MinimumValue ,2);
            Assert.AreEqual(shiftFairnessList[1].MinimumValue ,2);
            Assert.AreEqual(shiftFairnessList[2].MinimumValue ,1);
        }

        [Test]
        public void VerifyThatMaxValueIsPopulated()
        {
            var shiftFairnessList = ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList); ;
            Assert.AreEqual(shiftFairnessList[0].MaximumValue, 4);
            Assert.AreEqual(shiftFairnessList[1].MaximumValue, 7);
            Assert.AreEqual(shiftFairnessList[2].MaximumValue, 1);
        }

        [Test]
        public void VerifyThatAverageValueIsPopulated()
        {
            var shiftFairnessList = ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList); ;
            Assert.AreEqual(shiftFairnessList[0].AverageValue , 2.75);
            Assert.AreEqual(shiftFairnessList[1].AverageValue, 5);
            Assert.AreEqual(shiftFairnessList[2].AverageValue, 1);
        }

        [Test]
        public void VerifyThatSdValueIsPopulated()
        {
            var shiftFairnessList = ShiftFairnessCalculator.GetShiftFairness(_shiftCategoryPerAgentList); ;
            Assert.AreEqual(Math.Round(shiftFairnessList[0].StandardDeviationValue,2), 0.83);
            Assert.AreEqual(Math.Round(shiftFairnessList[1].StandardDeviationValue,2), 1.87);
            Assert.AreEqual(Math.Round(shiftFairnessList[2].StandardDeviationValue,2), 0.00);
        }
    }

    
}
