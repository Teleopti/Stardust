using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls;
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
			_presenter = new ShiftFairnessGridPresenter(this);	
		}

        public void UpdateModel(IDistributionInformationExtractor distributionInformationExtractor)
        {
            _model = distributionInformationExtractor;
			_presenter.ReSort();
            initializeComponent();
        }

		private void initializeComponent()
		{
			ResetVolatileData();

			QueryColCount -= shiftFairnessGridQueryColCount;
			QueryRowCount -= shiftFairnessGridQueryRowCount;
			QueryCellInfo -= shiftFairnessGridQueryCellInfo;
			CellDoubleClick -= shiftFairnessGridCellDoubleClick;

			QueryColCount += shiftFairnessGridQueryColCount;
			QueryRowCount += shiftFairnessGridQueryRowCount;
			QueryCellInfo += shiftFairnessGridQueryCellInfo;
			CellDoubleClick += shiftFairnessGridCellDoubleClick;

			ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
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
			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex == 0 && e.RowIndex == 0) return;
			if (e.ColIndex > 4) return;
			if (e.RowIndex > _model.ShiftCategories.Count + 1) return;

			if (e.ColIndex > 0 && e.RowIndex == 0)
			{
				if (e.ColIndex == 1) e.Style.CellValue = UserTexts.Resources.Min;
				if (e.ColIndex == 2) e.Style.CellValue = UserTexts.Resources.Max;
				if (e.ColIndex == 3) e.Style.CellValue = UserTexts.Resources.Average;
				if (e.ColIndex == 4) e.Style.CellValue = UserTexts.Resources.StandardDeviation;
			}

			if (e.ColIndex == 0 && e.RowIndex > 0)
			{
				if (e.RowIndex < _model.ShiftCategories.Count + 1)
				{
					e.Style.CellValue = _presenter.SortedShiftCategories()[e.RowIndex - 1].Description.Name;
					e.Style.Tag = _presenter.SortedShiftCategories()[e.RowIndex - 1];
				}
				else
				{
					e.Style.CellValue = UserTexts.Resources.TotalColon;
				}
			}

			if (e.ColIndex > 0 && e.RowIndex > 0)
			{
				var shiftCategory = this[e.RowIndex, 0].Tag as IShiftCategory;

				foreach (var shiftFairness in _model.GetShiftFairness())
				{
					if (shiftFairness.ShiftCategory.Equals(shiftCategory))
					{
						if (e.ColIndex == 1) e.Style.CellValue = shiftFairness.MinimumValue;
						if (e.ColIndex == 2) e.Style.CellValue = shiftFairness.MaximumValue;
						if (e.ColIndex == 3) e.Style.CellValue = shiftFairness.AverageValue;
						if (e.ColIndex == 4) e.Style.CellValue = shiftFairness.StandardDeviationValue;
					}
				}

				if (e.ColIndex == 4 && e.RowIndex == _model.ShiftCategories.Count + 1)
				{
					e.Style.CellValue = _presenter.CalculateTotalStandardDeviation(_model.GetShiftFairness());
				}

				e.Style.ReadOnly = true;
			}

			e.Handled = true;
		}

		void shiftFairnessGridQueryRowCount(object sender,GridRowColCountEventArgs e)
		{
			e.Count = _model.ShiftCategories.Count + 1;
			e.Handled = true;
		}

		void shiftFairnessGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = 4;
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
			}

			base.Dispose(disposing);
		}
    }
}
