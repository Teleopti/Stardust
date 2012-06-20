using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsDetailView : ScheduleViewBase, IAgentRestrictionsDetailView
	{
		private readonly AgentRestrictionsDetailModel _model;
		private IWorkShiftWorkTime _workShiftWorkTime;

		public AgentRestrictionsDetailView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IWorkShiftWorkTime workShiftWorkTime)
			: base(grid)
		{
			if(schedulerState == null) throw new ArgumentNullException("schedulerState");

			_model = new AgentRestrictionsDetailModel(schedulerState.RequestedPeriod.Period());
			Presenter = new AgentRestrictionsDetailPresenter(this, _model, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag);

			_workShiftWorkTime = workShiftWorkTime;

			InitializeGrid();
		}

		private void InitializeGrid()
		{
			ViewGrid.Rows.HeaderCount = 0;
			ViewGrid.Cols.HeaderCount = 0;
			ViewGrid.ColWidths.SetRange(1, ViewGrid.ColCount, 120);
			ViewGrid.ColWidthEntries[0].Width = 140;
			ViewGrid.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			ViewGrid.NumberedRowHeaders = false;
			ViewGrid.RowHeights.SetRange(1, ViewGrid.RowCount, 80);
			ViewGrid.RowHeightEntries[0].Height = 30;
		}

		protected override int CellWidth()
		{
			return 120;
		}

		internal override void QueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			if (e.Index == (int)ColumnType.None)
			{
				e.Size = CellWidth();
				e.Handled = true;
			}
			else if (e.Index >= (int)ColumnType.RowHeaderColumn)
			{
				e.Size = CellWidth();
				e.Handled = true;
			}
		}

		internal override void QueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = Presenter.ColCount;
			e.Handled = true;
		}

		internal override void QueryRowCount(object sender, GridRowColCountEventArgs e)
		{

			e.Count = Presenter.RowCount;
			e.Handled = true;
		}

		internal override void CreateHeaders()
		{
			ViewGrid.Rows.HeaderCount = 0;
			ViewGrid.Cols.HeaderCount = 0;
			ViewGrid.Rows.FrozenCount = 0;
			ViewGrid.Cols.FrozenCount = 0;
			ViewGrid.Model.Options.MergeCellsMode = GridMergeCellsMode.None;
		}

		public void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, IRestrictionExtractor restrictionExtractor, RestrictionSchedulingOptions schedulingOptions, TimeSpan periodTarget)
		{
			IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor = new AgentRestrictionsDetailEffectiveRestrictionExtractor(_workShiftWorkTime, restrictionExtractor, schedulingOptions);
			var preferenceNightRestChecker = new PreferenceNightRestChecker();
			_model.LoadDetails(scheduleMatrixPro, restrictionExtractor, schedulingOptions, effectiveRestrictionExtractor, periodTarget, preferenceNightRestChecker);
			ViewGrid.Refresh();
			InitializeGrid();
		}
	}
}
