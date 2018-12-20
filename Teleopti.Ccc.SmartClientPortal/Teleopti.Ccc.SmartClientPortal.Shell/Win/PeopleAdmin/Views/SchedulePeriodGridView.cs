using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public class UpdateClosePreviousEventArgs : EventArgs
	{
		public string ProgressStateText { get; set; }
		public int ProgressStateBar { get; set; }
	}

	public class SchedulePeriodGridView : DropDownGridViewBase
	{
		public event EventHandler<UpdateClosePreviousEventArgs> ClosePreviousStatusChanged;

		private Rectangle rect;

		private IList<ISchedulePeriod> _selectedSchedulePeriodCollection = new List<ISchedulePeriod>();

		private ToolStripMenuItem _addNewSchedulePeriodMenuItem;
		private ToolStripMenuItem _deleteSchedulePeriodMenuItem;
		private ToolStripMenuItem _pasteSpecialSchedulePeriodMenuItem;
		private ToolStripMenuItem _copySpecialSchedulePeriodMenuItem;
		private IShiftCategoryLimitationView _shiftCategoryLimitationView;


		private ColumnBase<SchedulePeriodModel> _fullNameColumn;
		private ColumnBase<SchedulePeriodModel> _startDateColumn;
		private ColumnBase<SchedulePeriodModel> _numberColumn;
		private ColumnBase<SchedulePeriodModel> _scheduleTypeColumn;
		private ColumnBase<SchedulePeriodModel> _pushButtonColumn;
		private ColumnBase<SchedulePeriodModel> _gridInCellColumn;
		private ColumnBase<SchedulePeriodModel> _daysOffColumn;
		private ColumnBase<SchedulePeriodModel> _hoursPerDayColumn;
		private ColumnBase<SchedulePeriodModel> _averageWorkTimePerDayColumn;
		private ColumnBase<SchedulePeriodModel> _overrideDaysOffColumn;
		private ColumnBase<SchedulePeriodModel> _mustHavePreference;
		private ColumnBase<SchedulePeriodModel> _overridePeriodColumn;
		private ColumnBase<SchedulePeriodModel> _balanceInColumn;
		private ColumnBase<SchedulePeriodModel> _extraColumn;
		private ColumnBase<SchedulePeriodModel> _seasonalityColumn;
		private ColumnBase<SchedulePeriodModel> _balanceOutColumn;


		private readonly List<ColumnBase<SchedulePeriodModel>> _gridColumns = new
			List<ColumnBase<SchedulePeriodModel>>();

		private ColumnBase<SchedulePeriodChildModel> _childLineColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridStartDateColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridNumberColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridScheduleTypeColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridFreeDaysColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridHoursPerDayColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridOverrideAverageWorkTimePerDayColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridOverrideDaysOffColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridMustHavePreference;
		private ColumnBase<SchedulePeriodChildModel> _childGridOverridePeriodColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridBalanceInColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridExtraColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridSeasonalityColumn;
		private ColumnBase<SchedulePeriodChildModel> _childGridBalanceOutColumn;

		private readonly IList<IColumn<SchedulePeriodChildModel>> _childGridColumns = new
			List<IColumn<SchedulePeriodChildModel>>();


		internal override ViewType Type
		{
			get { return ViewType.SchedulePeriodView; }
		}

		public override int ParentGridLastColumnIndex
		{
			get { return _gridColumns.Count - TotalHiddenColumnsInParentGrid; }
		}

		internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<SchedulePeriodModel>
															(FilteredPeopleHolder.SchedulePeriodGridViewCollection));
			}

			base.QueryCellInfo(e);
		}

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
				_gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<SchedulePeriodModel>
															 (FilteredPeopleHolder.SchedulePeriodGridViewCollection));
		}

		internal override void CellButtonClicked(GridCellButtonClickedEventArgs e)
		{
			if (e.ColIndex == PushButtonColumnIndex)
			{
				LoadChildGrid(e.RowIndex);
			}
		}

		internal override void DrawCellButton(GridDrawCellButtonEventArgs e)
		{
			if (e.Style.CellType == GridCellModelConstants.CellTypePushButton)
			{
				rect = new Rectangle(e.Button.Bounds.X, e.Button.Bounds.Y, PeopleAdminConstants.RectangleWidth,
					Grid.DefaultRowHeight);
				e.Button.Bounds = rect;
			}
		}

		internal override void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				int rowIndex = Grid.CurrentCell.RowIndex;

				_selectedSchedulePeriodCollection.Clear();
				CanCopyRow = false;
				CanCopyChildRow = false;

				if ((rowIndex > PeopleAdminConstants.GridHeaderIndex) &&
					!FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
					PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					CanCopyRow = true;
					IPerson selectedPerson = FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].Parent;

					foreach (SchedulePeriod period in selectedPerson.PersonSchedulePeriodCollection)
					{
						// Parent processings
						if (!_selectedSchedulePeriodCollection.Contains(period))
							_selectedSchedulePeriodCollection.Add(period);
					}
				}
			}
		}

		private void ParentColumn_CellChanged(object sender, ColumnCellChangedEventArgs<SchedulePeriodModel> e)
		{
			e.DataItem.CanBold = true;

			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _gridColumns.Count, Grid);
		}

		private void ParentColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<SchedulePeriodModel> e)
		{
			if (e.DataItem.CanBold)
			{
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
			}
		}

		internal override void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			GridControl grid = (GridControl)sender;

			ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				grid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;
			if (schedulePeriodChildCollection != null)
				if (ValidCell(e.ColIndex, e.RowIndex, grid.RowCount))
					_childGridColumns[e.ColIndex].SaveCellInfo(e, schedulePeriodChildCollection);

			grid.Refresh();
		}

		internal override void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			PeopleAdminHelper.CreateColWidthForChildGrid(e, ValidColumn(e.Index), ParentGridLastColumnIndex,
														 RenderingAddValue,
														 Grid.ColWidths[e.Index + ParentChildGridMappingValue]);
		}

		internal override void ChildGridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
		{
			PeopleAdminHelper.CreateRowHeightForChildGrid(e);
		}

		internal override void ChildGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			PeopleAdminHelper.CreateColumnCountForChildGrid(e, _childGridColumns.Count -
				PeopleAdminConstants.GridColumnCountMappingValue);
		}

		internal override void ChildGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				((GridControl)sender).Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

			PeopleAdminHelper.CreateRowCountForChildGrid(e, schedulePeriodChildCollection);
		}

		internal override void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			GridControl gridControl = ((GridControl)sender);

			ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				gridControl.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;
			PeopleWorksheet.StateHolder.CurrentChildName = gridControl.Text;

			if (schedulePeriodChildCollection != null)
			{

				if (ValidCell(e.ColIndex, e.RowIndex, gridControl.RowCount))
				{
					_childGridColumns[e.ColIndex].GetCellInfo(e, schedulePeriodChildCollection);
				}
			}
			base.ChildGridQueryCellInfo(sender, e);
		}

		internal override void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			_selectedSchedulePeriodCollection.Clear();
			CanCopyRow = false;
			CanCopyChildRow = false;

			GridModel gridModel = sender as GridModel;

			if (gridModel != null)
			{
				GridControl grid = gridModel.ActiveGridView as GridControl;

				if (grid != null)
				{
					ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
						grid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

					if (schedulePeriodChildCollection != null)
					{
						CanCopyChildRow = true;

						GridRangeInfoList gridRangeInfoList = grid.Model.SelectedRanges;

						HandleChildGridCanCopy(schedulePeriodChildCollection, gridRangeInfoList);
					}
				}
			}
		}

		private void ChildColumn_CellChanged(object sender,
											 ColumnCellChangedEventArgs<SchedulePeriodChildModel> e)
		{
			e.DataItem.CanBold = true;

			GridControl grid =
				FilteredPeopleHolder.SchedulePeriodGridViewCollection[Grid.CurrentCell.RowIndex -
				PeopleAdminConstants.GridCollectionMapValue].GridControl;

			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _childGridColumns.Count, grid);
		}

		private void ChildColumn_CellDisplayChanged(object sender,
													ColumnCellDisplayChangedEventArgs
														<SchedulePeriodChildModel> e)
		{
			if (e.DataItem.CanBold)
			{
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
			}
		}

		internal override void SelectionChanged(GridSelectionChangedEventArgs e, bool eventCancel)
		{
			int rangeLength = Grid.Model.SelectedRanges.Count;
			if (rangeLength == 0)
			{
				eventCancel = true;
				return;
			}

			IList<ISchedulePeriod> list = new List<ISchedulePeriod>();
			IList<ISchedulePeriodModel> gridDataList = new List<ISchedulePeriodModel>();

			for (int rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
			{
				GridRangeInfo rangeInfo = Grid.Model.SelectedRanges[rangeIndex];

				int top = 1; // This is to skip if Range is Empty.
				int length = 0;

				#region Prepare range information

				switch (rangeInfo.RangeType)
				{
					case GridRangeInfoType.Cols:
					case GridRangeInfoType.Table:
						top = 1;
						length = FilteredPeopleHolder.FilteredPersonCollection.Count;
						RowCount += length;
						break;

					case GridRangeInfoType.Rows:
					case GridRangeInfoType.Cells:
						top = rangeInfo.Top;
						length = top + rangeInfo.Height - 1;
						RowCount += rangeInfo.Height;
						break;

					default:
						break;
				}

				#endregion

				#region TODO: need to refactor

				// Fixes for the Bug 4948
				if (rangeInfo.IsTable)
				{

					for (int i = 0; i < FilteredPeopleHolder.SchedulePeriodGridViewCollection.Count; i++)
					{

						// Parent value set
						if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[i].ExpandState)
						{
							if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[i].SchedulePeriod != null)
							{
								list.Add(FilteredPeopleHolder.SchedulePeriodGridViewCollection[i].SchedulePeriod);
								gridDataList.Add(FilteredPeopleHolder.SchedulePeriodGridViewCollection[i]);
							}
						}
						// Child Value Set
						else
						{
							AddChildSchedulePeriodList(list, gridDataList, i);
						}
					}
				}


				else if ((rangeInfo.RangeType == GridRangeInfoType.Rows) &&
					(FilteredPeopleHolder.PersonPeriodGridViewCollection[rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue]
					.ExpandState))
				{
					AddChildSchedulePeriodList(list, gridDataList, rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue);
				}
				#endregion

				else if (top > 0)
					for (int index = top - 1; index < length; index++)
					{
						if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[index].SchedulePeriod != null)
						{
							list.Add(FilteredPeopleHolder.SchedulePeriodGridViewCollection[index].SchedulePeriod);
							gridDataList.Add(FilteredPeopleHolder.SchedulePeriodGridViewCollection[index]);
						}
					}
			}
			_shiftCategoryLimitationView.SetSchedulePeriodList(list);
		}

		private void AddChildSchedulePeriodList(IList<ISchedulePeriod> list, IList<ISchedulePeriodModel> gridDataList, int index)
		{
			GridControl grid = FilteredPeopleHolder.SchedulePeriodGridViewCollection[index].GridControl;

			if (grid != null)
			{
				ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				grid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

				if (schedulePeriodChildCollection != null && schedulePeriodChildCollection.Count > 0)
				{
					for (int i = 0; i < schedulePeriodChildCollection.Count; i++)
					{
						list.Add(schedulePeriodChildCollection[i].SchedulePeriod);
						gridDataList.Add(schedulePeriodChildCollection[i]);
					}
				}

			}
		}

		internal override void ChildGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			GridControl grid = sender as GridControl;

			if (grid != null)
			{
				ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
					grid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

				if (schedulePeriodChildCollection != null)
				{
					int rangeLength = grid.Model.SelectedRanges.Count;

					if (rangeLength != 0)
					{

						IList<ISchedulePeriod> list = new List<ISchedulePeriod>();
						IList<ISchedulePeriodModel> gridDataList = new List<ISchedulePeriodModel>();

						for (int rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
						{
							GridRangeInfo rangeInfo = grid.Model.SelectedRanges[rangeIndex];

							int top = 1; // This is to skip if Range is Empty.
							int length = 0;

							// TODO: Need to refactor this /kosalanp.

							#region Prepare range information

							switch (rangeInfo.RangeType)
							{
								case GridRangeInfoType.Cols:
								case GridRangeInfoType.Table:
									top = 1;
									length = FilteredPeopleHolder.FilteredPersonCollection.Count;
									RowCount += length;
									break;

								case GridRangeInfoType.Rows:
								case GridRangeInfoType.Cells:
									top = rangeInfo.Top;
									length = top + rangeInfo.Height - 1;
									RowCount += rangeInfo.Height;
									break;

								default:
									break;
							}

							#endregion

							if (top > 0)
								for (int index = top - 1; index < length; index++)
								{
									if (schedulePeriodChildCollection.Count >= length)
									{
										list.Add(schedulePeriodChildCollection[index].SchedulePeriod);
										gridDataList.Add(schedulePeriodChildCollection[index]);
									}
								}
						}
						_shiftCategoryLimitationView.SetSchedulePeriodList(list);
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SchedulePeriodGridView"/> class.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="filteredPeopleHolder">The filtered people holder.</param>
		/// <remarks>
		/// Created By: kosalanp
		/// Created Date: 07-05-2008
		/// </remarks>
		public SchedulePeriodGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder)
			: base(grid, filteredPeopleHolder)
		{
			InitializeCellModels();
			GridDropDownMonthCalendarAdvCellModel cellModel =
				new GridDropDownMonthCalendarAdvCellModel(grid.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
		}

		public bool Toggle78424 { get; set; }

		public override void SetView(IShiftCategoryLimitationView view)
		{
			_shiftCategoryLimitationView = view;
			_shiftCategoryLimitationView.ShiftCategories = PeopleWorksheet.StateHolder.ShiftCategories;
			_shiftCategoryLimitationView.InitializePresenter();
		}

		/// <summary>
		/// Inits this instance.
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-06-11
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeCellModels()
		{
			Grid.CellModels.Add(GridCellModelConstants.CellTypeGridInCell, new GridInCellModel(Grid.Model));

			PercentCellModel cellModel = new PercentCellModel(Grid.Model);
			cellModel.MinMax = new MinMax<double>(-1, 1);
			Grid.CellModels.Add(GridCellModelConstants.CellTypePercentCell, cellModel);
		}

		/// <summary>
		/// Creates the child grid header.
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-06-11
		/// </remarks>
		private void CreateChildGridHeader()
		{
			_childGridColumns.Add(new RowHeaderColumn<SchedulePeriodChildModel>());

			_childLineColumn = new LineColumn<SchedulePeriodChildModel>("FullName");
			_childGridColumns.Add(_childLineColumn);

			_childGridStartDateColumn = new EditableDateOnlyColumnForPeriodGrids<SchedulePeriodChildModel>
				("PeriodDate", UserTexts.Resources.StartDate);
			_childGridStartDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridStartDateColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridStartDateColumn);

			_childGridNumberColumn = new NumericCellColumnPeriod<SchedulePeriodChildModel>("Number", UserTexts.Resources.Number, 1);
			_childGridNumberColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridNumberColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridNumberColumn);

			var dropDownList = LanguageResourceHelper.TranslateEnumToList<SchedulePeriodType>();
			if(Toggle78424)
				dropDownList.RemoveAt(dropDownList.Count-1);
			
			_childGridScheduleTypeColumn = new DropDownColumnForSchedulePeriodGrids<SchedulePeriodChildModel,
				KeyValuePair<SchedulePeriodType, string>>("PeriodType",
									 UserTexts.Resources.Type,
									 dropDownList,
									"value", "key", typeof(SchedulePeriodType));
			_childGridScheduleTypeColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridScheduleTypeColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridScheduleTypeColumn);

			_childGridHoursPerDayColumn =
				new ReadOnlyHourMinutesColumnForSchedulePeriod<SchedulePeriodChildModel>
					("AverageWorkTimePerDay", UserTexts.Resources.HoursPerDay);
			_childGridHoursPerDayColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridHoursPerDayColumn);

			_childGridOverrideAverageWorkTimePerDayColumn = new EditableAverageWorkTimeInSchedulePeriodColumn
				<SchedulePeriodChildModel>("AverageWorkTimePerDayOverride", UserTexts.Resources.OverrideHoursPerDay);
			_childGridOverrideAverageWorkTimePerDayColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridOverrideAverageWorkTimePerDayColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridOverrideAverageWorkTimePerDayColumn);

			_childGridFreeDaysColumn = new ReadOnlyTextColumnPeriod<SchedulePeriodChildModel>("DaysOff", UserTexts.Resources.DaysOff, true);
			_childGridFreeDaysColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridFreeDaysColumn);

			_childGridOverrideDaysOffColumn = new NumericCellColumnForSchedulePeriod
				<SchedulePeriodChildModel>("OverrideDaysOff", UserTexts.Resources.OverrideDaysOff, 150);
			_childGridOverrideDaysOffColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridOverrideDaysOffColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridOverrideDaysOffColumn);

			_childGridMustHavePreference =
				new NumericCellColumnPeriod<SchedulePeriodChildModel>("MustHavePreference", UserTexts.Resources.MustHave);
			_childGridMustHavePreference.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridMustHavePreference.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridMustHavePreference);

			_childGridOverridePeriodColumn = new EditablePeriodTimeOverrideSchedulePeriodColumn<SchedulePeriodChildModel>
				("PeriodTime", UserTexts.Resources.OverridePeriod);
			_childGridOverridePeriodColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridOverridePeriodColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridOverridePeriodColumn);

			_childGridBalanceInColumn =
				new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodChildModel>
				("BalanceIn", UserTexts.Resources.BalanceIn);
			_childGridBalanceInColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridBalanceInColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridBalanceInColumn);

			_childGridExtraColumn =
				new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodChildModel>
				("Extra", UserTexts.Resources.Extra);
			_childGridExtraColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridExtraColumn);

			_childGridSeasonalityColumn =
				 new PercentageCellColumnForSchedulePeriod<SchedulePeriodChildModel>
				("Seasonality", UserTexts.Resources.Seasonality);
			_childGridSeasonalityColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridSeasonalityColumn);

			_childGridBalanceOutColumn =
				new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodChildModel>
				("BalanceOut", UserTexts.Resources.BalanceOut);
			_childGridBalanceOutColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridBalanceOutColumn);
		}

		private void CreateParentGridHeaders()
		{
			_gridColumns.Add(new RowHeaderColumn<SchedulePeriodModel>());

			_pushButtonColumn = new PushButtonColumn<SchedulePeriodModel>(UserTexts.Resources.FullName, "PeriodCount");
			_gridColumns.Add(_pushButtonColumn);

			_gridInCellColumn = new GridInCellColumn<SchedulePeriodModel>("GridControl");
			_gridColumns.Add(_gridInCellColumn);

			_fullNameColumn = new ReadOnlyTextColumn<SchedulePeriodModel>("FullName", UserTexts.Resources.FullName);
			_fullNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_gridColumns.Add(_fullNameColumn);

			_startDateColumn = new EditableDateOnlyColumnForPeriodGrids<SchedulePeriodModel>("PeriodDate", UserTexts.Resources.Date);
			_startDateColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_startDateColumn.CellChanged += ParentColumn_CellChanged;
			_startDateColumn.ColumnComparer = new SchedulePeriodStartDateComparer();
			_gridColumns.Add(_startDateColumn);

			_numberColumn = new NumericCellColumnPeriod<SchedulePeriodModel>("Number", UserTexts.Resources.Number, 1);
			_numberColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_numberColumn.CellChanged += ParentColumn_CellChanged;
			_numberColumn.ColumnComparer = new SchedulePeriodNumberComparer();
			_gridColumns.Add(_numberColumn);
			var dropDownList = LanguageResourceHelper.TranslateEnumToList<SchedulePeriodType>();
			if (Toggle78424)
				dropDownList.RemoveAt(dropDownList.Count - 1);

			_scheduleTypeColumn = new DropDownColumnForSchedulePeriodGrids<SchedulePeriodModel, KeyValuePair<SchedulePeriodType, string>>
				("PeriodType", UserTexts.Resources.Type, dropDownList, "value", "key", typeof(SchedulePeriodType));
			//_scheduleTypeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			//_scheduleTypeColumn.CellChanged += ParentColumn_CellChanged;
			//_scheduleTypeColumn.ColumnComparer = new SchedulePeriodUnitComparer();
			//_gridColumns.Add(_scheduleTypeColumn);
			_scheduleTypeColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_scheduleTypeColumn.CellChanged += ParentColumn_CellChanged;
			_scheduleTypeColumn.ColumnComparer = new SchedulePeriodUnitComparer();
			_gridColumns.Add(_scheduleTypeColumn);

			_hoursPerDayColumn = new ReadOnlyHourMinutesColumnForSchedulePeriod<SchedulePeriodModel>
				("AverageWorkTimePerDay", UserTexts.Resources.HoursPerDay);
			_hoursPerDayColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_hoursPerDayColumn.ColumnComparer = new SchedulePeriodAverageWorkTimePerDayComparer();
			_gridColumns.Add(_hoursPerDayColumn);

			_averageWorkTimePerDayColumn = new EditableAverageWorkTimeInSchedulePeriodColumn<SchedulePeriodModel>
				("AverageWorkTimePerDayOverride", UserTexts.Resources.OverrideHoursPerDay);
			_averageWorkTimePerDayColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_averageWorkTimePerDayColumn.CellChanged += ParentColumn_CellChanged;
			_averageWorkTimePerDayColumn.ColumnComparer = new SchedulePeriodAverageWorkTimePerDayComparer();
			_gridColumns.Add(_averageWorkTimePerDayColumn);

			_daysOffColumn = new ReadOnlyTextColumnPeriod<SchedulePeriodModel>
				("DaysOff", UserTexts.Resources.DaysOff, true);
			_daysOffColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_daysOffColumn.ColumnComparer = new SchedulePeriodDaysOffComparer();
			_gridColumns.Add(_daysOffColumn);

			_overrideDaysOffColumn = new NumericCellColumnForSchedulePeriod<SchedulePeriodModel>
					("OverrideDaysOff", UserTexts.Resources.OverrideDaysOff, 150);
			_overrideDaysOffColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_overrideDaysOffColumn.CellChanged += ParentColumn_CellChanged;
			_overrideDaysOffColumn.ColumnComparer = new SchedulePeriodDaysOffComparer();
			_gridColumns.Add(_overrideDaysOffColumn);

			_mustHavePreference = new NumericCellColumnPeriod<SchedulePeriodModel>
				("MustHavePreference", UserTexts.Resources.MustHave);
			_mustHavePreference.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_mustHavePreference.CellChanged += ParentColumn_CellChanged;
			_mustHavePreference.ColumnComparer = new SchedulePeriodMustHavePreferenceComparer();
			_gridColumns.Add(_mustHavePreference);

			_overridePeriodColumn = new EditablePeriodTimeOverrideSchedulePeriodColumn<SchedulePeriodModel>
				("PeriodTime", UserTexts.Resources.OverridePeriod);
			_overridePeriodColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_overridePeriodColumn.CellChanged += ParentColumn_CellChanged;
			_overridePeriodColumn.ColumnComparer = new SchedulePeriodTimeOverrideComparer();
			_gridColumns.Add(_overridePeriodColumn);

			_balanceInColumn = new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodModel>
				("BalanceIn", UserTexts.Resources.BalanceIn);
			_balanceInColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_balanceInColumn.CellChanged += ParentColumn_CellChanged;
			_balanceInColumn.ColumnComparer = new SchedulePeriodBalanceInComparer();
			_gridColumns.Add(_balanceInColumn);

			_extraColumn = new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodModel>
				("Extra", UserTexts.Resources.Extra);
			_extraColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_extraColumn.CellChanged += ParentColumn_CellChanged;
			_extraColumn.ColumnComparer = new SchedulePeriodExtraComparer();
			_gridColumns.Add(_extraColumn);

			_seasonalityColumn = new PercentageCellColumnForSchedulePeriod<SchedulePeriodModel>
				("Seasonality", "Seasonality");
			_seasonalityColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_seasonalityColumn.CellChanged += ParentColumn_CellChanged;
			_seasonalityColumn.ColumnComparer = new SchedulePeriodSeasonalityComparer();
			_gridColumns.Add(_seasonalityColumn);

			_balanceOutColumn = new EditableTimeSpanInSchedulePeriodColumn<SchedulePeriodModel>
				("BalanceOut", UserTexts.Resources.BalanceOut);
			_balanceOutColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_balanceOutColumn.CellChanged += ParentColumn_CellChanged;
			_balanceOutColumn.ColumnComparer = new SchedulePeriodBalanceOutComparer();
			_gridColumns.Add(_balanceOutColumn);
		}

		private void LoadChildGrid(int rowIndex)
		{
			if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
			{
				// Set grid range for covered ranged
				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex, rowIndex,
															 ParentGridLastColumnIndex);

				// Set out of focus form current cell.This helps  to fire save cell info in child grid.
				SetOutOfFocusFromCurrentCell();

				if (!FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
					PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = true;

					GridControl grid =
						FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].GridControl;

					GetChildSchedulePeriods(rowIndex, grid);

					LoadChildGrid(rowIndex, GridInCellColumnIndex, gridInfo);
					Grid.CurrentCell.MoveTo(rowIndex, GridInCellColumnIndex);
				}
				else
				{
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = false;

					// Get child grid and dispose it
					GridControl gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
					RemoveChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;
					FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(rowIndex -
						PeopleAdminConstants.GridCollectionMapValue);
				}

				Grid.Invalidate();
			}
		}

		private void RemoveChildGrid(GridControl abstractGrid, GridRangeInfo gridRange)
		{
			Grid.CoveredRanges.Remove(gridRange);
			Grid.Controls.Remove(abstractGrid);
		}

		private void LoadChildGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
		{
			BindDropDownGridEvents();

			GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
								PeopleWorksheet.StateHolder.SchedulePeriodChildGridData,
								_childGridColumns.Count - 1, PeopleWorksheet.StateHolder.CurrentChildName);
		}

		private void AddSchedulePeriod()
		{
			if (Grid.Model.CurrentCellInfo == null) return;

			InsertSchedulePeriod();
		}

		private void InsertSchedulePeriod()
		{
			GridRangeInfoList gridRangeInfoList = Grid.Model.SelectedRanges;

			for (int index = 0; index < gridRangeInfoList.Count; index++)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index];

				if (gridRangeInfo.IsTable || gridRangeInfo.IsCols)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 1; i <= Grid.RowCount; i++)
						AddSchedulePeriod(i);
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						AddSchedulePeriod(gridRangeInfo.Top);
					}
					else
					{
						for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
						{
							AddSchedulePeriod(row);
						}
					}
				}
			}
		}

		private void AddSchedulePeriod(int rowIndex)
		{
			if (rowIndex == 0)
				return;

			FilteredPeopleHolder.AddSchedulePeriod(rowIndex - 1);

			GridControl grid =
				FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].
					GridControl;

			GetChildSchedulePeriods(rowIndex, grid);

			if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].ExpandState)
			{
				CellEmbeddedGrid childGrid = Grid[rowIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;
				if (childGrid != null)
				{
					// Merging name column's all cells
					childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, childGrid.RowCount + 1, 1));

					childGrid.Tag = PeopleWorksheet.StateHolder.SchedulePeriodChildGridDataWhenAddChild;
					childGrid.Refresh();
					Grid.RowHeights[rowIndex] += DefaultRowHeight;
				}
			}
			else
			{
				FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(rowIndex - 1);
				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex - 1, rowIndex,
															 ParentGridLastColumnIndex);
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void GetChildSchedulePeriods(int rowIndex, GridControl grid)
		{
			if (grid != null)
			{
				ReadOnlyCollection<SchedulePeriodChildModel> cachedCollection = grid.Tag as
																						  ReadOnlyCollection
																							  <
																							  SchedulePeriodChildModel
																							  >;

				PeopleWorksheet.StateHolder.GetChildSchedulePeriods(rowIndex - 1,
																	FilteredPeopleHolder.
																		SchedulePeriodGridViewCollection,
																	cachedCollection, FilteredPeopleHolder.CommonNameDescription);
			}
			else
			{
				PeopleWorksheet.StateHolder.GetChildSchedulePeriods(rowIndex - 1,
																	FilteredPeopleHolder.
																		SchedulePeriodGridViewCollection, FilteredPeopleHolder.CommonNameDescription);
			}
		}

		private void DeleteWhenAllSelected()
		{
			// This scenario is used for when user is selecting entire grid using button give top in 
			// that grid.
			for (int i = 1; i <= Grid.RowCount; i++)
			{
				if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[i - 1].ExpandState)
				{
					RemoveChild(i, true);
				}
				else
				{
					FilteredPeopleHolder.DeleteSchedulePeriod(i - 1, FilteredPeopleHolder.SelectedDate);

					FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(i - 1);
					Grid.InvalidateRange(
						GridRangeInfo.Cells(i, GridInCellColumnIndex - 1, i, ParentGridLastColumnIndex));
				}
			}
		}

		private void DeleteWhenRangeSelected(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo.Height == 1)
			{
				DeleteSchedulePeriod(gridRangeInfo.Top, gridRangeInfo.IsRows);
			}
			else
			{
				for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
				{
					if (row != 0)
					{
						DeleteSchedulePeriod(row, gridRangeInfo.IsRows);
					}
				}
			}
		}

		private void DeleteSchedulePeriod(int index, bool isRows)
		{
			if (index == 0)
				return;
			if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[index - 1].ExpandState)
			{
				// Remove child for child grid
				RemoveChild(index, isRows);
			}
			else
			{
				ISchedulePeriod currentSchedulePeriod = FilteredPeopleHolder.SchedulePeriodGridViewCollection[index -
																											  1].
					SchedulePeriod;

				FilteredPeopleHolder.DeleteSchedulePeriod(index - 1, currentSchedulePeriod);

				FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(index - 1);

				Grid.InvalidateRange(GridRangeInfo.Cells(index, 1,
														 index, ParentGridLastColumnIndex));
			}
		}

		private void SchedulePeriodChildDelete(int rowIndex, CellEmbeddedGrid childGrid)
		{
			FilteredPeopleHolder.DeleteSchedulePeriod(rowIndex - 1);

			ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				childGrid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

			IList<SchedulePeriodChildModel> periodCollection =
				new List<SchedulePeriodChildModel>(schedulePeriodChildCollection);
			periodCollection.Clear();

			GridRangeInfo gridInfo =
				GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);

			childGrid.Tag = new ReadOnlyCollection<SchedulePeriodChildModel>(periodCollection);
			childGrid.RowCount = periodCollection.Count;
			childGrid.Invalidate();

			// remove child grid
			FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].ExpandState = false;
			FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].GridControl = null;

			//// Get child grid and dispose it
			GridControl gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
			RemoveChildGrid(gridControl, gridInfo);

			Grid.RowHeights[rowIndex] = DefaultRowHeight;

			FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(rowIndex - 1);

			Grid.InvalidateRange(gridInfo);
		}

		private void RemoveChild(int rowIndex, bool isDeleteAll)
		{
			CellEmbeddedGrid childGrid = Grid[rowIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;

			if (childGrid != null)
			{
				if (isDeleteAll)
				{
					SchedulePeriodChildDelete(rowIndex, childGrid);
				}
				else
				{
					GridRangeInfoList gridRangeInfoList = childGrid.Model.SelectedRanges.GetRowRanges(GridRangeInfoType.Cells | GridRangeInfoType.Rows);
					for (int index = gridRangeInfoList.Count; index > 0; index--)
					{
						GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];
						var top = gridRangeInfo.Top;
						var bottom = gridRangeInfo.Bottom;
						if (top == 0) continue;
						if (gridRangeInfo.Height == 1)
						{
							SchedulePeriodChildDelete(rowIndex, top - 1, childGrid);
						}
						else
						{
							for (int row = bottom; row >= top; row--)
							{
								SchedulePeriodChildDelete(rowIndex, row - 1, childGrid);
							}
						}
					}
				}
			}
		}

		private void SchedulePeriodChildDelete(int rowIndex, int childPersonPeriodIndex,
												CellEmbeddedGrid childGrid)
		{
			GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex, rowIndex,
														 ParentGridLastColumnIndex);

			ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
				childGrid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;

			if (schedulePeriodChildCollection == null) return;

			ISchedulePeriod schedulePeriod = schedulePeriodChildCollection[childPersonPeriodIndex].ContainedEntity;

			FilteredPeopleHolder.DeleteSchedulePeriod(rowIndex - 1, schedulePeriod);

			IPerson person = FilteredPeopleHolder.FilteredPersonCollection[rowIndex - 1];

			if (person.PersonSchedulePeriodCollection.Count == 0)
			{
				DisposeChildGridWhenDelete(rowIndex, gridInfo);

				FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(rowIndex - 1);
				Grid.InvalidateRange(gridInfo);
			}

			else
			{
				IList<SchedulePeriodChildModel> scheduleChildList =
					new List<SchedulePeriodChildModel>(schedulePeriodChildCollection);
				scheduleChildList.RemoveAt(childPersonPeriodIndex);

				childGrid.Tag = new ReadOnlyCollection<SchedulePeriodChildModel>(scheduleChildList);
				childGrid.RowCount = scheduleChildList.Count;
				childGrid.Invalidate();

				Grid.RowHeights[rowIndex] = childGrid.RowCount * DefaultRowHeight + RenderingAddValue;
			}
		}

		private SchedulePeriodModel CurrentPersonPeriodView => FilteredPeopleHolder.SchedulePeriodGridViewCollection[CurrentRowIndex];

		private bool IsValidRow()
		{
			return Grid.CurrentCell.RowIndex > 0;
		}

		private bool IsCurrentRowExpanded()
		{
			return IsValidRow()
					&& CurrentPersonPeriodView.ExpandState;
		}

		private int CurrentRowIndex => Grid.CurrentCell.RowIndex - 1;

		private int GetColumnIndex()
		{
			int columnIndex;
			if (IsCurrentRowExpanded())
			{
				columnIndex = CurrentPersonPeriodView.GridControl.CurrentCell.ColIndex +
					PeopleAdminConstants.GridCurrentCellColumnAddValue;
			}
			else
			{
				int parentColIndex = gridColumnIndex();
				columnIndex = (parentColIndex == PeopleAdminConstants.GridParentColumnIndexCheckValue) ?
					(PeopleAdminConstants.GridColumnIndexValue) :
					parentColIndex;
			}
			return columnIndex;
		}

		private int gridColumnIndex()
		{
			if (Grid.CurrentCell.ColIndex == -1)
				Grid.CurrentCell.MoveTo(0, 0);
			return Grid.CurrentCell.ColIndex;
		}

		private IEnumerable<Tuple<IPerson, int>> sortSchedulePeriodData(bool isAscending)
		{
			// Gets the filtered people grid data as a collection
			var schedulePeriodcollection = FilteredPeopleHolder.SchedulePeriodGridViewCollection.ToList();

			int columnIndex = GetColumnIndex();

			// Gets the sort column to sort
			var sortColumn = _gridColumns[columnIndex].BindingProperty;
			// Gets the coparer erquired to sort the data
			var comparer = _gridColumns[columnIndex].ColumnComparer;

			if (string.IsNullOrEmpty(sortColumn)) return Enumerable.Empty<Tuple<IPerson,int>>();

			// Holds the results of the sorting process
			IList<SchedulePeriodModel> result;
			if (comparer != null)
			{
				// Sorts the person collection in ascending order
				schedulePeriodcollection.Sort(comparer);
				if (!isAscending)
					schedulePeriodcollection.Reverse();

				result = schedulePeriodcollection;
			}
			else
			{
				// Gets the sorted people collection
				result =
					GridHelper.Sort(new Collection<SchedulePeriodModel>(schedulePeriodcollection),
						sortColumn,
						isAscending);
			}
			
			return result.Select((t, i) => new Tuple<IPerson, int>(t.Parent, i));
		}

		private void CopyAllSchedulePeriods(int rowIndex)
		{
			if (rowIndex >= PeopleAdminConstants.GridHeaderIndex)
			{
				//Current period of receiver replaced with current period of the sender ??
				IPerson personPaste = FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].Parent;
				bool isParent = !FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].ExpandState;

				if (personPaste != null)
				{
					if (CanCopyRow)
					{
						foreach (SchedulePeriod period in _selectedSchedulePeriodCollection)
						{
							if (!personPaste.PersonSchedulePeriodCollection.Contains(period))
							{
								ValidateAndAddSchedulePeriod(personPaste, period);
							}
						}
					}

					if (CanCopyChildRow)
					{
						foreach (SchedulePeriod period in _selectedSchedulePeriodCollection)
						{
							ValidateAndAddSchedulePeriod(personPaste, period);
						}
					}

					if (isParent)
						RefreshParentGridRange(rowIndex);
					else
						RefreshChildGrid(rowIndex);
				}
			}
		}

		private static void ValidateAndAddSchedulePeriod(IPerson person, SchedulePeriod schedulePeriod)
		{
			ISchedulePeriod period = (schedulePeriod.Clone()) as SchedulePeriod;

			if (period == null) return;

			period.DateFrom = PeriodDateService.GetValidPeriodDate(
					PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.SchedulePeriodView, person),
					period.DateFrom);

			// Add person periods to person 
			person.AddSchedulePeriod(period);
		}

		private void RefreshParentGridRange(int index)
		{
			FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(index);
			Grid.InvalidateRange(GridRangeInfo.Cells(index + PeopleAdminConstants.GridCollectionMapValue,
				PeopleAdminConstants.GridInvalidateRowLeftValue, index + PeopleAdminConstants.GridCollectionMapValue
				, ParentGridLastColumnIndex));
		}

		private void RefreshChildGrid(int index)
		{
			PeopleWorksheet.StateHolder.GetChildSchedulePeriods(index,
																FilteredPeopleHolder.SchedulePeriodGridViewCollection, FilteredPeopleHolder.CommonNameDescription);

			CellEmbeddedGrid childGrid = Grid[index + PeopleAdminConstants.GridCollectionMapValue,
				GridInCellColumnIndex].Control as CellEmbeddedGrid;

			if (childGrid != null)
			{
				childGrid.Tag = PeopleWorksheet.StateHolder.SchedulePeriodChildGridData;
				// Merging name column's all cellsட
				childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells
					(PeopleAdminConstants.GridCoveredRangeDefaultValue,
					PeopleAdminConstants.GridCoveredRangeDefaultValue,
					childGrid.RowCount,
					PeopleAdminConstants.GridCoveredRangeDefaultValue));

				Grid.RowHeights[index + PeopleAdminConstants.GridCollectionMapValue] =
					PeopleWorksheet.StateHolder.SchedulePeriodChildGridData.Count * DefaultRowHeight +
					RenderingAddValue;

				childGrid.Refresh();
			}
		}

		private void DisposeChildGridWhenDelete(int rowIndex, GridRangeInfo gridInfo)
		{
			//remove child grid
			FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
				PeopleAdminConstants.GridCollectionMapValue].ExpandState = false;
			FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex -
				PeopleAdminConstants.GridCollectionMapValue].GridControl = null;

			//// Get child grid and dispose it
			GridControl gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
			RemoveChildGrid(gridControl, gridInfo);

			Grid.RowHeights[rowIndex] = DefaultRowHeight + RenderingAddValue;
		}

		private void HandleChildGridCanCopy(ReadOnlyCollection<SchedulePeriodChildModel>
			schedulePeriodChildCollection, GridRangeInfoList gridRangeInfoList)
		{
			for (int index = gridRangeInfoList.Count; index > 0; index--)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

				if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
				{
					ISchedulePeriod schedulePeriod = schedulePeriodChildCollection[gridRangeInfo.Top -
						PeopleAdminConstants.GridCollectionMapValue].ContainedEntity;

					AddSchedulePeriodToSelectedSchedulePeriodCollection(schedulePeriod);
				}
				else
				{
					for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						ISchedulePeriod schedulePeriod =
							schedulePeriodChildCollection[row - PeopleAdminConstants.GridCollectionMapValue]
							.ContainedEntity;

						AddSchedulePeriodToSelectedSchedulePeriodCollection(schedulePeriod);
					}
				}
			}
		}

		private void AddSchedulePeriodToSelectedSchedulePeriodCollection(ISchedulePeriod schedulePeriod)
		{
			InParameter.NotNull("schedulePeriod", schedulePeriod);

			if (!_selectedSchedulePeriodCollection.Contains(schedulePeriod))
			{
				_selectedSchedulePeriodCollection.Add(schedulePeriod);
			}
		}

		public override IEnumerable<Tuple<IPerson,int>> Sort(bool isAscending)
		{
			// Sorts the people data
			var result = sortSchedulePeriodData(isAscending);
			
			return result;
		}

		public override void PerformSort(IEnumerable<Tuple<IPerson, int>> order)
		{
			if (!(order?.Any() ?? false)) return;

			Grid.CurrentCell.MoveLeft();

			// Dispose the child grids
			DisposeChildGrids();

			var result = (from x in FilteredPeopleHolder.SchedulePeriodGridViewCollection
						  join y in order
					on x.Parent equals y.Item1
					into a
				from b in a.DefaultIfEmpty(new Tuple<IPerson, int>(null, int.MaxValue))
				orderby b.Item2
				select x).ToList();
			
			// Sets the filtered list
			FilteredPeopleHolder.SetSortedSchedulePeriodFilteredList(result);

			Grid.CurrentCell.MoveRight();
			
			// Refresh the grid view to get affect the sorted data
			Invalidate();
		}

		internal override void PrepareView()
		{
			ColCount = _gridColumns.Count;

			RowCount = FilteredPeopleHolder.SchedulePeriodGridViewCollection.Count;

			Grid.RowCount = RowCount;
			Grid.ColCount = ColCount - PeopleAdminConstants.GridCollectionMapValue;
			Grid.Model.Data.RowCount = RowCount;

			Grid.Cols.HeaderCount = 0;
			Grid.Rows.HeaderCount = 0;
			Grid.Name = "SchedulePeriodView";

			int length = _gridColumns.Count;
			for (int index = 0; index < length; index++)
			{
				if (index <= 3)
				{
					Grid.ColWidths[index] = _gridColumns[index].PreferredWidth +
											DefaultColumnWidthAddValue;
				}
				else
				{
					Grid.ColWidths[index] = _gridColumns[index].PreferredWidth - DefaultColumnWidthAddValue;
				}
			}
			Grid.ColWidths[0] = _gridColumns[0].PreferredWidth;
		}

		internal override void MergeHeaders()
		{
			Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 0, 3));
		}

		internal override void CreateHeaders()
		{
			CreateParentGridHeaders();
			CreateChildGridHeader();

			// Hide column which is used as a container for grid in cell implementation 
			int pushButtonCol = _gridColumns.IndexOf(_pushButtonColumn);
			Grid.Cols.Hidden[pushButtonCol + 1] = true;
		}

		public override void Invalidate()
		{
			Grid.Invalidate();
		}

		internal override void CreateContextMenu()
		{
			Grid.ContextMenuStrip = new ContextMenuStrip();

			_addNewSchedulePeriodMenuItem = new ToolStripMenuItem(UserTexts.Resources.New);
			_addNewSchedulePeriodMenuItem.Click += AddNewGridRow;
			Grid.ContextMenuStrip.Items.Add(_addNewSchedulePeriodMenuItem);

			_deleteSchedulePeriodMenuItem = new ToolStripMenuItem(UserTexts.Resources.Delete);
			_deleteSchedulePeriodMenuItem.Click += DeleteSelectedGridRows;
			Grid.ContextMenuStrip.Items.Add(_deleteSchedulePeriodMenuItem);

			_copySpecialSchedulePeriodMenuItem = new ToolStripMenuItem(UserTexts.Resources.CopySpecial);
			_copySpecialSchedulePeriodMenuItem.Click += CopySpecial;
			Grid.ContextMenuStrip.Items.Add(_copySpecialSchedulePeriodMenuItem);

			_pasteSpecialSchedulePeriodMenuItem = new ToolStripMenuItem(UserTexts.Resources.PasteNew);
			_pasteSpecialSchedulePeriodMenuItem.Click += PasteSpecial;
			Grid.ContextMenuStrip.Items.Add(_pasteSpecialSchedulePeriodMenuItem);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		/// <remarks>
		/// Created By: kosalanp
		/// Created Date: 07-05-2008
		/// </remarks>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-07-18
		/// </remarks>
		internal override void AddNewGridRow<T>(object sender, T eventArgs)
		{
			AddSchedulePeriod();
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		/// <remarks>
		/// Created By: kosalanp
		/// Created Date: 07-05-2008
		/// </remarks>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-07-21
		/// </remarks>
		internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
		{
			if (Grid.Model.SelectedRanges.Count > 0)
			{
				GridRangeInfoList gridRangeInfoList = Grid.Model.SelectedRanges;
				for (int index = gridRangeInfoList.Count; index > 0; index--)
				{
					GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];

					if (gridRangeInfo.IsTable)
					{
						DeleteWhenAllSelected();
					}
					else
					{
						DeleteWhenRangeSelected(gridRangeInfo);
					}
				}
			}
		}

		/// <summary>
		/// Copies the special.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The event args.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-07-31
		/// </remarks>
		internal void CopySpecial<T>(object sender, T eventArgs)
		{
			if (Grid.Model.CurrentCellInfo == null)
			{
				//TODO:Need to implement when person is not selected scenario 
				return;
			}

			int rowIndex = Grid.CurrentCell.RowIndex;

			if (rowIndex == 0) return;

			_selectedSchedulePeriodCollection.Clear();
			CanCopyRow = false;
			CanCopyChildRow = false;

			if (!FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].ExpandState)
			{
				CanCopyRow = true;
				IPerson selectedPerson = FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex - 1].
					Parent;

				foreach (SchedulePeriod period in selectedPerson.PersonSchedulePeriodCollection)
				{
					// Parent processings
					if (!_selectedSchedulePeriodCollection.Contains(period))
						_selectedSchedulePeriodCollection.Add(period);
				}
			}
			else
			{
				CanCopyChildRow = true;

				// child copy processings
				GridControl grid = Grid[Grid.CurrentCell.RowIndex, GridInCellColumnIndex].Control as GridControl;
				if (grid == null) return;

				ReadOnlyCollection<SchedulePeriodChildModel> schedulePeriodChildCollection =
					grid.Tag as ReadOnlyCollection<SchedulePeriodChildModel>;
				if (schedulePeriodChildCollection == null) return;

				GridRangeInfoList gridRangeInfoList = grid.Model.SelectedRanges;
				for (int index = gridRangeInfoList.Count; index > 0; index--)
				{
					GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];
					if (gridRangeInfo.Height == 1)
					{
						ISchedulePeriod schedulePeriod = schedulePeriodChildCollection[gridRangeInfo.Top - 1].
							ContainedEntity;
						if (!_selectedSchedulePeriodCollection.Contains(schedulePeriod))
							_selectedSchedulePeriodCollection.Add(schedulePeriod);
					}
					else
					{
						for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
						{
							ISchedulePeriod schedulePeriod = schedulePeriodChildCollection[row - 1].
								ContainedEntity;
							if (!_selectedSchedulePeriodCollection.Contains(schedulePeriod))
								_selectedSchedulePeriodCollection.Add(schedulePeriod);
						}
					}
				}
			}
		}

		/// <summary>
		/// Pastes the special.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The event args.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-07-31
		/// </remarks>
		internal void PasteSpecial<T>(object sender, T eventArgs)
		{
			GridRangeInfoList gridRangeInfoList = Grid.Model.Selections.Ranges;

			if (!CanCopyRow && !CanCopyChildRow) return;
			for (int index = gridRangeInfoList.Count; index > 0; index--)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];

				if (gridRangeInfo.IsTable)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 1; i <= Grid.RowCount; i++)
						CopyAllSchedulePeriods(i - 1);
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						CopyAllSchedulePeriods(gridRangeInfo.Top - 1);
					}
					else
					{
						for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
						{
							CopyAllSchedulePeriods(row - 1);
						}
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		/// <remarks>
		/// Created By: kosalanp
		/// Created Date: 07-05-2008
		/// </remarks>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-08-07
		/// </remarks>
		internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
		{
			// Content base copy paste is for grouping grid
			PasteSpecial(sender, eventArgs);
		}

		/// <summary>
		/// Selecteds the date change.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-08-19
		/// </remarks>
		internal override void SelectedDateChange(object sender, EventArgs e)
		{
			// Colleps all expanded rows
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex + 1, GridInCellColumnIndex, rowIndex + 1,
															 ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].ExpandState = false;

					//TODO: 
					//// Get child grid and dispose it
					GridControl gridControl = Grid[rowIndex + 1, GridInCellColumnIndex].Control as GridControl;
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].GridControl = null;
					RemoveChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex + 1] = DefaultRowHeight;
				}
			}

			// ReCall Person periods
			FilteredPeopleHolder.GetParentSchedulePeriods(FilteredPeopleHolder.SelectedDate);
			Grid.Invalidate();
		}

		/// <summary>
		/// Dsiposes the child grids of the grid view.
		/// </summary>
		/// <remarks>
		/// Created By: Savani Nirasha
		/// Created Date: 09-18-2008
		/// </remarks>
		internal override void DisposeChildGrids()
		{
			if (ParentGridLastColumnIndex < 0) return;
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				if (rowIndex >= FilteredPeopleHolder.SchedulePeriodGridViewCollection.Count) break;

				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex + 1, GridInCellColumnIndex, rowIndex + 1,
															 ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					GridControl gridControl = Grid[rowIndex + 1, GridInCellColumnIndex].Control as GridControl;
					FilteredPeopleHolder.SchedulePeriodGridViewCollection[rowIndex].GridControl = null;
					RemoveChildGrid(gridControl, gridInfo);

					FilteredPeopleHolder.GetParentSchedulePeriodWhenUpdated(rowIndex);
					Grid.RowHeights[rowIndex + 1] = DefaultRowHeight;
				}
			}
		}

		internal override void SetSelectedPersons(IList<IPerson> selectedPersons)
		{
			// Selection events will not be raised
			Grid.Model.Selections.Clear(false);

			GridRangeInfo range = GridRangeInfo.Empty;
			foreach (IPerson person in selectedPersons)
			{
				for (int i = 0; i < FilteredPeopleHolder.SchedulePeriodGridViewCollection.Count; i++)
				{
					if (FilteredPeopleHolder.SchedulePeriodGridViewCollection[i].Parent.Id == person.Id)
					{
						range = range.UnionRange(GridRangeInfo.Row(i + 1));
					}
				}
			}
			Grid.Selections.Add(range);
		}

		internal override IList<IPerson> GetSelectedPersons()
		{
			IList<IPerson> selectedPersons = new List<IPerson>();

			GridRangeInfoList gridRangeInfoList = Grid.Model.SelectedRanges;
			for (int index = gridRangeInfoList.Count; index > 0; index--)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];

				if (gridRangeInfo.IsTable)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 0; i < Grid.RowCount; i++)
						selectedPersons.Add(
							FilteredPeopleHolder.SchedulePeriodGridViewCollection[i].Parent);
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						if (gridRangeInfo.Top > 0)
							selectedPersons.Add(
								FilteredPeopleHolder.SchedulePeriodGridViewCollection[gridRangeInfo.Top - 1].Parent);
					}
					else
					{
						for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
						{
							if ((row != 0) && !(row > FilteredPeopleHolder.SchedulePeriodGridViewCollection.Count))
							{
								selectedPersons.Add(
									FilteredPeopleHolder.SchedulePeriodGridViewCollection[row - 1].Parent);
							}
						}
					}
				}
			}
			return selectedPersons;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void OnClosePreviousPeriod()
		{
			var selectedPersons = GetSelectedPersons();
			var selectedDate = FilteredPeopleHolder.SelectedDate;

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var defaultScenario = new DefaultScenarioLoader().Load(new ScenarioRepository(unitOfWork));
				double progressCounter = 1;

				foreach (var selectedPerson in selectedPersons)
				{
					var updateClosePreviousEventArgs = new UpdateClosePreviousEventArgs
					{
						ProgressStateBar = (int)((progressCounter / selectedPersons.Count) * 100),
						ProgressStateText = UserTexts.Resources.ClosingPreviousPeriod
					};

					InvokeClosePreviousStatusChanged(updateClosePreviousEventArgs);

					progressCounter++;

					var schedulePeriod = selectedPerson.SchedulePeriod(selectedDate);

					if (schedulePeriod == null) continue;

					var previousPeriodEndDate = schedulePeriod.DateFrom.AddDays(-1);
					var schedulePeriodPrevious = selectedPerson.SchedulePeriod(previousPeriodEndDate);

					if (schedulePeriodPrevious == null || schedulePeriodPrevious.RealDateTo() != previousPeriodEndDate) continue;

					var timeZoneInfo = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone;
					var dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(schedulePeriodPrevious.DateFrom.Date, previousPeriodEndDate.Date, timeZoneInfo);
					var dateOnlyPeriod = new DateOnlyPeriod(schedulePeriodPrevious.DateFrom, previousPeriodEndDate);
					var schedules = new ScheduleDataLoader().LoadSchedule(unitOfWork, dateTimePeriod, selectedPerson, defaultScenario);
					IScheduleContractTimeCalculator scheduleContractTimeCalculator = new ScheduleContractTimeCalculator(schedules, selectedPerson, dateOnlyPeriod);
					IScheduleTargetTimeCalculator scheduleTargetTimeCalculator = new ScheduleTargetTimeCalculator(schedules, selectedPerson, dateOnlyPeriod);
					var schedulePeriodCloseCalculator = new SchedulePeriodCloseCalculator(scheduleContractTimeCalculator, scheduleTargetTimeCalculator, schedulePeriodPrevious, schedulePeriod);
					schedulePeriodCloseCalculator.CalculateBalanceOut();
				}
			}
		}

		public void InvokeClosePreviousStatusChanged(UpdateClosePreviousEventArgs e)
		{
			var handler = ClosePreviousStatusChanged;
			if (handler != null) handler(this, e);
		}

		protected override void Dispose(bool disposing)
		{
			if (_addNewSchedulePeriodMenuItem != null)
				_addNewSchedulePeriodMenuItem.Dispose();

			if (_deleteSchedulePeriodMenuItem != null)
				_deleteSchedulePeriodMenuItem.Dispose();

			if (_pasteSpecialSchedulePeriodMenuItem != null)
				_pasteSpecialSchedulePeriodMenuItem.Dispose();

			if (_copySpecialSchedulePeriodMenuItem != null)
				_copySpecialSchedulePeriodMenuItem.Dispose();
			_selectedSchedulePeriodCollection = null;
			_shiftCategoryLimitationView = null;
			base.Dispose(disposing);
		}
	}
}