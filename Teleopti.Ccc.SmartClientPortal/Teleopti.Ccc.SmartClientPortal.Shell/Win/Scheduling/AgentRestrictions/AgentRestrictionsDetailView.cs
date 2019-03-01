using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsDetailView : ScheduleViewBase, IAgentRestrictionsDetailView
	{
		private readonly AgentRestrictionsDetailModel _model;
		private readonly IWorkShiftWorkTime _workShiftWorkTime;
		private bool _useScheduling;
		private IScheduleMatrixPro _currentMatrix;

		public AgentRestrictionsDetailView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IWorkShiftWorkTime workShiftWorkTime, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
			: base(grid, timeZoneGuard)
		{
			if(schedulerState == null) throw new ArgumentNullException("schedulerState");

			_model = new AgentRestrictionsDetailModel(schedulerState.RequestedPeriod.Period());
			Presenter = new AgentRestrictionsDetailPresenter(this, _model, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer);

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

			var restrictionCombiner = new RestrictionCombiner();
			var personalShiftRestrictionCombiner = new PersonalShiftRestrictionCombiner(restrictionCombiner);
			var meetingRestrictionCombinder = new MeetingRestrictionCombiner(restrictionCombiner);
			IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor = new AgentRestrictionsDetailEffectiveRestrictionExtractor(_workShiftWorkTime, restrictionExtractor, schedulingOptions, personalShiftRestrictionCombiner, meetingRestrictionCombinder);
			var preferenceNightRestChecker = new PreferenceNightRestChecker();
			_model.LoadDetails(scheduleMatrixPro, schedulingOptions, effectiveRestrictionExtractor, periodTarget, preferenceNightRestChecker);
			_useScheduling = schedulingOptions.UseScheduling;
		}

		public void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, IRestrictionExtractor restrictionExtractor)
		{
			_currentMatrix = scheduleMatrixPro;
			var restrictionCombiner = new RestrictionCombiner();
			var personalShiftRestrictionCombiner = new PersonalShiftRestrictionCombiner(restrictionCombiner);
			var meetingRestrictionCombinder = new MeetingRestrictionCombiner(restrictionCombiner);
			var schedulingOptions = new RestrictionSchedulingOptions
			{
				UseScheduling = true,
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false
			};
			IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor =
				new AgentRestrictionsDetailEffectiveRestrictionExtractor(_workShiftWorkTime, restrictionExtractor,
					schedulingOptions, personalShiftRestrictionCombiner, meetingRestrictionCombinder);
			var preferenceNightRestChecker = new PreferenceNightRestChecker();

			_model.LoadDetails(scheduleMatrixPro, schedulingOptions, effectiveRestrictionExtractor, TimeSpan.Zero, preferenceNightRestChecker);
			_useScheduling = schedulingOptions.UseScheduling;
		}

		public override void AddSelectedSchedulesInColumnToList(GridRangeInfo range, int colIndex, List<IScheduleDay> selectedSchedules)
		{
			if(range == null) throw new ArgumentNullException("range");
			if(selectedSchedules == null) throw new ArgumentNullException("selectedSchedules");

			for (int j = range.Top; j <= range.Bottom; j++)
			{
				if (colIndex >= 0)
				{
					IScheduleDay schedulePart = ViewGrid.Model[j, colIndex].CellValue as IScheduleDay;
					if (schedulePart != null)
					{
						selectedSchedules.Add(getNewSchedulePartForPersonToAvoidConflictWithMyself(schedulePart));
					}
				}

			}
		}

		private IScheduleDay getNewSchedulePartForPersonToAvoidConflictWithMyself(IScheduleDay schedulePart)
		{
			return Presenter.SchedulerState.Schedules[schedulePart.Person].ScheduledDay(schedulePart.DateOnlyAsPeriod.DateOnly);
		}

		public override Point GetCellPositionForAgentDay(IEntity person, DateOnly dayDate)
		{
			Point point = new Point(-1, -1);

			for (int i = 1; i <= ViewGrid.RowCount; i++)
			{
				for (int j = 1; j <= ViewGrid.ColCount; j++)
				{
					IScheduleDay schedulePart = ViewGrid.Model[i, j].CellValue as IScheduleDay;

					if (schedulePart != null && schedulePart.Period.Contains(dayDate.Date))
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
			GridRangeInfo info = GridRangeInfo.Cell(ViewGrid.Rows.HeaderCount + 1, ViewGrid.Cols.HeaderCount + 1);
			ViewGrid.Selections.Clear(true);
			ViewGrid.CurrentCell.Activate(ViewGrid.Rows.HeaderCount + 1, ViewGrid.Cols.HeaderCount + 1, GridSetCurrentCellOptions.SetFocus);
			ViewGrid.Selections.ChangeSelection(info, info, true);
			ViewGrid.CurrentCell.MoveTo(ViewGrid.Rows.HeaderCount + 1, ViewGrid.Cols.HeaderCount + 1);
		}

		public override DateOnly SelectedDateLocal()
		{
			var tag = ViewGrid[ViewGrid.CurrentCell.RowIndex, ViewGrid.CurrentCell.ColIndex].Tag;
			if ((tag is DateOnly)) return (DateOnly)tag;
			return Presenter.SelectedPeriod.DateOnlyPeriod.StartDate;
		}

		public override void InvalidateSelectedRow(IScheduleDay schedulePart)
		{
			if (schedulePart == null) throw new ArgumentNullException(nameof(schedulePart));

			Point point = GetCellPositionForAgentDay(schedulePart.Person, schedulePart.DateOnlyAsPeriod.DateOnly);

			if (point.X != -1 && point.Y != -1)
			{
				ViewGrid.InvalidateRange(GridRangeInfo.Row(point.X));
			}
		}

		internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
		{
			IScheduleDay cellValue = e.Style.CellValue as IScheduleDay;
			if (cellValue != null && _useScheduling)
				AddMarkersToCell(e, cellValue, cellValue.SignificantPart());
		}

		internal override void CellClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex == 0)
				e.Cancel = true;
		}

		public void SelectDateIfExists(DateOnly dateOnly)
		{
			var rows = ViewGrid.RowCount;
			var cols = ViewGrid.ColCount;

			for (var row = 0; row < rows; row++)
			{
				for (var col = 0; col < cols; col++)
				{
					var tag = ViewGrid[row, col].Tag;

					if (!(tag is DateOnly)) continue;
					if (!tag.Equals(dateOnly)) continue;
					var info = GridRangeInfo.Cell(row, col);
					ViewGrid.Selections.Clear(true);
					ViewGrid.CurrentCell.Activate(row, col, GridSetCurrentCellOptions.SetFocus);
					ViewGrid.Selections.ChangeSelection(info, info, true);
					ViewGrid.CurrentCell.MoveTo(row, col);

					break;
				}
			}
		}

		public void DeleteSelectedRestrictions(IUndoRedoContainer undoRedo, IScheduleTag defaultScheduleTag, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			IScheduleMatrixPro matrix;
			matrix = _currentMatrix;

			var clipHandler = new ClipHandler<IScheduleDay>();
			GridHelper.GridCopySelection(ViewGrid, clipHandler, true);
			var list = DeleteList(clipHandler);
			IList<IScheduleDay> strippedList = list.Where(s => matrix.UnlockedDays.Any(m => m.Day == s.DateOnlyAsPeriod.DateOnly)).ToList();

			undoRedo.CreateBatch(string.Format(CultureInfo.CurrentCulture, Resources.UndoRedoDeleteSchedules,
											   strippedList.Count));

			var deleteService = new DeleteSchedulePartService();

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(Presenter.SchedulerState.SchedulingResultState,
														 scheduleDayChangeCallback,
														 new ScheduleTagSetter(defaultScheduleTag));

			var options = new DeleteOption { Preference = true, StudentAvailability = true };
			deleteService.Delete(strippedList, rollbackService, options);


			undoRedo.CommitBatch();
			OnPasteCompleted();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void PasteSelectedRestrictions(IUndoRedoContainer undoRedo)
		{
			if (Presenter.ClipHandlerSchedule.ClipList.Count == 0)
				return;

			if (!Clipboard.ContainsData("PersistableScheduleData"))
				return;

			var options = new PasteOptions { Preference = true, StudentAvailability = true };
			var pasteAction = new SchedulePasteAction(options, Presenter.LockManager, Presenter.SchedulePartFilter, TimeZoneGuard);
			undoRedo.CreateBatch(Resources.UndoRedoPaste);
			IList<IScheduleDay> pasteList =
							   GridHelper.HandlePasteScheduleGridFrozenColumn(ViewGrid, Presenter.ClipHandlerSchedule, pasteAction);

			IScheduleMatrixPro matrix;
			matrix = _currentMatrix;

			IList<IScheduleDay> strippedList = pasteList.Where(s => matrix.UnlockedDays.Any(m => m.Day == s.DateOnlyAsPeriod.DateOnly)).ToList();

			if (!pasteList.IsEmpty())
				Presenter.TryModify(strippedList);

			undoRedo.CommitBatch();

			OnPasteCompleted();

			InvalidateSelectedRow(Presenter.ClipHandlerSchedule.ClipList[0].ClipValue);
		}
	}
}
