using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer
{
    public class DateOnlyPeriodVisualizerHeader : GradientLabel
    {
        private DateOnlyPeriod _containedPeriod;
        private CultureInfo _culture = Thread.CurrentThread.CurrentCulture;
        

        public DateOnlyPeriod ContainedPeriod
        {
            get { return _containedPeriod; }
            set { _containedPeriod = value; }
        }

        private bool IsRightToLeft
        {
            get
            {
                if(RightToLeft == RightToLeft.Yes)
                    return true;

                return false;
            }
        }

        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }

        private int distance(int lastStartX, int nextStartX)
        {
            if (IsRightToLeft)
                return lastStartX - nextStartX;

            return nextStartX - lastStartX;
        }

        private void paintDates(PaintEventArgs e)
        {
            DateTimePeriod containedDateTimePeriod = _containedPeriod.ToDateTimePeriod((TimeZoneInfo.Utc));
            LengthToTimeCalculator calculatior = new LengthToTimeCalculator(containedDateTimePeriod, Width);
            int lastStartX = 0;
            if (IsRightToLeft)
                lastStartX = Width;
            int monthStep = 1;
            
            DateTime displayDate = containedDateTimePeriod.StartDateTime;
            do
            {
                int nextStartX = (int)calculatior.PositionFromDateTime(displayDate.AddMonths(monthStep), IsRightToLeft);
                string displayString = displayDate.ToString("Y", _culture);
                displayString = fitTextToWidth(displayString, distance(lastStartX, nextStartX), e.Graphics);
                SizeF size = e.Graphics.MeasureString(displayString, Font);

                int stringX = (int)(lastStartX + ((distance(lastStartX, nextStartX)) / 2) - (size.Width / 2));
                if (IsRightToLeft)
                    stringX = (int)(lastStartX - ((distance(lastStartX, nextStartX)) / 2) - (size.Width / 2));
                e.Graphics.DrawString(displayString, Font, Brushes.Black, stringX, 2);

                lastStartX = nextStartX;
                displayDate = displayDate.AddMonths(monthStep);

            } while (displayDate < containedDateTimePeriod.EndDateTime);
            
        }

        private string fitTextToWidth(string textToFit, float maxWidth, Graphics g)
        {
            string treeDots = "...";
            SizeF size = g.MeasureString(textToFit, Font);
            if (size.Width > maxWidth)
            {
                //remove last char, add three dots and measure again until it fits
                textToFit = textToFit.Substring(0, textToFit.Length);
                string textAndDots = textToFit + treeDots;
                while (g.MeasureString(textAndDots, Font).Width > maxWidth)
                {
                    textToFit = textToFit.Substring(0, textToFit.Length - 1);
                    if (textToFit.Length == 0)
                        return string.Empty;
                    textAndDots = textToFit + treeDots;
                }
                return textAndDots;
            }
            return textToFit;
        }

        private bool checkGuideLine(int nextStartX)
        {
            if (IsRightToLeft)
            {
                return (nextStartX - 2 > 0);
            }
            return (nextStartX + 2 < Width);
        }

        private void paintGuideLines(PaintEventArgs e)
        {
            DateTimePeriod containedDateTimePeriod = _containedPeriod.ToDateTimePeriod((TimeZoneInfo.Utc));
            LengthToTimeCalculator calculatior = new LengthToTimeCalculator(containedDateTimePeriod, Width);
            DateTime displayDate = containedDateTimePeriod.StartDateTime;
            do
            {
                int nextStartX = (int)calculatior.PositionFromDateTime(displayDate.AddMonths(1), IsRightToLeft);

                if (checkGuideLine(nextStartX))
                    e.Graphics.DrawLine(Pens.DarkGray, nextStartX, 2, nextStartX, Height - 4);
                displayDate = displayDate.AddMonths(1);

            } while (displayDate < containedDateTimePeriod.EndDateTime);
        }

        protected override void OnPaint(PaintEventArgs e)
        {   
            base.OnPaint(e);
            paintGuideLines(e);
            paintDates(e);
        }
    }
}