using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllStudentAvailabilityAvailableCommandTest
    {
        private GridlockAllStudentAvailabilityAvailableCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayStudentAvailabilityRestrictionExtractor _scheduleDayStudentAvailabilityRestrictionExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDays = new List<IScheduleDay>();
            _gridScheduleExtractor = _mock.StrictMock<IGridSchedulesExtractor>();
            _gridlockManager = _mock.StrictMock<IGridlockManager>();
            _scheduleDayStudentAvailabilityRestrictionExtractor = _mock.StrictMock<IScheduleDayStudentAvailabilityRestrictionExtractor>();
            _target = new GridlockAllStudentAvailabilityAvailableCommand(_gridScheduleExtractor, _scheduleDayStudentAvailabilityRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockAllRotations()
        {
            using (_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
                Expect.Call(_scheduleDayStudentAvailabilityRestrictionExtractor.AllAvailable(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(() => _gridlockManager.AddLock(_scheduleDays, LockType.Normal));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }
    }
}
