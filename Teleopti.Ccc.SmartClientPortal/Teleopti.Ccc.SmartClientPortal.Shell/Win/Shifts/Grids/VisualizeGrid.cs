using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

using VisualPayloadInfo = Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.VisualPayloadInfo;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts.Grids
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

			var column = new VisualizeGridColumn<ReadOnlyCollection<VisualPayloadInfo>>("PayloadInfo", " ");
			AddColumn(column);
			
			Grid.CellModels.Model.TableStyle.Borders.All = new GridBorder(GridBorderStyle.None);
			
			Grid.CellDrawn += gridCellDrawn;
			Grid.CellMouseHover += gridCellMouseHover;
			Grid.LostFocus += gridLostFocus;
			Grid.MouseLeave += gridMouseLeave;
			Grid.PrepareViewStyleInfo += Grid_PrepareViewStyleInfo;
		}

		void Grid_PrepareViewStyleInfo(object sender, GridPrepareViewStyleInfoEventArgs e)
		{
			e.Style.TextMargins.Top = 5;
		}

		void gridMouseLeave(object sender, EventArgs e)
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

		private void drawTimeLine(GridDrawCellEventArgs e)
		{
			if (Presenter.GetNumberOfRowsToBeShown() == 0)
				return;

			int numberOfColumns = Presenter.Explorer.Model.VisualizeGridColumnCount;
			if (numberOfColumns == 0)
				return;

			int widthToReduce = e.Bounds.X;
			if (e.ColIndex == 1 && (e.RowIndex == 0 || e.RowIndex == 1))
			{
				int startHour = Presenter.Explorer.Model.ShiftStartTime.Hours;
				var startSpan = new TimeSpan(Presenter.Explorer.Model.ShiftStartTime.Hours, 0, 0);
				if (Presenter.Explorer.Model.IsRightToLeft)
				{
					startHour = Presenter.Explorer.Model.ShiftEndTime.Add(TimeSpan.FromHours(1)).Hours;
					if (startHour == 0)
						startHour = 11;
					startSpan = new TimeSpan(startHour, 0, 0);
				}

				for (int i = 0; i <= (numberOfColumns); i++)
				{
					float width = widthToReduce + Presenter.Explorer.Model.WidthPerHour * (i);
					float timeSlotWidth = Presenter.Explorer.Model.WidthPerPixel * Presenter.Explorer.Model.DefaultSegment;

					drawLine(e.Graphics, width, 0, width, e.Bounds.Height, pen);

					pen.Width = 1;
					if (i <= (numberOfColumns - 1))
					{
						int numberfSlotsPerHour = (defaultHourWidth / Presenter.Explorer.Model.DefaultSegment);

						for (int k = 0; k <= numberfSlotsPerHour; k++)
						{
							drawLine(e.Graphics, (width + timeSlotWidth * k), 0, (width + timeSlotWidth * k), 4, pen);
						}

						var displayHour = new TimeSpan(startSpan.Hours, 0, 0);
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

		private void drawShifts(GridDrawCellEventArgs e)
		{
			if (Presenter.GetNumberOfRowsToBeShown() == 0)
				return;
			if (Presenter.GetNumberOfRowsToBeShown() <= (e.RowIndex - 1))
				return;

			if (e.ColIndex == 1 && e.RowIndex > 0)
			{
				ReadOnlyCollection<VisualPayloadInfo> payLoadInfoList = Presenter.ModelCollection[e.RowIndex - 1];

				int widthToReduce = e.Bounds.X;
				var startTime = new TimeSpan(Presenter.Explorer.Model.ShiftStartTime.Hours, 0, 0);

				for (int i = 0; i <= (payLoadInfoList.Count - 1); i++)
				{
					VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
					TimeSpan timeDiff = startTime.Subtract(payLoadInfo.StartTime.Subtract(WorkShift.BaseDate));

					float layerLength = ((int)timeDiff.TotalHours * Presenter.Explorer.Model.WidthPerHour) + timeDiff.Minutes * Presenter.Explorer.Model.WidthPerPixel;
					if (layerLength < 0)
						layerLength *= (-1);

					TimeSpan layerPeriodDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
					var width = (float)Math.Round(layerPeriodDiff.TotalMinutes, 0);
					width *= Presenter.Explorer.Model.WidthPerPixel;

					if (width > 0 && !float.IsInfinity(width))
					{
						RectangleF rectangle = e.Bounds;
						rectangle.Y = rectangle.Y + 1;
						rectangle.Height = rectangle.Height - 2;
						rectangle.X = widthToReduce + layerLength;
						if (Presenter.Explorer.Model.IsRightToLeft)
							rectangle.X = e.Bounds.Width - (layerLength + width) + widthToReduce;
						rectangle.Width = width;

						using (var linearGradientBrush = new LinearGradientBrush(rectangle,
																								 payLoadInfo.Color,
																								 payLoadInfo.Color,
																								 fieldAngle, false))
						{
							e.Graphics.FillRectangle(linearGradientBrush, rectangle);
						}
						if (!string.IsNullOrEmpty(payLoadInfo.UnderlyingActivities))
						{
							using (var brush = new HatchBrush(HatchStyle.ZigZag,Color.Black, payLoadInfo.Color))
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

		private void showShiftToolTipText(GridCellMouseEventArgs e)
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
					VisualPayloadInfo payLoadInfo = payLoadInfoList[i];
					if (index == 0)
					{
						int initialDifference = startTime.CompareTo(payLoadInfo.StartTime.TimeOfDay);
						if (initialDifference < 0 && checkForPrevious)
						{
							TimeSpan diff = startTime.Subtract(payLoadInfo.StartTime.TimeOfDay);
							var layerDuration = (float)(diff.TotalMinutes * Presenter.Explorer.Model.WidthPerPixel);
							widthToReduce += (layerDuration < 0) ? layerDuration * (-1) : layerDuration;
							checkForPrevious = false;
						}
					}

					TimeSpan timeDiff = payLoadInfo.EndTime.Subtract(payLoadInfo.StartTime);
					float panelWidth = (timeDiff.Hours * Presenter.Explorer.Model.WidthPerHour) + (timeDiff.Minutes * Presenter.Explorer.Model.WidthPerPixel);
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

		private static void drawLine(Graphics graphics, float pX, float pY, float qX, float qY, Pen pen)
		{
			var pointX = new PointF(pX, pY);
			var pointY = new PointF(qX, qY);
			graphics.DrawLine(pen, pointX, pointY);
		}

		private void gridCellDrawn(object sender, GridDrawCellEventArgs e)
		{
			drawTimeLine(e);
			drawShifts(e);
		}

		private void gridCellMouseHover(object sender, GridCellMouseEventArgs e)
		{
			showShiftToolTipText(e);
		}

		private void gridLostFocus(object sender, EventArgs e)
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
