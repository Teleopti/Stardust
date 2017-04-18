using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllPreferencesMustHaveCommandTest
    {
        private GridlockAllPreferencesMustHaveCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayPreferenceRestrictionExtractor _scheduleDayPreferenceRestrictionExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDays = new List<IScheduleDay>();
            _gridScheduleExtractor = _mock.StrictMock<IGridSchedulesExtractor>();
            _gridlockManager = _mock.StrictMock<IGridlockManager>();
            _scheduleDayPreferenceRestrictionExtractor = _mock.StrictMock<IScheduleDayPreferenceRestrictionExtractor>();
            _target = new GridlockAllPreferencesMustHaveCommand(_gridScheduleExtractor, _scheduleDayPreferenceRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockAllMustHavePreferences()
        {
            using (_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
                Expect.Call(_scheduleDayPreferenceRestrictionExtractor.AllRestrictedDaysMustHave(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(() => _gridlockManager.AddLock(_scheduleDays, LockType.Normal));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }
    }
}
