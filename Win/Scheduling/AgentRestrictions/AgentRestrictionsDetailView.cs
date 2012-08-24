using System;
using System.Collections.Generic;
using System.Drawing;
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
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private bool _useScheduling;

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

		public void InitializeGrid()
		{
			ViewGrid.Rows.HeaderCount = 0;
			ViewGrid.Cols.HeaderCount = 0;
			ViewGrid.ColWidths.SetRange(1, ViewGrid.ColCount, 120);
			ViewGrid.ColWidthEntries[0].Width = 140;
			ViewGrid.Model.Options.SelectCellsMouseButtonsMask = MouseButtons.Left;
			ViewGrid.NumberedRowHeaders = false;
			ViewGrid.RowHeights.SetRange(1, ViewGrid.RowCount, 80);
			ViewGrid.RowHeightEntries[0].Height = 30;

			if (!ViewGrid.CellModels.ContainsKey("AgentRestrictionsDetailViewCellModel")) ViewGrid.Model.CellModels.Add("AgentRestrictionsDetailViewCellModel", new AgentRestrictionsDetailViewCellModel(ViewGrid.Model));
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
			_useScheduling = schedulingOptions.UseScheduling;
			//ViewGrid.Refresh();
			//InitializeGrid();	
		}

		public override void AddSelectedSchedulesInColumnToList(GridRangeInfo range, int colIndex, ICollection<IScheduleDay> selectedSchedules)
		{
			for (int j = range.Top; j <= range.Bottom; j++)
			{
				if (colIndex >= 0)
				{
					IScheduleDay schedulePart = ViewGrid.Model[j, colIndex].CellValue as IScheduleDay;

					if (schedulePart != null)
						selectedSchedules.Add(schedulePart);
				}

			}
		}

		public override Point GetCellPositionForAgentDay(IEntity person, System.DateTime date)
		{
			Point point = new Point(-1, -1);

			for (int i = 1; i <= ViewGrid.RowCount; i++)
			{
				for (int j = 1; j <= ViewGrid.ColCount; j++)
				{
					IScheduleDay schedulePart = ViewGrid.Model[i, j].CellValue as IScheduleDay;

					if (schedulePart != null && schedulePart.Period.Contains(date))
					{
						point = new Point(j, i);
						break;
					}
				}
			}

			return point;
		}

		public override void SelectFirstDayInGrid()
		{
			GridRangeInfo info = GridRangeInfo.Cell(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1);
			TheGrid.Selections.Clear(true);
			TheGrid.CurrentCell.Activate(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1, GridSetCurrentCellOptions.SetFocus);
			TheGrid.Selections.ChangeSelection(info, info, true);
			TheGrid.CurrentCell.MoveTo(TheGrid.Rows.HeaderCount + 1, TheGrid.Cols.HeaderCount + 1);
		}

		public override DateOnly SelectedDateLocal()
		{
			DateOnly tag;
			if (ViewGrid.CurrentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
			{
				tag = (DateOnly)ViewGrid.Model[1, ViewGrid.CurrentCell.ColIndex].Tag;
			}
			else
			{
				tag = Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;
			}

			return tag;
		}

		public override void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules)
		{
			//if (_singleAgentRestrictionPresenter == null)
			//    return;
			//AgentInfoHelper agentInfoHelper = _singleAgentRestrictionPresenter.SelectedAgentInfo();
			//if (agentInfoHelper != null)
			//    ((RestrictionSummaryPresenter)Presenter).GetNextPeriod(agentInfoHelper);

			var personsToReload = new HashSet<IPerson>();
			foreach (IScheduleDay schedulePart in schedules)
			{
				personsToReload.Add(schedulePart.Person);
				Point point = GetCellPositionForAgentDay(schedulePart.Person, schedulePart.Period.StartDateTime);

				if (point.X != -1 && point.Y != -1)
				{
					TheGrid.InvalidateRange(GridRangeInfo.Row(point.X));
				}
			}
			//_singleAgentRestrictionPresenter.Reload(personsToReload);
		}

		internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
		{
			IScheduleDay cellValue = e.Style.CellValue as IScheduleDay;
			if (cellValue != null && _useScheduling)
				AddMarkersToCell(e, cellValue, cellValue.SignificantPart());
		}



		public void SelectDateIfExists(DateOnly dateOnly)
		{
			var rows = TheGrid.RowCount;
			var cols = TheGrid.ColCount;

			for (var row = 0; row < rows; row++)
			{
				for (var col = 0; col < cols; col++)
				{
					var tag = TheGrid[row, col].Tag;

					if (!(tag is DateOnly)) continue;
					if (!tag.Equals(dateOnly)) continue;
					var info = GridRangeInfo.Cell(row, col);
					TheGrid.Selections.Clear(true);
					TheGrid.CurrentCell.Activate(row, col, GridSetCurrentCellOptions.SetFocus);
					TheGrid.Selections.ChangeSelection(info, info, true);
					TheGrid.CurrentCell.MoveTo(row, col);

					break;
				}
			}
		}
	}
}
