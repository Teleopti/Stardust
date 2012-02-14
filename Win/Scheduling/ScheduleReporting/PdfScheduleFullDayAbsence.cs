using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleFullDayAbsence : PdfScheduleTemplate
    {
        public PdfScheduleFullDayAbsence(float columnWidth, IScheduleDay schedulePart, ICccTimeZoneInfo timeZoneInfo, bool rightToLeft, CultureInfo culture)
        {
            CultureInfo = culture;
            //IPersonAbsence absence = schedulePart.PersonAbsenceCollection()[0];
            var projection = schedulePart.ProjectionService().CreateProjection();

            IAbsence absence = null;

            foreach (var layer in projection)
            {
                absence = layer.Payload as IAbsence;
            }

            var contractTime = projection.ContractTime();
            var start = projection.Period().Value.StartDateTimeLocal(timeZoneInfo);
            //DateTime end = projection.Period().Value.EndDateTimeLocal(timeZoneInfo);
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            //Height = render(top, start, absence.Layer.Payload.Description.Name, absence.Layer.Payload.DisplayColor, contractTime);
            Height = render(top, start, absence.Description.Name, absence.DisplayColor, contractTime);

            Template.Reset(new SizeF(columnWidth, Height));
            //Height = render(top, start, absence.Layer.Payload.Description.Name, absence.Layer.Payload.DisplayColor, contractTime);
            Height = render(top, start, absence.Description.Name, absence.DisplayColor, contractTime);
        }


        private float render(float top, DateTime startDateTime, string text, Color color, TimeSpan contractTime)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            top = RenderSplitter(color, top, 5);
            top = RenderContractTime(contractTime, top);

            return top;
        }

 



        
    }
}
