﻿using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfScheduleFullDayAbsence : PdfScheduleTemplate
    {

        public PdfScheduleFullDayAbsence(float columnWidth, SchedulePartDto schedulePart, bool rightToLeft, CultureInfo culture):base(culture)
        {
        	if (schedulePart == null) throw new ArgumentNullException("schedulePart");
        	PersonAbsenceDto absence = schedulePart.PersonAbsenceCollection.First();
            //IVisualLayerCollection projection = schedulePart.ProjectionService().CreateProjection();
            TimeSpan contractTime = schedulePart.ContractTime.TimeOfDay;
            DateTime start = schedulePart.ProjectedLayerCollection.First().Period.LocalStartDateTime;
            //DateTime end = projection.Period().Value.EndDateTimeLocal(timeZoneInfo);
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            Height = Render(top, start, absence.AbsenceLayer.Absence.Name, absence.AbsenceLayer.Absence.DisplayColor, contractTime);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, start, absence.AbsenceLayer.Absence.Name, absence.AbsenceLayer.Absence.DisplayColor, contractTime);
        }


        private float Render(float top, DateTime startDateTime, string text, ColorDto colorDto, TimeSpan contractTime)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            var color = new PdfColor(colorDto.Red, colorDto.Green, colorDto.Blue);
            top = RenderSplitter(color, top, 5);
            top = RenderContractTime(contractTime, top);

            return top;
        }

 



        
    }
}
