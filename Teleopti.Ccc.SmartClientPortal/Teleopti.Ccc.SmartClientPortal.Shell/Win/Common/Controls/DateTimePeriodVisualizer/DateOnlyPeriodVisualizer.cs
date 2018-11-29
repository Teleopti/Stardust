using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer
{
    public partial class DateOnlyPeriodVisualizer : UserControl
    {
        private DateOnlyPeriodVisualizerRow _dateOnlyPeriodVisualizerRow;
        private DateOnlyPeriod _containedPeriod;


        public DateOnlyPeriodVisualizer(DateOnlyPeriodVisualizerRow dateOnlyPeriodVisualizerRow)
        {
            InitializeComponent();
            _dateOnlyPeriodVisualizerRow = dateOnlyPeriodVisualizerRow;
        }

        public DateOnlyPeriod ContainedPeriod
        {
            get { return _containedPeriod; }
            set { _containedPeriod = value; }
        }

        public void Draw()
        {
            Controls.Clear();
            for (int i = 0; i < _dateOnlyPeriodVisualizerRow.Periods.Count; i++)
            {
                GradientLabel label = new GradientLabel();
                label.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, _dateOnlyPeriodVisualizerRow.DisplayColor, _dateOnlyPeriodVisualizerRow.DisplayColor);
                label.BorderAppearance = BorderStyle.None;
                label.Location = new Point(1, 1);
                label.Name = "periodLabel" + i;
                label.Size = new Size(10, 16);
                label.TabIndex = 0;
                label.TextAlign = ContentAlignment.MiddleLeft;
                toolTip1.SetToolTip(label, _dateOnlyPeriodVisualizerRow.Periods[i].ArabicSafeDateString());
                Controls.Add(label);
            }
        }

        private bool IsRightToLeft
        {
            get
            {
                if (RightToLeft == RightToLeft.Yes)
                    return true;

                return false;
            }
        }

        private int distance(int lastStartX, int nextStartX)
        {
            if (IsRightToLeft)
                return lastStartX - nextStartX;

            return nextStartX - lastStartX;
        }

        private void positionLabels()
        {
            for (int i = 0; i < _dateOnlyPeriodVisualizerRow.Periods.Count; i++)
            {
                DateOnlyPeriod period = _dateOnlyPeriodVisualizerRow.Periods[i];
                DateTimePeriod containedDateTimePeriod = _containedPeriod.ToDateTimePeriod((TimeZoneInfo.Utc));
                DateTimePeriod periodDateTimePeriod = period.ToDateTimePeriod((TimeZoneInfo.Utc));
                LengthToTimeCalculator calculatior = new LengthToTimeCalculator(containedDateTimePeriod, Width);
                Control label = Controls.Find("periodLabel" + i, false)[0];
                double left = calculatior.PositionFromDateTime(periodDateTimePeriod.StartDateTime, IsRightToLeft);
                double right = calculatior.PositionFromDateTime(periodDateTimePeriod.EndDateTime, IsRightToLeft);
                if (left < 0)
                    left = -10;
                if (right < 0)
                    right = -9;
                if (left > Width)
                    left = Width + 9;
                if (right > Width)
                    right = Width + 10;
                if (IsRightToLeft)
                    label.Left = (int)right;
                else
                {
                    label.Left = (int)left;
                }
                label.Width = distance((int)left,(int)right);
            }
        }
  
        private bool checkGuideLine(int nextStartX)
        {
            if(IsRightToLeft)
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
            positionLabels();
            paintGuideLines(e);
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}