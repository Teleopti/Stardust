using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.DefinitionSets
{
    public partial class VisualizeView : PayrollBaseUserControl
    {
        private float _pixelSize;
        private float _widthPerHour;
        private const int totalHoursToDraw = 36;
        private readonly ToolTip _visualizeToolTip = new ToolTip();
        private readonly StringFormat _format = new StringFormat();
        private Point _mousePointlocation;

        public VisualizeView()
        {
            InitializeComponent();
            
        }

        public VisualizeView(IExplorerView explorerView)
            : base(explorerView)
        {
            InitializeComponent();
            SetTexts();
            createGridView();

            dateNavigateControlThinLayout1.SetSelectedDate(DateOnly.Today);
            dateNavigateControlThinLayout1.SelectedDateChanged += dateNavigateControlSelectedDateChanged;
            tableLayoutPanel4.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
	        labelMultiplicatorVisualizer.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

        }

        public override void RefreshView()
        {
            setVisualizeColumnWidth();
            if (ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
            {
                //Commented by shirng:is there any special processing has to happen in here?
                //ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Sort();
            }
            if (ExplorerView.ExplorerPresenter.Model.SelectedDate.HasValue)
            {
                ExplorerView.ExplorerPresenter.VisualizePresenter.LoadModel(
                    ExplorerView.ExplorerPresenter.Model.SelectedDate.Value,
										TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
                gcProjection.RowCount = 1;// ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count;
                gcProjection.ColCount = 1;
            }
            gcProjection.Invalidate();
            gcProjection[1, 0].CellValue = Resources.Final;
        }

        public override void Reload()
        {
            RefreshView();
        }

        private void createGridView()
        {
            GridHelper.GridStyle(gcProjection);
            ReadOnlyCollection<SFGridColumnBase<VisualPayloadInfo>> columns = configureGridColumns<VisualPayloadInfo>();

            var helper = new SFGridColumnGridHelper<VisualPayloadInfo>(gcProjection, columns,
                                                                       ExplorerView.ExplorerPresenter.VisualizePresenter
                                                                           .ModelCollection);
            helper.Dispose();

            gcProjection.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, gcProjection.ColCount));
        }

        private ReadOnlyCollection<SFGridColumnBase<T>> configureGridColumns<T>()
        {
            IList<SFGridColumnBase<T>> columns = new List<SFGridColumnBase<T>>();
            gcProjection.Rows.HeaderCount = 0;
            createColumns(columns);
            gcProjection.RowCount = gridRowCount();
            gcProjection.ColCount = (columns.Count - 1);
            gcProjection.RowCount = 1;
            gcProjection.Cols.HeaderCount = 0;
            gcProjection.Rows.HeaderCount = 0;
            gcProjection.NumberedColHeaders = false;
            gcProjection.RowHeights[0] = 40;
            gcProjection.ColWidths[0] = 100;
            gcProjection.DefaultRowHeight = 30;
            gcProjection.CellDrawn += gcProjectionCellDrawn;
            gcProjection.CellMouseHover += gcProjectionCellMouseHover;
            return new ReadOnlyCollection<SFGridColumnBase<T>>(columns);
        }

        void gcProjectionCellMouseHover(object sender, GridCellMouseEventArgs e)
        {
            showToolTip(e);
        }

        private void gcProjectionCellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (ExplorerView.ExplorerPresenter.Model.SelectedDate.HasValue)
            {
                drawProjection(e);
                drawTimeLine(e);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString(System.IFormatProvider)")]
        private void drawTimeLine(GridDrawCellEventArgs e)
        {
            if (e.ColIndex == 1 && (e.RowIndex == 0 || e.RowIndex == 1))
            {
                int widthToReduce = e.Bounds.X;
                DateTime startDateTime = ExplorerView.ExplorerPresenter.Model.SelectedDate.Value.Date.Subtract(TimeSpan.FromHours(6));
                DateTime endDateTime = ExplorerView.ExplorerPresenter.Model.SelectedDate.Value.Date.AddDays(1).AddHours(6);

                DateTime startDate = startDateTime;
                DateTime endDate = endDateTime;
                if (ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                {
                    startDate = endDateTime;
                    endDate = startDateTime;
                }

                float remainWidth = e.Bounds.X;
                
                for (int i = 0; i <= (totalHoursToDraw - 1); i++)
                {
                    float width = widthToReduce + (_widthPerHour * i);

                    int height = 10;
                    const int textHeight = 28;

                    int lineX = 17;

                    if ((i == 0) || (i == 6) || (i == 30) || (i == 36))
                    {
                        height = 38;
                       
                        lineX = 0;

                        
                        float blockSize = 0;
                        DateTime date = startDate;
                        if (i == 0)
                        {
                            blockSize = 6 * _widthPerHour;
                        }
                        else if (i == 30)
                        {
                            blockSize = 6 * _widthPerHour;
                            date = endDate;
                        }
                        else if (i == 6)
                        {
                            blockSize = 24 * _widthPerHour;
                            date = ExplorerView.ExplorerPresenter.Model.SelectedDate.Value.Date;
                        }

                        string weekDayDate = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", LanguageResourceHelper.TranslateEnumValue(date.DayOfWeek), date.ToShortDateString());
                        SizeF size = e.Graphics.MeasureString(weekDayDate, new Font("Arial", 7));

                        e.Graphics.DrawString(weekDayDate,
                                              new Font("Arial", 7),
                                              Brushes.Black,
                                              new PointF(remainWidth + (blockSize / 2 - size.Width / 2), 3));
                        
                        remainWidth += blockSize;
                        
                    }
                    
                    string hour = startDate.TimeOfDay.Hours.ToString(CultureInfo.CurrentUICulture);
                    SizeF hourSize = e.Graphics.MeasureString(hour, new Font("Arial", 7));
                   
                    float f = (width + (_widthPerHour/2)) - (hourSize.Width/2)+1;
                    e.Graphics.DrawString(hour, new Font("Arial", 7), Brushes.Black, new PointF(f, textHeight));
                    using (var pen = new Pen(Brushes.Black))
                    {
                        int totalHeight = height + lineX;
                        drawLine(e.Graphics, width, lineX, width, totalHeight, pen);
                        drawLine(e.Graphics, width + (_widthPerHour / 2), 22, width + (_widthPerHour / 2), 17, pen);
                        drawLine(e.Graphics, e.Bounds.Left, 17, e.Bounds.Right, 17, pen);
                    }
                    if (!ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                        startDate = startDate.AddHours(1);
                    else
                        startDate = startDate.Subtract(TimeSpan.FromHours(1));
                    
                }
            }
            
        }

        private IList<VisualPayloadInfo> getLayers()
        {
            IList<VisualPayloadInfo> projectionLayer = new List<VisualPayloadInfo>();
            for (int i = 1; i < ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count; i++)
            {
                projectionLayer.Add(ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection[i]);
            }
            return projectionLayer;
        }

        private void drawProjection(GridDrawCellEventArgs e)
        {
            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
                var selectedDate = ExplorerView.ExplorerPresenter.Model.SelectedDate.GetValueOrDefault(DateOnly.Today).Date;
                int widthToReduce = e.Bounds.X;

                DateTime prevDate = selectedDate.Subtract(TimeSpan.FromDays(1));
                DateTime left = prevDate.Date.AddHours(18);

                if (ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                {
                    prevDate = selectedDate.AddDays(1);
                    left = prevDate.Date.AddHours(6);
                }

                IList<VisualPayloadInfo> layers = getLayers();
                int count = layers.Count;
                for (int i = 0; i <= (count - 1); i++)
                {
                    float layerBeginPosition;
                    VisualPayloadInfo viewModel = layers[i];

                    if (!ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                    {
                        layerBeginPosition = (float)(viewModel.StartTime.Subtract(left).TotalMinutes * _pixelSize);
                    }
                    else
                    {
                        layerBeginPosition = (float) (left.Subtract(viewModel.EndTime).TotalMinutes*_pixelSize);
                        _format.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                    }

                    float layerWidth = (float)viewModel.EndTime.Subtract(viewModel.StartTime).TotalMinutes * _pixelSize;

                    if (layerWidth > 0 && !float.IsInfinity(layerWidth))
                    {
                        Rectangle layer = e.Bounds;
                        layer.X = widthToReduce + (int)Math.Round(layerBeginPosition, 0);
                        clipLayerOutsideGrid(e, layerWidth, ref layer);

                        if (layer.Height != 0 && layer.Width != 0)
                        {
                            using (var linearGradientBrush = new LinearGradientBrush(layer, Color.WhiteSmoke, viewModel.Color, 90, false))
                            {
                                layer.Inflate(0,-2);
                                e.Graphics.FillRectangle(linearGradientBrush, layer);
                                
                                e.Graphics.DrawRectangle(new Pen(viewModel.Color), layer);
                                layer.Offset(0,4);
                                
                                e.Graphics.DrawString(string.Format(CultureInfo.CurrentUICulture, "{0} {1}", viewModel.ShortName,viewModel.Value), new Font("SegoeUI", 8), Brushes.Black, layer, _format);
                                viewModel.Bounds = layer;
                            }
                        }
                    }
                }
            }
        }

        private void clipLayerOutsideGrid(GridDrawCellEventArgs e, float layerWidth, ref Rectangle layer)
        {
            layer.Width = (int)Math.Round(layerWidth, 0);
            if (!ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
            {
                if (layer.X < e.Bounds.X)
                {
                    layer.Width = (int) Math.Round(layerWidth, 0) + (layer.X - e.Bounds.X); //negative
                    layer.X = e.Bounds.X;
                }
            }
            else
            {
                layer.Width = (int)Math.Round(layerWidth, 0);
                if (layer.X + layer.Width > e.Bounds.Width)
                {
                    layer.Width = e.Bounds.Width-layer.X+e.Bounds.X;
                }   
            }
        }

        private void showToolTip(GridCellMouseEventArgs e)
        {
            if (_mousePointlocation == e.MouseEventArgs.Location)
                return;

            _mousePointlocation = e.MouseEventArgs.Location;

            foreach (VisualPayloadInfo payloadInfo in ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection)
            {
                if (payloadInfo.Bounds.Contains(_mousePointlocation))
                {
                    Point point = Cursor.Position; point.X += 10; point.Y += 20;
                    string toolTipText = string.Format(CultureInfo.CurrentUICulture, "{0} {1}", payloadInfo.ShortName,
                                                       payloadInfo.Value);
                    _visualizeToolTip.Show(toolTipText, gcProjection, gcProjection.PointToClient(point), 2000);
                    break;
                }
            }
        }

        private static void createColumns<T>(ICollection<SFGridColumnBase<T>> gridColumns)
        {
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("Name", 150, "Name"));
            gridColumns.Add(new SFGridVisualizeColumn<T>(string.Empty, " "));

        }

        private static void drawLine(Graphics graphics, float pX, float pY, float qX, float qY, Pen pen)
        {
            var pointX = new PointF(pX, pY);
            var pointY = new PointF(qX, qY);
            graphics.DrawLine(pen, pointX, pointY);
        }

        private int gridRowCount()
        {
            //we need a extra projection layer..+1 if for that.
            int sourceListCount = ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count;
            int gridHeaderCount = gcProjection.Rows.HeaderCount;
            return (sourceListCount + gridHeaderCount);
        }

        private void setVisualizeColumnWidth()
        {
            gcProjection.ColWidths[0] = 150;
            int widthToAdd = gcProjection.ColWidths[0];
            float panelWidth = ExplorerView.GetWidthOfVisualizeControlContainer();
            const float columnCount = 36;

            _widthPerHour = (panelWidth - widthToAdd) / columnCount;
            _pixelSize = _widthPerHour / 60;

            double width = columnCount * _pixelSize * 60;
            var newWidth = (int)width;

            gcProjection.ColWidths[1] = newWidth;
        }

        private void dateNavigateControlSelectedDateChanged(object sender, Domain.Common.CustomEventArgs<DateOnly> e)
        {
            ExplorerView.ExplorerPresenter.Model.SetSelectedDate(e.Value);
            RefreshView();
        }
    }
}
