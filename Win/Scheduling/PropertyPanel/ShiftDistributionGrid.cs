﻿using System;
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
    public class ShiftDistributionGrid : TeleoptiGridControl, IShiftDistributionGrid
    {
        private IDistributionInformationExtractor _model;
        private ShiftDistributionGridPresenter _presenter;

        public ShiftDistributionGrid()
        {
            base.Initialize();
        }

        public void UpdateModel(IDistributionInformationExtractor model)
        {
            ResetVolatileData();
            _model = model;
            _presenter = new ShiftDistributionGridPresenter(this, _model.GetShiftDistribution());
            initializeComponent();
            _presenter.ReSort();
        }

        private void initializeComponent()
        {

            QueryColCount += shiftPerAgentGridQueryColCount;
            QueryRowCount += shiftPerAgentGridQueryRowCount;
            QueryCellInfo += shiftPerAgentGridQueryCellInfo;
            CellDoubleClick += shiftPerAgentGridCellDoubleClick;

            ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
        }

        void shiftPerAgentGridCellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            if (e.RowIndex == 0)
            {
                BeginUpdate();
                _presenter.Sort(e.ColIndex);
                EndUpdate();

                Refresh();
            }
        }

        private void shiftPerAgentGridQueryCellInfo(object sender,
                                                    Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
        {
            if (e.ColIndex < 0 || e.RowIndex < 0) return;
            if (e.ColIndex == 0 && e.RowIndex == 0) return;
            if (e.ColIndex > _model.ShiftCategories.Count) return;
            if (e.RowIndex > _model.Dates.Count) return;

            if (e.ColIndex > 0 && e.RowIndex == 0)
            {
                e.Style.CellValue = _model.ShiftCategories[e.ColIndex - 1].Description.Name;
                e.Style.Tag = _model.ShiftCategories[e.ColIndex - 1];
            }

            if (e.ColIndex == 0 && e.RowIndex > 0)
            {
                e.Style.CellValue = _presenter.SortedDates() [e.RowIndex - 1].Date.ToShortDateString();
                e.Style.Tag = _presenter.SortedDates()[e.RowIndex - 1];
            }

            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                var date = (DateOnly) this[e.RowIndex, 0].Tag;
                var shiftCategory = this[0, e.ColIndex].Tag as IShiftCategory;

                var count = _presenter.ShiftCategoryCount(date, shiftCategory);
                if (count.HasValue)
                    e.Style.CellValue = count;
            }

            e.Handled = true;
        }

        private void shiftPerAgentGridQueryRowCount(object sender,
                                                    Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
        {
            e.Count = _model.Dates.Count;
            e.Handled = true;
        }

        private void shiftPerAgentGridQueryColCount(object sender,
                                                    Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs e)
        {
            e.Count = _model.ShiftCategories.Count;
            e.Handled = true;
        }

        
        public IDistributionInformationExtractor ExtractorModel
	    {
			get { return _model; }
	    }
    }
}

