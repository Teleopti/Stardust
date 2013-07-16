using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;
using VisualPayloadInfo = Teleopti.Ccc.WinCode.Shifts.VisualPayloadInfo;

namespace Teleopti.Ccc.Win.Shifts.Grids
{
    public class VisualizeGrid : GridViewBase<IVisualizePresenter, ReadOnlyCollection<VisualPayloadInfo>>
    {
        private const int fieldAngle = 90;
        private const int defaultHourWidth = 60;
        private readonly ToolTip _visualizeToolTip = new ToolTip();
        private RowHeaderColumn<ReadOnlyCollection<VisualPayloadInfo>> _rowHeaderColumn;
        private readonly TimeSpan defaultTick = new TimeSpan(1, 0, 0);
        private readonly Pen pen = new Pen(Brushes.Black);
        private readonly Font font = new Font("Arial", 7);
        private readonly Brush defaultBrush = Brushes.Black;
		private DateTime _lastTime = DateTime.Now;

        public VisualizeGrid(IVisualizePresenter presenter, GridControl grid)
            : base(presenter, grid)
        {
        }

        internal override ShiftCreatorViewType Type
        {
            get { return ShiftCreatorViewType.VisualizingGrid; }
        }

        internal override void CreateHeaders()
        {
            _rowHeaderColumn = new RowHeaderColumn<ReadOnlyCollection<VisualPayloadInfo>>();
            AddColumn(_rowHeaderColumn);

            VisualizeGridColumn<ReadOnlyCollection<VisualPayloadInfo>> column = new VisualizeGridColumn<ReadOnlyCollection<VisualPayloadInfo>>("PayloadInfo", " ");
            AddColumn(column);
            
            Grid.CellModels.Model.TableStyle.Borders.All = new GridBorder(GridBorderStyle.None);
            
            Grid.CellDrawn += Grid_CellDrawn;
            Grid.CellMouseHover += Grid_CellMouseHover;
            Grid.LostFocus += Grid_LostFocus;
			Grid.MouseLeave += Grid_MouseLeave;
        }

		void Grid_MouseLeave(object sender, EventArgs e)
		{
			_visualizeToolTip.RemoveAll();
		}

        internal override void PrepareView()
        {
            ColCount = GridColumns.Count;

            Grid.RowCount = Presenter.GetNumberOfRowsToBeShown();
            Grid.ColCount = ColCount - 1;

            Grid.Cols.HeaderCount = 0;
            Grid.Rows.HeaderCount = 0;
            Grid.NumberedRowHeaders = true;
            Grid.ColWidths[0] = 45;

            Grid.Name = "";
        }

        internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
			if (Presenter.ModelCollection.Count == 0) return;
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                GridColumns[e.ColIndex].GetCellInfo(e, Presenter.ModelCollection);
                
                if (e.RowIndex > 0 && e.ColIndex == 0)
                {
                    GridModel gridModel = e.Style.GetGridModel();
                    if (gridModel != null)
                    {
                        if (Presenter.ContractTimes() != null)
                        {
                            var displayTime = Presenter.ContractTimes()[e.RowIndex - 1];
                            e.Style.CellValue = TimeHelper.GetLongHourMinuteTimeString(displayTime, CultureInfo.CurrentCulture);
                        }
                    }
                }
            }


