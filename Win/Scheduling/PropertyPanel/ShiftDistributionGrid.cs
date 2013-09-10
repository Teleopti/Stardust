using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftDistributionGrid : TeleoptiGridControl, IShiftDistributionGrid
    {
        private IDistributionInformationExtractor _model;
        private ShiftDistributionGridPresenter _presenter;
        private static readonly Color ColorCells = Color.AntiqueWhite;

        public ShiftDistributionGrid()
        {
            base.Initialize();
            if (!CellModels.ContainsKey("IgnoreCellModel")) CellModels.Add("IgnoreCellModel", new IgnoreCellModel(Model));
        }

        public void UpdateModel(IDistributionInformationExtractor model)
        {
            ResetVolatileData();
            _model = model;
            _presenter = new ShiftDistributionGridPresenter(this, _model.GetShiftDistribution());
            _presenter.Sort(0);
            initializeComponent();
        }

        private void initializeComponent()
        {
            ResetVolatileData();

            QueryColCount -= shiftPerAgentGridQueryColCount;
            QueryRowCount -= shiftPerAgentGridQueryRowCount;
            QueryCellInfo -= shiftPerAgentGridQueryCellInfo;
            CellDoubleClick -= shiftPerAgentGridCellDoubleClick;

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
                                                    GridQueryCellInfoEventArgs e)
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
                WeekendOrWeekday(e, (DateOnly) e.Style.Tag, false, true);
            }

            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                var date = (DateOnly) this[e.RowIndex, 0].Tag;
               
                var shiftCategory = this[0, e.ColIndex].Tag as IShiftCategory;
                e.Style.CellType = "IntegerReadOnlyCell";

                var count = _presenter.ShiftCategoryCount(date, shiftCategory);
                if (count.HasValue)
                    e.Style.CellValue = count;
                else
                    e.Style.CellValue = 0;
            }

                

            e.Handled = true;
        }

        public void WeekendOrWeekday(GridQueryCellInfoEventArgs e, DateOnly date, bool backColor, bool textColor)
        {
            if (DateHelper.IsWeekend(date, CultureInfo.CurrentCulture))
            {
                if (backColor)
                    e.Style.BackColor = ColorHolidayCell;
                if (textColor)
                    e.Style.TextColor = ColorHolidayHeader;
            }
            else
            {
                if (backColor)
                    e.Style.BackColor = ColorCells;

                e.Style.TextColor = ForeColor;
            }
        }

        private void shiftPerAgentGridQueryRowCount(object sender,
                                                    GridRowColCountEventArgs e)
        {
            e.Count = _model.Dates.Count;
            e.Handled = true;
        }

        private void shiftPerAgentGridQueryColCount(object sender,
                                                    GridRowColCountEventArgs e)
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

