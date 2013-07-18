using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public partial class VisualizeView : PayrollBaseUserControl
    {
        private float _pixelSize;
        private float _widthPerHour;
        private const int TotalHoursToDraw = 36;
        private readonly ToolTip _visualizeToolTip = new ToolTip();
        private StringFormat _format = new StringFormat();
        private Point _mousePointlocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizeView"/> class.
        /// </summary>
        /// <param name="explorerView">The explorer view.</param>
        public VisualizeView(IExplorerView explorerView)
            : base(explorerView)
        {
            InitializeComponent();
            SetTexts();
            CreateGridView();

            dateNavigateControlThinLayout1.SetSelectedDate(DateOnly.Today);
            dateNavigateControlThinLayout1.SelectedDateChanged += DateNavigateControl_SelectedDateChanged;

        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        public override void RefreshView()
        {
            SetVisualizeColumnWidth();
            if (ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
            {
                //Commented by shirng:is there any special processing has to happen in here?
                //ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Sort();
            }
            if (ExplorerView.ExplorerPresenter.Model.SelectedDate.HasValue)
            {
                ExplorerView.ExplorerPresenter.VisualizePresenter.LoadModel(
                    ExplorerView.ExplorerPresenter.Model.SelectedDate.Value,
                    StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);
                gcProjection.RowCount = 1;// ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count;
                gcProjection.ColCount = 1;
            }
            gcProjection.Invalidate();
            gcProjection[1, 0].CellValue = Resources.Final;
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public override void Reload()
        {
            RefreshView();
        }

        /// <summary>
        /// Creates the grid view.
        /// </summary>
        private void CreateGridView()
        {
            GridHelper.GridStyle(gcProjection);
            ReadOnlyCollection<SFGridColumnBase<VisualPayloadInfo>> columns = ConfigureGridColumns<VisualPayloadInfo>();

            var helper = new SFGridColumnGridHelper<VisualPayloadInfo>(gcProjection, columns,
                                                                       ExplorerView.ExplorerPresenter.VisualizePresenter
                                                                           .ModelCollection);
            helper.Dispose();

            gcProjection.Model.ColWidths.ResizeToFit(GridRangeInfo.Cols(0, gcProjection.ColCount));
        }

        private ReadOnlyCollection<SFGridColumnBase<T>> ConfigureGridColumns<T>()
        {
            IList<SFGridColumnBase<T>> columns = new List<SFGridColumnBase<T>>();
            gcProjection.Rows.HeaderCount = 0;
            CreateColumns(columns);
            gcProjection.RowCount = GridRowCount();
            gcProjection.ColCount = (columns.Count - 1);
            gcProjection.RowCount = 1;
            gcProjection.Cols.HeaderCount = 0;
            gcProjection.Rows.HeaderCount = 0;
            gcProjection.NumberedColHeaders = false;
            gcProjection.RowHeights[0] = 40;
            gcProjection.ColWidths[0] = 100;
            gcProjection.DefaultRowHeight = 30;
            gcProjection.CellDrawn += gcProjection_CellDrawn;
            gcProjection.CellMouseHover += gcProjection_CellMouseHover;
            return new ReadOnlyCollection<SFGridColumnBase<T>>(columns);
        }

        /// <summary>
        /// Handles the CellMouseHover event of the gcProjection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellMouseEventArgs"/> instance containing the event data.</param>
        void gcProjection_CellMouseHover(object sender, GridCellMouseEventArgs e)
        {
            ShowToolTip(e);
        }

        /// <summary>
        /// Handles the CellDrawn event of the gcProjection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        private void gcProjection_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (ExplorerView.ExplorerPresenter.Model.SelectedDate.HasValue)
            {
                DrawProjection(e);
                DrawTimeLine(e);
            }
        }

        /// <summary>
        /// Draws the time line.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString(System.IFormatProvider)")]
        private void DrawTimeLine(GridDrawCellEventArgs e)
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
                
                for (int i = 0; i <= (TotalHoursToDraw - 1); i++)
                {
                    float width = widthToReduce + (_widthPerHour * i);

                    int height = 10;
                    int textHeight = 28;

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
                    using (Pen pen = new Pen(Brushes.Black))
                    {
                        int totalHeight = height + lineX;
                        DrawLine(e.Graphics, width, lineX, width, totalHeight, pen);
                        DrawLine(e.Graphics, width + (_widthPerHour / 2), 22, width + (_widthPerHour / 2), 17, pen);
                        DrawLine(e.Graphics, e.Bounds.Left, 17, e.Bounds.Right, 17, pen);
                    }
                    if (!ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                        startDate = startDate.AddHours(1);
                    else
                        startDate = startDate.Subtract(TimeSpan.FromHours(1));
                    
                }
            }
            
        }

        /// <summary>
        /// Gets the layers.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private IList<VisualPayloadInfo> GetLayers()
        {
            IList<VisualPayloadInfo> projectionLayer = new List<VisualPayloadInfo>();
            for (int i = 1; i < ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count; i++)
            {
                projectionLayer.Add(ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection[i]);
            }
            return projectionLayer;
        }

        /// <summary>
        /// Draws the projection.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        private void DrawProjection(GridDrawCellEventArgs e)
        {
            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
                DateTime selectedDate = ExplorerView.ExplorerPresenter.Model.SelectedDate.Value;
                int widthToReduce = e.Bounds.X;

                DateTime prevDate = selectedDate.Subtract(TimeSpan.FromDays(1));
                DateTime left = prevDate.Date.AddHours(18);

                if (ExplorerView.ExplorerPresenter.Model.IsRightToLeft)
                {
                    prevDate = selectedDate.AddDays(1);
                    left = prevDate.Date.AddHours(6);
                }

                IList<VisualPayloadInfo> layers = GetLayers();
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

        /// <summary>
        /// Shows the tool tip.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellMouseEventArgs"/> instance containing the event data.</param>
        private void ShowToolTip(GridCellMouseEventArgs e)
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

        /// <summary>
        /// Creates the columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gridColumns">The grid columns.</param>
        private static void CreateColumns<T>(ICollection<SFGridColumnBase<T>> gridColumns)
        {
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("Name", 150, "Name"));
            gridColumns.Add(new SFGridVisualizeColumn<T>(string.Empty, " "));

        }

        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="pX">The p X.</param>
        /// <param name="pY">The p Y.</param>
        /// <param name="qX">The q X.</param>
        /// <param name="qY">The q Y.</param>
        /// <param name="pen">The pen.</param>
        private static void DrawLine(Graphics graphics, float pX, float pY, float qX, float qY, Pen pen)
        {
            PointF pointX = new PointF(pX, pY);
            PointF pointY = new PointF(qX, qY);
            graphics.DrawLine(pen, pointX, pointY);
        }

        /// <summary>
        /// Calculates and returns the grid row count
        /// </summary>
        /// <returns>Row count</returns>
        private int GridRowCount()
        {
            //we need a extra projection layer..+1 if for that.
            int sourceListCount = ExplorerView.ExplorerPresenter.VisualizePresenter.ModelCollection.Count;
            int gridHeaderCount = gcProjection.Rows.HeaderCount;
            return (sourceListCount + gridHeaderCount);
        }

        private void SetVisualizeColumnWidth()
        {
            gcProjection.ColWidths[0] = 150;
            int widthToAdd = gcProjection.ColWidths[0];
            float panelWidth = ExplorerView.GetWidthOfVisualizeControlContainer();
            float columnCount = 36;

            _widthPerHour = (panelWidth - widthToAdd) / columnCount;
            _pixelSize = _widthPerHour / 60;

            double width = columnCount * _pixelSize * 60;
            int newWidth = (int)width;

            gcProjection.ColWidths[1] = newWidth;
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the DateNavigateControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DateTime"/> instance containing the event data.</param>
        private void DateNavigateControl_SelectedDateChanged(object sender, Domain.Common.CustomEventArgs<DateOnly> e)
        {
            ExplorerView.ExplorerPresenter.Model.SetSelectedDate(e.Value);
            RefreshView();
        }
    }
}
