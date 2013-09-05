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
    public class DetailView : ScheduleViewBase
    {
        private const int ABSENCEHEIGHT = 6;
        private const int ABSENCESPACE = 2;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public DetailView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(grid)
        {
            Presenter = new DetailPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag);
            grid.Name = "DetailView";
        }

        #region Grid stuff

        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            e.Size = 35;
            e.Handled = true;
        }

        protected override int CellWidth()
        {
            return 60;
        }

        #endregion

        #region Draw Cell content

        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.RowIndex > 1 && e.ColIndex > ColHeaders)
            {
                var scheduleRange = e.Style.CellValue as IScheduleDay;
                if (scheduleRange != null)
                {
                    drawAssignmentFromSchedule(e, scheduleRange);
                    drawAbsenceFromSchedule(e, scheduleRange);
                    drawDayOffFromSchedule(e, scheduleRange);

                    AddMarkersToCell(e, scheduleRange, scheduleRange.SignificantPart());
                }
            }
        }

        //draw assignment
        private void drawAssignmentFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            IPersonAssignment pa = scheduleRange.AssignmentHighZOrder();

            if (pa != null)
            {
                Rectangle rect = DetailViewHelper.GetAssRect(e.Bounds, ViewBaseHelper.GetAssignmentDisplayMode(pa, scheduleRange));

                if (pa.MainShift != null)
                {
                    Color color = pa.MainShift.ShiftCategory.DisplayColor;
                    string shortName = pa.MainShift.ShiftCategory.Description.ShortName;
                    SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);

                    //if (totalItems > 0)
                    if(scheduleRange.PersonAbsenceCollection().Count > 0)
                    {
                        rect = new Rectangle(rect.X, rect.Y + 2, rect.Width, rect.Height / 2);
                    }
                    var point =
                        new Point(rect.X - (int)stringWidth.Width / 2 + rect.Width / 2,
                                  rect.Y - (int)stringWidth.Height / 2 + rect.Height / 2);

                    using (var brush = new SolidBrush(color))
                    {
                        GridHelper.FillRoundedRectangle(e.Graphics, rect, 4, brush, -4);
                        e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, point);
                    }
                }    
            }
        }

        //draw dayoff
        private static void drawDayOffFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleRange)
        {
            var personDayOffs = scheduleRange.PersonDayOffCollection();
            if (personDayOffs.Count == 0) return;

            Rectangle dayOffRect = DetailViewHelper.GetAbsRect(e.Bounds, DisplayMode.DayOff);
            dayOffRect.Offset(0, e.Bounds.Y);

            using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, personDayOffs[0].DayOff.DisplayColor, Color.LightGray))
            {
                e.Graphics.FillRectangle(brush, dayOffRect);
            }
        }

        //draw absences
        private static void drawAbsenceFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            IVisualLayerCollection layerCollection = scheduleDay.ProjectionService().CreateProjection();

            var absenceCollection = scheduleDay.PersonAbsenceCollection();
            for (int i = 0; i < absenceCollection.Count; i++)
            {
                IPersonAbsence pa = absenceCollection[i];
                Rectangle absRect = DetailViewHelper.GetAbsRect(e.Bounds, ViewBaseHelper.GetAbsenceDisplayMode(pa, scheduleDay, layerCollection));
                absRect.Offset(0, e.Bounds.Y + e.Bounds.Height - ((ABSENCEHEIGHT + ABSENCESPACE) * (absenceCollection.Count - i)));

                using (var brush = new SolidBrush(pa.Layer.Payload.ConfidentialDisplayColor(scheduleDay.Person,scheduleDay.DateOnlyAsPeriod.DateOnly)))
                {
                    e.Graphics.FillRectangle(brush, absRect);
                }
            }
        }

        #endregion

    }
}
