using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel
{
	public partial class ShiftStatisticsControl : UserControl, INeedShiftCategoryDistributionModel
	{
		private IShiftStatisticsPresenter _presenter;
		private int _lastSortColumn;

		public ShiftStatisticsControl()
		{
			InitializeComponent();
			GridHelper.GridStyle(gridControl1);
			gridControl1.ResizingColumns += shiftPerAgentGridResizingColumns;
			gridControl1.CellModels.Add("IntegerReadOnlyCell", new NumericReadOnlyCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("IgnoreCellModel", new IgnoreCellModel(gridControl1.Model));
			gridControl1.CellModels.Add("NumericReadOnlyCell", new NumericReadOnlyCellModel(gridControl1.Model){NumberOfDecimals = 2});
		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			if (_presenter == null)
			{
				model.ResetNeeded += modelResetNeeded;
			}
			_presenter = new ShiftStatisticsPresenter(model);
			gridControl1.ResetVolatileData();
			gridControl1.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			gridControl1.UpdateScrollBars();
			gridControl1.Invalidate(true);
		}

		void modelResetNeeded(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(modelResetNeeded), sender, e);
			}
			else
			{
				gridControl1.ResetVolatileData();
				_presenter.ReSort(_lastSortColumn, true);
				gridControl1.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
				gridControl1.UpdateScrollBars();
				gridControl1.Invalidate(true);
			}
		}

		void shiftPerAgentGridResizingColumns(object sender, GridResizingColumnsEventArgs e)
		{
			e.Cancel = true;
		}

		private void gridControl1_QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if (_presenter == null)
				return;

			e.Count = _presenter.RowCount();
			e.Handled = true;
		}

		private void gridControl1_QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = 3;
			e.Handled = true;
		}

		private void gridControl1_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (_presenter == null)
				return;

			_presenter.SetCellInfo(e.Style, e.RowIndex, e.ColIndex, null);
		}


		private void gridControl1_CellDoubleClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex == 0 && e.ColIndex > -1)
			{
				_lastSortColumn = e.ColIndex;
				_presenter.ReSort(_lastSortColumn, false);
				gridControl1.Invalidate();
			}
		}

	}
}
