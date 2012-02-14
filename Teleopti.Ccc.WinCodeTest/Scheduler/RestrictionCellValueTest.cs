using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class RestrictionCellValueTest
    {
        private RestrictionCellValue _target;
        private IScheduleDay _part;
        private MockRepository _mock;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _part = _mock.StrictMock<IScheduleDay>();
            _target = new RestrictionCellValue(_part, true, true, true, true, true);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target.SchedulePart);
            Assert.IsTrue(_target.UseAvailability);
            Assert.IsTrue(_target.UsePreference);
            Assert.IsTrue(_target.UseRotation);
            Assert.IsTrue(_target.UseSchedule);
            Assert.IsTrue(_target.UseStudentAvailability);
        }
    }
}