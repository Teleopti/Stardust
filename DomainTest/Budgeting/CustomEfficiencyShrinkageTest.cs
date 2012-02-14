using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class CustomEfficiencyShrinkageTest
    {
        [Test]
        public void ShouldContainNameWhenConstructed()
        {
            var sickString = "Sick";
            var tiredString = "tired";
            var sick = new CustomEfficiencyShrinkage(sickString);
            Assert.AreEqual(sickString, sick.ShrinkageName);
            sick.ShrinkageName = tiredString;
            Assert.AreEqual(tiredString, sick.ShrinkageName);
        }

        [Test]
        public void ShouldGetRightOrderIndex()
        {
            var shrinkage1 = new CustomEfficiencyShrinkage("shrinkage1");
            var shrinkage2 = new CustomEfficiencyShrinkage("shrinkage2");
            var bg = new BudgetGroup();
            bg.AddCustomEfficiencyShrinkage(shrinkage1);
            bg.AddCustomEfficiencyShrinkage(shrinkage2);
            Assert.AreEqual(0, shrinkage1.OrderIndex);
            Assert.AreEqual(1, shrinkage2.OrderIndex);
        }
    }
}
