using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftDistributionTest
    {
        [Test]
        public void VerifyThatShiftCategoryIsPopulated()
        {
            DateOnly dateOnly = DateOnly.Today;
            IShiftCategory shiftCategory = new ShiftCategory("test1");
            const int count = 2;
            var shiftDistribution = new ShiftDistribution(dateOnly, shiftCategory.Description.Name, count);

            Assert.AreEqual(shiftDistribution.ShiftCategoryName, shiftCategory.Description.Name);
        }

        [Test]
        public void VerifyThatDateIsPopulated()
        {
            DateOnly dateOnly = DateOnly.Today;
            IShiftCategory shiftCategory = new ShiftCategory("test1");
            const int count = 2;
            var shiftDistribution = new ShiftDistribution(dateOnly, shiftCategory.Description.Name, count);

            Assert.AreEqual(shiftDistribution.DateOnly, DateOnly.Today);
        }

        [Test]
        public void VerifyThatCountIsPopulated()
        {
            DateOnly dateOnly = DateOnly.Today;
            IShiftCategory shiftCategory = new ShiftCategory("test1");
            const int count = 2;
            var shiftDistribution = new ShiftDistribution(dateOnly, shiftCategory.Description.Name, count);

            Assert.AreEqual(shiftDistribution.Count, 2);
        }
    }
}
