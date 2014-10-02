using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class DeleteOptionTest
    {
        private DeleteOption _deleteOption;

        [SetUp]
        public void Setup()
        {
            _deleteOption = new DeleteOption();
        }

        [Test]
        public void VerifyProperties()
        {
            _deleteOption.Default = true;
            _deleteOption.Absence = true;
            Assert.IsTrue(_deleteOption.Absence);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.DayOff = true;
            Assert.IsTrue(_deleteOption.DayOff);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.MainShift = true;
            Assert.IsTrue(_deleteOption.MainShift);
            Assert.IsFalse(_deleteOption.Default);

			_deleteOption.Default = true;
			_deleteOption.MainShiftSpecial = true;
			Assert.IsTrue(_deleteOption.MainShiftSpecial);
			Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.PersonalShift = true;
            Assert.IsTrue(_deleteOption.PersonalShift);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.Overtime = true;
            Assert.IsTrue(_deleteOption.Overtime);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.Preference = true;
            Assert.IsTrue(_deleteOption.Preference);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            _deleteOption.StudentAvailability = true;
            Assert.IsTrue(_deleteOption.StudentAvailability);
            Assert.IsFalse(_deleteOption.Default);

            _deleteOption.Default = true;
            Assert.IsFalse(_deleteOption.Absence);
            Assert.IsFalse(_deleteOption.DayOff);
            Assert.IsFalse(_deleteOption.MainShift);
            Assert.IsFalse(_deleteOption.PersonalShift);
            Assert.IsTrue(_deleteOption.Default);

            _deleteOption.OvertimeAvailability = true;
            Assert.IsTrue(_deleteOption.OvertimeAvailability);
        }

        
    }
}
