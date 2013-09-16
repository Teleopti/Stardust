using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftPerAgentGrid : TeleoptiGridControl, IShiftPerAgentGrid
    {
	    private  IDistributionInformationExtractor _model;
	    private readonly ShiftPerAgentGridPresenter _presenter;
	    private readonly ISchedulerStateHolder _schedulerState;

		public ShiftPerAgentGrid(ISchedulerStateHolder schedulerState)
		{
			base.Initialize();
			_presenter = new ShiftPerAgentGridPresenter(this);
			_schedulerState = schedulerState;
			QueryColCount += shiftPerAgentGridQueryColCount;
			QueryRowCount += shiftPerAgentGridQueryRowCount;
			QueryCellInfo += shiftPerAgentGridQueryCellInfo;
			CellDoubleClick += shiftPerAgentGridCellDoubleClick;
			ResizingColumns += shiftPerAgentGridResizingColumns;
			
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
		}

		void shiftPerAgentGridResizingColumns(object sender, GridResizingColumnsEventArgs e)
		{
			e.Cancel = true;
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

	    void shiftPerAgentGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (_model == null)
			{
				e.Handled = true;
				return;
			}

			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex == 0 && e.RowIndex == 0) return;
			
			if (e.RowIndex > _model.PersonInvolved.Count) return;

			if (e.ColIndex > 0 && e.RowIndex == 0)
			{
				var shiftCategoryList = _model.GetShiftCategories();
				if (e.ColIndex > shiftCategoryList.Count)
					return;
				e.Style.CellValue = shiftCategoryList[e.ColIndex - 1].Description.Name;
				e.Style.Tag = shiftCategoryList[e.ColIndex - 1];	
			}

			if (e.ColIndex == 0 && e.RowIndex > 0)
			{
				e.Style.CellValue = _schedulerState.CommonAgentName(_presenter.SortedPersonInvolved()[e.RowIndex - 1]);
			}

			if (e.ColIndex > 0 && e.RowIndex > 0)
			{
				var person = _presenter.SortedPersonInvolved()[e.RowIndex - 1];
				var shiftCategory = this[0, e.ColIndex].Tag as IShiftCategory;
				e.Style.CellType = "IntegerReadOnlyCell";
			    if (shiftCategory != null)
			        e.Style.CellValue = _presenter.ShiftCategoryCount(person, shiftCategory, _model.PersonCache);
			    else
			        e.Style.CellValue = 0;
			}

			e.Handled = true;
		}

		void shiftPerAgentGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if(_model != null)
				e.Count = _model.PersonInvolved.Count;
			e.Handled = true;
		}

		void shiftPerAgentGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			if(_model != null)
				e.Count = _model.GetShiftCategories().Count;
			e.Handled = true;
		}

	    public IDistributionInformationExtractor ExtractorModel
	    {
			get { return _model; }
	    }

	    public ISchedulerStateHolder SchedulerState
	    {
			get { return _schedulerState; }
	    }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				QueryColCount -= shiftPerAgentGridQueryColCount;
				QueryRowCount -= shiftPerAgentGridQueryRowCount;
				QueryCellInfo -= shiftPerAgentGridQueryCellInfo;
				CellDoubleClick -= shiftPerAgentGridCellDoubleClick;
				ResizingColumns -= shiftPerAgentGridResizingColumns;
			}

			base.Dispose(disposing);
		}
    }
}
