using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftCategoryPerAgentTest
    {
        [Test]
        public void VerifyThatPersonIsPopulated()
        {
            var person = PersonFactory.CreatePerson("person1");
            var shiftCategory = new ShiftCategory("shiftcategory");
            const int count = 2;
            var shiftCategoryPerAgent = new ShiftCategoryPerAgent(person, shiftCategory.Description.Name, count);
            Assert.AreEqual(shiftCategoryPerAgent.Person.Name ,person.Name );
        }

        [Test]
        public void VerifyThatCounIsPopulated()
        {
            var person = PersonFactory.CreatePerson("person1");
            var shiftCategory = new ShiftCategory("shiftcategory");
            const int count = 2;
            var shiftCategoryPerAgent = new ShiftCategoryPerAgent(person, shiftCategory.Description.Name, count);
            Assert.AreEqual(shiftCategoryPerAgent.Count, 2);
        }

        [Test]
        public void VerifyThatShiftCategoryIsPopulated()
        {
            var person = PersonFactory.CreatePerson("person1");
            var shiftCategory = new ShiftCategory("shiftcategory");
            const int count = 2;
            var shiftCategoryPerAgent = new ShiftCategoryPerAgent(person, shiftCategory.Description.Name, count);
            Assert.AreEqual(shiftCategoryPerAgent.ShiftCategoryName, shiftCategory.Description.Name );
        }
    }
}
