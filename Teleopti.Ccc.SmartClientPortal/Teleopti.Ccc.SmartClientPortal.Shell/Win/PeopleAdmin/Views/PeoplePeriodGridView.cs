using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public class PeoplePeriodGridView : GridViewBase
	{
		private const int defaultGridInCellColumnIndex = 2;
		private const int defaultParentChildGridMappingValue = 2;
		private const int defaultParentGridLastColumnIndex = 0;
		private const int defaultPushButtonColumnIndex = 1;
		private const int defaultTotalHiddenColumnsInParentGrid = 1;
		private const int defaultRowHeightValue = 20;
		private const int defaultRenderingAddValue = 2;
		private const int defaultColumnWidthAddValueInGrids = 10;
		private const int defaultRowIndex = 1;

		private Rectangle cellButtonRect;

		private ToolStripMenuItem _addNewPersonPeriodMenuItem;
		private ToolStripMenuItem _copySpecialPersonPeriodMenuItem;
		private ToolStripMenuItem _deletePersonPeriodMenuItem;
		private ToolStripMenuItem _pasteSpecialPersonPeriodMenuItem;

		private IList<IPersonPeriodModel> _currentSelectedPersonPeriods;
		private readonly IList<IPersonPeriod> _selectedPersonPeriodCollection = new List<IPersonPeriod>();

		private readonly List<ColumnBase<PersonPeriodModel>> _gridColumns
			= new List<ColumnBase<PersonPeriodModel>>();

		private ColumnBase<PersonPeriodModel> _contractColumn;
		private ColumnBase<PersonPeriodModel> _contractScheduleColumn;
		private ColumnBase<PersonPeriodModel> _externalLogOnColumn;
		private ColumnBase<PersonPeriodModel> _fullNameColumn;
		private ColumnBase<PersonPeriodModel> _gridInCellColumn;
		private ColumnBase<PersonPeriodModel> _noteColumn;
		private ColumnBase<PersonPeriodModel> _partTimePercentageColumn;
		private ColumnBase<PersonPeriodModel> _periodDateColumn;
		private ColumnBase<PersonPeriodModel> _pushButtonColumn;
		private ColumnBase<PersonPeriodModel> _ruleSetBagColumn;
		private ColumnBase<PersonPeriodModel> _budgetGroupColumn;
		private ColumnBase<PersonPeriodModel> _skillColumn;
		private ColumnBase<PersonPeriodModel> _teamColumn;

		private readonly IList<ColumnBase<PersonPeriodChildModel>> _childGridColumns
			= new List<ColumnBase<PersonPeriodChildModel>>();

		private ColumnBase<PersonPeriodChildModel> _childGridContractColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridContractScheduleColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridExternalLogOnColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridNoteColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridPartTimePercentageColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridPeriodDateColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridRuleSetBagColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridBudgetGroupColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridSkillColumn;
		private ColumnBase<PersonPeriodChildModel> _childGridTeamColumn;
		private ColumnBase<PersonPeriodChildModel> _childLineColumn;
		private ColumnBase<PersonPeriodChildModel> _childRowHeaderColumn;

		public PeoplePeriodGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder) :
			base(grid, filteredPeopleHolder)
		{
			initCellModels(grid);
			Grid.HorizontalScroll += gridHorizontalScroll;
			_selectedPersonPeriodCollection.Clear();
		}

		protected override void Dispose(bool disposing)
		{
			if (Grid != null)
				Grid.HorizontalScroll -= gridHorizontalScroll;
			_currentSelectedPersonPeriods = null;
			if (_addNewPersonPeriodMenuItem != null)
				_addNewPersonPeriodMenuItem.Click -= AddNewGridRow;
			_addNewPersonPeriodMenuItem = null;
			if (_deletePersonPeriodMenuItem != null)
				_deletePersonPeriodMenuItem.Click -= DeleteSelectedGridRows;
			_deletePersonPeriodMenuItem = null;
			if (_copySpecialPersonPeriodMenuItem != null)
				_copySpecialPersonPeriodMenuItem.Click -= CopySpecial;
			_copySpecialPersonPeriodMenuItem = null;
			if (_pasteSpecialPersonPeriodMenuItem != null)
				_pasteSpecialPersonPeriodMenuItem.Click -= PasteSpecial;
			_pasteSpecialPersonPeriodMenuItem = null;

			if (GridCreator != null)
			{
				GridCreator.DropDownGridQueryCellInfo -= ChildGridQueryCellInfo;
				GridCreator.DropDownGridQueryRowCount -= ChildGridQueryRowCount;
				GridCreator.DropDownGridQueryColCount -= ChildGridQueryColCount;
				GridCreator.DropDownGridQueryRowHeight -= ChildGridQueryRowHeight;
				GridCreator.DropDownGridQueryColWidth -= ChildGridQueryColWidth;
				GridCreator.DropDownGridQuerySaveCellInfo -= ChildGridQuerySaveCellInfo;
				GridCreator.DropDownGridSelectionChanged -= ChildGridSelectionChanged;
				GridCreator.DropDownGridClipboardCanCopy -= ChildGridClipboardCanCopy;
				GridCreator.DropDownGridClipboardPaste -= ChildGridClipboardPaste;
				GridCreator.Dispose();
				GridCreator = null;
			}

			base.Dispose(disposing);
		}

		internal override ViewType Type => ViewType.PeoplePeriodView;

		public int ParentGridLastColumnIndex => _gridColumns.Count - defaultTotalHiddenColumnsInParentGrid;

		private int currentRowIndex => Grid.CurrentCell.RowIndex - PeopleAdminConstants.GridCurrentRowIndexMapValue;

		private PersonPeriodModel currentPersonPeriodView => FilteredPeopleHolder.PersonPeriodGridViewCollection[currentRowIndex];

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
				_gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<PersonPeriodModel>
															 (FilteredPeopleHolder.PersonPeriodGridViewCollection));
		}

		public override void RefreshParentGrid()
		{
			// To overcome rendering issues in message broker
			Grid.CurrentCell?.MoveTo(1, _gridColumns.IndexOf(_contractColumn) + 2);
		}

		internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<PersonPeriodModel>
															(FilteredPeopleHolder.PersonPeriodGridViewCollection));
			}

			base.QueryCellInfo(e);
		}

		internal override void CellButtonClicked(GridCellButtonClickedEventArgs e)
		{
			if (e.ColIndex == defaultPushButtonColumnIndex)
			{
				// Load child grid when push button clicked.
				LoadChildGrid(e.RowIndex);
			}
		}

		internal override void DrawCellButton(GridDrawCellButtonEventArgs e)
		{
			if (e.Style.CellType == GridCellModelConstants.CellTypePushButton)
			{
				cellButtonRect = new Rectangle(e.Button.Bounds.X, e.Button.Bounds.Y, PeopleAdminConstants.RectangleWidth,
					Grid.DefaultRowHeight);
				e.Button.Bounds = cellButtonRect;
			}
		}

		public static int DefaultRowHeight => defaultRowHeightValue;

		internal override void SelectedDateChange(object sender, EventArgs e)
		{
			// Colleps all expanded rows
			for (var rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				var gridInfo = GridRangeInfo.Cells(rowIndex + PeopleAdminConstants.GridCollectionMapValue,
					defaultGridInCellColumnIndex, rowIndex + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex + PeopleAdminConstants.GridCollectionMapValue, defaultGridInCellColumnIndex].
						Control as GridControl;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].GridControl = null;
					removeChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex + PeopleAdminConstants.GridCollectionMapValue] = DefaultRowHeight;
				}
			}

			// ReCall Person periods
			FilteredPeopleHolder.GetParentPersonPeriods();
			updateGrids();
			Grid.Invalidate();
		}

		public override void RefreshChildGrids()
		{
			// Updating child adapters
			var adaptersWithChildren = FilteredPeopleHolder.
				PersonPeriodGridViewCollection.Where(s => s.GridControl != null);

			foreach (var adapter in adaptersWithChildren)
			{
				// This is for overcome rendering issue with message broker
				adapter.GridControl.CurrentCell.MoveTo(1, _childGridColumns.IndexOf(_childGridContractColumn) + 2);
			}
		}

		internal override void SelectionChanged(GridSelectionChangedEventArgs e, bool eventCancel)
		{
			updateGrids();
		}

		private void updateGrids()
		{
			var rangeLength = Grid.Model.SelectedRanges.Count;
			if (rangeLength == 0)
			{
				return;
			}

			IList<IPersonPeriod> list = new List<IPersonPeriod>();
			IList<IPersonPeriodModel> gridDataList = new List<IPersonPeriodModel>();
			for (var rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
			{
				var rangeInfo = Grid.Model.SelectedRanges[rangeIndex];

				var top = 1; // This is to skip if Range is Empty.
				var length = 0;

				// TODO: Need to refactor this /kosalanp.

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
				}

				// Fixes for the Bug 4948
				if (rangeInfo.IsTable)
				{

					for (var i = 0; i < FilteredPeopleHolder.PersonPeriodGridViewCollection.Count; i++)
					{
						// Parent value set
						if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[i].ExpandState)
						{
							list.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection[i].Period);
							gridDataList.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection[i]);
						}
						// Child Value Set
						else
						{
							addChildPersonPeriodList(list, gridDataList, i);
						}
					}
				}
				else if ((rangeInfo.RangeType == GridRangeInfoType.Rows) &&
					(FilteredPeopleHolder.PersonPeriodGridViewCollection[rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue]
					.ExpandState))
				{
					addChildPersonPeriodList(list, gridDataList, rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue);
				}
				else if (top > 0)
					for (var index = top - 1; index < length; index++)
					{
						list.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection[index].Period);
						gridDataList.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection[index]);
					}
			}

			_currentSelectedPersonPeriods = gridDataList;
			CurrentGrid = Grid;

			FilteredPeopleHolder.SetPersonSkillGridViewDataByPersons(new ReadOnlyCollection<IPersonPeriod>
																		 (list),
																	 new ReadOnlyCollection
																		 <IPersonPeriodModel>(gridDataList));
			FilteredPeopleHolder.SetPersonExternalLogOnByPersonPeriod(new ReadOnlyCollection<IPersonPeriod>
																		  (list),
																	  new ReadOnlyCollection
																		  <IPersonPeriodModel>(gridDataList));
		}

		private void addChildPersonPeriodList(IList<IPersonPeriod> list, IList<IPersonPeriodModel> gridDataList, int index)
		{
			var grid = FilteredPeopleHolder.PersonPeriodGridViewCollection[index].GridControl;

			var personPeriodChildCollection = grid?.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection == null || personPeriodChildCollection.Count <= 0) return;
			foreach (var model in personPeriodChildCollection)
			{
				list.Add(model.Period);
				gridDataList.Add(model);
			}
		}

		public bool CanCopyChildRow { get; set; }

		public bool CanCopyRow { get; set; }

		internal override void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				var rowIndex = Grid.CurrentCell.RowIndex;

				_selectedPersonPeriodCollection.Clear();
				CanCopyRow = false;
				CanCopyChildRow = false;

				//TODO: Need to be checked Parent and child copy all functionality.
				if ((rowIndex > PeopleAdminConstants.GridHeaderIndex) && (!FilteredPeopleHolder.PersonPeriodGridViewCollection
					[rowIndex - PeopleAdminConstants.GridCollectionMapValue].ExpandState))
				{
					CanCopyRow = true;
					var selectedPerson = FilteredPeopleHolder.PersonPeriodGridViewCollection
						[rowIndex - PeopleAdminConstants.GridCollectionMapValue].Parent;

					foreach (var period in selectedPerson.PersonPeriodCollection)
					{
						// add perticular period into the selected person period collection.
						addPersonPeriodToSelectedPersonPeriodCollection(period);
					}
				}
			}
		}

		void ParentColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;

		}

		void parentColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonPeriodModel> e)
		{
			e.DataItem.CanBold = true;
			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _gridColumns.Count, Grid);
		}

		void teamColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.SiteTeamAdapterCollection.OrderBy(a => a.Team.SiteAndTeam).ToList());
		}

		void ContractColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, PeopleWorksheet.StateHolder.ContractCollection);
		}

		void ContractScheduleColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, PeopleWorksheet.StateHolder.ContractScheduleCollection);
		}

		void PartTimePercentageColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, PeopleWorksheet.StateHolder.PartTimePercentageCollection);
		}

		void ruleSetBagColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.RuleSetBagCollection);
		}

		public virtual int ParentChildGridMappingValue => defaultParentChildGridMappingValue;

		internal void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			// This is solution for child resizing and navaigtion rendering issues.Add value 2 to match parent and 
			// Child columns
			PeopleAdminHelper.CreateColWidthForChildGrid(e, ValidColumn(e.Index), ParentGridLastColumnIndex,
														 defaultRenderingAddValue,
														 Grid.ColWidths[e.Index + ParentChildGridMappingValue]);
		}

		internal void ChildGridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
		{
			PeopleAdminHelper.CreateRowHeightForChildGrid(e);
		}

		internal void ChildGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			PeopleAdminHelper.CreateColumnCountForChildGrid(e, _childGridColumns.Count -
																PeopleAdminConstants.GridColumnCountMappingValue);
		}

		internal void ChildGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			var personPeriodChildCollection = ((GridControl)sender).Tag as ReadOnlyCollection<PersonPeriodChildModel>;
			PeopleAdminHelper.CreateRowCountForChildGrid(e, personPeriodChildCollection);
		}

		internal void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			var gridControl = ((GridControl)sender);

			var personPeriodChildCollection = gridControl.Tag as ReadOnlyCollection<PersonPeriodChildModel>;
			PeopleWorksheet.StateHolder.CurrentChildName = gridControl.Text;

			if (personPeriodChildCollection != null)
			{
				if (ValidCell(e.ColIndex, e.RowIndex, gridControl.RowCount))
				{
					_childGridColumns[e.ColIndex].GetCellInfo(e, personPeriodChildCollection);
				}
			}

			if (e.Style.HasPasswordChar) return;
			e.Style.CellTipText = e.Style.FormattedText;
		}

		internal void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			var grid = (GridControl)sender;
			var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{
				if (ValidCell(e.ColIndex, e.RowIndex, grid.RowCount) && _childGridColumns.Count > e.ColIndex)
				{
					_childGridColumns[e.ColIndex].SaveCellInfo(e, personPeriodChildCollection);
				}
			}
		}

		internal void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			_selectedPersonPeriodCollection.Clear();
			CanCopyRow = false;
			CanCopyChildRow = false;
			var gridModel = sender as GridModel;

			var grid = gridModel?.ActiveGridView;
			// child copy processings
			var personPeriodChildCollection = grid?.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{
				CanCopyChildRow = true;
				var gridRangeInfoList = grid.Model.SelectedRanges;

				handleChildGridCanCopy(personPeriodChildCollection, gridRangeInfoList);
			}
		}

		internal void ChildGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			var grid = sender as GridControl;

			var personPeriodChildCollection = grid?.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{
				var rangeLength = grid.Model.SelectedRanges.Count;

				if (rangeLength != 0)
				{
					IList<IPersonPeriod> list = new List<IPersonPeriod>();
					IList<IPersonPeriodModel> gridDataList = new List<IPersonPeriodModel>();

					for (var rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
					{
						var rangeInfo = grid.Model.SelectedRanges[rangeIndex];

						var top = 1; // This is to skip if Range is Empty.
						var length = 0;

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
						}

						#endregion

						if (top > 0)
							for (var index = top - 1; index < length; index++)
							{
								if (personPeriodChildCollection.Count >= length)
								{
									list.Add(personPeriodChildCollection[index].Period);
									gridDataList.Add(personPeriodChildCollection[index]);
								}
							}
					}

					CurrentGrid = grid;

					FilteredPeopleHolder.SetPersonSkillGridViewDataByPersons(new
						ReadOnlyCollection<IPersonPeriod>
						(list),
						new ReadOnlyCollection
							<IPersonPeriodModel>
							(gridDataList));

					FilteredPeopleHolder.SetPersonExternalLogOnByPersonPeriod(new
						ReadOnlyCollection<IPersonPeriod>
						(list),
						new ReadOnlyCollection
							<IPersonPeriodModel>
							(gridDataList));

					invalidateTabPages();
				}
			}
		}

		void ChildColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}

		void childColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonPeriodChildModel> e)
		{
			e.DataItem.CanBold = true;

			var grid =
				FilteredPeopleHolder.PersonPeriodGridViewCollection[Grid.CurrentCell.RowIndex -
				PeopleAdminConstants.GridCollectionMapValue].GridControl;

			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _childGridColumns.Count, grid);
		}

		static void childGridContractColumnCellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);

			setDataSource(e, PeopleWorksheet.StateHolder.ContractCollection);
		}

		void childGridTeamColumnCellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.SiteTeamAdapterCollection);
		}

		static void childGridContractScheduleColumnCellDisplayChanged(object sender,
		  ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, PeopleWorksheet.StateHolder.ContractScheduleCollection);
		}

		static void childGridPartTimePercentageColumnCellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, PeopleWorksheet.StateHolder.PartTimePercentageCollection);
		}

		void childGridRuleSetBagColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs
			<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.RuleSetBagCollection);
		}
		
		void gridHorizontalScroll(object sender, ScrollEventArgs e)
		{
			PrepareView();
		}

		private void initCellModels(GridControl grid)
		{
			Grid.CellModels.Add(GridCellModelConstants.CellTypeGridInCell, new GridInCellModel(Grid.Model));
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(grid.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
			if (!grid.CellModels.ContainsKey(GridCellModelConstants.CellTypeDropDownCellModel))
				grid.CellModels.Add(GridCellModelConstants.CellTypeDropDownCellModel, new DropDownCellModel(grid.Model));
		}

		private bool isValidRow()
		{
			return Grid.CurrentCell.RowIndex > PeopleAdminConstants.GridHeaderIndex;
		}

		private bool isCurrentRowExpanded()
		{
			return isValidRow() && currentPersonPeriodView.ExpandState;
		}

		private int getColumnIndex()
		{
			int columnIndex;
			if (isCurrentRowExpanded())
			{
				columnIndex = currentPersonPeriodView.GridControl.CurrentCell.ColIndex +
							  PeopleAdminConstants.GridCurrentCellColumnAddValue;
			}
			else
			{
				var parentColIndex = gridColumnIndex();
				columnIndex = parentColIndex == PeopleAdminConstants.GridParentColumnIndexCheckValue
								  ? PeopleAdminConstants.GridColumnIndexValue
								  : parentColIndex;
			}
			return columnIndex;
		}

		private int gridColumnIndex()
		{
			if (Grid.CurrentCell.ColIndex == -1)
				Grid.CurrentCell.MoveTo(0, 0);
			return Grid.CurrentCell.ColIndex;
		}

		private IEnumerable<Tuple<IPerson, int>> sortPeoplePeriodData(bool isAscending)
		{
			// Gets the filtered people grid data as a collection
			var personcollection = FilteredPeopleHolder.PersonPeriodGridViewCollection.ToList();

			var columnIndex = getColumnIndex();

			// Gets the sort column to sort
			var sortColumn = _gridColumns[columnIndex].BindingProperty;
			// Gets the coparer erquired to sort the data
			var comparer = _gridColumns[columnIndex].ColumnComparer;
			
			if (string.IsNullOrEmpty(sortColumn))
			{
				return Enumerable.Empty<Tuple<IPerson, int>>();
			}

			// Holds the results of the sorting process
			IList<PersonPeriodModel> result;
			if (comparer != null)
			{
				// Sorts the person collection in ascending order
				personcollection.Sort(comparer);
				if (!isAscending)
					personcollection.Reverse();

				result = personcollection;
			}
			else
			{
				// Gets the sorted people collection
				result = GridHelper.Sort(
					new Collection<PersonPeriodModel>(personcollection),
					sortColumn,
					isAscending
				);
			}
			
			return result.Select((t, i) => new Tuple<IPerson, int>(t.Parent, i));
		}

		private void createChildGridHeader()
		{
			// Grid must have a Header column
			_childRowHeaderColumn = new RowHeaderColumn<PersonPeriodChildModel>();
			_childGridColumns.Add(_childRowHeaderColumn);

			_childLineColumn = new LineColumn<PersonPeriodChildModel>("FullName");
			_childGridColumns.Add(_childLineColumn);

			_childGridPeriodDateColumn = new EditableDateOnlyColumnForPeriodGrids<PersonPeriodChildModel>
				("PeriodDate", Resources.Date);
			_childGridPeriodDateColumn.CellChanged += childColumnCellChanged;
			_childGridPeriodDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridPeriodDateColumn);

			_childGridTeamColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				SiteTeamModel>
				("SiteTeam", Resources.SiteTeam, FilteredPeopleHolder.SiteTeamAdapterCollection,
				 "Description", false);
			_childGridTeamColumn.CellChanged += childColumnCellChanged;
			_childGridTeamColumn.CellDisplayChanged += childGridTeamColumnCellDisplayChanged;
			_childGridColumns.Add(_childGridTeamColumn);

			_childGridSkillColumn = new ReadOnlySkillDescriptionColumn<PersonPeriodChildModel>
				("PersonSkills", Resources.PersonSkill);
			_childGridSkillColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridSkillColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridSkillColumn);

			_childGridExternalLogOnColumn = new ReadOnlyCollectionColumn<PersonPeriodChildModel>
				("ExternalLogOnNames", Resources.ExternalLogOn);
			_childGridExternalLogOnColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridExternalLogOnColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridExternalLogOnColumn);

			_childGridContractColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel, IContract>
				("Contract", Resources.Contract, PeopleWorksheet.StateHolder.ContractCollection,
				 "Description", typeof(Contract), false);
			_childGridContractColumn.CellDisplayChanged += childGridContractColumnCellDisplayChanged;
			_childGridContractColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridContractColumn);

			_childGridContractScheduleColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IContractSchedule>("ContractSchedule", Resources.ContractScheduleLower,
									PeopleWorksheet.StateHolder.ContractScheduleCollection, "Description",
														  typeof(ContractSchedule), false);
			_childGridContractScheduleColumn.CellDisplayChanged +=
									 childGridContractScheduleColumnCellDisplayChanged;
			_childGridContractScheduleColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridContractScheduleColumn);

			_childGridPartTimePercentageColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IPartTimePercentage>("PartTimePercentage", Resources.PartTimePercentageLower,
									 PeopleWorksheet.StateHolder.PartTimePercentageCollection, "Description",
									 typeof(PartTimePercentage), false);
			_childGridPartTimePercentageColumn.CellDisplayChanged += childGridPartTimePercentageColumnCellDisplayChanged;
			_childGridPartTimePercentageColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridPartTimePercentageColumn);

			_childGridRuleSetBagColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IRuleSetBag>("RuleSetBag", Resources.RuleSetBag,
							 FilteredPeopleHolder.RuleSetBagCollection, "Description", typeof(RuleSetBag));
			_childGridRuleSetBagColumn.CellDisplayChanged += childGridRuleSetBagColumnCellDisplayChanged;

			_childGridRuleSetBagColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridRuleSetBagColumn);

			_childGridBudgetGroupColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IBudgetGroup>("BudgetGroup", Resources.Budgets,
							 FilteredPeopleHolder.BudgetGroupCollection, "Name", typeof(BudgetGroup));
			_childGridBudgetGroupColumn.CellDisplayChanged += childGridBudgetGroupColumnCellDisplayChanged;

			_childGridBudgetGroupColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridBudgetGroupColumn);

			_childGridNoteColumn = new EditableTextColumnForPeriodGrids<PersonPeriodChildModel>
				("Note", PeopleAdminConstants.GridNoteColumnWidth, Resources.Note);
			_childGridNoteColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridNoteColumn.CellChanged += childColumnCellChanged;
			_childGridColumns.Add(_childGridNoteColumn);
		}

		private void childGridBudgetGroupColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.BudgetGroupCollection);
		}

		private void createParentGridHeaders()
		{
			_gridColumns.Add(new RowHeaderColumn<PersonPeriodModel>());

			_pushButtonColumn = new PushButtonColumn<PersonPeriodModel>(Resources.FullName,
																				  "PeriodCount");
			_gridColumns.Add(_pushButtonColumn);

			_gridInCellColumn = new GridInCellColumn<PersonPeriodModel>("GridControl");
			_gridColumns.Add(_gridInCellColumn);

			_fullNameColumn = new ReadOnlyTextColumn<PersonPeriodModel>("FullName",
																				  Resources.FullName);
			_fullNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_gridColumns.Add(_fullNameColumn);

			_periodDateColumn = new EditableDateOnlyColumnForPeriodGrids<PersonPeriodModel>("PeriodDate",
																									  Resources.Date);
			_periodDateColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_periodDateColumn.CellChanged += parentColumnCellChanged;
			_periodDateColumn.ColumnComparer = new PersonPeriodDateComparer();
			_gridColumns.Add(_periodDateColumn);

			_teamColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel,SiteTeamModel>("SiteTeam", Resources.SiteTeam, FilteredPeopleHolder.SiteTeamAdapterCollection, "Description", false)
			{
				ColumnComparer = new PersonPeriodTeamComparer()
			};
			_teamColumn.CellDisplayChanged += teamColumnCellDisplayChanged;
			_teamColumn.CellChanged += parentColumnCellChanged;
			
			_gridColumns.Add(_teamColumn);

			_skillColumn = new ReadOnlySkillDescriptionColumn<PersonPeriodModel>("PersonSkills", Resources.PersonSkill)
			{
				ColumnComparer = new PersonPeriodPersonSkillComparer()
			};
			_skillColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_skillColumn.CellChanged += parentColumnCellChanged;

			_gridColumns.Add(_skillColumn);

			_externalLogOnColumn = new ReadOnlyCollectionColumn<PersonPeriodModel>("ExternalLogOnNames", Resources.ExternalLogOn)
			{
				ColumnComparer = new PersonPeriodExternalLogOnNameComparer()
			};
			_externalLogOnColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_externalLogOnColumn.CellChanged += parentColumnCellChanged;
			_gridColumns.Add(_externalLogOnColumn);

			_contractColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IContract>("Contract", Resources.Contract, PeopleWorksheet.StateHolder.ContractCollection, "Description", typeof(Contract), false)
			{
				ColumnComparer = new PersonPeriodContractComparer()
			};
			_contractColumn.CellChanged += parentColumnCellChanged;
			_contractColumn.CellDisplayChanged += ContractColumn_CellDisplayChanged;
			_gridColumns.Add(_contractColumn);

			_contractScheduleColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IContractSchedule>("ContractSchedule", Resources.ContractScheduleLower, PeopleWorksheet.StateHolder.ContractScheduleCollection, "Description", typeof(ContractSchedule), false)
			{
				ColumnComparer = new PersonPeriodContractScheduleComparer()
			};
			_contractScheduleColumn.CellChanged += parentColumnCellChanged;
			_contractScheduleColumn.CellDisplayChanged += ContractScheduleColumn_CellDisplayChanged;
			_gridColumns.Add(_contractScheduleColumn);

			_partTimePercentageColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IPartTimePercentage>("PartTimePercentage", Resources.PartTimePercentageLower,  PeopleWorksheet.StateHolder.PartTimePercentageCollection, "Description", typeof(PartTimePercentage), false)
			{
				ColumnComparer = new PersonPeriodPartTimePercentageComparer()
			};
			_partTimePercentageColumn.CellDisplayChanged += PartTimePercentageColumn_CellDisplayChanged;
			_partTimePercentageColumn.CellChanged += parentColumnCellChanged;

			_gridColumns.Add(_partTimePercentageColumn);

			_ruleSetBagColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IRuleSetBag>("RuleSetBag", Resources.RuleSetBag, FilteredPeopleHolder.RuleSetBagCollection, "Description", typeof(RuleSetBag))
			{
				ColumnComparer = new PersonPeriodRuleSetBagComparer()
			};
			_ruleSetBagColumn.CellDisplayChanged += ruleSetBagColumnCellDisplayChanged;
			_ruleSetBagColumn.CellChanged += parentColumnCellChanged;
			_gridColumns.Add(_ruleSetBagColumn);

			_budgetGroupColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IBudgetGroup>("BudgetGroup", Resources.BudgetGroup, FilteredPeopleHolder.BudgetGroupCollection, "Name", typeof(BudgetGroup))
			{
				ColumnComparer = new PersonPeriodBudgetGroupComparer()
			};
			_budgetGroupColumn.CellDisplayChanged += budgetGroupColumnCellDisplayChanged;
			_budgetGroupColumn.CellChanged += parentColumnCellChanged;
			_gridColumns.Add(_budgetGroupColumn);

			_noteColumn = new EditableTextColumnForPeriodGrids<PersonPeriodModel>("Note", PeopleAdminConstants.GridNoteColumnWidth, Resources.Note);
			_noteColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_noteColumn.CellChanged += parentColumnCellChanged;

			_gridColumns.Add(_noteColumn);
		}

		private void budgetGroupColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			setBoldProperty(e);
			setDataSource(e, FilteredPeopleHolder.BudgetGroupCollection);
		}

		private void removeChildGrid(GridControlBase childGrid, GridRangeInfo gridRange)
		{
			// Remove covered range and remove grid form control collection
			Grid.CoveredRanges.Remove(gridRange);
			Grid.Controls.Remove(childGrid);
		}

		private void AddPersonPeriod()
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				insertPersonPeriod();
			}
		}

		private void addPersonPeriodToSelectedPersonPeriodCollection(IPersonPeriod personPeriod)
		{
			InParameter.NotNull("personPeriod", personPeriod);

			if (!_selectedPersonPeriodCollection.Contains(personPeriod))
			{
				_selectedPersonPeriodCollection.Add(personPeriod);
			}
		}

		private void insertPersonPeriod()
		{
			var gridRangeInfoList = Grid.Model.SelectedRanges;

			for (var index = 0; index < gridRangeInfoList.Count; index++)
			{
				var gridRangeInfo = gridRangeInfoList[index];
				if (gridRangeInfo.IsTable || gridRangeInfo.IsCols)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (var i = 1; i <= Grid.RowCount; i++)
					{
						AddPersonPeriod(i);
					}
				}
				else
				{
					if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
					{
						AddPersonPeriod(gridRangeInfo.Top);
					}
					else
					{
						for (var row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
						{
							AddPersonPeriod(row);
						}
					}
				}
			}
		}

		private void AddPersonPeriod(int rowIndex)
		{
			if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
			{
				// Add Person period and get child person periods
				PeopleWorksheet.StateHolder.AddPersonPeriod(rowIndex - PeopleAdminConstants.GridCollectionMapValue,
															FilteredPeopleHolder.PersonPeriodGridViewCollection,
															FilteredPeopleHolder);

				var grid =
						 FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue].
							 GridControl;

				getChildPersonPeriods(rowIndex, grid);


				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
					.ExpandState)
				{
					addPersonPeriodToChildGrid(rowIndex);
				}
				else
				{
					// Get Parent person period and invalidate range
					FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex - PeopleAdminConstants.GridCollectionMapValue);
					Grid.InvalidateRange(GridRangeInfo.Cells(rowIndex, PeopleAdminConstants.GridInvalidateRowLeftValue, rowIndex,
															 ParentGridLastColumnIndex));
				}

			}
		}

		private void addPersonPeriodToChildGrid(int rowIndex)
		{
			var childGrid = Grid[rowIndex, defaultGridInCellColumnIndex].Control as CellEmbeddedGrid;

			if (childGrid != null)
			{
				// Set child person period for tag and merging name column's all cells 
				childGrid.Tag = PeopleWorksheet.StateHolder.PersonPeriodChildGridDataWhenAddChild;
				childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(PeopleAdminConstants.GridCoveredRangeDefaultValue,
																	  PeopleAdminConstants.GridCoveredRangeDefaultValue,
																	  childGrid.RowCount,
																	  PeopleAdminConstants.GridCoveredRangeDefaultValue));
				Grid.RowHeights[rowIndex] += DefaultRowHeight;
				childGrid.Refresh();

				Grid.RefreshRange(GridRangeInfo.Cells(rowIndex, PeopleAdminConstants.GridInvalidateRowLeftValue, rowIndex,
													  ParentGridLastColumnIndex));
			}
		}

		public void SetOutOfFocusFromCurrentCell()
		{
			var index = Grid.Model.CurrentCellInfo?.RowIndex ?? defaultRowIndex;
			Grid.CurrentCell.MoveTo(index, defaultParentGridLastColumnIndex);
			Grid.Selections.Clear(false);
		}

		private void LoadChildGrid(int rowIndex)
		{
			if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
			{
				// Set grid range for covered ranged
				var gridInfo = GridRangeInfo.Cells(rowIndex, defaultGridInCellColumnIndex, rowIndex,
															 ParentGridLastColumnIndex);

				// Set out of focus form current cell.This helps  to fire save cell info in child grid.
				SetOutOfFocusFromCurrentCell();

				if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
					PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = true;

					var grid =
						FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].GridControl;

					getChildPersonPeriods(rowIndex, grid);

					LoadChildGrid(rowIndex, defaultGridInCellColumnIndex, gridInfo);

					Grid.CurrentCell.MoveTo(rowIndex, defaultGridInCellColumnIndex, GridSetCurrentCellOptions.SetFocus);
				}
				else
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = false;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex, defaultGridInCellColumnIndex].Control as GridControl;
					removeChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;
					FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex -
						PeopleAdminConstants.GridCollectionMapValue);
				}
				Grid.Invalidate();
			}
		}

		private void getChildPersonPeriods(int rowIndex, GridControlBase grid)
		{
			if (grid != null)
			{
				var cachedCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

				PeopleWorksheet.StateHolder.GetChildPersonPeriods(rowIndex - PeopleAdminConstants.GridCollectionMapValue,
				FilteredPeopleHolder, cachedCollection);
			}
			else
			{
				PeopleWorksheet.StateHolder.GetChildPersonPeriods(rowIndex - PeopleAdminConstants.GridCollectionMapValue,
																	  FilteredPeopleHolder);
			}
		}

		private void removeChild(int rowIndex, bool isDeleteAll)
		{
			var childGrid = Grid[rowIndex, defaultGridInCellColumnIndex].Control as CellEmbeddedGrid;
			if (childGrid != null)
			{
				if (isDeleteAll)
				{
					PersonPeriodChildDelete(rowIndex, childGrid);
				}
				else
				{
					var gridRangeInfoList = childGrid.Model.SelectedRanges.GetRowRanges(GridRangeInfoType.Cells | GridRangeInfoType.Rows);
					for (var index = gridRangeInfoList.Count; index > 0; index--)
					{
						var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];
						var top = gridRangeInfo.Top;
						var bottom = gridRangeInfo.Bottom;
						if (top == 0) continue;
						if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
						{
							PersonPeriodChildDelete(rowIndex, gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue,
													childGrid);
						}
						else
						{
							for (var row = bottom; row >= top; row--)
							{
								PersonPeriodChildDelete(rowIndex, row - PeopleAdminConstants.GridCollectionMapValue, childGrid);
							}
						}
					}
				}
			}
		}

		private void PersonPeriodChildDelete(int rowIndex, int childPersonPeriodIndex,
											 CellEmbeddedGrid childGrid)
		{
			var gridInfo = GridRangeInfo.Cells(rowIndex, PeopleAdminConstants.GridInvalidateRowLeftValue, rowIndex,
														 ParentGridLastColumnIndex);

			var personPeriodChildCollection = childGrid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{

				var personPeriod = personPeriodChildCollection[childPersonPeriodIndex].ContainedEntity;

				FilteredPeopleHolder.DeletePersonPeriod(rowIndex - PeopleAdminConstants.GridCollectionMapValue, personPeriod);





				if (childGrid.RowCount == 0)
					Grid.RowHeights[rowIndex] = DefaultRowHeight + defaultRenderingAddValue;


				var person = FilteredPeopleHolder.FilteredPersonCollection[rowIndex - 1];

				if (person.PersonPeriodCollection.Count == 0)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.ExpandState = false;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.GridControl = null;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex, defaultGridInCellColumnIndex].Control as GridControl;
					removeChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;

					FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex - PeopleAdminConstants.GridCollectionMapValue);
					Grid.InvalidateRange(gridInfo);
				}
				else
				{
					IList<PersonPeriodChildModel> periodCollection =
						new List<PersonPeriodChildModel>(personPeriodChildCollection);
					periodCollection.RemoveAt(childPersonPeriodIndex);

					childGrid.Tag = new ReadOnlyCollection<PersonPeriodChildModel>(periodCollection);
					childGrid.RowCount = periodCollection.Count;
					childGrid.Invalidate();

					Grid.RowHeights[rowIndex] = childGrid.RowCount * DefaultRowHeight + defaultRenderingAddValue;

				}
			}
		}

		private void PersonPeriodChildDelete(int rowIndex, CellEmbeddedGrid childGrid)
		{
			//Delete all person period by index
			FilteredPeopleHolder.DeletePersonPeriod(rowIndex - PeopleAdminConstants.GridCollectionMapValue);

			// Get person period child colleciton and clear those because all person periods were
			// deleted.
			var personPeriodChildCollection = childGrid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;
			if (personPeriodChildCollection != null)
			{
				IList<PersonPeriodChildModel> periodCollection =
					new List<PersonPeriodChildModel>(personPeriodChildCollection);
				periodCollection.Clear();

				var gridInfo = GridRangeInfo.Cells(rowIndex, PeopleAdminConstants.GridInvalidateRowLeftValue, rowIndex,
																			ParentGridLastColumnIndex);

				// Set period collection to child grid tag, update row count and invalidate child grid.
				childGrid.Tag = new ReadOnlyCollection<PersonPeriodChildModel>(periodCollection);
				childGrid.RowCount = periodCollection.Count;
				childGrid.Invalidate();

				// Set expand state and remove child grid
				FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue].
					ExpandState = false;
				FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue].
					GridControl = null;

				// Dispose child grid and set default row height
				var gridControl = Grid[rowIndex, defaultGridInCellColumnIndex].Control as GridControl;
				removeChildGrid(gridControl, gridInfo);
				Grid.RowHeights[rowIndex] = DefaultRowHeight;

				// Get person period when updated and invalidate range
				FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex - PeopleAdminConstants.GridCollectionMapValue);
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void copyAllPersonPeriods(int rowIndex)
		{
			if (rowIndex >= PeopleAdminConstants.GridHeaderIndex)
			{

				// Get perticular person for paste
				var personPaste = FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].Parent;
				var isParent = !FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState;

				if (personPaste != null)
				{
					if (CanCopyRow)
					{
						foreach (var personPeriod in _selectedPersonPeriodCollection)
						{
							if (!personPaste.PersonPeriodCollection.Contains(personPeriod))
							{
								validateAndAddPersonPeriod(personPaste, personPeriod);
							}
						}
					}

					if (CanCopyChildRow)
					{
						foreach (var period in _selectedPersonPeriodCollection)
						{
							validateAndAddPersonPeriod(personPaste, period);
						}
					}

					if (isParent)
						refreshParentGridRange(rowIndex);
					else
						refreshChildGrid(rowIndex);
				}
			}
		}

		private static void validateAndAddPersonPeriod(IPerson personPaste, IPersonPeriod personPeriod)
		{
			var period = personPeriod.NoneEntityClone();

			if (period == null) return;

			period.StartDate = PeriodDateService.GetValidPeriodDate(
					PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.PeoplePeriodView, personPaste),
					period.StartDate);

			// Add person periods to person 
			personPaste.AddPersonPeriod(period);
		}

		private void refreshParentGridRange(int index)
		{
			FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(index);
			Grid.InvalidateRange(GridRangeInfo.Cells(index + PeopleAdminConstants.GridCollectionMapValue,
				PeopleAdminConstants.GridInvalidateRowLeftValue, index + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex));
		}

		private void refreshChildGrid(int index)
		{
			PeopleWorksheet.StateHolder.GetChildPersonPeriods(index, FilteredPeopleHolder);

			var childGrid = Grid[index + PeopleAdminConstants.GridCollectionMapValue, defaultGridInCellColumnIndex].Control
				as CellEmbeddedGrid;

			if (childGrid != null)
			{
				childGrid.Tag = PeopleWorksheet.StateHolder.PersonPeriodChildGridData;

				// Merging name column's all cells
				childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(PeopleAdminConstants.GridCoveredRangeDefaultValue,
																	  PeopleAdminConstants.GridCoveredRangeDefaultValue,
																	  childGrid.RowCount,
																	  PeopleAdminConstants.GridCoveredRangeDefaultValue));

				Grid.RowHeights[index + PeopleAdminConstants.GridCollectionMapValue] =
					PeopleWorksheet.StateHolder.PersonPeriodChildGridData.Count * DefaultRowHeight +
					defaultRenderingAddValue;

				childGrid.Refresh();
			}
		}

		private void deleteWhenRangeSelected(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
			{
				deletePersonPeriod(gridRangeInfo.Top, gridRangeInfo.IsRows);
			}
			else
			{
				for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
				{
					if (row != PeopleAdminConstants.GridHeaderIndex)
					{
						deletePersonPeriod(row, gridRangeInfo.IsRows);
					}
				}
			}
		}

		private void deletePersonPeriod(int index, bool isRows)
		{
			if (index == 0)
				return;
			if (FilteredPeopleHolder.PersonPeriodGridViewCollection[index -
																	PeopleAdminConstants.GridCurrentRowIndexMapValue].ExpandState)
			{
				// Remove child for child grid
				removeChild(index, isRows);
			}
			else
			{
				var currentPersonPeriod = FilteredPeopleHolder.PersonPeriodGridViewCollection[index -
																	PeopleAdminConstants.GridCurrentRowIndexMapValue].Period;

				FilteredPeopleHolder.DeletePersonPeriod(index - PeopleAdminConstants.GridCurrentRowIndexMapValue,
														currentPersonPeriod);

				FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(index -
																	  PeopleAdminConstants.GridCurrentRowIndexMapValue);

				Grid.InvalidateRange(GridRangeInfo.Cells(index, PeopleAdminConstants.GridInvalidateRowLeftValue,
														 index, ParentGridLastColumnIndex));

			}
		}

		private void deleteWhenAllSelected()
		{
			// This scenario is used for when user is selecting entire grid using button give top in 
			// that grid.
			for (var i = 1; i <= Grid.RowCount; i++)
			{
				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i - PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					removeChild(i, true);
				}
				else
				{
					FilteredPeopleHolder.DeletePersonPeriod(i - PeopleAdminConstants.GridCollectionMapValue,
															FilteredPeopleHolder.SelectedDate);
					FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(i - PeopleAdminConstants.GridCollectionMapValue);
					Grid.InvalidateRange(GridRangeInfo.Cells(i, PeopleAdminConstants.GridInvalidateRowLeftValue, i,
															 ParentGridLastColumnIndex));
				}
			}
		}

		public DropDownGridCreator GridCreator { get; private set; }

		internal virtual void ChildGridClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
		}

		public virtual void BindDropDownGridEvents()
		{
			// Create Drop Down grid creator for child grid creation
			GridCreator = new DropDownGridCreator();

			GridCreator.DropDownGridQueryCellInfo += ChildGridQueryCellInfo;
			GridCreator.DropDownGridQueryRowCount += ChildGridQueryRowCount;
			GridCreator.DropDownGridQueryColCount += ChildGridQueryColCount;
			GridCreator.DropDownGridQueryRowHeight += ChildGridQueryRowHeight;
			GridCreator.DropDownGridQueryColWidth += ChildGridQueryColWidth;
			GridCreator.DropDownGridQuerySaveCellInfo += ChildGridQuerySaveCellInfo;
			GridCreator.DropDownGridSelectionChanged += ChildGridSelectionChanged;
			GridCreator.DropDownGridClipboardCanCopy += ChildGridClipboardCanCopy;
			GridCreator.DropDownGridClipboardPaste += ChildGridClipboardPaste;
		}

		private void LoadChildGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
		{
			BindDropDownGridEvents();

			GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
								PeopleWorksheet.StateHolder.PersonPeriodChildGridData,
								_childGridColumns.Count - PeopleAdminConstants.GridColumnCountMappingValue,
								PeopleWorksheet.StateHolder.CurrentChildName);
		}

		private void invalidateTabPages()
		{
			foreach (TabPageAdv tab in FilteredPeopleHolder.TabControlPeopleAdmin.TabPages)
			{
				if (tab.Controls.Count > PeopleAdminConstants.ControlStartIndexValue && tab.Controls[PeopleAdminConstants.ControlStartIndexValue].GetType()
																	== typeof(GridControl))
					((GridControl)tab.Controls[PeopleAdminConstants.ControlStartIndexValue]).Invalidate();
			}
		}

		private void handleChildGridCanCopy(ReadOnlyCollection<PersonPeriodChildModel>
			personPeriodChildCollection, GridRangeInfoList gridRangeInfoList)
		{
			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

				if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
				{
					var personPeriod = personPeriodChildCollection[gridRangeInfo.Top -
						PeopleAdminConstants.GridCollectionMapValue].ContainedEntity;

					addPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
				}
				else
				{
					for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						var personPeriod = personPeriodChildCollection[row - PeopleAdminConstants.GridCollectionMapValue].
							ContainedEntity;

						addPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
					}
				}
			}
		}

		private static void setBoldProperty<T>(ColumnCellDisplayChangedEventArgs<T> e) where T : IPersonPeriodModel
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}

		private static void setDataSource<T>(ColumnCellDisplayChangedEventArgs<T> e, object datasource)
			where T : IPersonPeriodModel
		{

			e.QueryCellInfoEventArg.Style.ClearCache();
			e.QueryCellInfoEventArg.Style.DataSource = null;
			e.QueryCellInfoEventArg.Style.DataSource = datasource;
			e.QueryCellInfoEventArg.Style.FormattedText = string.Empty;
		}

		public override void PerformSort(IEnumerable<Tuple<IPerson, int>> order)
		{
			if (!(order?.Any() ?? false)) return;

			Grid.CurrentCell.MoveLeft();

			// Dispose the child grids
			DisposeChildGrids();

			var result = (from x in FilteredPeopleHolder.PersonPeriodGridViewCollection
						  join y in order
					on x.Parent equals y.Item1
					into a
				from b in a.DefaultIfEmpty(new Tuple<IPerson, int>(null, int.MaxValue))
				orderby b.Item2
				select x).ToList();

			// Sets the filtered list
			FilteredPeopleHolder.SetSortedPersonPeriodFilteredList(result);

			Grid.CurrentCell.MoveRight();

			// Refresh the grid view to get affect the sorted data
			Invalidate();
		}

		public override IEnumerable<Tuple<IPerson, int>> Sort(bool isAscending)
		{
			// Sorts the people period data and invalidate view
			var result = sortPeoplePeriodData(isAscending);
			return result;
		}

		internal override void CreateHeaders()
		{
			createParentGridHeaders();
			createChildGridHeader();

			// Hide column which is used as a container for grid in cell implementation 
			var pushButtonCol = _gridColumns.IndexOf(_pushButtonColumn);
			Grid.Cols.Hidden[pushButtonCol + PeopleAdminConstants.GridCollectionMapValue] = true;
		}

		internal override void PrepareView()
		{
			ColCount = _gridColumns.Count;
			RowCount = FilteredPeopleHolder.PersonPeriodGridViewCollection.Count;

			Grid.RowCount = RowCount;
			Grid.ColCount = ColCount - PeopleAdminConstants.GridCollectionMapValue;
			Grid.Model.Data.RowCount = RowCount;

			Grid.Cols.HeaderCount = PeopleAdminConstants.GridHeaderCountDefaultValue;
			Grid.Rows.HeaderCount = PeopleAdminConstants.GridHeaderCountDefaultValue;
			Grid.Name = "PeoplePeriodView";

			var length = _gridColumns.Count;
			for (var index = 0; index < length; index++)
			{
				Grid.ColWidths[index] = _gridColumns[index].PreferredWidth + defaultColumnWidthAddValueInGrids;
			}
			Grid.ColWidths[0] = _gridColumns[0].PreferredWidth;
		}

		internal override void MergeHeaders()
		{
			Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(PeopleAdminConstants.GridMergeHeaderTopValue,
															 PeopleAdminConstants.GridMergeHeaderLeftValue,
															 PeopleAdminConstants.GridMergeHeaderBottomValue,
															 PeopleAdminConstants.GridMergeHeaderRightValue));
		}

		public override void Invalidate()
		{
			Grid.Invalidate();
		}

		internal override void CreateContextMenu()
		{
			Grid.ContextMenuStrip = new ContextMenuStrip();

			_addNewPersonPeriodMenuItem = new ToolStripMenuItem(Resources.New);
			_addNewPersonPeriodMenuItem.Click += AddNewGridRow;
			Grid.ContextMenuStrip.Items.Add(_addNewPersonPeriodMenuItem);

			_deletePersonPeriodMenuItem = new ToolStripMenuItem(Resources.Delete);
			_deletePersonPeriodMenuItem.Click += DeleteSelectedGridRows;
			Grid.ContextMenuStrip.Items.Add(_deletePersonPeriodMenuItem);

			_copySpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.CopySpecial);
			_copySpecialPersonPeriodMenuItem.Click += CopySpecial;
			Grid.ContextMenuStrip.Items.Add(_copySpecialPersonPeriodMenuItem);

			_pasteSpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.PasteNew);
			_pasteSpecialPersonPeriodMenuItem.Click += PasteSpecial;
			Grid.ContextMenuStrip.Items.Add(_pasteSpecialPersonPeriodMenuItem);
		}

		internal override void AddNewGridRow<T>(object sender, T eventArgs)
		{
			AddPersonPeriod();
		}

		internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
		{
			if (Grid.Model.SelectedRanges.Count > PeopleAdminConstants.GridSelectedRangesCountBoundary)
			{
				var gridRangeInfoList = Grid.Model.SelectedRanges;
				for (var index = gridRangeInfoList.Count; index > 0; index--)
				{
					var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

					if (gridRangeInfo.IsTable)
					{
						deleteWhenAllSelected();
					}
					else
					{
						deleteWhenRangeSelected(gridRangeInfo);
					}
				}
			}
		}

		internal void PasteSpecial<T>(object sender, T eventArgs)
		{
			var gridRangeInfoList = Grid.Model.Selections.Ranges;

			if (CanCopyRow || CanCopyChildRow)
			{
				for (var index = gridRangeInfoList.Count; index > 0; index--)
				{
					var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

					if (gridRangeInfo.IsTable)
					{
						// This scenario is used for when user is selecting entire grid using button give top in 
						// that grid.
						for (var i = 1; i <= Grid.RowCount; i++)
						{
							copyAllPersonPeriods(i - PeopleAdminConstants.GridCollectionMapValue);
						}
					}

					else
					{
						if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
						{
							copyAllPersonPeriods(gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue);
						}
						else
						{
							for (var row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
							{
								copyAllPersonPeriods(row - PeopleAdminConstants.GridCollectionMapValue);
							}
						}
					}
				}
			}
		}

		public override void Reinitialize()
		{
			Grid.Refresh();
		}

		internal void CopySpecial<T>(object sender, T eventArgs)
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				var rowIndex = Grid.CurrentCell.RowIndex;

				if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
				{
					_selectedPersonPeriodCollection.Clear();
					CanCopyRow = false;
					CanCopyChildRow = false;

					if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.ExpandState)
					{
						CanCopyRow = true;
						var selectedPerson = FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
							PeopleAdminConstants.GridCollectionMapValue].Parent;

						foreach (PersonPeriod period in selectedPerson.PersonPeriodCollection)
						{
							addPersonPeriodToSelectedPersonPeriodCollection(period);
						}
					}
					else
					{
						CanCopyChildRow = true;

						// child copy processings
						var grid = Grid[Grid.CurrentCell.RowIndex, defaultGridInCellColumnIndex].Control
							as GridControl;

						copyPersonPeriodFromChildGrid(grid);
					}

				}
			}
		}

		private void copyPersonPeriodFromChildGrid(GridControl grid)
		{
			var personPeriodChildCollection = grid?.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{
				var gridRangeInfoList = grid.Model.SelectedRanges;

				for (var index = gridRangeInfoList.Count; index > 0; index--)
				{
					var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

					if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
					{
						var personPeriod = personPeriodChildCollection
							[gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue].ContainedEntity;

						addPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
					}
					else
					{
						for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
						{
							var personPeriod = personPeriodChildCollection[row -
																		   PeopleAdminConstants.GridCollectionMapValue].ContainedEntity;

							addPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
						}
					}
				}
			}
		}

		internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
		{
			// Content base copy paste for grouping grid
			PasteSpecial(sender, eventArgs);
		}

		internal override IList<IPerson> GetSelectedPersons()
		{
			IList<IPerson> selectedPersons = new List<IPerson>();

			if (_currentSelectedPersonPeriods != null && _currentSelectedPersonPeriods.Count != 0 && _currentSelectedPersonPeriods[0] != null)
			{
				foreach (var model in _currentSelectedPersonPeriods)
				{
					if (model.Parent != null)
					{
						selectedPersons.Add(model.Parent);
					}
				}
			}
			else
			{
				selectedPersons = getSelectedPersonsInGrd();
			}

			return selectedPersons;
		}

		private IList<IPerson> getSelectedPersonsInGrd()
		{
			IList<IPerson> selectedPersons = new List<IPerson>();

			var gridRangeInfoList = Grid.Model.SelectedRanges;

			if (FilteredPeopleHolder.PersonPeriodGridViewCollection.Count != 0)
			{
				for (var index = gridRangeInfoList.Count; index > 0; index--)
				{
					var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

					if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
					{
						if (gridRangeInfo.Top > 0)
							selectedPersons.Add(
								FilteredPeopleHolder.PersonPeriodGridViewCollection[
									gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue].Parent);
					}
					else
					{
						for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
						{
							selectedPersons.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection
								[row - PeopleAdminConstants.GridCurrentRowIndexMapValue].Parent);
						}
					}
				}
			}
			return selectedPersons;
		}

		internal override void SetSelectedPersons(IList<IPerson> selectedPersons)
		{
			// Selection events will not be raised		
			Grid.Model.Selections.Clear(false);

			var range = GridRangeInfo.Empty;
			foreach (var person in selectedPersons)
			{
				for (var i = 0; i < FilteredPeopleHolder.PersonPeriodGridViewCollection.Count; i++)
				{
					if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i].Parent.Id == person.Id)
					{
						range = range.UnionRange(GridRangeInfo.Row(i + PeopleAdminConstants.GridCollectionMapValue));
					}
				}
			}
			Grid.Selections.Add(range);
		}

		internal override void DisposeChildGrids()
		{
			if (ParentGridLastColumnIndex < 0) return;

			for (var rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				var gridInfo =
					GridRangeInfo.Cells(rowIndex + PeopleAdminConstants.GridCollectionMapValue, defaultGridInCellColumnIndex,
					rowIndex + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection.Count == rowIndex) break;

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex + PeopleAdminConstants.GridCollectionMapValue, defaultGridInCellColumnIndex]
						.Control as GridControl;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].GridControl = null;
					removeChildGrid(gridControl, gridInfo);
					Grid.RowHeights[rowIndex + PeopleAdminConstants.GridCollectionMapValue] = DefaultRowHeight;
				}
			}
		}
	}
}
