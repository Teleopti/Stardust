using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    public partial class LayerVisualizer : UserControl
    {
        private class LayerLabel : GradientPanel
        {
            private LayerLabel(){}

            public LayerLabel(ICustomScheduleAppointment layer)
                : this()
            {
                Layer = layer;
                BackgroundColor = new BrushInfo(GradientStyle.Vertical, Color.White, Layer.DisplayColor);
                BorderStyle = BorderStyle.None;
            }

            public ICustomScheduleAppointment Layer { get; private set; }
        }

        private DateTime _startDateTime;
        private DateTime _endDateTime;
        private DateTime _timeBarDateTime;
        private readonly IList<LayerLabel> _layerLabelCollection = new List<LayerLabel>();
        private static readonly object LockObject = new object();

        public LayerVisualizer()
        {
            InitializeComponent();
        }

        public void SetDelayedLayerCollection(IList<ICustomScheduleAppointment> layerCollection)
        {
            lock (LockObject)
            {
                    foreach (var layerLabel in _layerLabelCollection)
                    {
                        Controls.Remove(layerLabel);
                        layerLabel.Dispose();
                    }
                    _layerLabelCollection.Clear();

                    LayerLabel newLabel;
                    if (layerCollection != null)
                    {
                        foreach (ICustomScheduleAppointment layer in layerCollection)
                        {
                            newLabel = new LayerLabel(layer);
                            newLabel.Location = new Point(0, gradientPanelHeader.Bottom + 2);
                            newLabel.Size = new Size(1, 1);
                            newLabel.Name = "LayerLabel" + (_layerLabelCollection.Count + 1);
                            Controls.Add(newLabel);
                            _layerLabelCollection.Add(newLabel);
                            toolTip1.SetToolTip(newLabel,
                                                layer.Subject + " " + layer.StartTime.ToShortTimeString() + " - " +
                                                layer.EndTime.ToShortTimeString());
                        }
                    ResizeLabels();
                    Invalidate(true);
                }
            }
        }

        public void SetTimeBarDateTime(DateTime timeBarDateTime)
        {
            _timeBarDateTime = timeBarDateTime;
            ResizeProgress();
        }

        public void SetControlDateTimePeriod(DateTime startDateTime, DateTime endDateTime)
        {
            //todo start<end
            _startDateTime = startDateTime;
            _endDateTime = endDateTime;
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
            ResizeLabels();
            ResizeProgress();
        }

        private void ResizeProgress()
        {
            int x1 = GetPixelFromDateTime(_startDateTime);
            int x2 = GetPixelFromDateTime(_timeBarDateTime);
            int width = Math.Min(x2,10000000) - Math.Min(x1,10000000);

            if (RightToLeft == RightToLeft.Yes)
            {
                width = width*-1;
            }
            if(width<=0)
            {
                gradientPanelTimeIndicator.Visible = false;
                return;
            }

            gradientPanelTimeIndicator.Visible = true;
            gradientPanelTimeIndicator.BringToFront();
            toolTip1.SetToolTip(gradientPanelTimeIndicator, _timeBarDateTime.ToShortTimeString());
            toolTip1.SetToolTip(tableLayoutPanelTimeIndicator, _timeBarDateTime.ToShortTimeString());
            toolTip1.SetToolTip(gradientPanelCurrentTime, _timeBarDateTime.ToShortTimeString());
            
            gradientPanelTimeIndicator.Width = width;
            if (RightToLeft == RightToLeft.Yes)
            {
                gradientPanelTimeIndicator.Left = x1 - gradientPanelTimeIndicator.Width;
            }
            else
            {
                gradientPanelTimeIndicator.Left = x1;
            }
        }

        private void ResizeLabels()
        {
            foreach (var layerLabel in _layerLabelCollection)
            {
                int x1 = GetPixelFromDateTime(layerLabel.Layer.StartTime);
                int x2 = GetPixelFromDateTime(layerLabel.Layer.EndTime);
                layerLabel.Height = Height - gradientPanelHeader.Bottom + 2;
                layerLabel.Width = Math.Max(Math.Abs(x2 - x1),1);
                if (RightToLeft == RightToLeft.Yes)
                    layerLabel.Left = x1 - layerLabel.Width;
                else
                    layerLabel.Left = x1;
            }
        }

        private void DrawControlScale(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF cellRectangle = ClientRectangle;
            DateTime pointer = _startDateTime.Date.AddHours(_startDateTime.Hour);
            while (pointer < _endDateTime)
            {
                float x = GetPixelFromDateTime(pointer);
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), cellRectangle.Left + x, cellRectangle.Top,
                               cellRectangle.Left + x, cellRectangle.Bottom);
                pointer = pointer.AddMinutes(30);
            }
        }

        private void DrawHeaderScale(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF cellRectangle = ClientRectangle;
            string time1 = DateTime.MinValue.Add(TimeSpan.FromHours(12)).ToShortTimeString();
            SizeF size1 = g.MeasureString(time1, Font);
            float lastX = 0;
            DateTime pointer = _startDateTime.Date.AddHours(_startDateTime.Hour);
            bool first = true;
            while (pointer < _endDateTime)
            {
                string time = pointer.ToShortTimeString();
                
                SizeF size = g.MeasureString(time, Font);
                float x = GetPixelFromDateTime(pointer);
                if (x > 0)
                    g.DrawLine(new Pen(Color.DarkGray, 1), x, e.ClipRectangle.Bottom - ((e.ClipRectangle.Height - size.Height) / 2), x, e.ClipRectangle.Bottom);
                pointer = pointer.AddHours(1);
                if (!first)
                {
                    if (Math.Abs(x - lastX) <= size1.Width)
                        continue;

                    if (x > 0)
                    {

                        if ((x + (size.Width/2)) < cellRectangle.Width)
                            g.DrawString(time, Font, Brushes.Black,
                                         new PointF(cellRectangle.Left + x - size.Width/2,
                                                    cellRectangle.Top + 5));
                    }

                }

                lastX = x;
                first = false;
            }
        }

        private int GetPixelFromDateTime(DateTime layerDateTime)
        {
            double spanningMinutes = _endDateTime.Subtract(_startDateTime).TotalMinutes;
            double pixelPerMinute = Width / spanningMinutes;
            int px = ((int)((int)(layerDateTime.Subtract(_startDateTime).TotalMinutes) * pixelPerMinute));
            if (RightToLeft != RightToLeft.Yes)
                return px;

            return Width - px;
        }

        private void LayerVisualizer_RightToLeftChanged(object sender, EventArgs e)
        {
            gradientPanelTimeIndicator.RightToLeft = RightToLeft;
            ResizeLabels();
            ResizeProgress();
        }

        private void gradientPanelHeader_Paint(object sender, PaintEventArgs e)
        {
            DrawHeaderScale(e);
        }
    }
}