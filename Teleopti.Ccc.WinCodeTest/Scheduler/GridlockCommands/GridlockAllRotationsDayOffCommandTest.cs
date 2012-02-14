﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.GridlockCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllRotationsDayOffCommandTest
    {
        private GridlockAllRotationsDayOffCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayRotationRestrictionExtractor _scheduleDayRotationRestrictionExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDays = new List<IScheduleDay>();
            _gridScheduleExtractor = _mock.StrictMock<IGridSchedulesExtractor>();
            _gridlockManager = _mock.StrictMock<IGridlockManager>();
            _scheduleDayRotationRestrictionExtractor = _mock.StrictMock<IScheduleDayRotationRestrictionExtractor>();
            _target = new GridlockAllRotationsDayOffCommand(_gridScheduleExtractor, _scheduleDayRotationRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockAllRotationsDayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
                Expect.Call(_scheduleDayRotationRestrictionExtractor.AllRestrictedDayOffs(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(() => _gridlockManager.AddLock(_scheduleDays, LockType.Normal));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }
    }
}
