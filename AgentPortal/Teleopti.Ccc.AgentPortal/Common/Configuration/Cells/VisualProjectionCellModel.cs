using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Schedules;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Cells
{
    [Serializable]
    public class VisualProjectionCellModel : GridStaticCellModel
    {
        public VisualProjectionCellModel(GridModel grid)
            : base(grid)
        {
        }

        protected VisualProjectionCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");

            //Hmm...
            info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new VisualProjectionCellRenderer(control, this);
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

    public class VisualProjectionCellRenderer : GridStaticCellRenderer
    {
        readonly IDictionary<int,IList<ToolTipData>> _toolTipLists = new Dictionary<int, IList<ToolTipData>>();
        private readonly ToolTip _toolTip = new ToolTip();
        private int _lastRow;
        private CultureInfo _currentCultureInfo;

        public VisualProjectionCellRenderer(GridControlBase grid, GridCellModelBase cellModel )
            : base(grid, cellModel)
        {
            PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            _currentCultureInfo = (person.CultureLanguageId.HasValue
                                            ? CultureInfo.GetCultureInfo(person.CultureLanguageId.Value)
                                            : CultureInfo.CurrentCulture).FixPersianCulture();
        }

        protected override void OnMouseHover(int rowIndex, int colIndex, MouseEventArgs e)
        {
            if (_lastRow != rowIndex)
            {
                _toolTip.SetToolTip(Grid,"");
                _toolTip.Hide(Grid);
                _lastRow = rowIndex;
            }
                
            string theToolTip = GetToolTip(rowIndex, e.X);
            if (theToolTip != _toolTip.GetToolTip(Grid))
            {
                _toolTip.Hide(Grid);
                if (!String.IsNullOrEmpty(theToolTip) )
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

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            TimePeriod timePeriod = (TimePeriod)style.Tag;
            PixelConverter pixelConverter = new PixelConverter(clientRectangle.Width, timePeriod, Grid.IsRightToLeft());

            for (int i = (int)timePeriod.StartTime.TotalHours; i <= (int)timePeriod.EndTime.TotalHours; i++)
            {
                int x = pixelConverter.GetPixelFromTimeSpan(TimeSpan.FromHours(i));
                if (x > 0)
                    g.DrawLine(new Pen(Color.LightGray, 1), clientRectangle.Left + x, clientRectangle.Top, clientRectangle.Left + x, clientRectangle.Bottom);
            }

            if (_toolTipLists.ContainsKey(rowIndex))
                _toolTipLists.Remove(rowIndex);
            
            IList<ToolTipData> tipDatas = new List<ToolTipData>();

            foreach (ActivityVisualLayer layer in ((IList<ActivityVisualLayer>)style.CellValue))
            {
                RectangleF rect = getLayerRectangle(pixelConverter, layer.Period, clientRectangle);
                if (!rect.IsEmpty)
                {
                    RectangleF rect2 = rect;
                    rect2.Inflate(1, 1);
                    using (Brush brush = new LinearGradientBrush(rect2, Color.WhiteSmoke, layer.Color, 90, false))
                    {
                        g.FillRectangle(brush, rect);
                        ToolTipData tipData = new ToolTipData(rect.X, rect.X + rect.Width, layer.Description + "  " + layer.Period.ToShortTimeString(_currentCultureInfo));
                        tipDatas.Add(tipData);
                    }
                }
            }
            _toolTipLists.Add(rowIndex, tipDatas);
        }

        private RectangleF getLayerRectangle(PixelConverter pixelConverter, TimePeriod period, RectangleF clientRect)
        {
            if (clientRect.Height < 9)
                return RectangleF.Empty;

            int x1 = pixelConverter.GetPixelFromTimeSpan(period.StartTime);
            int x2 = pixelConverter.GetPixelFromTimeSpan(period.EndTime);

            if (Grid.IsRightToLeft())
            {
                int tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (x2 - x1 < 1)
                return RectangleF.Empty;
            return new RectangleF(clientRect.Left + x1, clientRect.Top + 4, x2 - x1, clientRect.Height - 8);
        }
    }
    public class ToolTipData
        {
        private readonly float _fromX;
        private readonly float _toX;
        private readonly string _theToolTip;

        public ToolTipData(float fromX, float toX, string theToolTip)
        {
            _fromX = fromX;
            _toX = toX;
            _theToolTip = theToolTip;
        }

        public float FromX { get { return _fromX; } }
        public float ToX { get { return _toX; } }
         public string TheToolTip { get { return _theToolTip; } }
        }
}