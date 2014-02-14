﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.GridlockCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllRotationsFulfilledCommandTest
    {
        private GridlockAllRotationsFulfilledCommand _target;
        private IList<IScheduleDay> _scheduleDays;
        private IGridSchedulesExtractor _gridScheduleExtractor;
        private MockRepository _mock;
        private IGridlockManager _gridlockManager;
        private IScheduleDayRotationRestrictionExtractor _scheduleDayRotationRestrictionExtractor;
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
            _scheduleDayRotationRestrictionExtractor = _mock.StrictMock<IScheduleDayRotationRestrictionExtractor>();
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
            _target = new GridlockAllRotationsFulfilledCommand(_gridScheduleExtractor, _restrictionChecker, _scheduleDayRotationRestrictionExtractor, _gridlockManager);
        }

        [Test]
        public void ShouldLockFulfilledRotations()
        {
            using (_mock.Record())
            {
                Expect.Call(_gridScheduleExtractor.Extract()).Return(_scheduleDays);
				Expect.Call(_scheduleDayRotationRestrictionExtractor.RestrictionFulfilled(_restrictionChecker, _day1)).Return(_day1);
                Expect.Call(_scheduleDayRotationRestrictionExtractor.RestrictionFulfilled(_restrictionChecker, _day2)).Return(null);
                Expect.Call(() => _gridlockManager.AddLock(new List<IScheduleDay> { _day1 }, LockType.Normal));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }  
    }
}
