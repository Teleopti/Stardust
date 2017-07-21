using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.UserTexts;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SetTagCommandTest
    {
        private SetTagCommand _target;
        private MockRepository _mocks;
        private IGridSchedulesExtractor _schedulesExtractor;
        private IScheduleDay _scheduleDay;
        private ISchedulePresenterBase _schedulePresenterBase;
        private IScheduleViewBase _scheduleViewBase;
        private IUndoRedoContainer _undoRedoContainer;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleTag _scheduleTag;
        private IGridlockManager _lockManager;
 

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _schedulesExtractor = _mocks.StrictMock<IGridSchedulesExtractor>();
            _scheduleViewBase = _mocks.StrictMock<IScheduleViewBase>();
            _undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _schedulePresenterBase = _mocks.StrictMock<ISchedulePresenterBase>();
            _scheduleDays = new List<IScheduleDay>{_scheduleDay};
            _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            _lockManager = _mocks.StrictMock<IGridlockManager>();
            _target = new SetTagCommand(_undoRedoContainer, _schedulesExtractor, _schedulePresenterBase, _scheduleViewBase, _scheduleTag , _lockManager);
        }

        [Test]
        public void ShouldModifyExtractedDays()
        {
            using(_mocks.Record())
            {
                Expect.Call(_schedulesExtractor.ExtractSelected()).Return(_scheduleDays);
                Expect.Call(_lockManager.UnlockedDays(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(()=>_undoRedoContainer.CreateBatch(Resources.UndoRedoScheduling));
                Expect.Call(_schedulePresenterBase.TryModify(_scheduleDays, _scheduleTag)).Return(true);
                Expect.Call(()=>_undoRedoContainer.CommitBatch());
            }

            using(_mocks.Record())
            {
                _target.Execute();
            }
        }

        [Test]
        public void ShouldShowErrorOnValidationException()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulesExtractor.ExtractSelected()).Return(_scheduleDays);
                Expect.Call(_lockManager.UnlockedDays(_scheduleDays)).Return(_scheduleDays);
                Expect.Call(() => _undoRedoContainer.CreateBatch(Resources.UndoRedoScheduling));
                Expect.Call(_schedulePresenterBase.TryModify(_scheduleDays, _scheduleTag)).Throw(new ValidationException());
                Expect.Call(() => _scheduleViewBase.ShowErrorMessage(null, null)).IgnoreArguments();
                Expect.Call(() => _undoRedoContainer.RollbackBatch());
            }

            using (_mocks.Record())
            {
                _target.Execute();
            }    
        }

        [Test]
        public void ShouldNotChangeLockedDays()
        {
            using(_mocks.Record())
            {
                Expect.Call(_schedulesExtractor.ExtractSelected()).Return(_scheduleDays);
                Expect.Call(_lockManager.UnlockedDays(_scheduleDays)).Return(new List<IScheduleDay>());
                Expect.Call(() => _undoRedoContainer.CreateBatch(Resources.UndoRedoScheduling));
                Expect.Call(() => _undoRedoContainer.CommitBatch());
            }

            using(_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
