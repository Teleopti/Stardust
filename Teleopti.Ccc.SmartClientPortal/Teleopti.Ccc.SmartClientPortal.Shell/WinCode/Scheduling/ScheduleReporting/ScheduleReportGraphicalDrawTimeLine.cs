using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportGraphicalDrawTimeline
	{
		private readonly PdfPage _page;
		private int _left;
		private readonly int _noteWidth;
		private readonly int _top;
		private const int LinePadding = 20;
		private readonly bool _rightToLeft;
		private const int HourLineLength = 4;
		private readonly CultureInfo _culture;
		private readonly PdfSolidBrush _brush;
		private const int Padding = 30;

		public ScheduleReportGraphicalDrawTimeline(CultureInfo culture, bool rightToLeft, int top, PdfPage page, int left, int noteWidth)
		{
			_left = left;
			_noteWidth = noteWidth;
			_top = top;
			_rightToLeft = rightToLeft;
			_culture = culture;
			_brush = new PdfSolidBrush(Color.DimGray);
			_page = page;
		}

		public float Draw(IList<DateOnly> dates, ISchedulingResultStateHolder stateHolder, IList<IPerson> persons, DateOnly day)
		{
            //var timelinePeriod = ConvertTimelinePeriodToCurrentDate(day, dates, stateHolder, persons);

            var timelinePeriod = TimelinePeriod(dates, stateHolder, persons, day);

			var totalHours = timelinePeriod.ElapsedTime().TotalHours;
			var pageWidth = _page.GetClientSize().Width;
			var hourWidth = (pageWidth - _left - _noteWidth) / totalHours;
			var lineTop = _top + LinePadding;
			var format = new PdfStringFormat { RightToLeft = _rightToLeft };
			var pen = new PdfPen(Color.Gray, 1);
			var hourTop = Point.Empty;
			
			
			if(_rightToLeft)
			{
				var rectWidth = pageWidth - _left - _noteWidth;
				_left = (int)(pageWidth - _left - rectWidth);
			}

			var lastLeftText = -1000;

			for (var i = 0; i <= totalHours; i++)
			{
				var position = (int)Math.Round(i * hourWidth, 0) + 0;
				hourTop = new Point(_left + position, lineTop - HourLineLength);
				var hourBottom = new Point(_left + position, lineTop + HourLineLength);

				_page.Graphics.DrawLine(pen, hourTop, hourBottom);

				string timeString;

				if (_rightToLeft)
					timeString = TimeZoneHelper.ConvertFromUtc(timelinePeriod.EndDateTime.AddHours(-i), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).ToShortTimeString();
				else
					timeString = TimeZoneHelper.ConvertFromUtc(timelinePeriod.StartDateTime.AddHours(i), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).ToShortTimeString();

				const float fontSize = 6f;
				var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, _culture);
				var stringWidth = font.MeasureString(timeString);
				var hourText = new Point((int)(_left + position - (stringWidth.Width / 2)), lineTop - 6 - (int)font.Height);

				if (hourText.X > lastLeftText + 5)
				{
					_page.Graphics.DrawString(timeString, font, _brush, hourText, format);
					lastLeftText = hourText.X + (int)stringWidth.Width;
				}
			}

			
			_page.Graphics.DrawLine(pen, _left, lineTop, hourTop.X, lineTop);

			return _top + Padding;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private DateTime MinDateTime(IList<DateOnly> dates, ISchedulingResultStateHolder stateHolder, IList<IPerson> persons, DateOnly day)
        {
            if (dates == null)
                throw new ArgumentNullException("dates");

            if (stateHolder == null)
                throw new ArgumentNullException("stateHolder");

            if (persons == null)
                throw new ArgumentNullException("persons");

            var minTime = TimeSpan.FromHours(24);
            var previousDay = false;
            var dic = stateHolder.Schedules;

            foreach (var person in persons)
            {
                foreach (var dateOnly in dates)
                {
                    var part = dic[person].ScheduledDay(dateOnly);
                    var layerCollection = part.ProjectionService().CreateProjection();

                    if (!layerCollection.HasLayers) continue;

                    var period = layerCollection.Period();

                    if (period == null) continue;

                    if(period.Value.StartDateTime.Date < dateOnly.Date)
                    {
                        if(previousDay)
                        {
                            if (period.Value.StartDateTime.TimeOfDay < minTime)
                                minTime = period.Value.StartDateTime.TimeOfDay;
                        }
                        else
                        {
                            minTime = period.Value.StartDateTime.TimeOfDay;
                        }
                        previousDay = true;
                    }
                    else
                    {
                        if(!previousDay)
                        {
                            if (period.Value.StartDateTime.TimeOfDay < minTime)
                                minTime = period.Value.StartDateTime.TimeOfDay;    
                        }
                    }
                }
            }

            minTime = minTime.Subtract(TimeSpan.FromMinutes(minTime.Minutes));

            var startDate = day;

            if (previousDay)
            {
                startDate = day.AddDays(-1);
            }

            return DateTime.SpecifyKind(startDate.Date.AddHours(minTime.Hours).AddMinutes(minTime.Minutes),DateTimeKind.Utc);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private DateTime MaxDateTime(IList<DateOnly> dates, ISchedulingResultStateHolder stateHolder, IList<IPerson> persons, DateOnly day)
        {
            if (dates == null)
                throw new ArgumentNullException("dates");

            if (stateHolder == null)
                throw new ArgumentNullException("stateHolder");

            if (persons == null)
                throw new ArgumentNullException("persons");

            var maxTime = TimeSpan.FromHours(0);
            var nextDay = false;
            var dic = stateHolder.Schedules;

            foreach (var person in persons)
            {
                foreach (var dateOnly in dates)
                {
                    var part = dic[person].ScheduledDay(dateOnly);
                    var layerCollection = part.ProjectionService().CreateProjection();

                    if (!layerCollection.HasLayers) continue;

                    var period = layerCollection.Period();

                    if (period == null) continue;

                    if (period.Value.EndDateTime.Date > dateOnly.Date)
                    {
                        if(nextDay)
                        {
                            if (period.Value.EndDateTime.TimeOfDay > maxTime)
                                maxTime = period.Value.EndDateTime.TimeOfDay;   
                        }
                        else
                        {
                            maxTime = period.Value.EndDateTime.TimeOfDay;   
                        }

                        nextDay = true;
                    }
                    else
                    {
                        if(!nextDay)
                        {
                            if (period.Value.EndDateTime.TimeOfDay > maxTime)
                                maxTime = period.Value.EndDateTime.TimeOfDay;    
                        }
                    }
                }
            }

            if (maxTime.Minutes != 0)
                maxTime = maxTime.Add(new TimeSpan(0, 60 - maxTime.Minutes, 0));

            var endDate = day;

            if (nextDay || maxTime.Days >= 1)
            {
                endDate = day.AddDays(1);
            }

            return DateTime.SpecifyKind(endDate.Date.AddHours(maxTime.Hours).AddMinutes(maxTime.Minutes),DateTimeKind.Utc);
        }

        public DateTimePeriod TimelinePeriod(IList<DateOnly> dates, ISchedulingResultStateHolder stateHolder, IList<IPerson> persons, DateOnly day)
        {
            var startDateTime = MinDateTime(dates, stateHolder, persons, day);
            var endDateTime = MaxDateTime(dates, stateHolder, persons, day);

            if (startDateTime.Equals(endDateTime))
            {
                startDateTime = TimeZoneHelper.ConvertToUtc(day.Date.AddHours(8), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
                endDateTime = TimeZoneHelper.ConvertToUtc(day.Date.AddHours(17), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            }

            return new DateTimePeriod(startDateTime, endDateTime);
        }
	}
}
