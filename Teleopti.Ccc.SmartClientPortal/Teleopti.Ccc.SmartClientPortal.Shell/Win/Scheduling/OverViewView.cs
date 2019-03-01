using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    internal class OverviewView : ScheduleViewBase
    {
    	private readonly OverviewDrawAbsenceDayOff _drawAbsenceDayOff;
    	private readonly OverviewDrawAbsence _drawAbsence;
    	private readonly OverviewDrawMainShift _drawMainShift;
    	private readonly OverviewDrawDayOff _drawDayOff;
	
        public OverviewView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
            : base(grid, timeZoneGuard)
        {
            Presenter = new OverviewPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer);
            grid.Name = "SummaryView";
			_drawAbsenceDayOff = new OverviewDrawAbsenceDayOff(CellFontSmall);
			_drawAbsence = new OverviewDrawAbsence(CellFontSmall);
			_drawMainShift = new OverviewDrawMainShift(CellFontSmall);
			_drawDayOff = new OverviewDrawDayOff();
        }

        protected override int CellWidth()
        {
            return 22;
        }
        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            e.Size = 20;
            e.Handled = true;
        }
      

        //draw cell
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Scheduling.OverviewDrawMainShift.Draw(Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs,System.String,System.Drawing.Color)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Scheduling.OverviewDrawAbsenceDayOff.Draw(Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs,System.String,System.Drawing.Color)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Scheduling.OverviewDrawAbsence.Draw(Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs,System.String,System.Drawing.Color)")]
		internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.RowIndex > 1 && e.ColIndex > ColHeaders)
            {
                IScheduleDay scheduleDay = e.Style.CellValue as IScheduleDay;
	            if (scheduleDay != null)
	            {
		            var significantPart = scheduleDay.SignificantPartForDisplay();

		            String symbol = String.Empty;
		            Color color2 = Color.White;

		            if (scheduleDay.PersonAssignment() != null)
		            {
			            if (significantPart == SchedulePartView.MainShift)
			            {
				            IPersonAssignment pa = scheduleDay.PersonAssignment();

				            if (pa != null)
				            {
					            if (pa.ShiftCategory != null)
					            {
						            if (ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleDay) == DisplayMode.BeginsToday ||
						                ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleDay) == DisplayMode.BeginsAndEndsToday)
						            {
							            color2 = pa.ShiftCategory.DisplayColor;
							            symbol = "|";
						            }
					            }
				            }
			            }
		            }

		            var absenceCollection = scheduleDay.PersonAbsenceCollection();
		            if (absenceCollection.Length > 0)
		            {
			            if (significantPart == SchedulePartView.FullDayAbsence ||
			                significantPart == SchedulePartView.ContractDayOff)
			            {
				            color2 = absenceCollection[0].Layer.Payload.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person);
				            symbol = "X";
			            }
		            }

		            if (significantPart == SchedulePartView.DayOff)
		            {
			            color2 = Color.LightGray;
			            symbol = "-";
		            }

		            if (!String.IsNullOrEmpty(symbol))
		            {
			            if (significantPart == SchedulePartView.ContractDayOff)
			            {
				            _drawAbsenceDayOff.Draw(e, symbol, color2);
			            }

			            if (significantPart == SchedulePartView.FullDayAbsence)
			            {
				            _drawAbsence.Draw(e, symbol, color2);
			            }

			            if (significantPart == SchedulePartView.MainShift)
			            {
				            _drawMainShift.Draw(e, symbol, color2);
			            }

			            if (significantPart == SchedulePartView.DayOff)
			            {
				            var color = scheduleDay.PersonAssignment().DayOff().DisplayColor;
				            _drawDayOff.Draw(e, color);
			            }
		            }

		            AddMarkersToCell(e, scheduleDay, significantPart);
	            }
            }
        }
    }
}
