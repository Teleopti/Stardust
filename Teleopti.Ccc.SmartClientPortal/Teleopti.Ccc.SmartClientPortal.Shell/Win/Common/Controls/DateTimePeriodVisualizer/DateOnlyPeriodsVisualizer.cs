using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer
{
    public partial class DateOnlyPeriodsVisualizer : UserControl
    {
        private readonly IList<DateOnlyPeriodVisualizerRow> _rows = new List<DateOnlyPeriodVisualizerRow>();
        private float _labelTextWidth;
        private DateOnlyPeriod _containedPeriod;
        private DateTime _seedDate;
        private CultureInfo _culture = Thread.CurrentThread.CurrentCulture;
        private int _monthsOnEachSide = 1;

        public DateOnlyPeriodsVisualizer()
        {
            InitializeComponent();
            _seedDate = DateTime.Now;
            _containedPeriod = defaultPeriod();
            gradientLabelColumnHeader.ContainedPeriod = _containedPeriod;
        }

        public IList<DateOnlyPeriodVisualizerRow> Rows
        { get { return _rows; } }

        public DateOnlyPeriod ContainedPeriod
        {
            get { return _containedPeriod; }
            set
            {
                _containedPeriod = value;
                gradientLabelColumnHeader.ContainedPeriod = _containedPeriod;
                gradientLabelColumnHeader.Invalidate();
                Draw();
            }
        }

        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value;
                gradientLabelColumnHeader.Culture = _culture;
            }
        }

        public int MonthsOnEachSide
        {
            get { return _monthsOnEachSide; }
            set { _monthsOnEachSide = value;
                ContainedPeriod = defaultPeriod();
                gradientLabelColumnHeader.ContainedPeriod = _containedPeriod;
            }
        }

        public void Draw()
        {
            toolTip1.SetToolTip(gradientLabelColumnHeader, ContainedPeriod.DateString);
            clearRows();
            createRows();
            tableLayoutPanel1.ColumnStyles[0].Width = _labelTextWidth;
        }

        public void Previous()
        {
            _seedDate = _seedDate.AddMonths(-1);
            ContainedPeriod = defaultPeriod();
        }

        public void Next()
        {
            _seedDate = _seedDate.AddMonths(1);
            ContainedPeriod = defaultPeriod();
        }

        private void clearRows()
        {
            tableLayoutPanel1.RowCount = 1;
            for (int i = 0; i < _rows.Count; i++)
            {
                tableLayoutPanel1.Controls.RemoveByKey("labelText" + i);
                tableLayoutPanel1.Controls.RemoveByKey("periodVisualizer" + i);
            }
        }

        private void createRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                tableLayoutPanel1.RowCount++;
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
                GradientLabel label = newLabelTextFromTemplate();
                label.Name = "labelText" + i;
                label.AutoSize = true;
                label.Text = _rows[i].Text;
                float newWidth = rowHeaderWidth(_rows[i].Text);
                if (newWidth > _labelTextWidth)
                    _labelTextWidth = newWidth;
                tableLayoutPanel1.Controls.Add(label, 0, i + 1);
                DateOnlyPeriodVisualizer periodVisualizer = new DateOnlyPeriodVisualizer(_rows[i]);
                periodVisualizer.Name = "periodVisualizer" + i;
                periodVisualizer.ContainedPeriod = ContainedPeriod;
                tableLayoutPanel1.Controls.Add(periodVisualizer, 1, i + 1);
                periodVisualizer.Dock = DockStyle.Fill;
                periodVisualizer.Margin = new Padding(0);
                periodVisualizer.Draw();
            }
        }

        private GradientLabel newLabelTextFromTemplate()
        {
            var label = new GradientLabel
            {
                BackgroundColor = gradientLabelTextTemplate.BackgroundColor,
                BorderSides = gradientLabelTextTemplate.BorderSides,
                BorderAppearance = gradientLabelTextTemplate.BorderAppearance,
                Dock = DockStyle.Fill,
                Location = new Point(1, 1),
                Margin = new Padding(0),
                Size = new Size(140, 20),
                TabIndex = 0,
                TextAlign = ContentAlignment.MiddleLeft
            };

            return label;
        }

        private float rowHeaderWidth(string text)
        {
            float result;
            using (GradientLabel label = new GradientLabel())
            {
                Graphics g = label.CreateGraphics();
                result = g.MeasureString(text + " ", Font).Width + 3;
                g.Dispose();
            }
            return result;
        }

        private DateOnlyPeriod defaultPeriod()
        {
            DateTime start = _seedDate.AddMonths(-_monthsOnEachSide);
            DateOnly startDate = new DateOnly(start.Year, start.Month, 1);
            DateTime end = _seedDate.AddMonths(_monthsOnEachSide + 1);
            DateOnly enDate = new DateOnly(end.Year, end.Month, 1).AddDays(-1);
            return new DateOnlyPeriod(startDate, enDate);
        }
    }
}