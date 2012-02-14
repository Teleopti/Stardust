#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;

#endregion

namespace Teleopti.Ccc.AgentPortal.Reports.Grid
{
    /// <summary>
    /// Represents a VisualPayloadInfo
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// </remarks>
    internal struct VisualPayloadInfo
    {
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly Color _color;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualPayloadInfo"/> struct.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="color">The color.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VisualPayloadInfo(DateTime startTime, DateTime endTime, Color color, string name)
        {
            _startTime = startTime;
            _endTime = endTime;
            _color = color;
            _name = name;
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public DateTime EndTime
        {
            get { return _endTime; }
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>The color.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public Color Color
        {
            get { return _color; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public string Name
        {
            get { return _name; }
        }
    }

    /// <summary>
    /// Represents the VisualGridDrawingInfo
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 2008-09-24
    /// </remarks>
    internal struct MyScheduleGridDrawingInfo
    {
        private readonly float _widthPerHour;
        private readonly float _pixelSize;
        private readonly TimeSpan _startTime;
        private readonly TimeSpan _endTime;
        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly bool _isRightToLeft;
        private readonly IList<IList<VisualPayloadInfo>> _visualPayloadInfoList;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyScheduleGridDrawingInfo"/> struct.
        /// </summary>
        /// <param name="widthPerHour">The width per hour.</param>
        /// <param name="pixelSize">Size of the pixel.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="columnCount">The column count.</param>
        /// <param name="rowCount">The row count.</param>
        /// <param name="isRightToLeft">if set to <c>true</c> [is right to left].</param>
        /// <param name="visualizeGridViewAdapters">The visualize grid view adapters.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MyScheduleGridDrawingInfo(float widthPerHour,
                                     float pixelSize,
                                     TimeSpan startTime,
                                     TimeSpan endTime,
                                     int columnCount,
                                     int rowCount,
                                     bool isRightToLeft,
                                     IList<IList<VisualPayloadInfo>> visualizeGridViewAdapters)
        {
            _widthPerHour = widthPerHour;
            _pixelSize = pixelSize;

            _startTime = startTime;
            _endTime = endTime;
            _columnCount = columnCount;
            _rowCount = rowCount;
            _isRightToLeft = isRightToLeft;
            _visualPayloadInfoList = visualizeGridViewAdapters;
        }

        /// <summary>
        /// Gets the width per hour.
        /// </summary>
        /// <value>The width per hour.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public float WidthPerHour
        {
            get { return _widthPerHour; }
        }

        /// <summary>
        /// Gets the size of the pixel.
        /// </summary>
        /// <value>The size of the pixel.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public float PixelSize
        {
            get { return _pixelSize; }
        }

        /// <summary>
        /// Gets the visualize grid view adapters.
        /// </summary>
        /// <value>The visualize grid view adapters.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public IList<IList<VisualPayloadInfo>> VisualizeGridViewAdapters
        {
            get { return _visualPayloadInfoList; }
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public TimeSpan StartTime
        {
            get { return _startTime; }
        }

        ///// <summary>
        ///// Gets the end time.
        ///// </summary>
        ///// <value>The end time.</value>
        ///// <remarks>
        ///// Created by:VirajS
        ///// Created date: 2008-09-24
        ///// </remarks>
        ////[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        //public TimeSpan EndTime
        //{
        //    get { return EndTime; }
        //}

        /// <summary>
        /// Gets the column count.
        /// </summary>
        /// <value>The column count.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public int ColumnCount
        {
            get { return _columnCount; }
        }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        /// <value>The row count.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        public int RowCount
        {
            get { return _rowCount; }
        }
    }

    /// <summary>
    /// Represents a IGridDataManager
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// </remarks>
    internal interface IGridDataManager
    {
        void SetVisualGridDrawingInfo(MyScheduleGridDrawingInfo info);
    }

    /// <summary>
    /// Represents a VisualizeGridView class
    /// </summary>
    internal class MyScheduleGrid : GridViewBase, IGridDataManager
    {
        #region Fields - Instance Member

        private readonly IList<IColumn<MyScheduleGridAdapter>> _gridColumns = new List<IColumn<MyScheduleGridAdapter>>();

        private readonly ToolTip _visualizeToolTip = new ToolTip();

        private readonly IList<MyScheduleGridAdapter> _empty = new List<MyScheduleGridAdapter>();

        private RowHeaderColumn<MyScheduleGridAdapter> _rowHeaderColumn;

        private MyScheduleGridDrawingInfo _myScheduleGridDrawingInfo;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - VisualizeGridView Members

        #endregion

        #endregion

        #region Events - Instance Member

        /// <summary>
        /// Handles the CellDrawn event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private void Grid_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            DrawTimeLine(e);
            DrawShifts(e);
        }

        /// <summary>
        /// Handles the CellMouseHover event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellMouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private void Grid_CellMouseHover(object sender, GridCellMouseEventArgs e)
        {
            ShowShiftToolTipText(e);
        }

        /// <summary>
        /// Handles the LostFocus event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private void Grid_LostFocus(object sender, EventArgs e)
        {
            _visualizeToolTip.RemoveAll();
        }

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - VisualizeGridView Members

        /// <summary>
        /// Create column headers
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override void CreateHeaders()
        {
            _gridColumns.Clear();

            _rowHeaderColumn = new RowHeaderColumn<MyScheduleGridAdapter>();
            _gridColumns.Add(_rowHeaderColumn);

            MyScheduleGridColumn<MyScheduleGridAdapter> column =
                new MyScheduleGridColumn<MyScheduleGridAdapter>("MyScheduleLayerCollection", " ");
            _gridColumns.Add(column);
            Grid.CellModels.Model.TableStyle.Borders.All = new GridBorder(GridBorderStyle.None);

            Grid.CellDrawn += Grid_CellDrawn;
            Grid.CellMouseHover += Grid_CellMouseHover;
            Grid.LostFocus += Grid_LostFocus;
        }

        /// <summary>
        /// Prepares the view
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override void PrepareView()
        {
            ColCount = _gridColumns.Count;

            Grid.RowCount = _myScheduleGridDrawingInfo.RowCount;
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;

            Grid.Name = "";
        }

        /// <summary>
        /// Query the cell info
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                _gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<MyScheduleGridAdapter>(_empty));
            }

            e.Handled = true;
        }

