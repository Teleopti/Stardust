using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
    internal class CutPasteHandlerFactory : IDisposable
    {
        private SchedulingScreen _owner;
        private Func<ScheduleViewBase> _scheduleViewFunction;
        private Action<DeleteOption> _startDeleteAction;
        private Action _checkPastePermissionsAction;
        private Action<PasteOptions> _pasteFromClipboardAction;
        private Action _enablePasteOperationAction;
		private readonly bool _useRightToLeft;

		public CutPasteHandlerFactory(SchedulingScreen owner, Func<ScheduleViewBase> scheduleViewFunction, Action<DeleteOption> startDeleteAction, Action checkPastePermissionsAction, Action<PasteOptions> pasteFromClipboardAction, Action enablePasteOperationAction, bool useRightToLeft)
        {
            _owner = owner;
            _scheduleViewFunction = scheduleViewFunction;
            _startDeleteAction = startDeleteAction;
            _checkPastePermissionsAction = checkPastePermissionsAction;
            _pasteFromClipboardAction = pasteFromClipboardAction;
            _enablePasteOperationAction = enablePasteOperationAction;
			_useRightToLeft = useRightToLeft;
		}

        public ICutPasteHandler For(ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.SchedulerGridMain:
                    return new ScheduleGridCutPasteHandler(_owner,_scheduleViewFunction,_startDeleteAction, _checkPastePermissionsAction, _pasteFromClipboardAction, _enablePasteOperationAction, _useRightToLeft);
                case ControlType.ShiftEditor:
                    return new EditorCutPasteHandler();
                case ControlType.SchedulerGridSkillData:
                    return new ResourceGridCutPasteHandler(_owner);
                default:
                    return new DummyCutPasteHandler();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _owner = null;
                _checkPastePermissionsAction = null;
                _enablePasteOperationAction = null;
                _pasteFromClipboardAction = null;
                _scheduleViewFunction = null;
                _startDeleteAction = null;
            }
        }
    }
}