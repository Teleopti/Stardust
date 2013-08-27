using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftFairnessGrid : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext, IShiftFairnessGrid
    {
	    private readonly IDistributionInformationExtractor _model;
	    private readonly ShiftFairnessGridPresenter _presenter;

		public ShiftFairnessGrid(IDistributionInformationExtractor model)
		{
			_model = model;
			_presenter = new ShiftFairnessGridPresenter(this);
			initializeComponent();
		}

		private void initializeComponent()
		{
			//ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);

			ResetVolatileData();

			QueryColCount += ShiftFairnessGrid_QueryColCount;
			QueryRowCount += ShiftFairnessGrid_QueryRowCount;
			QueryCellInfo += ShiftFairnessGrid_QueryCellInfo;
			QueryColWidth += ShiftFairnessGrid_QueryColWidth;
			

			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			ResumeLayout(false);
		}

		void ShiftFairnessGrid_QueryColWidth(object sender, Syncfusion.Windows.Forms.Grid.GridRowColSizeEventArgs e)
		{
			if (e.Index == 0)
				e.Size = 50;
			else
				e.Size = 120;

			e.Handled = true;
		}

		
		void ShiftFairnessGrid_QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex == 0 && e.RowIndex == 0) return;
			if (e.ColIndex > 4) return;
			if (e.RowIndex > _model.ShiftCategories.Count + 1) return;

			if (e.ColIndex > 0 && e.RowIndex == 0)
			{
				if (e.ColIndex == 1) e.Style.CellValue = UserTexts.Resources.Min;
				if (e.ColIndex == 2) e.Style.CellValue = UserTexts.Resources.Max;
				if (e.ColIndex == 3) e.Style.CellValue = "xxAvarage";
				if (e.ColIndex == 4) e.Style.CellValue = UserTexts.Resources.StandardDeviation;
			}

			if (e.ColIndex == 0 && e.RowIndex > 0)
			{
				if (e.RowIndex < _model.ShiftCategories.Count + 1)
				{
					e.Style.CellValue = _model.ShiftCategories[e.RowIndex - 1].Description.Name;
					e.Style.Tag = _model.ShiftCategories[e.RowIndex - 1];
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
			}

			e.Handled = true;
		}

		void ShiftFairnessGrid_QueryRowCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
		{
			e.Count = _model.ShiftCategories.Count + 1;
			e.Handled = true;
		}

		void ShiftFairnessGrid_QueryColCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
		{
			e.Count = 4;
			e.Handled = true;
		}

        public bool HasColumns { get; private set; }
        public void RefreshGrid()
        {
            throw new NotImplementedException();
        }

        public AbstractDetailView Owner { get; set; }
        public void GoToDate(DateTime theDate)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLocalCurrentDate(int column)
        {
            throw new NotImplementedException();
        }

        public IDictionary<int, GridRow> EnabledChartGridRows { get; private set; }
        public ReadOnlyCollection<GridRow> AllGridRows { get; private set; }
        public int MainHeaderRow { get; private set; }
        public IList<GridRow> EnabledChartGridRowsMicke65()
        {
            throw new NotImplementedException();
        }

        public void SetRowVisibility(string key, bool enabled)
        {
            throw new NotImplementedException();
        }

        public bool HasHelp { get; private set; }
        public string HelpId { get; private set; }
    }
}
