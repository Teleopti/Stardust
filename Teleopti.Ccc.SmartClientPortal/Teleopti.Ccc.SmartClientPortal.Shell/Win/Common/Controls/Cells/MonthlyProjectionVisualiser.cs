using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    public partial class MonthlyProjectionVisualiser : UserControl
    {
        private class LayerLabel : GradientPanel
        {
            private readonly DateOnlyProjectionItem _layer;

            private LayerLabel(){}

            public LayerLabel(DateOnlyProjectionItem layer)
                : this()
            {
                _layer = layer;
                BackgroundColor = new BrushInfo(GradientStyle.Vertical, _layer.DisplayColor, _layer.DisplayColor);
                BorderStyle = BorderStyle.None;
            }

            public DateOnlyProjectionItem Layer
            {
                get { return _layer; }
            }
        }

        private readonly IList<LayerLabel> _layerLabelCollection = new List<LayerLabel>();
        private DateOnlyPeriod _period;
        private LengthToDateCalculator _lengthCalculator;
        private static readonly object LockCollection = new object();

        public MonthlyProjectionVisualiser()
        {
            InitializeComponent();
        }

        public void SetLayerCollection(IList<DateOnlyProjectionItem> layerCollection)
        {
            foreach (var layerLabel in _layerLabelCollection.ToList())
            {
                Controls.Remove(layerLabel);
                layerLabel.Dispose();
            }
            _layerLabelCollection.Clear();

            LayerLabel newLabel;
            if (layerCollection != null)
            {
                foreach (DateOnlyProjectionItem layer in layerCollection)
                {
                    newLabel = new LayerLabel(layer);
                    newLabel.Location = new Point(0, 0);
                    newLabel.Size = new Size(1, Height);
                    newLabel.Name = "LayerLabel" + (_layerLabelCollection.Count + 1);
                    Controls.Add(newLabel);
                    _layerLabelCollection.Add(newLabel);
                    toolTip1.SetToolTip(newLabel, layer.ToolTipText);
                }
            }

            Redraw();
        }

        public void SetControlDatePeriod(DateOnlyPeriod period)
        {
            _period = period;
            _lengthCalculator = new LengthToDateCalculator(period, Width);
            ResizeLabels();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawControlScale(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _lengthCalculator = new LengthToDateCalculator(_period, Width);
            ResizeLabels();
        }

        private void Redraw()
        {
            ResizeLabels();
            Invalidate(true);
        }

        private void ResizeLabels()
        {
            lock (LockCollection)
            {
                foreach (var layerLabel in _layerLabelCollection)
                {
                    double x1 = _lengthCalculator.PositionFromDateTime(layerLabel.Layer.Period.StartDate);
                    double x2 = _lengthCalculator.PositionFromDateTime(layerLabel.Layer.Period.EndDate);
                    layerLabel.Width = (int) Math.Abs(x2 - x1);
                    layerLabel.Left =
                        _lengthCalculator.RectangleFromDateTimePeriod(layerLabel.Layer.Period, new Point(), 1,
                                                                      (RightToLeft == RightToLeft.Yes)).Left;
                }
            }
        }

        private void DrawControlScale(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF cellRectangle = ClientRectangle;
            DateTime pointer = DateHelper.GetFirstDateInMonth(_period.StartDate.Date, CultureInfo.CurrentCulture);
            pointer = CultureInfo.CurrentCulture.Calendar.AddMonths(pointer, 1);
            while (pointer < _period.EndDate.Date)
            {
                float x = (float) _lengthCalculator.PositionFromDateTime(new DateOnly(pointer), (RightToLeft == RightToLeft.Yes));
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), cellRectangle.Left + x, cellRectangle.Top,
                               cellRectangle.Left + x, cellRectangle.Bottom);
                pointer = CultureInfo.CurrentCulture.Calendar.AddMonths(pointer, 1);
            }
        }

        private void LayerVisualizer_RightToLeftChanged(object sender, EventArgs e)
        {
            ResizeLabels();
        }
    }

    public class DateOnlyProjectionItem
    {
        public DateOnlyPeriod Period { get; set; }
        public string ToolTipText { get; set; }
        public Color DisplayColor { get; set; }
    }
}