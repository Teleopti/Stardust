using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
    [Serializable]
    public class ScheduleCellModel : GridStaticCellModel
    {

        public ScheduleCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected ScheduleCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new ScheduleCellCellRenderer(control, this);
        }


        public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
        {
            return false;
        }

        public override bool ApplyText(GridStyleInfo style, string text)
        {
            return ApplyFormattedText(style, text, -1);
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

    }

    public class ScheduleCellCellRenderer : GridStaticCellRenderer
    {
        readonly IDictionary<int, IList<ToolTipData>> _toolTipLists = new Dictionary<int, IList<ToolTipData>>();
        private readonly ToolTip _toolTip = new ToolTip();
        private int _lastRow;

        public ScheduleCellCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
            : base(grid, cellModel)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        protected override void OnMouseHover(int rowIndex, int colIndex, MouseEventArgs e)
        {
            if (_lastRow != rowIndex)
            {
                _toolTip.SetToolTip(Grid, "");
                _toolTip.Hide(Grid);
                _lastRow = rowIndex;
            }

            var theToolTip = GetToolTip(rowIndex, e.X);
            if (theToolTip != _toolTip.GetToolTip(Grid))
            {
                _toolTip.Hide(Grid);
                if (!String.IsNullOrEmpty(theToolTip))
                    _toolTip.Show(theToolTip, Grid);
            }
            base.OnMouseMove(rowIndex, colIndex, e);
        }

        private string GetToolTip(int rowIndex, float x)
        {
            string ret = "";
            IList<ToolTipData> tipDatas;
            if (_toolTipLists.TryGetValue(rowIndex, out tipDatas))
                foreach (var tipData in tipDatas)
                {
                    if (tipData.FromX <= x && tipData.ToX >= x)
                        ret = tipData.TheToolTip;
                }

            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            clientRectangle = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width + 1, clientRectangle.Height); //sync with header +1 width

            if (style.Tag == null) return;
            var timePeriod = (DateTimePeriod)style.Tag;
            var pixelConverter = new LengthToTimeCalculator(timePeriod, clientRectangle.Width);

            if (_toolTipLists.ContainsKey(rowIndex)) _toolTipLists.Remove(rowIndex);
            IList<ToolTipData> tipDatas = new List<ToolTipData>();

            var scheduleDisplayRow = style.CellValue as ScheduleDisplayRow;
            if (scheduleDisplayRow == null) return;

            DrawDayOff(g, scheduleDisplayRow, timePeriod, pixelConverter, clientRectangle, tipDatas);
            DrawScheduleDay(g, scheduleDisplayRow, pixelConverter, clientRectangle, tipDatas);
            DrawScheduleDayBefore(g, scheduleDisplayRow, timePeriod, pixelConverter, clientRectangle, tipDatas);
            DrawScheduleDayAfter(g, scheduleDisplayRow, timePeriod, pixelConverter, clientRectangle, tipDatas);
            DrawMeeting(g, scheduleDisplayRow, pixelConverter, clientRectangle);
            
     
            _toolTipLists.Add(rowIndex, tipDatas);
        }

        private void DrawMeeting(Graphics g, ScheduleDisplayRow scheduleDisplayRow, LengthToTimeCalculator pixelConverter, Rectangle clientRectangle)
        {
            var rect = GetLayerRectangle(pixelConverter, scheduleDisplayRow.MeetingPeriod, clientRectangle);
            
            var color = Color.FromArgb(50, 120, 120, 120);
            
            using(var pen1 = new Pen(Brushes.LightGreen, 3))
            {
                g.DrawLine(pen1, new Point((int)rect.X + 1, clientRectangle.Y - 1), new Point((int)rect.X + 1, clientRectangle.Bottom + 1));      
            }

            using(var pen2 = new Pen(Brushes.IndianRed, 3))
            {
                g.DrawLine(pen2, new Point((int)rect.Right - 1, clientRectangle.Y - 1), new Point((int)rect.Right - 1, clientRectangle.Bottom + 1));   
            }
                
            using(var pen3 = new Pen(Brushes.Black, 1))
            {
                g.DrawLine(pen3, new Point((int)rect.X, clientRectangle.Y - 1), new Point((int)rect.X, clientRectangle.Bottom + 1));
                g.DrawLine(pen3, new Point((int)rect.Right, clientRectangle.Y - 1), new Point((int)rect.Right, clientRectangle.Bottom + 1));   
            }

            using (var brush = new SolidBrush(color))
            {
                rect.Inflate(-1, 0);
                g.FillRectangle(brush, rect);
            }
        }


        private void DrawDayOff(Graphics g, ScheduleDisplayRow scheduleDisplayRow, DateTimePeriod timePeriod, LengthToTimeCalculator pixelConverter, Rectangle clientRectangle, IList<ToolTipData> tipDatas)
        {
            if (scheduleDisplayRow.ScheduleDay.SignificantPart() == SchedulePartView.DayOff)
            {

				var personDayOff = scheduleDisplayRow.ScheduleDay.PersonAssignment().DayOff();

                var start = timePeriod.StartDateTime;
                var end = timePeriod.EndDateTime;

                if(end.Subtract(start) > TimeSpan.FromHours(24)) end = start.AddDays(1);
               
                var period = new DateTimePeriod(start, end);

                var rect = GetLayerRectangle(pixelConverter, period, clientRectangle);
                rect.Inflate(0, -4);

                var shortName = personDayOff.Description.ShortName;
                var stringMeasure = g.MeasureString(shortName, Grid.Font);
                var x = (int)(rect.Width / 2 - stringMeasure.Width / 2 + rect.Left);
                var y = (int)(rect.Height / 2 - stringMeasure.Height / 2 + rect.Top);
                var point = new Point(x, y);


                if (!rect.IsEmpty)
                {
                    var rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, personDayOff.DisplayColor, Color.LightGray))
                    {
                        g.FillRectangle(brush, rect);
                        g.DrawString(shortName, Grid.Font, Brushes.Black, point);
                        var tipData = new ToolTipData(rect.X, rect.X + rect.Width, UserTexts.Resources.DayOff);
                        tipDatas.Add(tipData);
                    }
                }
            }    
        }

        private void DrawScheduleDay(Graphics g, ScheduleDisplayRow scheduleDisplayRow, LengthToTimeCalculator pixelConverter, Rectangle clientRectangle, IList<ToolTipData> tipDatas)
        {
            var list = new List<IVisualLayer>();
            list.AddRange(scheduleDisplayRow.ScheduleDay.ProjectionService().CreateProjection());

            foreach (var layer in list)
            {
                var rect = GetLayerRectangle(pixelConverter, layer.Period, clientRectangle);

                rect.Inflate(0, -4);
                var upperRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height / 2 - 5);
                var lowerRect = new RectangleF(rect.X, rect.Y + rect.Height / 2 - 5, rect.Width, rect.Height / 2 + 5);
                if (!rect.IsEmpty)
                {
                    var rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (
                        Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, layer.Payload.ConfidentialDisplayColor(scheduleDisplayRow.ScheduleDay.Person), 90, false))
                    {
                        g.FillRectangle(brush, upperRect);
                        var tipData = new ToolTipData(rect.X, rect.X + rect.Width,
                                                      layer.Payload.ConfidentialDescription(scheduleDisplayRow.ScheduleDay.Person) + "  " +
                                                      layer.Period.TimePeriod(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone).ToShortTimeString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture));
                        tipDatas.Add(tipData);
                    }

                    using (var sBrush = new SolidBrush(layer.Payload.ConfidentialDisplayColor(scheduleDisplayRow.ScheduleDay.Person)))
                    {
                        g.FillRectangle(sBrush, lowerRect);
                    }
                }
            }   
        }

        private void DrawScheduleDayBefore(Graphics g, ScheduleDisplayRow scheduleDisplayRow, DateTimePeriod timePeriod, LengthToTimeCalculator pixelConverter, Rectangle clientRectangle, IList<ToolTipData> tipDatas)
        {
            var list = new List<IVisualLayer>();
            list.AddRange(scheduleDisplayRow.ScheduleDayBefore.ProjectionService().CreateProjection());

            foreach (var layer in list)
            {
                var start = layer.Period.StartDateTime;
                var end = layer.Period.EndDateTime;

                if (layer.Period.EndDateTime < timePeriod.StartDateTime) continue;

                if (layer.Period.StartDateTime < timePeriod.StartDateTime)
                {
                    start = timePeriod.StartDateTime;
                }

                var period = new DateTimePeriod(start, end);

                var rect = GetLayerRectangle(pixelConverter, period, clientRectangle);

                rect.Inflate(0, -4);
                var upperRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height / 2 - 5);
                var lowerRect = new RectangleF(rect.X, rect.Y + rect.Height / 2 - 5, rect.Width, rect.Height / 2 + 5);
                if (!rect.IsEmpty)
                {
                    var color = Color.FromArgb(80, layer.Payload.ConfidentialDisplayColor(scheduleDisplayRow.ScheduleDayBefore.Person));
                    var colorSmoke = Color.FromArgb(80, Color.WhiteSmoke);
                    var rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (
                        Brush brush = new LinearGradientBrush(rect2, colorSmoke, color, 90, false))
                    {
                        g.FillRectangle(brush, upperRect);
                        var tipData = new ToolTipData(rect.X, rect.X + rect.Width,
                                                      layer.Payload.ConfidentialDescription(scheduleDisplayRow.ScheduleDayBefore.Person) + "  " +
                                                      period.TimePeriod(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone).ToShortTimeString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture));
                        tipDatas.Add(tipData);
                    }



                    using (var sBrush = new SolidBrush(color))
                    {
                        g.FillRectangle(sBrush, lowerRect);
                    }
                }
            }    
        }

        private void DrawScheduleDayAfter(Graphics g, ScheduleDisplayRow scheduleDisplayRow, DateTimePeriod timePeriod, LengthToTimeCalculator pixelConverter, Rectangle clientRectangle, IList<ToolTipData> tipDatas)
        {
            var list = new List<IVisualLayer>();
            list.AddRange(scheduleDisplayRow.ScheduleDayAfter.ProjectionService().CreateProjection());

            foreach (var layer in list)
            {
                var start = layer.Period.StartDateTime;
                var end = layer.Period.EndDateTime;

                if (layer.Period.StartDateTime > timePeriod.EndDateTime) continue;

                if (layer.Period.EndDateTime > timePeriod.EndDateTime)
                {
                    end = timePeriod.EndDateTime;
                }

                var period = new DateTimePeriod(start, end);

                var rect = GetLayerRectangle(pixelConverter, period, clientRectangle);

                rect.Inflate(0, -4);
                var upperRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height / 2 - 5);
                var lowerRect = new RectangleF(rect.X, rect.Y + rect.Height / 2 - 5, rect.Width, rect.Height / 2 + 5);
                if (!rect.IsEmpty)
                {
                    var color = Color.FromArgb(80, layer.Payload.ConfidentialDisplayColor(scheduleDisplayRow.ScheduleDayAfter.Person));
                    var colorSmoke = Color.FromArgb(80, Color.WhiteSmoke);
                    var rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (
                        Brush brush = new LinearGradientBrush(rect2, colorSmoke, color, 90, false))
                    {
                        g.FillRectangle(brush, upperRect);
                        var tipData = new ToolTipData(rect.X, rect.X + rect.Width,
                                                      layer.Payload.ConfidentialDescription(scheduleDisplayRow.ScheduleDayAfter.Person) + "  " +
                                                      period.TimePeriod(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone).ToShortTimeString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture));
                        tipDatas.Add(tipData);
                    }

                    using (var sBrush = new SolidBrush(color))
                    {
                        g.FillRectangle(sBrush, lowerRect);
                    }
                }
            }    
        }

        private RectangleF GetLayerRectangle(LengthToTimeCalculator pixelConverter, DateTimePeriod period, RectangleF clientRect)
        {
            var x1 = pixelConverter.PositionFromDateTime(period.StartDateTime, Grid.IsRightToLeft());
				var x2 = pixelConverter.PositionFromDateTime(period.EndDateTime, Grid.IsRightToLeft());

            if (Grid.IsRightToLeft())
            {
                var tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (x2 - x1 < 1)
                return RectangleF.Empty;
            return new RectangleF((float)(clientRect.Left + x1), clientRect.Top, (float)(x2 - x1), clientRect.Height);
        }
    }
}

