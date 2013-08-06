using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class PeriodView : ScheduleViewBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PeriodView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(grid)
        {
            Presenter = new PeriodPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag)
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
                        DrawAssignmentFromSchedule(e, scheduleRange);
                    if(significantPart == SchedulePartView.FullDayAbsence)
                        DrawAbsenceFromSchedule(e, scheduleRange);
					if (significantPart == SchedulePartView.ContractDayOff)
						DrawAbsenceAndDayOff(e, scheduleRange);
                    if(significantPart == SchedulePartView.DayOff)
                        DrawDayOffFromSchedule(e, scheduleRange);
                    AddMarkersToCell(e, scheduleRange, significantPart);
                }
            }
        }

    	private void DrawAbsenceAndDayOff(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
    	{
			IAbsence absence = SignificantAbsence(scheduleDay);
			String shortName = absence.ConfidentialDescription(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly).ShortName;
			SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
			Point point = new Point(e.Bounds.X - (int)stringWidth.Width / 2 + e.Bounds.Width / 2, e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);
			using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, absence.ConfidentialDisplayColor(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly)))
			{
				GridHelper.FillRoundedRectangle(e.Graphics, e.Bounds, 1, brush, -4);
				e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
			}
    	}

        private void DrawAssignmentFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
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

		private void DrawAbsenceFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
		{
			IAbsence absence = SignificantAbsence(scheduleDay);
			String shortName = absence.ConfidentialDescription(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly).ShortName;
			SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
			Point point = new Point(e.Bounds.X - (int) stringWidth.Width/2 + e.Bounds.Width/2,
			                        e.Bounds.Y - (int) stringWidth.Height/2 + e.Bounds.Height/2);

			using (SolidBrush brush = new SolidBrush(absence.ConfidentialDisplayColor(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly)))
			{
				GridHelper.FillRoundedRectangle(e.Graphics, e.Bounds, 1, brush, -4);
				e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
			}
		}

        private void DrawDayOffFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            var personDayOffs = scheduleRange.PersonDayOffCollection();
            if (personDayOffs.Count == 0) return;

            IPersonDayOff personDayOff = personDayOffs[0];
            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, e.Bounds.Width - 8, e.Bounds.Height - 8);
            string shortName = personDayOff.DayOff.Description.ShortName;
            SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
            Point point = new Point(e.Bounds.X - (int)stringWidth.Width / 2 + e.Bounds.Width / 2, e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);

            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, personDayOff.DayOff.DisplayColor, Color.LightGray))
            {
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
            }
        }

    }
}
