using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IScheduleViewBase : IViewBase, IAddScheduleLayers
    {
        int ColHeaders { get; }
        int RowHeaders { get; }
        void SetCellBackTextAndBackColor(GridQueryCellInfoEventArgs e, DateOnly dateTime, bool backColor, bool textColor, IScheduleDay schedulePart);
        string DayHeaderTooltipText(GridStyleInfo gridStyle,DateOnly currentDate);
        bool IsRightToLeft { get; }
        bool IsOverviewColumnsHidden { get; }
        IHandleBusinessRuleResponse HandleBusinessRuleResponse { get; }
        void InvalidateSelectedRow(IScheduleDay schedulePart);
        void OnPasteCompleted();
        GridControl TheGrid { get; }
		
        IList<IScheduleDay> CurrentColumnSelectedSchedules();
		
        IList<IScheduleDay> SelectedSchedules();
		
        void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period);
		
        void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo);

        ICollection<DateOnly> AllSelectedDates();
	    ICollection<DateOnly> AllSelectedDates(IEnumerable<IScheduleDay> selectedSchedules);
    }
}
