using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
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
        void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules);
        void OnPasteCompleted();
        GridControl TheGrid { get; }


        /// <summary>
        /// Gets a list with selected schedules for current column
        /// </summary>
        /// <returns></returns>
        IList<IScheduleDay> CurrentColumnSelectedSchedules();

        /// <summary>
        /// Gets a list of the current selected schedules.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-26
        /// </remarks>
        IList<IScheduleDay> SelectedSchedules();

        /// <summary>
        /// Refresh range on Person, Period
        /// </summary>
        /// <param name="person"></param>
        /// <param name="period"></param>
        void RefreshRangeForAgentPeriod(IEntity person, DateTimePeriod period);

        //void ValidateSelectedPersons(IBusinessRuleCollection businessRules);
        //void ValidateSelectedPersons(IList<IScheduleDay> parts, IBusinessRuleCollection businessRules);

        void GridClipboardPaste(PasteOptions options, IUndoRedoContainer undoRedo);

        ICollection<DateOnly> AllSelectedDates();
	    ICollection<DateOnly> AllSelectedDates(IEnumerable<IScheduleDay> selectedSchedules);
    }
}
