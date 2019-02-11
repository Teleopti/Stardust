using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class DayViewNew : ScheduleViewBase
    {
        private const int timeLineHeaderIndex = 1;
        private const int timeLineHeigth = 30;
        private const int alphaFactor = 80;
        private readonly DayPresenterNew _presenter;

        public DayViewNew(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer)
            : base(grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            _presenter = new DayPresenterNew(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,
                scheduleDayChangeCallback, new DayPresenterScaleCalculator(), defaultScheduleTag, undoRedoContainer);
            Presenter = _presenter;
            grid.Name = "DayView";
        }

        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            //timeline
            if (e.Index == timeLineHeaderIndex)
            {
                e.Size = timeLineHeigth;
                e.Handled = true;
            }
            else
            {
                e.Size = 22;
                e.Handled = true;
            }
        }

        internal override void QueryColWidth(object sender, GridRowColSizeEventArgs e)
        {
            base.QueryColWidth(sender, e);
            if (e.Index >= (int)ColumnType.StartScheduleColumns)
            {
                e.Size = ViewGrid.ClientSize.Width - CalculateColHeadersWidth() - 5;
                e.Handled = true;
            }
        }

        public override void SetSelectedDateLocal(DateOnly dateOnly)
        {
            _presenter.SelectDate(dateOnly);
            _presenter.SortCommand.Execute(dateOnly);
			base.SetSelectedDateLocal(dateOnly);
			ViewGrid.Refresh();
        }

        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.ColIndex < (int)ColumnType.StartScheduleColumns)
            {
                return;
            }

            if (e.RowIndex == timeLineHeaderIndex)
            {
                drawTimeLine(e);
            }
            if (e.RowIndex > 1 && e.ColIndex > ColHeaders)
            {
				drawHourMarkers(e);
                var scheduleDay = e.Style.CellValue as IScheduleDay;
                if (scheduleDay != null)
                {
                    var significantPart = scheduleDay.SignificantPartForDisplay();

                    if (significantPart == SchedulePartView.MainShift || significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.Overtime)
                    {
						drawAssignmentFromSchedule(e, scheduleDay);	
                    }
                    else
                    {
                        if (significantPart == SchedulePartView.DayOff)
                        {
                            drawDayOffFromSchedule(e, scheduleDay);
                        }
                        else
                        {
                            if (significantPart == SchedulePartView.ContractDayOff)
                            {
                                drawCoveredDayOffFromSchedule(e, scheduleDay);
                            }
                            else
                            {
                                drawEmptyDay(e, scheduleDay);
                            }
                        }
                        
                    }

                    if (significantPart == SchedulePartView.DayOff)

                        drawDayOffFromSchedule(e, scheduleDay);
                    if (significantPart == SchedulePartView.ContractDayOff)
                        drawCoveredDayOffFromSchedule(e, scheduleDay);
					
                    AddMarkersToCell(e, scheduleDay, significantPart);
                }
            }
        }

        private void drawEmptyDay(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
            IPerson person = scheduleDay.Person;
            DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IScheduleDay yesterDay = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(-1));
            IScheduleDay tomorrow = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(1));

            drawYesterday(e, person, yesterDay, pixelConverter);
            drawTomorrow(e, person, pixelConverter, tomorrow);
        }

        private void drawTimeLine(GridDrawCellEventArgs e)
        {
            var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
            IList<DateTimePeriod> hours = new List<DateTimePeriod>(_presenter.ScalePeriod.AffectedHourCollection());
            var lastLeftText = -1000;

            for (int i = 1; i < hours.Count - 1; i++)
            {
                DateTimePeriod hour = hours[i];
                int position =
                    (int)Math.Round(pixelConverter.PositionFromDateTime(hour.StartDateTime, IsRightToLeft)) +
                    e.Bounds.X;
                var startUpper = new Point(position, e.Bounds.Y);
                var endUpper = new Point(position, e.Bounds.Y + 5);

                var startLower = new Point(position, e.Bounds.Bottom);
                var endLower = new Point(position, e.Bounds.Bottom - 5);

                var startBottomCell = new Point(e.Bounds.X, e.Bounds.Bottom - 1);
                var endBottomCell = new Point(e.Bounds.Right, e.Bounds.Bottom - 1);

                var startLeftCell = new Point(e.Bounds.Right - 1, e.Bounds.Y);
                var endLeftCell = new Point(e.Bounds.Right - 1, e.Bounds.Bottom - 1);

                e.Graphics.DrawLine(Pens.Black, startUpper, endUpper);
                e.Graphics.DrawLine(Pens.Black, startLower, endLower);
                e.Graphics.DrawLine(Pens.Black, startBottomCell, endBottomCell);
                e.Graphics.DrawLine(Pens.Black, startLeftCell, endLeftCell);


                string timeString = hour.StartDateTime.ToShortTimeString();
                SizeF stringWidth = e.Graphics.MeasureString(timeString, TimelineFont);
                var startTimeText = new Point((int)(startUpper.X - (stringWidth.Width / 2)), endUpper.Y + 2);

                if(startTimeText.X > lastLeftText + 5)
                {
                    e.Graphics.DrawString(timeString, TimelineFont, Brushes.Gray, startTimeText);
                    lastLeftText = startTimeText.X + (int)stringWidth.Width;
                }
                   
            }
        }

        private void drawAssignmentFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
            IPerson person = scheduleDay.Person;
            DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IScheduleDay yesterDay = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(-1));
            IScheduleDay tomorrow = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(1));

            drawYesterday(e, person, yesterDay, pixelConverter);

            foreach (IVisualLayer visualLayer in scheduleDay.ProjectionService().CreateProjection())
            {
                DateTimePeriod local = toLocalUtcPeriod(visualLayer.Period, TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
                int startPixel = (int)Math.Round(pixelConverter.PositionFromDateTime(local.StartDateTime, IsRightToLeft)) + e.Bounds.X;
                int endPixel = (int)Math.Round(pixelConverter.PositionFromDateTime(local.EndDateTime, IsRightToLeft)) + e.Bounds.X;

				if(visualLayer.DefinitionSet != null && visualLayer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
					drawOvertimeRect(e, visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person), startPixel, endPixel);
				else
					drawRect(e, visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person), startPixel, endPixel);
            }

            drawTomorrow(e, person, pixelConverter, tomorrow);
        }

		private void drawHourMarkers(GridDrawCellEventArgs e)
		{
			var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
			IList<DateTimePeriod> hours = new List<DateTimePeriod>(_presenter.ScalePeriod.AffectedHourCollection());

			for (int i = 1; i < hours.Count - 1; i++)
			{
				DateTimePeriod hour = hours[i];
				int position =
					(int) Math.Round(pixelConverter.PositionFromDateTime(hour.StartDateTime, IsRightToLeft)) +
					e.Bounds.X;
				var startUpper = new Point(position, e.Bounds.Y);
				var startLower = new Point(position, e.Bounds.Bottom);
	

				e.Graphics.DrawLine(Pens.LightGray, startUpper, startLower);

			}
		}

        private void drawDayOffFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
            IPerson person = scheduleDay.Person;
            DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IScheduleDay yesterDay = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(-1));
            IScheduleDay tomorrow = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(1));

            drawYesterday(e, person, yesterDay, pixelConverter);


            var baseDate = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, 0,
                                                 DateTimeKind.Utc);
            var local1 = new DateTimePeriod(baseDate.AddHours(8), baseDate.AddHours(16));
            int startPixel1 =
                (int) Math.Round(pixelConverter.PositionFromDateTime(local1.StartDateTime, IsRightToLeft)) +
                e.Bounds.X;
            int endPixel1 = (int) Math.Round(pixelConverter.PositionFromDateTime(local1.EndDateTime, IsRightToLeft)) + e.Bounds.X;
			if (!scheduleDay.HasDayOff())
				return;

            var personDayOff = scheduleDay.PersonAssignment().DayOff();
            
            string shortName = personDayOff.Description.ShortName;
            SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
            var point = new Point(startPixel1 + ((endPixel1-startPixel1)/2), e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);
            drawDayOffRect(e, personDayOff.DisplayColor, startPixel1, endPixel1, shortName, point);


            drawTomorrow(e, person, pixelConverter, tomorrow);
        }

        private void drawCoveredDayOffFromSchedule(GridDrawCellEventArgs e, IScheduleDay scheduleDay)
        {
            var pixelConverter = new LengthToTimeCalculator(_presenter.ScalePeriod, e.Bounds.Width);
            IPerson person = scheduleDay.Person;
            DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IScheduleDay yesterday = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(-1));
            IScheduleDay tomorrow = _presenter.SchedulerState.Schedules[person].ScheduledDay(dateOnly.AddDays(1));

            drawYesterday(e, person, yesterday, pixelConverter);


            var baseDate = DateTime.SpecifyKind(dateOnly.Date, DateTimeKind.Utc);
            var local1 = new DateTimePeriod(baseDate.AddHours(8), baseDate.AddHours(16));
            int startPixel1 =
                (int)Math.Round(pixelConverter.PositionFromDateTime(local1.StartDateTime, IsRightToLeft)) +
                e.Bounds.X;
            int endPixel1 = (int)Math.Round(pixelConverter.PositionFromDateTime(local1.EndDateTime, IsRightToLeft)) +
                            e.Bounds.X;

            IAbsence absence = SignificantAbsence(scheduleDay);

        	string shortName = "";
			if (scheduleDay.HasDayOff())
			{
				shortName = scheduleDay.PersonAssignment().DayOff().Description.ShortName;
			}

			SizeF stringWidth = e.Graphics.MeasureString(shortName, CellFontBig);
			var point = new Point(startPixel1 + ((endPixel1 - startPixel1) / 2), e.Bounds.Y - (int)stringWidth.Height / 2 + e.Bounds.Height / 2);
            drawContractDayOffRect(e, absence.ConfidentialDisplayColor_DONTUSE(scheduleDay.Person), startPixel1, endPixel1, shortName, point);

            drawTomorrow(e, person, pixelConverter, tomorrow);
        }

        private void drawTomorrow(GridDrawCellEventArgs e, IPerson person, LengthToTimeCalculator pixelConverter, IScheduleDay tomorrow)
        {
            foreach (IVisualLayer visualLayer in tomorrow.ProjectionService().CreateProjection())
            {
				DateTimePeriod local = toLocalUtcPeriod(visualLayer.Period, TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
                int startPixel =
                    (int) Math.Round(pixelConverter.PositionFromDateTime(local.StartDateTime, IsRightToLeft)) +
                    e.Bounds.X;
                int endPixel = (int) Math.Round(pixelConverter.PositionFromDateTime(local.EndDateTime, IsRightToLeft)) +
                               e.Bounds.X;

                if (startPixel > e.Bounds.X + e.Bounds.Width)
                    continue;

                Color color = Color.FromArgb(alphaFactor, visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person));
                drawRect(e, color, startPixel, endPixel);
            }
        }

        private void drawYesterday(GridDrawCellEventArgs e, IPerson person, IScheduleDay yesterday, LengthToTimeCalculator pixelConverter)
        {
            foreach (IVisualLayer visualLayer in yesterday.ProjectionService().CreateProjection())
            {
				DateTimePeriod local = toLocalUtcPeriod(visualLayer.Period, TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
                int startPixel =
                    (int) Math.Round(pixelConverter.PositionFromDateTime(local.StartDateTime, IsRightToLeft)) +
                    e.Bounds.X;
                int endPixel = (int) Math.Round(pixelConverter.PositionFromDateTime(local.EndDateTime, IsRightToLeft)) +
                               e.Bounds.X;
                if (endPixel > e.Bounds.X && startPixel < e.Bounds.X)
                    startPixel = e.Bounds.X;
                if (endPixel <= e.Bounds.X)
                    continue;

                Color color = Color.FromArgb(alphaFactor, visualLayer.Payload.ConfidentialDisplayColor_DONTUSE(person));
                drawRect(e, color, startPixel, endPixel);
            }
        }

        private static void drawRect(GridDrawCellEventArgs e, Color color, int startPixel, int endPixel)
        {
            var rect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height - 4);
            var upperRect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height / 2 - 4);
            var lowerRect = new Rectangle(rect.X, rect.Y + rect.Height / 2 - 4, rect.Width, rect.Height / 2 + 4);

            if (rect.Width < 1)
                return;
            
            using (LinearGradientBrush lBrush = GridHelper.GetGradientBrush(upperRect, color))
                {
                    e.Graphics.FillRectangle(lBrush, upperRect);
                }

            using(var sBrush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(sBrush, lowerRect);
            } 
        }

		private void drawOvertimeRect(GridDrawCellEventArgs e, Color color, int startPixel, int endPixel)
	    {
			var rect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height - 4);
			var upperRect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height / 2 - 4);
			var lowerRect = new Rectangle(rect.X, rect.Y + rect.Height / 2 - 4, rect.Width, rect.Height / 2 + 4);
			
			if (rect.Width < 1) return;

			

			using (LinearGradientBrush lBrush = GridHelper.GetGradientBrush(upperRect, color))
			{
				e.Graphics.FillRectangle(lBrush, upperRect);
			}

			var foreColor = Color.Orange;
			if (color.ToArgb() == Color.Orange.ToArgb()) foreColor = Color.DarkOrange;

			using (var brush = new HatchBrush(HatchStyle.WideUpwardDiagonal, foreColor, color))
			{
				e.Graphics.FillRectangle(brush, lowerRect);
			}
	    }

        private void drawDayOffRect(GridDrawCellEventArgs e, Color color, int startPixel, int endPixel, string text, Point startPoint)
        {
            var rect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height - 4);
            if (rect.Width < 1)
                return;

            using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, color, Color.LightGray))
            {
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawString(text, CellFontBig, Brushes.Black, startPoint);
            }
        }

        private void drawContractDayOffRect(GridDrawCellEventArgs e, Color color, int startPixel, int endPixel, string shortName, Point startPoint)
        {
            var rect = new Rectangle(startPixel, e.Bounds.Y + 2, endPixel - startPixel, e.Bounds.Height - 4);
            if (rect.Width < 1)
                return;

            using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, color))
            {
                e.Graphics.FillRectangle(brush, rect);
				e.Graphics.DrawString(shortName, CellFontBig, Brushes.Black, startPoint);
            }
        }

        static DateTimePeriod toLocalUtcPeriod(DateTimePeriod utcPeriod, TimeZoneInfo timeZone)
        {
            DateTime localStart = utcPeriod.StartDateTimeLocal(timeZone);
            DateTime localEnd = utcPeriod.EndDateTimeLocal(timeZone);

            // what if the summertime 3:05 becomes 2:05 on the day of timechange, so the endtime get lower that starttime?
            // solution1: we are not going to show it
            if(localStart > localEnd)
                localStart = localEnd;
            
            return new DateTimePeriod(new DateTime(localStart.Year, localStart.Month, localStart.Day, localStart.Hour, localStart.Minute, 0, 0, DateTimeKind.Utc), new DateTime(localEnd.Year, localEnd.Month, localEnd.Day, localEnd.Hour, localEnd.Minute, 0, 0, DateTimeKind.Utc));
        }
    }


}