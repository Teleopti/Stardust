﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
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
		private readonly AgentRestrictionGrid _agentRestrictionGrid;
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private bool _useScheduling;

		public AgentRestrictionsDetailView(AgentRestrictionGrid agentRestrictionGrid, GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IWorkShiftWorkTime workShiftWorkTime)
			: base(grid)
		{
			if(schedulerState == null) throw new ArgumentNullException("schedulerState");

			_model = new AgentRestrictionsDetailModel(schedulerState.RequestedPeriod.Period());
			Presenter = new AgentRestrictionsDetailPresenter(this, _model, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag);

			_agentRestrictionGrid = agentRestrictionGrid;
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
			ViewGrid.Name = "RestrictionView";

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
			if(schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor = new AgentRestrictionsDetailEffectiveRestrictionExtractor(_workShiftWorkTime, restrictionExtractor, schedulingOptions);
			var preferenceNightRestChecker = new PreferenceNightRestChecker();
			_model.LoadDetails(scheduleMatrixPro, restrictionExtractor, schedulingOptions, effectiveRestrictionExtractor, periodTarget, preferenceNightRestChecker);
			_useScheduling = schedulingOptions.UseScheduling;
			//ViewGrid.Refresh();
			//InitializeGrid();	
		}

		public override void AddSelectedSchedulesInColumnToList(GridRangeInfo range, int colIndex, ICollection<IScheduleDay> selectedSchedules)
		{
			if(range == null) throw new ArgumentNullException("range");
			if(selectedSchedules == null) throw new ArgumentNullException("selectedSchedules");

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

		public override Point GetCellPositionForAgentDay(IEntity person, System.DateTime dayDate)
		{
			Point point = new Point(-1, -1);

			for (int i = 1; i <= ViewGrid.RowCount; i++)
			{
				for (int j = 1; j <= ViewGrid.ColCount; j++)
				{
					IScheduleDay schedulePart = ViewGrid.Model[i, j].CellValue as IScheduleDay;

					if (schedulePart != null && schedulePart.Period.Contains(dayDate))
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
			//DateOnly tag;
			var tag = TheGrid[ViewGrid.CurrentCell.RowIndex, ViewGrid.CurrentCell.ColIndex].Tag;
			if ((tag is DateOnly)) return (DateOnly)tag;
			return Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;

			//if (ViewGrid.CurrentCell.ColIndex >= (int)ColumnType.StartScheduleColumns)
			//{
			//    tag = (DateOnly)ViewGrid.Model[1, ViewGrid.CurrentCell.ColIndex].Tag;
			//}
			//else
			//{
			//    tag = Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;
			//}

			//return tag;
		}

		public override void InvalidateSelectedRows(IEnumerable<IScheduleDay> schedules)
		{
			//if (_singleAgentRestrictionPresenter == null)
			//    return;
			//AgentInfoHelper agentInfoHelper = _singleAgentRestrictionPresenter.SelectedAgentInfo();
			//if (agentInfoHelper != null)
			//    ((RestrictionSummaryPresenter)Presenter).GetNextPeriod(agentInfoHelper);

			if(schedules == null) throw new ArgumentNullException("schedules");

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

		internal override void CellClick(object sender, GridCellClickEventArgs e)
		{
			//handle selection when click on col header
			if (e.RowIndex == 0)
				e.Cancel = true;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void DeleteSelectedRestrictions(IUndoRedoContainer undoRedo, IScheduleTag defaultScheduleTag)
		{
			IScheduleMatrixPro matrix = _agentRestrictionGrid.CurrentDisplayRow.Matrix;
			var clipHandler = new ClipHandler<IScheduleDay>();
			GridHelper.GridCopySelection(ViewGrid, clipHandler, true);
			var list = DeleteList(clipHandler);
			IList<IScheduleDay> strippedList = new List<IScheduleDay>();
			foreach (var scheduleDay in list)
			{
				IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(scheduleDay.DateOnlyAsPeriod.DateOnly);
				if(matrix.UnlockedDays.Contains(scheduleDayPro))
				{
					strippedList.Add(scheduleDay);
				}
			}

			undoRedo.CreateBatch(string.Format(CultureInfo.CurrentCulture, Resources.UndoRedoDeleteSchedules,
											   strippedList.Count));

			var deleteService = new DeleteSchedulePartService(Presenter.SchedulerState.SchedulingResultState);

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(Presenter.SchedulerState.SchedulingResultState,
														 new SchedulerStateScheduleDayChangedCallback(
															 new ResourceCalculateDaysDecider(), Presenter.SchedulerState),
														 new ScheduleTagSetter(defaultScheduleTag));

			var options = new DeleteOption { Preference = true, StudentAvailability = true };
			deleteService.Delete(strippedList, rollbackService, options);


			undoRedo.CommitBatch();
			OnPasteCompleted();

			//InvalidateSelectedRows(new List<IScheduleDay> { Presenter.ClipHandlerSchedule.ClipList[0].ClipValue });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void PasteSelectedRestrictions(IUndoRedoContainer undoRedo)
		{
			if (Presenter.ClipHandlerSchedule.ClipList.Count == 0)
				return;

			if (!Clipboard.ContainsData("PersistableScheduleData"))
				return;

			var options = new PasteOptions { Preference = true, StudentAvailability = true };
			using (var pasteAction = new SchedulePasteAction(options, Presenter.LockManager, Presenter.SchedulePartFilter))
			{
				undoRedo.CreateBatch(Resources.UndoRedoPaste);
				IList<IScheduleDay> pasteList =
								   GridHelper.HandlePasteScheduleGridFrozenColumn(ViewGrid, Presenter.ClipHandlerSchedule, pasteAction);

				IScheduleMatrixPro matrix = _agentRestrictionGrid.CurrentDisplayRow.Matrix;

				IList<IScheduleDay> strippedList = new List<IScheduleDay>();
				foreach (var scheduleDay in pasteList)
				{
					IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(scheduleDay.DateOnlyAsPeriod.DateOnly);
					if (matrix.UnlockedDays.Contains(scheduleDayPro))
					{
						strippedList.Add(scheduleDay);
					}
				}

				if (!pasteList.IsEmpty())
					Presenter.TryModify(strippedList);

				undoRedo.CommitBatch();

				OnPasteCompleted();

				InvalidateSelectedRows(new List<IScheduleDay> { Presenter.ClipHandlerSchedule.ClipList[0].ClipValue });
			}
		}
	}
}
