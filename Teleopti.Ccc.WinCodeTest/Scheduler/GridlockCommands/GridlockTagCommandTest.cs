using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockTagCommandTest
    {
        private GridlockTagCommand _target;
        private IScheduleDayTagExtractor _scheduleDayTagExtractor;
        private MockRepository _mocks;
        private IGridlockManager _gridlockManager;
        private IList<IScheduleDay> _extractedDays;
        private IScheduleTag _scheduleTag;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDayTagExtractor = _mocks.StrictMock<IScheduleDayTagExtractor>();
            _gridlockManager = _mocks.StrictMock<IGridlockManager>();
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            _extractedDays = new List<IScheduleDay>();
            _target = new GridlockTagCommand(_gridlockManager, _scheduleDayTagExtractor, _scheduleTag);
        }

        [Test]
        public void ShouldLockDaysWithTag()
        {
            using (_mocks.Record())
            {
                Expect.Call(_scheduleDayTagExtractor.Tag(_scheduleTag)).Return(_extractedDays);
                Expect.Call(() => _gridlockManager.AddLock(_extractedDays, LockType.Normal));
            }

            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
