using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class PeriodView : ScheduleViewBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PeriodView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer)
            : base(grid)
        {
            Presenter = new PeriodPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer)
                            {VisibleWeeks = 8};
            grid.Name = "PeriodView";
        }

		

        protected override int CellWidth()
        {
            return 30;
        }

        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            e.Size = 22;
            e.Handled = true;
        }
      
        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.RowIndex > 1 && e.ColIndex > ColHeaders)
            {
                var scheduleRange = e.Style.CellValue as IScheduleDay;
                if (scheduleRange != null)
                {
                    var significantPart = scheduleRange.SignificantPartForDisplay();

                    if(significantPart == SchedulePartView.MainShift)
                        drawAssignmentFromSchedule(e, scheduleRange);
                    if(significantPart == SchedulePartView.FullDayAbsence)
                        drawAbsenceFromSchedule(e, scheduleRange);
					if (significantPart == SchedulePartView.ContractDayOff)
						drawAbsenceAndDayOff(e, scheduleRange);
                    if(significantPart == SchedulePartView.DayOff)
                        drawDayOffFromSchedule(e, scheduleRange);
                    AddMarkersToCell(e, scheduleRange, significantPart);
                }
            }
        }

    	private void drawAbsenceAndDayOff(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
    	{
			IAbsence absence = SignificantAbsence(scheduleDay);
			String shortName = absence.ConfidentialDescription_DONTUSE(scheduleDay.Person).ShortName;
			SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
			Point point = new Point(e.Bounds.X - (int)stringWidth.Width / 2 + e.Bounds.Width / 2, e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);
			using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, absence.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person)))
			{
				GridHelper.FillRoundedRectangle(e.Graphics, e.Bounds, 1, brush, -4);
				e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
			}
    	}

        private void drawAssignmentFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            IPersonAssignment pa = scheduleRange.PersonAssignment();

            if (pa != null)
            {
                DisplayMode displayMode = ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleRange);
                if (displayMode == DisplayMode.BeginsToday || displayMode == DisplayMode.BeginsAndEndsToday)
                {
                    if (pa.ShiftCategory != null)
                    {
                        Color c = pa.ShiftCategory.DisplayColor;
                        String shortName = pa.ShiftCategory.Description.ShortName;
                        SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
                        Point point =
                            new Point(e.Bounds.X - (int)stringWidth.Width / 2 + e.Bounds.Width / 2,
                                      e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);

                        using (LinearGradientBrush lBrush = new LinearGradientBrush(e.Bounds, Color.WhiteSmoke, c, 90, false))
                        {
                            GridHelper.FillRoundedRectangle(e.Graphics, e.Bounds, 4, lBrush , -4);
                            e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
                        }
                    }
                }
            }
        }

		private void drawAbsenceFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
		{
			IAbsence absence = SignificantAbsence(scheduleDay);
			String shortName = absence.ConfidentialDescription_DONTUSE(scheduleDay.Person).ShortName;
			SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
			Point point = new Point(e.Bounds.X - (int) stringWidth.Width/2 + e.Bounds.Width/2,
			                        e.Bounds.Y - (int) stringWidth.Height/2 + e.Bounds.Height/2);

			using (SolidBrush brush = new SolidBrush(absence.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person)))
			{
				GridHelper.FillRoundedRectangle(e.Graphics, e.Bounds, 1, brush, -4);
				e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
			}
		}

        private void drawDayOffFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            if (!scheduleDay.HasDayOff()) 
				return;

            var dayOff = scheduleDay.PersonAssignment().DayOff();
            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, e.Bounds.Width - 8, e.Bounds.Height - 8);
            string shortName = dayOff.Description.ShortName;
            SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
            Point point = new Point(e.Bounds.X - (int)stringWidth.Width / 2 + e.Bounds.Width / 2, e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);

            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, dayOff.DisplayColor, Color.LightGray))
            {
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
            }
        }

    }
}
