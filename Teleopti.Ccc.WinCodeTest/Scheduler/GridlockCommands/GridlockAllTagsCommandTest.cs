using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.GridlockCommands;
using  Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class GridlockAllTagsCommandTest
    {
        private GridlockAllTagsCommand _target;
        private IScheduleDayTagExtractor _scheduleDayTagExtractor;
        private MockRepository _mocks;
        private IGridlockManager _gridlockManager;
        private IList<IScheduleDay> _extractedDays;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDayTagExtractor = _mocks.StrictMock<IScheduleDayTagExtractor>();
            _gridlockManager = _mocks.StrictMock<IGridlockManager>();
            _extractedDays = new List<IScheduleDay>();
            _target = new GridlockAllTagsCommand(_gridlockManager, _scheduleDayTagExtractor);
        }

        [Test]
        public void ShouldLockAllTaggedDays()
        {
            using(_mocks.Record())
            {
                Expect.Call(_scheduleDayTagExtractor.All()).Return(_extractedDays);
                Expect.Call(() => _gridlockManager.AddLock(_extractedDays, LockType.Normal));
            }

            using(_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
