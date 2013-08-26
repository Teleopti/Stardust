using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Forecasting.Forms;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	public class ShiftDistributionGrid : TeleoptiGridControl, ITaskOwnerGrid, IHelpContext, IShiftDistributionGrid
	{
		private IDistributionInformationExtractor _model;
		private ShiftDistributionGridPresenter _presenter;

        public ShiftDistributionGrid(IDistributionInformationExtractor model)
        {
            initializeComponent();
	        _model = model;
			_presenter = new ShiftDistributionGridPresenter(this);
            
            Cols.Size[0] = ColorHelper.GridHeaderColumnWidth();
            DefaultColWidth = 50;
            ColCount = _model.ShiftCategories.Count;
            ExcelLikeCurrentCell = true;
            Model.MergeCells.DelayMergeCells(GridRangeInfo.Table());
        }

        private void initializeComponent()
        {
            QueryColCount += shiftPerAgentGridQueryColCount;
            QueryRowCount += shiftPerAgentGridQueryRowCount;
            QueryCellInfo += shiftPerAgentGridQueryCellInfo;

           

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            ResumeLayout(false);
        }

        void shiftPerAgentGridQueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (e.ColIndex == 0 && e.RowIndex == 0) return;
            if (e.ColIndex > _model.ShiftCategories.Count) return;
            if (e.RowIndex > _model.Dates .Count) return;

            if (e.ColIndex > 0 && e.RowIndex == 0)
            {
                e.Style.CellValue = _model.ShiftCategories[e.ColIndex - 1].Description ;
                e.Style.Tag = _model.ShiftCategories[e.ColIndex - 1].Description ;
            }

            if (e.ColIndex == 0 && e.RowIndex > 0)
            {
                e.Style.CellValue = _model.Dates[e.RowIndex - 1].Date ;
                e.Style.Tag = _model.Dates[e.RowIndex - 1] ;
            }

            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                var date = (DateOnly) this[e.RowIndex , 0].Tag ;
                var shiftCategory = this[0, e.ColIndex ].Tag as string;

                foreach (var shiftDistribution in _model.GetShiftDistribution())
                {
                    if (shiftDistribution.DateOnly.Equals(date ))
                    {
                        if (shiftDistribution.ShiftCategory.Description.Name.Equals(shiftCategory))
                        {
                            e.Style.CellValue = shiftDistribution.Count;
                            break;
                        }
                    }
                }
            }

            e.Handled = true;
        }

        void shiftPerAgentGridQueryRowCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
        {
            e.Count = _model.Dates.Count;
            e.Handled = true;
        }

        void shiftPerAgentGridQueryColCount(object sender, Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
        {
            e.Count = _model.ShiftCategories.Count;
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
