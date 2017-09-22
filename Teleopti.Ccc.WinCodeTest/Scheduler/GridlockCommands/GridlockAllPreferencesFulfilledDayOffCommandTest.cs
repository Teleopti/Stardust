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
    public class GridlockAllPreferencesFulfilledDayOffCommandTest
    {
        private GridlockAllPreferencesFulfilledDayOffCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayPreferenceRestrictionExtractor _scheduleDayPreferenceRestrictionExtractor;
        private ICheckerRestriction _restrictionChecker;
        private IScheduleDay _day1;
        private IScheduleDay _day2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _day1 = _mock.StrictMock<IScheduleDay>();
            _day2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay> { _day1, _day2 };
            _gridScheduleExtractor = _mock.StrictMock<IGridSchedulesExtractor>();
            _gridlockManager = _mock.StrictMock<IGridlockManager>();
            _scheduleDayPreferenceRestrictionExtractor = _mock.StrictMock<IScheduleDayPreferenceRestrictionExtractor>();
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
            _target = new GridlockAllPreferencesFulfilledDayOffCommand(_gridScheduleExtractor, _restrictionChecker, _scheduleDayPreferenceRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockFulfilledDayOffPreferences()
        {
            using (_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
                Expect.Call(_scheduleDayPreferenceRestrictionExtractor.RestrictionFulfilledDayOff(_restrictionChecker, _day1)).Return(_day1);
                Expect.Call(_scheduleDayPreferenceRestrictionExtractor.RestrictionFulfilledDayOff(_restrictionChecker, _day2)).Return(null);
                Expect.Call(() => _gridlockManager.AddLock(new List<IScheduleDay> { _day1 }, LockType.Normal));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }  
    }
}
