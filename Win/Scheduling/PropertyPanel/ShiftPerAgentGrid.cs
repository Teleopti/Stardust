using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    public class ShiftPerAgentGrid : TeleoptiGridControl, IShiftPerAgentGrid
    {
	    private readonly IDistributionInformationExtractor _model;
	    private readonly ShiftPerAgentGridPresenter _presenter;
	    private readonly ISchedulerStateHolder _schedulerState;

		public ShiftPerAgentGrid(IDistributionInformationExtractor model, ISchedulerStateHolder schedulerState)
		{
			base.Initialize();
			_model = model;
			_schedulerState = schedulerState;
			_presenter = new ShiftPerAgentGridPresenter(this);

			initializeComponent();
		}

		private void initializeComponent()
		{
			ResetVolatileData();

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

	    void shiftPerAgentGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex < 0 || e.RowIndex < 0) return;
			if (e.ColIndex == 0 && e.RowIndex == 0) return;
			if (e.ColIndex > _model.ShiftCategories.Count) return;
			if (e.RowIndex > _model.PersonInvolved.Count) return;

			if (e.ColIndex > 0 && e.RowIndex == 0)
			{
				e.Style.CellValue = _model.ShiftCategories[e.ColIndex - 1].Description.Name ;
				e.Style.Tag = _model.ShiftCategories[e.ColIndex - 1];	
			}

			if (e.ColIndex == 0 && e.RowIndex > 0)
			{
				e.Style.CellValue = _schedulerState.CommonAgentName(_presenter.SortedPersonInvolved()[e.RowIndex - 1]);
				//e.Style.CellValue = _schedulerState.CommonAgentName(_model.PersonInvolved[e.RowIndex - 1]);
				e.Style.Tag = _presenter.SortedPersonInvolved()[e.RowIndex - 1];
				//e.Style.Tag = _model.PersonInvolved[e.RowIndex - 1];
			}

			if (e.ColIndex > 0 && e.RowIndex > 0)
			{
				var person = this[e.RowIndex, 0].Tag as IPerson;
				var shiftCategory = this[0, e.ColIndex].Tag as IShiftCategory;
				e.Style.CellValue = _presenter.ShiftCategoryCount(person, shiftCategory, _model.GetShiftCategoryPerAgent());
				e.Style.ReadOnly = true;
			}

			e.Handled = true;
		}

		void shiftPerAgentGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _model.PersonInvolved.Count;
			e.Handled = true;
		}

		void shiftPerAgentGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = _model.ShiftCategories.Count;
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
    }
}
