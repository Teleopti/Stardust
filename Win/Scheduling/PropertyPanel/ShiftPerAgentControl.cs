using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public partial class ShiftPerAgentControl : UserControl, INeedShiftCategoryDistributionModel
	{
		private IShiftPerAgentPresenter _presenter;

		public ShiftPerAgentControl()
		{
			InitializeComponent();
			GridHelper.GridStyle(gridControl1);
			gridControl1.ResizingColumns += shiftPerAgentGridResizingColumns;
			gridControl1.CellModels.Add("IntegerReadOnlyCell", initializeNumericNoDecimalsReadOnlyCell());
			gridControl1.ResetVolatileData();
		}
		
		public void SetModel(IShiftCategoryDistributionModel model)
		{
			if (_presenter == null)
			{
				model.ResetNeeded += modelResetNeeded;
			}
			_presenter = new ShiftPerAgentPresenter(model);
			_presenter.ReSort(null, true);
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
				_presenter.ReSort(null, true);
				gridControl1.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
				gridControl1.UpdateScrollBars();
				gridControl1.Invalidate(true);
			}
		}

		private void gridControl1_CellDoubleClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex > 0)
				return;

			object tag = gridControl1[0, e.ColIndex].Tag;
			_presenter.ReSort(tag, false);
			gridControl1.Invalidate();
		}

		private void gridControl1_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (_presenter == null)
				return;

			object tag = null;
			if (e.RowIndex > 0)
				tag = gridControl1[0, e.ColIndex].Tag;

			_presenter.SetCellInfo(e.Style, e.RowIndex, e.ColIndex, tag);

			e.Handled = true;
		}

		private void gridControl1_QueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if (_presenter == null)
				return;

			e.Count = _presenter.RowCount;
			e.Handled = true;
		}

		private void gridControl1_QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			if (_presenter == null)
				return;

			e.Count = _presenter.ColumnCount();
			e.Handled = true;
		}

		void shiftPerAgentGridResizingColumns(object sender, GridResizingColumnsEventArgs e)
		{
			e.Cancel = true;
		}

		private GridCellModelBase initializeNumericNoDecimalsReadOnlyCell()
		{
			var cellModel = new NumericReadOnlyCellModel(gridControl1.Model);
			return cellModel;
		}

	}
}
