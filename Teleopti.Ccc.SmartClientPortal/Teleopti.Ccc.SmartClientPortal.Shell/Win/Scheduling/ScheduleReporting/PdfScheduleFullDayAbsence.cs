using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleFullDayAbsence : PdfScheduleTemplate
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public PdfScheduleFullDayAbsence(float columnWidth, IScheduleDay schedulePart, TimeZoneInfo timeZoneInfo, bool rightToLeft, CultureInfo culture, bool dayOff)
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
            Height = render(top, start, absence.Description.Name, absence.DisplayColor, contractTime, dayOff);

            Template.Reset(new SizeF(columnWidth, Height));
            //Height = render(top, start, absence.Layer.Payload.Description.Name, absence.Layer.Payload.DisplayColor, contractTime);
            Height = render(top, start, absence.Description.Name, absence.DisplayColor, contractTime, dayOff);
        }


        private float render(float top, DateTime startDateTime, string text, Color color, TimeSpan contractTime, bool dayOff)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            top = RenderSplitter(color, top, 5);
			RenderStripesForDayOff(top, dayOff);
            top = RenderContractTime(contractTime, top);

            return top;
        }


		protected void RenderStripesForDayOff(float top, bool dayOff)
		{
			top = top - 8;
			if (!dayOff) return;
			var rect = new RectangleF(0, 0, 7, 7);
			var tillingBrush = new PdfTilingBrush(rect);
			var pen = new PdfPen(Color.Gray, 0.5f);
			tillingBrush.Graphics.DrawLine(pen, 1, 6, 6, 1);
	
			var rectangle = new RectangleF(5, top, ColumnWidth - 10, 5);
			Graphics.DrawRectangle(tillingBrush, rectangle);
		}   
    }
}
