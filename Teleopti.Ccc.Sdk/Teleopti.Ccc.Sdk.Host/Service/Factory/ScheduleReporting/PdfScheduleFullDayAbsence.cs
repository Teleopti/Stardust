using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory.ScheduleReporting
{
    internal class PdfScheduleFullDayAbsence : PdfScheduleTemplate
    {
        public PdfScheduleFullDayAbsence(float columnWidth, IScheduleDay schedulePart, TimeZoneInfo timeZoneInfo, bool rightToLeft, CultureInfo culture)
        {
            IPersonAbsence absence = schedulePart.PersonAbsenceCollection()[0];
            IVisualLayerCollection projection = schedulePart.ProjectionService().CreateProjection();
            TimeSpan contractTime = projection.ContractTime();
            DateTime start = projection.Period().Value.StartDateTimeLocal(timeZoneInfo);
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

	        Format = new PdfStringFormat {RightToLeft = rightToLeft};
	        ColumnWidth = columnWidth;

            const float top = 2;
            Height = Render(top, start, absence.Layer.Payload.Description.Name, absence.Layer.Payload.DisplayColor, contractTime,culture);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, start, absence.Layer.Payload.Description.Name, absence.Layer.Payload.DisplayColor, contractTime,culture);
        }

        private float Render(float top, DateTime startDateTime, string text, Color color, TimeSpan contractTime, CultureInfo culture)
        {
            top = RenderDate(startDateTime, top,culture);
            top = RenderText(text, top);
            top = RenderSplitter(color, top, 5);
            top = RenderContractTime(contractTime, top);

            return top;
        }
    }
}