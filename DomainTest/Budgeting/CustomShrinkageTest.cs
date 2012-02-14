using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class CustomShrinkageTest
    {
        [Test]
        public void ShouldContainNameWhenConstructed()
        {
            var sickString = "Sick";
            var tiredString = "tired";
            var sick = new CustomShrinkage(sickString);
            Assert.AreEqual(sickString, sick.ShrinkageName);
            sick.ShrinkageName = tiredString;
            Assert.AreEqual(tiredString, sick.ShrinkageName);
        }

        [Test]
        public void ShouldAddAbsence()
        {
            var vacation = new CustomShrinkage("Vacation");
            var holiday = AbsenceFactory.CreateAbsence("Holiday");
            vacation.AddAbsence(holiday);
            Assert.AreEqual(holiday, vacation.BudgetAbsenceCollection.First());
        }

        [Test]
        public void ShouldRemoveAllAbsences()
        {
            var vacation = new CustomShrinkage("Vacation");
            var holiday = AbsenceFactory.CreateAbsence("Holiday");
            var semester = AbsenceFactory.CreateAbsence("Semester");
            vacation.AddAbsence(holiday);
            vacation.AddAbsence(semester);
            Assert.AreEqual(2, vacation.BudgetAbsenceCollection.Count());
            vacation.RemoveAllAbsences();
            Assert.AreEqual(0, vacation.BudgetAbsenceCollection.Count());
        }

        [Test]
        public void ShouldGetRightOrderIndex()
        {
            var shrinkage1 = new CustomShrinkage("shrinkage1");
            var shrinkage2 = new CustomShrinkage("shrinkage2");
            var bg = new BudgetGroup();
            bg.AddCustomShrinkage(shrinkage1);
            bg.AddCustomShrinkage(shrinkage2);
            Assert.AreEqual(0, shrinkage1.OrderIndex);
            Assert.AreEqual(1, shrinkage2.OrderIndex);
        }
    }
}
