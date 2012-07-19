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
    internal class OverviewView : ScheduleViewBase
    {
        public OverviewView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(grid)
        {
            Presenter = new OverviewPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag);
            grid.Name = "SummaryView";
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
        #region Draw cell

        //draw cell
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
                    
                    if (scheduleDay.PersonAssignmentCollection().Count > 0)
                    {
                        if (significantPart == SchedulePartView.MainShift)
                        {
                            IPersonAssignment pa = scheduleDay.AssignmentHighZOrder();

                            if (pa != null)
                            {
                                if (pa.MainShift != null)
                                {
                                    if (ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleDay) == DisplayMode.BeginsToday || ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleDay) == DisplayMode.BeginsAndEndsToday)
                                    {
                                        color2 = pa.MainShift.ShiftCategory.DisplayColor;
                                        symbol = "|";
                                    }
                                }
                            }
                        }
                    }

                    var absenceCollection = scheduleDay.PersonAbsenceCollection();
                    if (absenceCollection.Count > 0)
                    {
                        if (significantPart == SchedulePartView.FullDayAbsence)
                        {
                            color2 = absenceCollection[0].Layer.Payload.ConfidentialDisplayColor(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly);
                            symbol = "X";
                        }
                    }

                    if (scheduleDay.PersonDayOffCollection().Count > 0)
                    {
                        if (significantPart == SchedulePartView.DayOff)
                        {
                            color2 = Color.LightGray;
                            symbol = "-";
                        }
                    }

                    if (!String.IsNullOrEmpty(symbol))
                    {
                        DrawRectangle(e, symbol, color2);
                    }

                    AddMarkersToCell(e, scheduleDay, significantPart);
                }
            }
        }

        //draw rectangle
        private void DrawRectangle(GridDrawCellEventArgs e, string symbol, Color color)
        {
            if (symbol == "-")
            {
                Rectangle rect = new Rectangle(e.Bounds.Location, e.Bounds.Size);
                rect.Inflate(-2, -2);
                IScheduleDay schedulePart = e.Style.CellValue as IScheduleDay;

                using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, schedulePart.PersonDayOffCollection()[0].DayOff.DisplayColor, Color.LightGray))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
            else
            {
                using (Brush lBrush = new SolidBrush(color))
                {
                    Rectangle rect = new Rectangle(e.Bounds.Location, e.Bounds.Size);
                    rect.Inflate(-2, -2);

                    SizeF stringWidth = e.Graphics.MeasureString(symbol, CellFontSmall);
                    Point point =
                        new Point(rect.X - (int)stringWidth.Width / 2 + rect.Width / 2,
                                  rect.Y - (int)stringWidth.Height / 2 + rect.Height / 2);

                    if(symbol == "X")
                        e.Graphics.FillRectangle(lBrush, rect);
                    else
                        GridHelper.FillRoundedRectangle(e.Graphics, rect, 2, lBrush, -1);

                    e.Graphics.DrawString(symbol, CellFontSmall, Brushes.Black, point);
                }
            }
        }

        #endregion
    }
}
