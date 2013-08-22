using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
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

        [SetUp]
        public void Setup()
        {
            _person1 = PersonFactory.CreatePerson("John");
            _person2 = PersonFactory.CreatePerson("Ashley");
            _person3 = PersonFactory.CreatePerson("Bill");
            _person4 = PersonFactory.CreatePerson("Stan");

            _shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent>();
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, "Morning", 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, "Morning", 3));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, "Morning", 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, "Morning", 4));

            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, "Day", 7));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, "Day", 6));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, "Day", 2));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, "Day", 5));

            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person1, "Late", 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person2, "Late", 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person3, "Late", 1));
            _shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(_person4, "Late", 1));
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
            Assert.IsTrue(shiftFairnessList[0].ShiftCategoryName.Equals( "Morning") );
            Assert.IsTrue(shiftFairnessList[1].ShiftCategoryName.Equals( "Day") );
            Assert.IsTrue(shiftFairnessList[2].ShiftCategoryName.Equals( "Late") );
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
