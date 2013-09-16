using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessGrid : TeleoptiGridControl, IShiftFairnessGrid
    {
	    private IDistributionInformationExtractor _model;
	    private readonly ShiftFairnessGridPresenter _presenter;

		public ShiftFairnessGrid()
		{
			base.Initialize();
			if (!CellModels.ContainsKey("IgnoreCellModel")) CellModels.Add("IgnoreCellModel", new IgnoreCellModel(Model));
			_presenter = new ShiftFairnessGridPresenter(this);
			QueryColCount += shiftFairnessGridQueryColCount;
			QueryRowCount += shiftFairnessGridQueryRowCount;
			QueryCellInfo += shiftFairnessGridQueryCellInfo;
			CellDoubleClick += shiftFairnessGridCellDoubleClick;
			ResizingColumns += shiftFairnessGridResizingColumns;
		}

        public void UpdateModel(IDistributionInformationExtractor distributionInformationExtractor)
        {
            _model = distributionInformationExtractor;
			_presenter.ReSort();
            initializeComponent();
			Refresh();
        }

		private void initializeComponent()
		{
			ResetVolatileData();
			ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			((NumericReadOnlyCellModel)CellModels["NumericReadOnlyCell"]).NumberOfDecimals = 2;
		}

		void shiftFairnessGridResizingColumns(object sender, GridResizingColumnsEventArgs e)
		{
			e.Cancel = true;
		}

		void shiftFairnessGridCellDoubleClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex == 0)
			{
				BeginUpdate();
				_presenter.Sort(e.ColIndex);
				EndUpdate();

				Refresh();
			}
		}

		void shiftFairnessGridQueryCellInfo(object sender,GridQueryCellInfoEventArgs e)
		{
			if (_model == null)
			{
				e.Handled = true;
				return;
			}

			if (e.ColIndex < (int)ShiftFairnessGridColumns.ShiftCategory || e.RowIndex < 0) return;
			if (e.ColIndex == (int)ShiftFairnessGridColumns.ShiftCategory && e.RowIndex == 0) return;
			if (e.ColIndex > (int)ShiftFairnessGridColumns.StandardDeviationValue) return;
			if (e.RowIndex > _model.GetShiftCategories().Count + 1) return;

			if (e.ColIndex > (int)ShiftFairnessGridColumns.ShiftCategory && e.RowIndex == 0)
			{
				if (e.ColIndex == (int)ShiftFairnessGridColumns.MinimumValue) e.Style.CellValue = UserTexts.Resources.Min;
				if (e.ColIndex == (int)ShiftFairnessGridColumns.MaximumValue) e.Style.CellValue = UserTexts.Resources.Max;
				if (e.ColIndex == (int)ShiftFairnessGridColumns.AverageValue) e.Style.CellValue = UserTexts.Resources.Average;
				if (e.ColIndex == (int)ShiftFairnessGridColumns.StandardDeviationValue) e.Style.CellValue = UserTexts.Resources.StandardDeviation;
			}

			if (e.ColIndex == (int)ShiftFairnessGridColumns.ShiftCategory && e.RowIndex > 0)
			{
				if (e.RowIndex < _model.GetShiftCategories().Count + 1)
				{
					e.Style.CellValue = _presenter.SortedShiftCategories()[e.RowIndex - 1].Description.Name;
					e.Style.Tag = _presenter.SortedShiftCategories()[e.RowIndex - 1];
				}
				else
				{
					e.Style.CellValue = UserTexts.Resources.TotalColon;
				}
			}

			if (e.ColIndex > (int)ShiftFairnessGridColumns.ShiftCategory && e.RowIndex > 0)
			{
				var shiftCategory = this[e.RowIndex, 0].Tag as IShiftCategory;

				foreach (var shiftFairness in _model.ShiftFairness)
				{
					if (shiftFairness.ShiftCategory.Equals(shiftCategory))
					{
						e.Style.CellType = "IntegerReadOnlyCell";
						if (e.ColIndex == (int)ShiftFairnessGridColumns.MinimumValue) e.Style.CellValue = shiftFairness.MinimumValue;
						if (e.ColIndex == (int)ShiftFairnessGridColumns.MaximumValue) e.Style.CellValue = shiftFairness.MaximumValue;
						if (e.ColIndex == (int) ShiftFairnessGridColumns.AverageValue)
						{
							e.Style.CellType = "NumericReadOnlyCell";
							e.Style.CellValue = shiftFairness.AverageValue;
						}
						if (e.ColIndex == (int) ShiftFairnessGridColumns.StandardDeviationValue)
						{
							e.Style.CellType = "NumericReadOnlyCell";
							e.Style.CellValue = shiftFairness.StandardDeviationValue;
						}
					}
				}

				if (e.RowIndex == _model.GetShiftCategories().Count + 1)
				{
					if (e.ColIndex == (int) ShiftFairnessGridColumns.StandardDeviationValue)
					{
						e.Style.CellType = "NumericReadOnlyCell";
						e.Style.CellValue = _presenter.CalculateTotalStandardDeviation(_model.ShiftFairness );	
					}
					else
					{
						e.Style.CellType = "IgnoreCellModel";
					}
				}
			}

			e.Handled = true;
		}

		void shiftFairnessGridQueryRowCount(object sender,GridRowColCountEventArgs e)
		{
			if(_model != null)
				e.Count = _model.GetShiftCategories().Count + 1;
			e.Handled = true;
		}

		void shiftFairnessGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			if(_model != null)
				e.Count = _presenter.ColCount;
			e.Handled = true;
		}

		public IDistributionInformationExtractor ExtractorModel
		{
			get { return _model; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				QueryColCount -= shiftFairnessGridQueryColCount;
				QueryRowCount -= shiftFairnessGridQueryRowCount;
				QueryCellInfo -= shiftFairnessGridQueryCellInfo;
				CellDoubleClick -= shiftFairnessGridCellDoubleClick;
				ResizingColumns -= shiftFairnessGridResizingColumns;
			}

			base.Dispose(disposing);
		}
    }
}
