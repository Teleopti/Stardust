﻿using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Pdf.Graphics;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.Factory.ScheduleReporting
{
    internal class PdfScheduleNotScheduled : PdfScheduleTemplate
    {

        public PdfScheduleNotScheduled(float columnWidth, DateOnly date, bool rightToLeft, CultureInfo culture)
        {
            Brush = new PdfSolidBrush(Color.DimGray);

            Template = new PdfTemplate(columnWidth, Height);
            Graphics = Template.Graphics;

            Format = new PdfStringFormat();
            Format.RightToLeft = rightToLeft;
            ColumnWidth = columnWidth;

            float top = 2;
            Height = render(top, date.Date, "",culture);

            Template.Reset(new SizeF(columnWidth, Height));
            Height = render(top, date.Date, Resources.NotScheduled,culture);
        }


        private float render(float top, DateTime startDateTime, string text, CultureInfo culture)
        {
            top = RenderDate(startDateTime, top,culture);
            top = RenderText(text, top);
            return top;
        }

    }
}