            e.Handled = true;
        }

        internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (ValidCell(e.ColIndex, e.RowIndex))
            {
                GridColumns[e.ColIndex].SaveCellInfo(e, Presenter.ModelCollection);
            }
        }

        private void DrawTimeLine(GridDrawCellEventArgs e)
        {
            int numberOfColumns;
            int widthToReduce;

            if (Presenter.GetNumberOfRowsToBeShown() == 0)
                return;

            numberOfColumns = Presenter.Explorer.Model.VisualizeGridColumnCount;
            if (numberOfColumns == 0)
                return;

            widthToReduce = e.Bounds.X;
            if (e.ColIndex == 1 && (e.RowIndex == 0 || e.RowIndex == 1))
            {
                int startHour;
                TimeSpan startSpan;

                startHour = Presenter.Explorer.Model.ShiftStartTime.Hours;
                startSpan = new TimeSpan(Presenter.Explorer.Model.ShiftStartTime.Hours, 0, 0);
                if (Presenter.Explorer.Model.IsRightToLeft)
                {
                    startHour = Presenter.Explorer.Model.ShiftEndTime.Hours;
                    if (startHour == 0)
                        startHour = 11;
                    startSpan = new TimeSpan(startHour, 0, 0);
                }

                for (int i = 0; i <= (numberOfColumns); i++)
                {
                    float width = widthToReduce + Presenter.Explorer.Model.WidthPerHour * (i);
                    float timeSlotWidth = Presenter.Explorer.Model.WidthPerPixel * Presenter.Explorer.Model.DefaultSegment;

                    DrawLine(e.Graphics, width, 0, width, e.Bounds.Height, pen);

                    pen.Width = 1;
                    if (i <= (numberOfColumns - 1))
                    {
                        int numberfSlotsPerHour = (defaultHourWidth / Presenter.Explorer.Model.DefaultSegment);

                        for (int k = 0; k <= numberfSlotsPerHour; k++)
                        {
                            DrawLine(e.Graphics, (width + timeSlotWidth * k), 0, (width + timeSlotWidth * k), 4, pen);
                        }

                        TimeSpan displayHour = new TimeSpan(startSpan.Hours, 0, 0);
                        if (startSpan.Hours < 0)
                            displayHour = new TimeSpan(startSpan.Hours * -1, 0, 0);

                        string hour = TimeHelper.TimeOfDayFromTimeSpan(displayHour, CultureInfo.CurrentCulture);
                        SizeF hourSize = e.Graphics.MeasureString(hour, font);
                        float hourStartingPoint = (timeSlotWidth * numberfSlotsPerHour) / 2 - (hourSize.Width / 2);
                        e.Graphics.DrawString(hour, font, defaultBrush, new PointF((width + hourStartingPoint), 6));
                    }
                    if (startHour == 24)
                    {
                        startHour = 1;
                        if (!Presenter.Explorer.Model.IsRightToLeft)
                            startSpan = startSpan.Add(defaultTick);
                        else
                            startSpan = startSpan.Subtract(defaultTick);
                    }
                    else
                    {
                        if (!Presenter.Explorer.Model.IsRightToLeft)
                        {
                            startHour++;
                            startSpan = startSpan.Add(defaultTick);
                        }
                        else
                        {
                            if (startHour == 0)
                            {
                                startHour = 12;
                                startSpan = TimeSpan.FromHours(12);
                            }
                            --startHour;
                            startSpan = startSpan.Subtract(defaultTick);
                        }
                    }
                }
            }
        }

        private void DrawShifts(GridDrawCellEventArgs e)
        {
            if (Presenter.GetNumberOfRowsToBeShown() == 0)
                return;
            if (Presenter.GetNumberOfRowsToBeShown() <= (e.RowIndex - 1))
                return;

            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
                ReadOnlyCollection<VisualPayloadInfo> payLoadInfoList = Presenter.ModelCollection[e.RowIndex - 1];

                int widthToReduce = e.Bounds.X;
                TimeSpan startTime = new TimeSpan(Presenter.Explorer.Model.ShiftStartTime.Hours, 0, 0);

                for (int i = 0; i <= (payLoadInfoList.Count - 1); i++)
                {
                    VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
                    float width;
                    TimeSpan timeDiff = startTime.Subtract(payLoadInfo.StartTime.Subtract(WorkShift.BaseDate));

                    float layerLength = ((int)timeDiff.TotalHours * Presenter.Explorer.Model.WidthPerHour) + timeDiff.Minutes * Presenter.Explorer.Model.WidthPerPixel;
                    if (layerLength < 0)
                        layerLength *= (-1);

                    TimeSpan layerPeriodDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
                    width = (float)Math.Round(layerPeriodDiff.TotalMinutes, 0);
                    width *= Presenter.Explorer.Model.WidthPerPixel;

                    if (width > 0 && !float.IsInfinity(width))
                    {
                        RectangleF rectangle = e.Bounds;
                        rectangle.X = widthToReduce + layerLength;
                        if (Presenter.Explorer.Model.IsRightToLeft)
                            rectangle.X = e.Bounds.Width - (layerLength + width) + widthToReduce;
                        rectangle.Width = width;

                        using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle,
                                                                                                 Color.WhiteSmoke,
                                                                                                 payLoadInfo.Color,
                                                                                                 fieldAngle, false))
                        {
                            e.Graphics.FillRectangle(linearGradientBrush, rectangle);
                        }
                        if (!string.IsNullOrEmpty(payLoadInfo.UnderlyingActivities))
                        {
                            using (HatchBrush brush = new HatchBrush(HatchStyle.ZigZag,Color.Black, payLoadInfo.Color))
                            {
                               rectangle.Y = rectangle.Y + 1;
                                rectangle.Height = rectangle.Height - 2;
                                     e.Graphics.FillRectangle(brush, rectangle);
                            }
                        }
                         
                    }
                }
            }
        }

        private void ShowShiftToolTipText(GridCellMouseEventArgs e)
        {
            if (Presenter.GetNumberOfRowsToBeShown() == 0)
                return;

            if ((e.RowIndex - 1) >= Presenter.GetNumberOfRowsToBeShown())
                return;

            PointF location = e.MouseEventArgs.Location;
            if ((location.Y >= (Grid.Height - 1)) || (location.Y <= Grid.RowHeights[0]))
            {
                _visualizeToolTip.RemoveAll();
                return;
            }
			if (_lastTime.AddMilliseconds(300) > DateTime.Now)
				return;
            if (e.ColIndex == 1 && e.RowIndex > 0)
            {
            	_lastTime = DateTime.Now;
                List<VisualPayloadInfo> payLoadInfoList = Presenter.ModelCollection[e.RowIndex-1].ToList();
                if(payLoadInfoList == null)
                    return;
                float widthToReduce = Grid.ColWidths[0];
                float layerLength = 0;
                int index = 0;
                bool checkForPrevious = true;

                TimeSpan startTime = TimeSpan.FromHours(Presenter.Explorer.Model.ShiftStartTime.Hours);

                bool found = false;
                if (Presenter.Explorer.Model.IsRightToLeft)
                    payLoadInfoList.Reverse();
                for (int i = 0; i <= (payLoadInfoList.Count - 1); i++)
                {
                    float panelWidth;
                    VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
                    if (index == 0)
                    {
                        int initialDifference = startTime.CompareTo(payLoadInfo.StartTime.TimeOfDay);
                        if (initialDifference < 0 && checkForPrevious)
                        {
                            TimeSpan diff = startTime.Subtract(payLoadInfo.StartTime.TimeOfDay);
                            float layerDuration = (float)(diff.TotalMinutes * Presenter.Explorer.Model.WidthPerPixel);
                            widthToReduce += (layerDuration < 0) ? layerDuration * (-1) : layerDuration;
                            checkForPrevious = false;
                        }
                    }

                    TimeSpan timeDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
                    panelWidth = (timeDiff.Hours * Presenter.Explorer.Model.WidthPerHour) + (timeDiff.Minutes * Presenter.Explorer.Model.WidthPerPixel);
                    layerLength += panelWidth;
                    if (layerLength < 0)
                        layerLength *= (-1);

                    float definedWidth = location.X;
                    bool isFound = false;
                    if (!Presenter.Explorer.Model.IsRightToLeft)
                    {
                        if ((definedWidth > widthToReduce) && (definedWidth < (widthToReduce + layerLength)))
                            isFound = true;
                    }
                    else
                    {
                        float op1 = (float)(payLoadInfo.EndTime.Subtract(startTime).TimeOfDay.TotalMinutes) * Presenter.Explorer.Model.WidthPerPixel;
                        if (((definedWidth - 19) > (Grid.ColWidths[1] - op1)) && ((definedWidth - 19) < ((Grid.ColWidths[1] - op1) + panelWidth)))
                            isFound = true;
                    }
                    if (isFound)
                    {
                        string toolTipText = payLoadInfo.Name + " " +
                                                 payLoadInfo.StartTime.ToString("t", CultureInfo.CurrentCulture) + " - " +
                                                 payLoadInfo.EndTime.ToString("t", CultureInfo.CurrentCulture);
                        toolTipText += payLoadInfo.UnderlyingActivities;
                        Point point = Cursor.Position;
                        point.X += 10;
                        point.Y += 20;
                        _visualizeToolTip.Show(toolTipText, Grid.Parent, Grid.Parent.PointToClient(point));
                        found = true;
                        break;
                    }
                    index++;
                }
                if (Presenter.Explorer.Model.IsRightToLeft)
                    payLoadInfoList.Reverse();

                if (!found)
                    _visualizeToolTip.RemoveAll();
            }
            else
            {
                _visualizeToolTip.Hide(Grid.Parent);
            }
        }

        private static void DrawLine(Graphics graphics, float pX, float pY, float qX, float qY, Pen pen)
        {
            PointF pointX = new PointF(pX, pY);
            PointF pointY = new PointF(qX, qY);
            graphics.DrawLine(pen, pointX, pointY);
        }

        private void Grid_CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            DrawTimeLine(e);
            DrawShifts(e);
        }

        private void Grid_CellMouseHover(object sender, GridCellMouseEventArgs e)
        {
            ShowShiftToolTipText(e);
        }

        private void Grid_LostFocus(object sender, EventArgs e)
        {
            _visualizeToolTip.RemoveAll();
        }

        public override void Add()
        {
        }

        public override void Delete()
        {
        }

        public override void Rename()
        {
        }

        public override void Sort(SortingMode mode)
        {
        }

        public override void RefreshView()
        {
            Presenter.Explorer.View.SetVisualGridDrawingInfo();
            Grid.RowCount = Presenter.ModelCollection.Count;
            Grid.ColWidths[1] = (int) Presenter.Explorer.Model.VisualColumnWidth;       
            Grid.Invalidate();
        }

        public override void Clear()
        {
            base.ClearView();
        }
    }
}
