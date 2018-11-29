using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
    public class PdfScheduleNotScheduled : PdfScheduleTemplate
    {
        public PdfScheduleNotScheduled(float columnWidth, DateOnly date, bool rightToLeft, CultureInfo culture)
        {
            CultureInfo = culture;
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat {RightToLeft = rightToLeft};
            ColumnWidth = columnWidth;

            const float top = 2;
            Height = render(top, date.Date, "");

            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, date.Date, Resources.NotScheduled);
        }

        private float render(float top, DateTime startDateTime, string text)
        {
            top = RenderDate(startDateTime, top);
            top = RenderText(text, top);
            return top;
        }

    }
}
