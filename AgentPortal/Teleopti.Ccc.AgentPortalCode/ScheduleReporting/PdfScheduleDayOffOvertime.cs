using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfScheduleDayOffOvertime : PdfScheduleTemplate
    {
        ActivityDto[] _arrActivityDto;
        private const int MAX_NUMBER_OF_CHARACTERS = 20;

        public PdfScheduleDayOffOvertime(float columnWidth, SchedulePartDto schedulePartDto, bool rightToLeft, CultureInfo culture):base(culture)
        {
        	if (schedulePartDto == null) throw new ArgumentNullException("schedulePartDto");
        	Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;
            const float top = 2;
            Height = Render(top, schedulePartDto.ProjectedLayerCollection, ScheduleReportDetail.All,
                schedulePartDto.PersonDayOff.Period.LocalStartDateTime, schedulePartDto.PersonDayOff.Name);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, schedulePartDto.ProjectedLayerCollection, ScheduleReportDetail.All,
                schedulePartDto.PersonDayOff.Period.LocalStartDateTime, schedulePartDto.PersonDayOff.Name);
        }

        private float Render(float top,ProjectedLayerDto[] payLoads, ScheduleReportDetail details, DateTime dayOffStart, string dayOffName)
        {
            top = RenderDate(dayOffStart, top);
            top = RenderText(dayOffName, top);
            top = RenderGraphics(top);

            if (details != ScheduleReportDetail.None)
            {
                if (payLoads.Length > 0)
                {
                    top = RenderSplitter(Color.Gray, top, 1);
                }

                foreach (ProjectedLayerDto visualLayer in payLoads)
                {
                    if (details == ScheduleReportDetail.Break)
                    {
                        ActivityDto activty = GetActivity(visualLayer.PayloadId);
                        if (activty != null)
                        {
                            top = RenderPayLoad(visualLayer, top);
                        }
                    }
                    else
                    {
                        top = RenderPayLoad(visualLayer, top);
                    }


                }
            }
            return top;
        }

        protected float RenderGraphics(float top)
        {
            var rect = new RectangleF(0, 0, 5, 5);
            var tillingBrush = new PdfTilingBrush(rect);
            var pen = new PdfPen(Color.Gray, 1);
            tillingBrush.Graphics.DrawLine(pen, 0, 5, 5, 0);

            var rectangle = new RectangleF(7, 28, ColumnWidth - 14, 5);
            Graphics.DrawRectangle(tillingBrush, rectangle);

            return top + RowSpace + 21;
        }

        private ActivityDto GetActivity(string activityId)
        {
            if (_arrActivityDto == null)
                _arrActivityDto = SdkServiceHelper.SchedulingService.GetActivities(new LoadOptionDto{LoadDeleted = true,LoadDeletedSpecified = true});

            foreach (ActivityDto activityDto in _arrActivityDto)
            {
                if (activityDto.Id == activityId)
                    return activityDto;
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderPayLoad(ProjectedLayerDto visualLayer, float top)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 7f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Regular, Culture);
            var timeRect = new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2);

            string timeString = ShortTimeString(visualLayer.Period.LocalStartDateTime.TimeOfDay) + " - " +
                                ShortTimeString(visualLayer.Period.LocalEndDateTime.TimeOfDay);
            Graphics.DrawString(timeString, font, Brush, timeRect, Format);
            top = top + fontSize + 2;
            var nameRect = new RectangleF(0, top, ColumnWidth, fontSize + 2);

            if (!string.IsNullOrEmpty(visualLayer.OvertimeDefinitionSetId))
            {
                if (visualLayer.Description.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    int commaPlace = visualLayer.Description.LastIndexOf(',');
                    string part1 = visualLayer.Description.Substring(0, commaPlace+1);
                    string part2 = visualLayer.Description.Substring(commaPlace+2, visualLayer.Description.Length - commaPlace-2);

                    Graphics.DrawString(part1, font, Brush, nameRect, Format);
                    nameRect = new RectangleF(0, top + fontSize + 2, ColumnWidth, fontSize + 2);
                    Graphics.DrawString(part2, font, Brush, nameRect, Format);
                }
                else
                    Graphics.DrawString(visualLayer.Description, font, Brush, nameRect, Format);
            }

            float lineStart = timeRect.Top;
            float lineEnd = nameRect.Bottom;
            var color = new PdfColor(visualLayer.DisplayColor.Red, visualLayer.DisplayColor.Green, visualLayer.DisplayColor.Blue);
            Graphics.DrawLine(new PdfPen(color, 5), 7, lineStart, 7, lineEnd);

            return nameRect.Bottom + 2;
        }

        private static string ShortTimeString(TimeSpan timeSpan)
        {
            DateTime dt = DateTime.MinValue.Add(timeSpan);
            return dt.ToShortTimeString();
        }
    }
}
