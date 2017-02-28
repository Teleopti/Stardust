using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class SetTagCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IUndoRedoContainer _undoRedo;
        private readonly ISchedulePresenterBase _presenter;
        private readonly IScheduleViewBase _view;
        private readonly IScheduleTag _scheduleTag;
        private readonly IGridlockManager _lockManager;
 
        public SetTagCommand(IUndoRedoContainer undoRedo, IGridSchedulesExtractor schedulesExtractor, ISchedulePresenterBase presenter, IScheduleViewBase view, IScheduleTag scheduleTag, IGridlockManager lockManager)
        {
            _undoRedo = undoRedo;
            _presenter = presenter;
            _schedulesExtractor = schedulesExtractor;
            _view = view;
            _scheduleTag = scheduleTag;
            _lockManager = lockManager;
        }

        public void Execute()
        {
            _undoRedo.CreateBatch(Resources.UndoRedoScheduling);

            try
            {
                var scheduleDayList = _schedulesExtractor.ExtractSelected();
                var unlockedDays = _lockManager.UnlockedDays(scheduleDayList);
                
                if (!unlockedDays.IsEmpty()) _presenter.TryModify(unlockedDays, _scheduleTag);

                _undoRedo.CommitBatch();
            }
            
            catch (ValidationException validationException)
            {
                _undoRedo.RollbackBatch();
                _view.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, validationException.Message), Resources.ValidationError);
            }
        }
    }
}
