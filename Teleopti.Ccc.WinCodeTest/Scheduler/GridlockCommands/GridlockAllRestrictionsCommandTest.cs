using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllRestrictionsCommandTest
    {
        private GridlockAllRestrictionsCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayRestrictionExtractor _scheduleDayRestrictionExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDays = new List<IScheduleDay>();
            _gridScheduleExtractor = _mock.StrictMock<IGridSchedulesExtractor>();
            _gridlockManager = _mock.StrictMock<IGridlockManager>();
            _scheduleDayRestrictionExtractor = _mock.StrictMock<IScheduleDayRestrictionExtractor>();
            _target = new GridlockAllRestrictionsCommand(_gridScheduleExtractor, _scheduleDayRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockAllRestrictions()
        {
            using(_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
                Expect.Call(_scheduleDayRestrictionExtractor.AllRestrictedDays(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(() => _gridlockManager.AddLock(_scheduleDays, LockType.Normal));
            }

            using(_mock.Playback())
            {
                _target.Execute();
            }
        }
    }
}