        /// <summary>
        /// Saves the cell info.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                _gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<MyScheduleGridAdapter>(_empty));
            }
        }

        /// <summary>
        /// Merge the headers
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override void MergeHeaders()
        {
        }

        /// <summary>
        /// Draws the time line.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private void DrawTimeLine(GridDrawCellEventArgs e)
        {
            int numberOfColumns;
            int widthToReduce;
            Pen pen = new Pen(Brushes.Black);
            Font font = new Font("Arial", 7);

            if (_myScheduleGridDrawingInfo.RowCount == 0)
                return;

            numberOfColumns = _myScheduleGridDrawingInfo.ColumnCount;
            if (numberOfColumns == 0)
            {
                return;
            }

            widthToReduce = Grid.ColWidths[0];

            if (e.ColIndex == 1 && (e.RowIndex == 0 || e.RowIndex == 1))
            {
                int startHour = _myScheduleGridDrawingInfo.StartTime.Hours;
                TimeSpan startSpan = _myScheduleGridDrawingInfo.StartTime;
                for (int i = 0; i <= (numberOfColumns); i++)
                {
                    float width = widthToReduce + _myScheduleGridDrawingInfo.WidthPerHour*(i);
                    float timeSlotWidth = _myScheduleGridDrawingInfo.PixelSize*15;

                    DrawLine(e.Graphics, width, 0, width, e.Bounds.Height, pen);

                    pen.Width = 1;
                    if (i <= (numberOfColumns - 1))
                    {
                        DrawLine(e.Graphics, (width + timeSlotWidth), 0, (width + timeSlotWidth), 4, pen);
                        DrawLine(e.Graphics, (width + timeSlotWidth*2), 0, (width + timeSlotWidth*2), 4, pen);
                        DrawLine(e.Graphics, (width + timeSlotWidth*3), 0, (width + timeSlotWidth*3), 4, pen);

                        //string hour = (startHour < 10) ? "0" + startHour.ToString(CultureInfo.CurrentUICulture) :
                        //                                startHour.ToString(CultureInfo.CurrentUICulture);
                        //hour += ":00";
                        string hour = new DateTime().Add(startSpan).ToString("t", CultureInfo.CurrentCulture);

                        SizeF hourSize = e.Graphics.MeasureString(hour, font);
                        e.Graphics.DrawString(hour,
                                              font,
                                              Brushes.Black,
                                              new PointF((width + timeSlotWidth*2) - (hourSize.Width/2), 6));
                    }
                    if (startHour == 24)
                    {
                        startHour = 1;
                    }
                    else
                    {
                        startHour++;
                        startSpan = startSpan.Add(new TimeSpan(1, 0, 0));
                    }
                }
            }
        }

        /// <summary>
        /// Draws the shifts.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        private void DrawShifts(GridDrawCellEventArgs e)
        {
            if (_myScheduleGridDrawingInfo.RowCount == 0)
                return;
            if (_myScheduleGridDrawingInfo.RowCount <= (e.RowIndex - 1))
                return;

            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
                int widthToReduce = Grid.ColWidths[0];

                IList<VisualPayloadInfo> payLoadInfoList =
                    _myScheduleGridDrawingInfo.VisualizeGridViewAdapters[e.RowIndex - 1];

                TimeSpan startTime = new TimeSpan(_myScheduleGridDrawingInfo.StartTime.Hours, 0, 0);

                for (int i = 0; i <= (payLoadInfoList.Count - 1); i++)
                {
                    VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
                    float width;
                    TimeSpan timeDiff;
                    if (startTime > payLoadInfo.StartTime.TimeOfDay)
                    {
                        timeDiff = payLoadInfo.StartTime.Subtract(startTime).TimeOfDay;
                    }
                    else
                    {
                        timeDiff = startTime.Subtract(payLoadInfo.StartTime.TimeOfDay);
                    }

                    float layerLength = (timeDiff.Hours*_myScheduleGridDrawingInfo.WidthPerHour) +
                                        timeDiff.Minutes*_myScheduleGridDrawingInfo.PixelSize;
                    if (layerLength < 0)
                    {
                        layerLength *= (-1);
                    }

                    TimeSpan layerPeriodDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
                    width = (float) Math.Round(layerPeriodDiff.TotalMinutes, 0);
                    width *= _myScheduleGridDrawingInfo.PixelSize;

                    if (width > 0 && !float.IsInfinity(width))
                    {
                        RectangleF rectangle = e.Bounds;
                        rectangle.X = widthToReduce + layerLength;
                        rectangle.Width = width;

                        Color layerColor = payLoadInfo.Color;

                        using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle,
                                                                                                 Color.WhiteSmoke,
                                                                                                 layerColor,
                                                                                                 90,
                                                                                                 false))
                        {
                            e.Graphics.FillRectangle(linearGradientBrush, rectangle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the shift tool tip text.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridCellMouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-24
        /// </remarks>
        private void ShowShiftToolTipText(GridCellMouseEventArgs e)
        {
            if (_myScheduleGridDrawingInfo.RowCount == 0)
                return;
            if ((e.RowIndex - 1) >= _myScheduleGridDrawingInfo.RowCount)
                return;

            PointF location = e.MouseEventArgs.Location;

            if ((location.Y >= (Grid.Height - 1)) || (location.Y <= Grid.RowHeights[0]))
            {
                _visualizeToolTip.RemoveAll();
                return;
            }

            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
                IList<VisualPayloadInfo> payLoadInfoList =
                    _myScheduleGridDrawingInfo.VisualizeGridViewAdapters[e.RowIndex - 1];

                float widthToReduce = Grid.ColWidths[0];
                float layerLength = 0;
                int index = 0;
                bool checkForPrevious = true;

                TimeSpan startTime = new TimeSpan(_myScheduleGridDrawingInfo.StartTime.Hours, 0, 0);

                bool found = false;
                for (int i = 0; i <= (payLoadInfoList.Count - 1); i++)
                {
                    VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
                    if (index == 0)
                    {
                        int initialDifference = startTime.CompareTo(payLoadInfo.StartTime.TimeOfDay);
                        if (initialDifference < 0 && checkForPrevious)
                        {
                            TimeSpan diff = startTime.Subtract(payLoadInfo.StartTime.TimeOfDay);
                            float tempValue = (diff.Hours*_myScheduleGridDrawingInfo.WidthPerHour) +
                                              (diff.Minutes*_myScheduleGridDrawingInfo.PixelSize);
                            widthToReduce += tempValue < 0 ? tempValue*(-1) : tempValue;
                            checkForPrevious = false;
                        }
                    }

                    TimeSpan timeDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
                    layerLength += (timeDiff.Hours*_myScheduleGridDrawingInfo.WidthPerHour) +
                                   (timeDiff.Minutes*_myScheduleGridDrawingInfo.PixelSize);
                    if (layerLength < 0)
                        layerLength *= (-1);

                    float definedWidth = location.X;
                    if ((definedWidth > widthToReduce) && (definedWidth < (widthToReduce + layerLength)))
                    {
                        string toolTipText = payLoadInfo.Name + " " +
                                             payLoadInfo.StartTime.ToString("t", CultureInfo.CurrentCulture) + " - " +
                                             payLoadInfo.EndTime.ToString("t", CultureInfo.CurrentCulture);

                        Point point = Cursor.Position;
                        point.X += 10;
                        point.Y += 20;
                        _visualizeToolTip.Show(toolTipText, Grid, Grid.PointToClient(point), 1000);
                        found = true;
                        break;
                    }
                    index++;
                }
                if (!found)
                    _visualizeToolTip.RemoveAll();
            }
            else
            {
                _visualizeToolTip.Hide(Grid);
            }
        }

        /// <summary>
        /// Draw a line on the grid column
        /// </summary>
        /// <param name="graphics">Instance of Graphics</param>
        /// <param name="pX">Starting X</param>
        /// <param name="pY">Starting Y</param>
        /// <param name="qX">Ending X</param>
        /// <param name="qY">Ending Y</param>
        /// <param name="pen">Instance of the Pen used to draw the line</param>
        private static void DrawLine(Graphics graphics, float pX, float pY, float qX, float qY, Pen pen)
        {
            PointF pointX = new PointF(pX, pY);
            PointF pointY = new PointF(qX, qY);
            graphics.DrawLine(pen, pointX, pointY);
        }

        #endregion

        #endregion

        #region Constructors

        public MyScheduleGrid(GridControl grid)
            : base(grid)
        {
        }

        #endregion

        /// <summary>
        /// Type of the view
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        internal override ViewType Type
        {
            get { return ViewType.MyScheduleGridView; }
        }

        #region IGridDataManager Members

        /// <summary>
        /// Sets the visual grid drawing info.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <remarks>
        /// Created by:VirajS
        /// </remarks>
        public void SetVisualGridDrawingInfo(MyScheduleGridDrawingInfo info)
        {
            _myScheduleGridDrawingInfo = info;
        }

        #endregion
    }
}