using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportGraphicalDrawSchedule
	{
		private readonly PdfPage _page;
		private int _left;
		private readonly int _noteWidth;
		private readonly int _top;
		private const int ScheduleHeight = 15;
		private readonly IScheduleDay _part;
		private readonly bool _rightToLeft;
		private const int PaddingToSchedule = 5;
	    private readonly bool _publicNote;
	    private readonly CultureInfo _cultureInfo;
        private readonly PdfSolidBrush _brush;
        private const int RowSpace = 2;

		public ScheduleReportGraphicalDrawSchedule(PdfPage page, int left, int noteWidth, int top, IScheduleDay part, bool rightToLeft, bool publicNote, CultureInfo cultureInfo)
		{
			_page = page;
			_left = left;
			_noteWidth = noteWidth;
			_top = top;
			_part = part;
			_rightToLeft = rightToLeft;
		    _publicNote = publicNote;
		    _cultureInfo = cultureInfo;
            _brush = new PdfSolidBrush(Color.DimGray);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public float Draw(DateTimePeriod timelinePeriod)
		{
			var pageWidth = _page.GetClientSize().Width;
		    var publicNoteWidth = (int) pageWidth - _left;
		    var scheduleWidth = publicNoteWidth - _noteWidth;

			if (_rightToLeft)
                _left = (int)(pageWidth - _left - scheduleWidth);

            var projectionRectangle = new Rectangle(_left, _top, scheduleWidth, ScheduleHeight);
			var layerCollection = _part.ProjectionService().CreateProjection();

			IList<Rectangle> rectangles = new List<Rectangle>();
		    var personPeriod = _part.Person.Period(_part.DateOnlyAsPeriod.DateOnly);
            
            if(personPeriod != null)
            {
                foreach (var layer in layerCollection)
                {
                    var t = timelinePeriod;

                    if(t.StartDateTime.Date > layer.Period.StartDateTime.Date)
                        t = timelinePeriod.MovePeriod(TimeSpan.FromDays(-1));

                    var rect = ViewBaseHelper.GetLayerRectangle(t, projectionRectangle, layer.Period, _rightToLeft);
                    rect = AdjustRectangleToHour(rect, layer, timelinePeriod, projectionRectangle);
                    if (rect.IsEmpty) continue;
                    rectangles.Add(rect);
                    var lBrush = GradientBrush(rect, layer.Payload.ConfidentialDisplayColor(_part.Person));
                    _page.Graphics.DrawRectangle(lBrush, rect);

					if(_part.SignificantPartForDisplay() == SchedulePartView.ContractDayOff)
					{
						var tilingBrush = TilingBrush(rect);
						_page.Graphics.DrawRectangle(tilingBrush, rect);
					}
                }
            }

            DrawDayOff(rectangles, projectionRectangle, timelinePeriod);
            DrawHourLines(rectangles, projectionRectangle, timelinePeriod);
			DrawNoteField(projectionRectangle, (int)pageWidth);

		    var top = _top + ScheduleHeight;

            if (_publicNote)
            {
                var note = _part.PublicNoteCollection().FirstOrDefault();
                var noteString = note != null ? note.GetScheduleNote(new NoFormatting()) : string.Empty;
                if (noteString.Length > 0) top = DrawPublicNote(noteString, publicNoteWidth);
            }

		    return top;
		}

		private static PdfTilingBrush TilingBrush(Rectangle destinationRectangle)
		{
			var rect = new RectangleF(0, 0, destinationRectangle.Height, destinationRectangle.Height);
			var tilingBrush = new PdfTilingBrush(rect);
			var pen = new PdfPen(Color.LightGray, 0.5f);
			tilingBrush.Graphics.DrawLine(pen, 0, destinationRectangle.Height, destinationRectangle.Height, 0);
			return tilingBrush;
		}

		private static PdfLinearGradientBrush GradientBrush(Rectangle destinationRectangle, Color color)
		{
			var rect = new Rectangle(destinationRectangle.X, destinationRectangle.Y - 1, destinationRectangle.Width, destinationRectangle.Height + 2);
			return new PdfLinearGradientBrush(rect, Color.WhiteSmoke, color, 90);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public Rectangle AdjustRectangleToHour(Rectangle layerRect, IVisualLayer layer, DateTimePeriod timelinePeriod, Rectangle projectionRectangle)
		{
            if(layer == null)
                throw new ArgumentNullException("layer");

			if (_rightToLeft)
				return layerRect;

			if (layer.Period.StartDateTime.Minute == 0)
			{
			    var startHour = layer.Period.StartDateTime.Hour;
			    var timeSpanDiff = layer.Period.StartDateTime.Date.Subtract(timelinePeriod.StartDateTime.Date);
			    startHour += 24 * timeSpanDiff.Days;
                var hourX = HourX(startHour, timelinePeriod, projectionRectangle);
				
				var diff = layerRect.X - hourX;

				layerRect.X = hourX;
				layerRect.Width = layerRect.Width + diff;	
			}

			if(layer.Period.EndDateTime.Minute == 0)
			{
				var endHour = layer.Period.EndDateTime.Hour;
                var timeSpanDiff = layer.Period.EndDateTime.Date.Subtract(timelinePeriod.StartDateTime.Date);
                endHour += 24 * timeSpanDiff.Days;
                var hourX = HourX(endHour, timelinePeriod, projectionRectangle);

				var diff = hourX - layerRect.Right;
				layerRect.Width = layerRect.Width + diff;	
			}

			return layerRect;
		}

		public int HourX(int hour, DateTimePeriod timelinePeriod, Rectangle projectionRectangle)
		{
			var totalHours = timelinePeriod.ElapsedTime().TotalHours;
			var hourWidth = projectionRectangle.Width / totalHours;
		    var diffHour = hour - timelinePeriod.StartDateTime.Hour;
            if (diffHour < 0)
                diffHour += 24;
			
			if(_rightToLeft)
				return projectionRectangle.Right - (int)Math.Round((diffHour) * hourWidth, 0) + 0;
			
			return projectionRectangle.Left + (int)Math.Round((diffHour) * hourWidth, 0) + 0;
		}

        private void DrawDayOff(ICollection<Rectangle> rectangles, Rectangle projectionRectangle, DateTimePeriod timeLinePeriod)
        {
            var drawDayOff = new ScheduleReportGraphicalDrawDayOff(_part, timeLinePeriod, projectionRectangle, _rightToLeft, _page.Graphics);
            var rectangle = drawDayOff.Draw();

            if (!rectangle.IsEmpty)
                rectangles.Add(rectangle);
        }

		private void DrawHourLines(IEnumerable<Rectangle> rectangles, Rectangle projectionRectangle, DateTimePeriod timeLinePeriod)
		{
			foreach (var layerRect in rectangles)
			{
				if (layerRect.IsEmpty) continue;

				var totalHours = timeLinePeriod.ElapsedTime().TotalHours;
				var hourWidth = projectionRectangle.Width / totalHours;
				var pen = new PdfPen(Color.LightGray, 0.5f);

				for (var i = 0; i <= totalHours; i++)
				{
					var position = (int)Math.Round(i * hourWidth, 0) + 0;
					var point = new Point(projectionRectangle.Left + position, layerRect.Top + 2);
					var pointAdjustedForEnd = new Point(point.X - 1, point.Y);

					if (!layerRect.Contains(point) && !layerRect.Contains(pointAdjustedForEnd)) continue;
				
					var hourTop = point;
					var hourBottom = new Point(point.X, layerRect.Bottom - 2);

					_page.Graphics.DrawLine(pen, hourTop, hourBottom);
				}
			}
		}

		private void DrawNoteField(Rectangle projectionRectangle, int pageRight)
		{
			var pen = new PdfPen(Color.Gray, 0.5f);

			if(_rightToLeft)
			{
				var p1 = new Point(0, projectionRectangle.Bottom);
				var p2 = new Point(_noteWidth - PaddingToSchedule, projectionRectangle.Bottom);
				_page.Graphics.DrawLine(pen, p1, p2);	
			}
			else
			{
				var p1 = new Point(projectionRectangle.Right + PaddingToSchedule, projectionRectangle.Bottom);
				var p2 = new Point(pageRight, projectionRectangle.Bottom);
				_page.Graphics.DrawLine(pen, p1, p2);	
			}
		}

        private int DrawPublicNote(string note, int width)
        {
			var format = new PdfStringFormat { RightToLeft = _rightToLeft, WordWrap = PdfWordWrapType.None };
			const float fontSize = 9f;
			var font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, _cultureInfo);
            var stringWidthHandler = new StringWidthHandler(font, width);
            var lines = stringWidthHandler.WordWrap(note);
            var top = _top + ScheduleHeight;

            for (var i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                _page.Graphics.DrawString(line, font, _brush, new RectangleF(_left, top + RowSpace, width, font.Height + RowSpace), format);
                top = (int)(top + font.Height + RowSpace);
            }

            return top;
        }
	}
}
