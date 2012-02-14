using System;
using System.Drawing;
using Syncfusion.Pdf.Graphics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.ScheduleReporting
{
    public class PdfScheduleNotScheduled : PdfScheduleTemplate
    {

        public PdfScheduleNotScheduled(float columnWidth, DateOnly date, bool rightToLeft)
        {
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            Height = Render(top, date.Date, "");

            Template.Reset(new SizeF(columnWidth, Height));
            Height = Render(top, date.Date, UserTexts.Resources.NotScheduled);
        }


        private float Render(float top, DateTime startDateTime, string text)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            return top;
        }

    }
}
