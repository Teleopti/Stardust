using System;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class DayPresenter : SchedulePresenterBase
    {
        private const int _dayHeaderIndex = 0;                              //index of day header
        private const int _timeLineHeaderIndex = 1;                         //index of timeline

        public DayPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, GridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback,
            defaultScheduleTag, undoRedoContainer)
        {
        }

        public DateOnly GetLocalDateFromColumn(int column)
        {
            if (column<-1) throw new ArgumentOutOfRangeException("column");
            return SelectedPeriod.DateOnlyPeriod.StartDate.AddDays(column - (int)ColumnType.StartScheduleColumns);
        }

        /// <summary>
        /// Creates the day header.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: claess/moved by zoet
        /// Created date: 2007-11-15
        /// </remarks>
		protected override void CreateDayHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < (int)ColumnType.StartScheduleColumns)
            {
                return;
            }

            if (e.RowIndex == _dayHeaderIndex)
            {
                // Text date header
                var localDate = GetLocalDateFromColumn(e.ColIndex);
                e.Style.Text = localDate.ToShortDateString(CultureInfo.CurrentCulture);
                e.Style.Tag = localDate;
                e.Style.CellTipText =
                    DateHelper.WeekNumber(localDate.Date, CultureInfo.CurrentCulture)
                        .ToString(CultureInfo.CurrentCulture);

                View.SetCellBackTextAndBackColor(e, localDate, false, true, null);
            } 
            else if (e.RowIndex == _timeLineHeaderIndex)
            {
                // Time line
                e.Style.Tag = GetLocalDateFromColumn(e.ColIndex);
            }
        }

        /// <summary>
        /// Collect data for merging week headers
        /// </summary>
        public override void MergeHeaders()
        {
            //we dont have a week header in this view
        }
    }
}
