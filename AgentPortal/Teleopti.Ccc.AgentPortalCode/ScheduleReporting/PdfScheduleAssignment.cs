using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfScheduleAssignment : PdfScheduleTemplate
    {
    	
    	ActivityDto[] _arrActivityDto;
        ShiftCategoryDto[] _arrShiftCategoryDto;

        public PdfScheduleAssignment(float columnWidth, SchedulePartDto schedulePart, bool rightToLeft, ScheduleReportDetail details, CultureInfo culture):base(culture)
        {
        	if (schedulePart == null) throw new ArgumentNullException("schedulePart");
        	Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            const float headerTop = top;

            DateTime start = schedulePart.ProjectedLayerCollection[0].Period.LocalStartDateTime;
            DateTime end = schedulePart.ProjectedLayerCollection[schedulePart.ProjectedLayerCollection.Length -1].Period.LocalEndDateTime;

        	var emptyColor = Color.Empty;
			string category = string.Empty;

        	ColorDto categoryColor = new ColorDto
        	                         	{
        	                         		Alpha = emptyColor.A,
        	                         		AlphaSpecified = true,
        	                         		Blue = emptyColor.B,
        	                         		BlueSpecified = true,
        	                         		Green = emptyColor.G,
        	                         		GreenSpecified = true,
        	                         		Red = emptyColor.R,
        	                         		RedSpecified = true
        	                         	};

			if (schedulePart.PersonAssignmentCollection.Length > 0 && schedulePart.PersonAssignmentCollection[0].MainShift != null)
			{
				ShiftCategoryDto shiftCategoryDto =
					GetShiftCategory(schedulePart.PersonAssignmentCollection[0].MainShift.ShiftCategoryId);

				category = shiftCategoryDto.Name;
				categoryColor = shiftCategoryDto.DisplayColor;
			}
        	Height = Render(top, start, category, end, TimeSpan.FromTicks(schedulePart.ContractTime.Ticks),
                             categoryColor, headerTop, schedulePart.ProjectedLayerCollection, details, schedulePart.PersonMeetingCollection);


            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, start, category, end, TimeSpan.FromTicks(schedulePart.ContractTime.Ticks),
                             categoryColor, headerTop, schedulePart.ProjectedLayerCollection, details, schedulePart.PersonMeetingCollection);

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

        private ShiftCategoryDto GetShiftCategory(string categoryId)
        {
            if (_arrShiftCategoryDto == null)
                _arrShiftCategoryDto = SdkServiceHelper.SchedulingService.GetShiftCategories(new LoadOptionDto { LoadDeleted = true, LoadDeletedSpecified = true });

            foreach (ShiftCategoryDto shiftCategoryDto in _arrShiftCategoryDto)
            {
                if (shiftCategoryDto.Id == categoryId)
                    return shiftCategoryDto;
            }
            return null;
        }

        private float Render(float top, DateTime startDateTime, string category, DateTime endDateTime, TimeSpan contractTime, ColorDto categoryColor, float headerTop, ProjectedLayerDto[] payLoads, ScheduleReportDetail details, PersonMeetingDto[] personMeetingDtos)
        {
            top = RenderDate(startDateTime, top);
            top = RenderCategory(category, top);
            top = RenderPeriod(startDateTime, endDateTime, top);
            top = RenderContractTime(contractTime, top);
            float headerBottom = top - 3;

            Color theColor = ColorHelper.CreateColorFromDto(categoryColor);
            var color = new PdfColor(theColor);
            Graphics.DrawLine(new PdfPen(color, 5), 7, headerTop, 7, headerBottom);

            if (details != ScheduleReportDetail.None)
            {
                if (payLoads.Length > 0)
                {
                    top = RenderSplitter(Color.Gray, top, 1);
                }

                foreach (ProjectedLayerDto visualLayer in payLoads)
                {
                    if(details == ScheduleReportDetail.Break)
                    {
                        ActivityDto activty = GetActivity(visualLayer.PayloadId);
                        if (activty != null)
                        {
                            //if (activty.ReportLevelDetail != ReportLevelDetail.None)
                            //{
                                top = RenderPayLoad(visualLayer, top, personMeetingDtos);
                            //}
                        }
                    }
                    else
                    {
                        top = RenderPayLoad(visualLayer, top, personMeetingDtos);
                    }
                    
                    
                }
            }
            return top;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderCategory(string category, float top)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 9f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, Culture);
            Graphics.DrawString(category, font, Brush, new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
            return top + fontSize + 2 + RowSpace;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderPeriod(DateTime startDateTime, DateTime endDateTime, float top)
        {
            Format.Alignment = PdfTextAlignment.Center;
            const float fontSize = 8f;
			PdfFont font = PdfFontManager.GetFont(fontSize, PdfFontStyle.Bold, Culture);
            Graphics.DrawString(
                ShortTimeString(startDateTime.TimeOfDay) + "--" + ShortTimeString(endDateTime.TimeOfDay), font, Brush,
                new RectangleF(0, top + RowSpace, ColumnWidth, fontSize + 2), Format);
            return top + fontSize + 2 + RowSpace;
        }

        private const int MAX_NUMBER_OF_CHARACTERS = 20;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private float RenderPayLoad(ProjectedLayerDto visualLayer, float top, PersonMeetingDto[] personMeetingDtos)
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

            if (!string.IsNullOrEmpty((visualLayer.MeetingId)))
            {
                foreach (PersonMeetingDto personMeetingDto in personMeetingDtos)
                {
                    if (personMeetingDto.MeetingId == visualLayer.MeetingId)
                    {
                        string subjectText = personMeetingDto.Subject;
                        string locationText = personMeetingDto.Location;
                        if (subjectText.Length > MAX_NUMBER_OF_CHARACTERS)
                        {
                            subjectText = subjectText.Substring(0, MAX_NUMBER_OF_CHARACTERS);
                        }
                        if (locationText.Length > MAX_NUMBER_OF_CHARACTERS)
                        {
                            locationText = locationText.Substring(0, MAX_NUMBER_OF_CHARACTERS);
                        }
                        Graphics.DrawString(subjectText, font, Brush, nameRect, Format);
                        nameRect = new RectangleF(0, top + fontSize + 2, ColumnWidth, fontSize + 2);
                        Graphics.DrawString(locationText, font, Brush, nameRect, Format);
                        break;
                    }
                }
                
            }
            else
            {
                if (visualLayer.Description.Length > MAX_NUMBER_OF_CHARACTERS)
                {
                    int commaPlace = visualLayer.Description.LastIndexOf(',');
                    string part1 = visualLayer.Description.Substring(0, commaPlace + 1);
                    string part2 = visualLayer.Description.Substring(commaPlace + 2, visualLayer.Description.Length - commaPlace - 2);

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
            //return top + fontSize + 2 + RowSpace;
        }

        private static string ShortTimeString(TimeSpan timeSpan)
        {
            DateTime dt = DateTime.MinValue.Add(timeSpan);
            return dt.ToShortTimeString();
        }
    }
}