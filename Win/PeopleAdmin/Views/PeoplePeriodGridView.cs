﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public class PeoplePeriodGridView : DropDownGridViewBase
	{
		private Rectangle cellButtonRect;

		public GridConstructor TestGrid { get; set; }

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

		internal override ViewType Type
		{
			get { return ViewType.PeoplePeriodView; }
		}

		public override int ParentGridLastColumnIndex
		{
			get
			{
				return _gridColumns.Count - TotalHiddenColumnsInParentGrid;
			}
		}

		private int CurrentRowIndex
		{
			get { return Grid.CurrentCell.RowIndex - PeopleAdminConstants.GridCurrentRowIndexMapValue; }
		}

		private PersonPeriodModel CurrentPersonPeriodView
		{
			get { return FilteredPeopleHolder.PersonPeriodGridViewCollection[CurrentRowIndex]; }
		}

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
				_gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<PersonPeriodModel>
															 (FilteredPeopleHolder.PersonPeriodGridViewCollection));
		}

		public override void RefreshParentGrid()
		{
			// To overcome rendering issues in message broker
			if (Grid.CurrentCell != null) Grid.CurrentCell.MoveTo(1, _gridColumns.IndexOf(_contractColumn) + 2);
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
			if (e.ColIndex == PushButtonColumnIndex)
			{
				// Load child grid when push button clicked.
				LoadChildGrid(e.RowIndex);
			}
		}

		internal override void CellClick(object sender, GridCellClickEventArgs e)
		{
			var grid = sender as GridControl;

			if ((e.RowIndex == PeopleAdminConstants.GridHeaderIndex) && (e.ColIndex > PeopleAdminConstants.GridHeaderIndex))
			{
				// Remove all selections in given column
				Grid.Selections.Remove(GridRangeInfo.Col(e.ColIndex));



				if (grid != null)
				{
					for (int i = 0; i < grid.RowCount; i++)
					{
						if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i].ExpandState == false)
						{
							// Add non expanded rows to selections range.
							Grid.Selections.Ranges.Add(GridRangeInfo.Cells(i + PeopleAdminConstants.GridCollectionMapValue,
																		   e.ColIndex, i + PeopleAdminConstants.GridCollectionMapValue
																		   , e.ColIndex));
						}
					}
				}
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

		internal override void SelectedDateChange(object sender, EventArgs e)
		{
			// Colleps all expanded rows
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex + PeopleAdminConstants.GridCollectionMapValue,
					GridInCellColumnIndex, rowIndex + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					GridControl gridControl = Grid[rowIndex + PeopleAdminConstants.GridCollectionMapValue, GridInCellColumnIndex].
						Control as GridControl;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].GridControl = null;
					RemoveChildGrid(gridControl, gridInfo);

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

			foreach (PersonPeriodModel adapter in adaptersWithChildren)
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

					default:
						break;
				}

				// Fixes for the Bug 4948
				if (rangeInfo.IsTable)
				{

					for (int i = 0; i < FilteredPeopleHolder.PersonPeriodGridViewCollection.Count; i++)
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
							AddChildPersonPeriodList(list, gridDataList, i);
						}
					}
				}
				else if ((rangeInfo.RangeType == GridRangeInfoType.Rows) &&
					(FilteredPeopleHolder.PersonPeriodGridViewCollection[rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue]
					.ExpandState))
				{
					AddChildPersonPeriodList(list, gridDataList, rangeInfo.Top - PeopleAdminConstants.GridCollectionMapValue);
				}
				else if (top > 0)
					for (int index = top - 1; index < length; index++)
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

		private void AddChildPersonPeriodList(IList<IPersonPeriod> list, IList<IPersonPeriodModel> gridDataList, int index)
		{
			var grid = FilteredPeopleHolder.PersonPeriodGridViewCollection[index].GridControl;

			if (grid != null)
			{
				var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

				if (personPeriodChildCollection != null && personPeriodChildCollection.Count > 0)
				{
					for (int i = 0; i < personPeriodChildCollection.Count; i++)
					{
						list.Add(personPeriodChildCollection[i].Period);
						gridDataList.Add(personPeriodChildCollection[i]);
					}
				}
			}
		}

		internal override void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				int rowIndex = Grid.CurrentCell.RowIndex;

				_selectedPersonPeriodCollection.Clear();
				CanCopyRow = false;
				CanCopyChildRow = false;

				//TODO: Need to be checked Parent and child copy all functionality.
				if ((rowIndex > PeopleAdminConstants.GridHeaderIndex) && (!FilteredPeopleHolder.PersonPeriodGridViewCollection
					[rowIndex - PeopleAdminConstants.GridCollectionMapValue].ExpandState))
				{
					CanCopyRow = true;
					IPerson selectedPerson = FilteredPeopleHolder.PersonPeriodGridViewCollection
						[rowIndex - PeopleAdminConstants.GridCollectionMapValue].Parent;

					foreach (IPersonPeriod period in selectedPerson.PersonPeriodCollection)
					{
						// add perticular period into the selected person period collection.
						AddPersonPeriodToSelectedPersonPeriodCollection(period);
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

		void ParentColumn_CellChanged(object sender, ColumnCellChangedEventArgs<PersonPeriodModel> e)
		{
			e.DataItem.CanBold = true;
			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _gridColumns.Count, Grid);
		}

		void TeamColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.SiteTeamAdapterCollection.OrderBy(a=>a.Team.SiteAndTeam).ToList());
		}

		void ContractColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, PeopleWorksheet.StateHolder.ContractCollection);
		}

		void ContractScheduleColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, PeopleWorksheet.StateHolder.ContractScheduleCollection);
		}

		void PartTimePercentageColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, PeopleWorksheet.StateHolder.PartTimePercentageCollection);
		}

		void RuleSetBagColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.RuleSetBagCollection);
		}

		internal override void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			// This is solution for child resizing and navaigtion rendering issues.Add value 2 to match parent and 
			// Child columns
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
			var personPeriodChildCollection = ((GridControl)sender).Tag as ReadOnlyCollection<PersonPeriodChildModel>;
			PeopleAdminHelper.CreateRowCountForChildGrid(e, personPeriodChildCollection);
		}

		internal override void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
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
			base.ChildGridQueryCellInfo(sender, e);
		}

		internal override void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
		    var grid =(GridControl) sender;
			var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

			if (personPeriodChildCollection != null)
			{
				if (ValidCell(e.ColIndex, e.RowIndex, grid.RowCount) && _childGridColumns.Count > e.ColIndex)
				{
					_childGridColumns[e.ColIndex].SaveCellInfo(e, personPeriodChildCollection);
				}
			}
		}

		internal override void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			_selectedPersonPeriodCollection.Clear();
			CanCopyRow = false;
			CanCopyChildRow = false;
			var gridModel = sender as GridModel;

			if (gridModel != null)
			{
				var grid = gridModel.ActiveGridView;
				if (grid != null)
				{
					// child copy processings
					var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

					if (personPeriodChildCollection != null)
					{
						CanCopyChildRow = true;
						GridRangeInfoList gridRangeInfoList = grid.Model.SelectedRanges;

						HandleChildGridCanCopy(personPeriodChildCollection, gridRangeInfoList);
					}
				}
			}
		}

		internal override void ChildGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			var grid = sender as GridControl;

			if (grid != null)
			{
				var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

				if (personPeriodChildCollection != null)
				{
					int rangeLength = grid.Model.SelectedRanges.Count;

					if (rangeLength != 0)
					{
						IList<IPersonPeriod> list = new List<IPersonPeriod>();
						IList<IPersonPeriodModel> gridDataList = new List<IPersonPeriodModel>();

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

						InvalidateTabPages();
					}
				}
			}
		}

		void ChildColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}

		void ChildColumn_CellChanged(object sender, ColumnCellChangedEventArgs<PersonPeriodChildModel> e)
		{
			e.DataItem.CanBold = true;

			GridControl grid =
				FilteredPeopleHolder.PersonPeriodGridViewCollection[Grid.CurrentCell.RowIndex -
				PeopleAdminConstants.GridCollectionMapValue].GridControl;

			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _childGridColumns.Count, grid);
		}

		static void ChildGridContractColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);

			SetDataSource(e, PeopleWorksheet.StateHolder.ContractCollection);
			//Grid.ResetVolatileData();
		}

		void ChildGridTeamColumn_CellDisplayChanged(object sender,
			ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.SiteTeamAdapterCollection);
		}

		static void ChildGridContractScheduleColumn_CellDisplayChanged(object sender,
		  ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, PeopleWorksheet.StateHolder.ContractScheduleCollection);
		}

		static void ChildGridPartTimePercentageColumn_CellDisplayChanged(object sender,
		   ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, PeopleWorksheet.StateHolder.PartTimePercentageCollection);
		}

		void ChildGridRuleSetBagColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs
			<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.RuleSetBagCollection);
		}

		public PeoplePeriodGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder) :
			base(grid, filteredPeopleHolder)
		{
			Init();
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(grid.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);

			Grid.HorizontalScroll += Grid_HorizontalScroll;
		}

		void Grid_HorizontalScroll(object sender, ScrollEventArgs e)
		{
			PrepareView();
		}

		private void Init()
		{
			Grid.CellModels.Add(GridCellModelConstants.CellTypeGridInCell, new GridInCellModel(Grid.Model));
		}

		private bool IsValidRow()
		{
			return Grid.CurrentCell.RowIndex > PeopleAdminConstants.GridHeaderIndex;
		}

		private bool IsCurrentRowExpanded()
		{
			return IsValidRow() && CurrentPersonPeriodView.ExpandState;
		}

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
				int parentColIndex = Grid.CurrentCell.ColIndex;
				columnIndex = (parentColIndex == PeopleAdminConstants.GridParentColumnIndexCheckValue)
								  ? (PeopleAdminConstants.GridColumnIndexValue)
								  : parentColIndex;
			}
			return columnIndex;
		}

		private void SortPeoplePeriodData(bool isAscending)
		{
			// Gets the filtered people grid data as a collection
			var personcollection = new List<PersonPeriodModel>(
				FilteredPeopleHolder.PersonPeriodGridViewCollection);

			var columnIndex = GetColumnIndex();

			// Gets the sort column to sort
			var sortColumn = _gridColumns[columnIndex].BindingProperty;
			// Gets the coparer erquired to sort the data
			IComparer<PersonPeriodModel> comparer = _gridColumns[columnIndex].ColumnComparer;

			Grid.CurrentCell.MoveLeft();

			// Dispose the child grids
			DisposeChildGrids();

			if (!string.IsNullOrEmpty(sortColumn))
			{
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

				// Sets the filtered list
				FilteredPeopleHolder.SetSortedPersonPeriodFilteredList(result);

				Grid.CurrentCell.MoveRight();
			}
		}

		private void CreateChildGridHeader()
		{
			// Grid must have a Header column
			_childRowHeaderColumn = new RowHeaderColumn<PersonPeriodChildModel>();
			_childGridColumns.Add(_childRowHeaderColumn);

			_childLineColumn = new LineColumn<PersonPeriodChildModel>("FullName");
			_childGridColumns.Add(_childLineColumn);

			_childGridPeriodDateColumn = new EditableDateOnlyColumnForPeriodGrids<PersonPeriodChildModel>
				("PeriodDate", Resources.Date);
			_childGridPeriodDateColumn.CellChanged += ChildColumn_CellChanged;
			_childGridPeriodDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridPeriodDateColumn);

			_childGridTeamColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				SiteTeamModel>
				("SiteTeam", Resources.SiteTeam, FilteredPeopleHolder.SiteTeamAdapterCollection,
				 "Description");
			_childGridTeamColumn.CellChanged += ChildColumn_CellChanged;
			_childGridTeamColumn.CellDisplayChanged += ChildGridTeamColumn_CellDisplayChanged;
			_childGridColumns.Add(_childGridTeamColumn);

			_childGridSkillColumn = new ReadOnlySkillDescriptionColumn<PersonPeriodChildModel>
				("PersonSkills", Resources.PersonSkill);
			_childGridSkillColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridSkillColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridSkillColumn);

			_childGridExternalLogOnColumn = new ReadOnlyCollectionColumn<PersonPeriodChildModel>
				("ExternalLogOnNames", Resources.ExternalLogOn);
			_childGridExternalLogOnColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridExternalLogOnColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridExternalLogOnColumn);

			_childGridContractColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel, IContract>
				("Contract", Resources.Contract, PeopleWorksheet.StateHolder.ContractCollection,
				 "Description", typeof(Contract));
			_childGridContractColumn.CellDisplayChanged += ChildGridContractColumn_CellDisplayChanged;
			_childGridContractColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridContractColumn);

			_childGridContractScheduleColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IContractSchedule>("ContractSchedule", Resources.ContractScheduleLower,
								   PeopleWorksheet.StateHolder.ContractScheduleCollection, "Description",
														  typeof(ContractSchedule));
			_childGridContractScheduleColumn.CellDisplayChanged +=
									 ChildGridContractScheduleColumn_CellDisplayChanged;
			_childGridContractScheduleColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridContractScheduleColumn);

			_childGridPartTimePercentageColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IPartTimePercentage>("PartTimePercentage", Resources.PartTimePercentageLower,
									 PeopleWorksheet.StateHolder.PartTimePercentageCollection, "Description",
									 typeof(PartTimePercentage));
			_childGridPartTimePercentageColumn.CellDisplayChanged += ChildGridPartTimePercentageColumn_CellDisplayChanged;
			_childGridPartTimePercentageColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridPartTimePercentageColumn);

			_childGridRuleSetBagColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IRuleSetBag>("RuleSetBag", Resources.RuleSetBag,
							 FilteredPeopleHolder.RuleSetBagCollection, "Description", typeof(RuleSetBag));
			_childGridRuleSetBagColumn.CellDisplayChanged += ChildGridRuleSetBagColumn_CellDisplayChanged;

			_childGridRuleSetBagColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridRuleSetBagColumn);

			_childGridBudgetGroupColumn = new DropDownColumnForPeriodGrids<PersonPeriodChildModel,
				IBudgetGroup>("BudgetGroup", Resources.Budgets,
							 FilteredPeopleHolder.BudgetGroupCollection, "Name", typeof(BudgetGroup));
			_childGridBudgetGroupColumn.CellDisplayChanged += ChildGridBudgetGroupColumn_CellDisplayChanged;

			_childGridBudgetGroupColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridBudgetGroupColumn);

			_childGridNoteColumn = new EditableTextColumnForPeriodGrids<PersonPeriodChildModel>
			   ("Note", PeopleAdminConstants.GridNoteColumnWidth, Resources.Note);
			_childGridNoteColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
			_childGridNoteColumn.CellChanged += ChildColumn_CellChanged;
			_childGridColumns.Add(_childGridNoteColumn);
		}

		private void ChildGridBudgetGroupColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodChildModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.BudgetGroupCollection);
		}

		private void CreateParentGridHeaders()
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
			_periodDateColumn.CellChanged += ParentColumn_CellChanged;
			_periodDateColumn.ColumnComparer = new PersonPeriodDateComparer();
			_gridColumns.Add(_periodDateColumn);

			_teamColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel,
				SiteTeamModel>("SiteTeam", Resources.SiteTeam, FilteredPeopleHolder.SiteTeamAdapterCollection,
																								  "Description");
			_teamColumn.CellDisplayChanged += TeamColumn_CellDisplayChanged;
			_teamColumn.CellChanged += ParentColumn_CellChanged;
			_teamColumn.ColumnComparer = new PersonPeriodTeamComparer();
			_gridColumns.Add(_teamColumn);

			_skillColumn = new ReadOnlySkillDescriptionColumn<PersonPeriodModel>("PersonSkills",
																						Resources.PersonSkill
																						   );
			_skillColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_skillColumn.CellChanged += ParentColumn_CellChanged;
			_skillColumn.ColumnComparer = new PersonPeriodPersonSkillComparer();
			_gridColumns.Add(_skillColumn);

			_externalLogOnColumn = new ReadOnlyCollectionColumn<PersonPeriodModel>
				("ExternalLogOnNames", Resources.ExternalLogOn);
			_externalLogOnColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_externalLogOnColumn.CellChanged += ParentColumn_CellChanged;
			_externalLogOnColumn.ColumnComparer = new PersonPeriodExternalLogOnNameComparer();
			_gridColumns.Add(_externalLogOnColumn);

			_contractColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IContract>("Contract",
																									   Resources.
																										   Contract,
																									   PeopleWorksheet.
																										   StateHolder.
																										   ContractCollection,
																									   "Description",
																									   typeof(Contract));
			_contractColumn.ColumnComparer = new PersonPeriodContractComparer();
			_contractColumn.CellChanged += ParentColumn_CellChanged;
			_contractColumn.CellDisplayChanged += ContractColumn_CellDisplayChanged;
			_gridColumns.Add(_contractColumn);

			_contractScheduleColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel,
				IContractSchedule>("ContractSchedule", Resources.ContractScheduleLower,
								   PeopleWorksheet.StateHolder.ContractScheduleCollection, "Description",
								   typeof(ContractSchedule));
			_contractScheduleColumn.ColumnComparer = new PersonPeriodContractScheduleComparer();
			_contractScheduleColumn.CellChanged += ParentColumn_CellChanged;
			_contractScheduleColumn.CellDisplayChanged += ContractScheduleColumn_CellDisplayChanged;
			_gridColumns.Add(_contractScheduleColumn);

			_partTimePercentageColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel,
				IPartTimePercentage>("PartTimePercentage", Resources.PartTimePercentageLower,
									 PeopleWorksheet.StateHolder.PartTimePercentageCollection, "Description",
									 typeof(PartTimePercentage));
			_partTimePercentageColumn.CellDisplayChanged += PartTimePercentageColumn_CellDisplayChanged;
			_partTimePercentageColumn.CellChanged += ParentColumn_CellChanged;
			_partTimePercentageColumn.ColumnComparer = new PersonPeriodPartTimePercentageComparer();
			_gridColumns.Add(_partTimePercentageColumn);

			_ruleSetBagColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IRuleSetBag>
				("RuleSetBag", Resources.RuleSetBag, FilteredPeopleHolder.RuleSetBagCollection,
				 "Description", typeof(RuleSetBag));
			_ruleSetBagColumn.ColumnComparer = new PersonPeriodRuleSetBagComparer();
			_ruleSetBagColumn.CellDisplayChanged += RuleSetBagColumn_CellDisplayChanged;
			_ruleSetBagColumn.CellChanged += ParentColumn_CellChanged;
			_gridColumns.Add(_ruleSetBagColumn);

			_budgetGroupColumn = new DropDownColumnForPeriodGrids<PersonPeriodModel, IBudgetGroup>
				("BudgetGroup", Resources.BudgetGroup, FilteredPeopleHolder.BudgetGroupCollection,
				 "Name", typeof(BudgetGroup));
			_budgetGroupColumn.ColumnComparer = new PersonPeriodBudgetGroupComparer();
			_budgetGroupColumn.CellDisplayChanged += BudgetGroupColumn_CellDisplayChanged;
			_budgetGroupColumn.CellChanged += ParentColumn_CellChanged;
			_gridColumns.Add(_budgetGroupColumn);

			_noteColumn = new EditableTextColumnForPeriodGrids<PersonPeriodModel>("Note",
																 PeopleAdminConstants.GridNoteColumnWidth,
																						  Resources.Note);
			_noteColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_noteColumn.CellChanged += ParentColumn_CellChanged;

			_gridColumns.Add(_noteColumn);
		}

		private void BudgetGroupColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonPeriodModel> e)
		{
			SetBoldProperty(e);
			SetDataSource(e, FilteredPeopleHolder.BudgetGroupCollection);
		}

		private void RemoveChildGrid(GridControlBase childGrid, GridRangeInfo gridRange)
		{
			// Remove covered range and remove grid form control collection
			Grid.CoveredRanges.Remove(gridRange);
			Grid.Controls.Remove(childGrid);
		}

		private void AddPersonPeriod()
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				InsertPersonPeriod();
			}
		}

		private void AddPersonPeriodToSelectedPersonPeriodCollection(IPersonPeriod personPeriod)
		{
			InParameter.NotNull("personPeriod", personPeriod);

			if (!_selectedPersonPeriodCollection.Contains(personPeriod))
			{
				_selectedPersonPeriodCollection.Add(personPeriod);
			}
		}

		private void InsertPersonPeriod()
		{
			var gridRangeInfoList = Grid.Model.SelectedRanges;

			for (int index = 0; index < gridRangeInfoList.Count; index++)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index];
				if (gridRangeInfo.IsTable)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 1; i <= Grid.RowCount; i++)
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
						for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
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

				GridControl grid =
						 FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue].
							 GridControl;

				GetChildPersonPeriods(rowIndex, grid);


				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
					.ExpandState)
				{
					AddPersonPeriodToChildGrid(rowIndex);
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

		private void AddPersonPeriodToChildGrid(int rowIndex)
		{
			var childGrid = Grid[rowIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;

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

		private void LoadChildGrid(int rowIndex)
		{
			if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
			{
				// Set grid range for covered ranged
				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex, GridInCellColumnIndex, rowIndex,
															 ParentGridLastColumnIndex);

				// Set out of focus form current cell.This helps  to fire save cell info in child grid.
				SetOutOfFocusFromCurrentCell();

				if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
					PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = true;

					GridControl grid =
						FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].GridControl;

					GetChildPersonPeriods(rowIndex, grid);

					LoadChildGrid(rowIndex, GridInCellColumnIndex, gridInfo);

					Grid.CurrentCell.MoveTo(rowIndex, GridInCellColumnIndex, GridSetCurrentCellOptions.SetFocus);
				}
				else
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
						PeopleAdminConstants.GridCollectionMapValue].ExpandState = false;

					// Get child grid and dispose it
					GridControl gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
					RemoveChildGrid(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;
					FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex -
						PeopleAdminConstants.GridCollectionMapValue);
				}
				Grid.Invalidate();
			}
		}

		private void GetChildPersonPeriods(int rowIndex, GridControlBase grid)
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

		private void RemoveChild(int rowIndex, bool isDeleteAll)
		{
			var childGrid = Grid[rowIndex, GridInCellColumnIndex].Control as CellEmbeddedGrid;
			if (childGrid != null)
			{
				if (isDeleteAll)
				{
					PersonPeriodChildDelete(rowIndex, childGrid);
				}
				else
				{
					GridRangeInfoList gridRangeInfoList = childGrid.Model.SelectedRanges.GetRowRanges(GridRangeInfoType.Cells | GridRangeInfoType.Rows);
					for (int index = gridRangeInfoList.Count; index > 0; index--)
					{
						GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];
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
							for (int row = bottom; row >= top; row--)
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

				IPersonPeriod personPeriod = personPeriodChildCollection[childPersonPeriodIndex].ContainedEntity;

				FilteredPeopleHolder.DeletePersonPeriod(rowIndex - PeopleAdminConstants.GridCollectionMapValue, personPeriod);





				if (childGrid.RowCount == 0)
					Grid.RowHeights[rowIndex] = DefaultRowHeight + RenderingAddValue;


				IPerson person = FilteredPeopleHolder.FilteredPersonCollection[rowIndex - 1];

				if (person.PersonPeriodCollection.Count == 0)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.ExpandState = false;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.GridControl = null;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
					RemoveChildGrid(gridControl, gridInfo);

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

					Grid.RowHeights[rowIndex] = childGrid.RowCount * DefaultRowHeight + RenderingAddValue;

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

				GridRangeInfo gridInfo = GridRangeInfo.Cells(rowIndex, PeopleAdminConstants.GridInvalidateRowLeftValue, rowIndex,
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
				var gridControl = Grid[rowIndex, GridInCellColumnIndex].Control as GridControl;
				RemoveChildGrid(gridControl, gridInfo);
				Grid.RowHeights[rowIndex] = DefaultRowHeight;

				// Get person period when updated and invalidate range
				FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(rowIndex - PeopleAdminConstants.GridCollectionMapValue);
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void CopyAllPersonPeriods(int rowIndex)
		{
			if (rowIndex >= PeopleAdminConstants.GridHeaderIndex)
			{

				// Get perticular person for paste
				IPerson personPaste = FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].Parent;
				bool isParent = !FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState;

				if (personPaste != null)
				{
					if (CanCopyRow)
					{
						foreach (IPersonPeriod personPeriod in _selectedPersonPeriodCollection)
						{
							if (!personPaste.PersonPeriodCollection.Contains(personPeriod))
							{
								ValidateAndAddPersonPeriod(personPaste, personPeriod);
							}
						}
					}

					if (CanCopyChildRow)
					{
						foreach (IPersonPeriod period in _selectedPersonPeriodCollection)
						{
							ValidateAndAddPersonPeriod(personPaste, period);
						}
					}

					if (isParent)
						RefreshParentGridRange(rowIndex);
					else
						RefreshChildGrid(rowIndex);
				}
			}
		}
		 
		private static void ValidateAndAddPersonPeriod(IPerson personPaste, IPersonPeriod personPeriod)
		{
			IPersonPeriod period = personPeriod.NoneEntityClone();

			if (period == null) return;

			period.StartDate = PeriodDateService.GetValidPeriodDate(
					PeriodDateDictionaryBuilder.GetDateOnlyDictionary(null, ViewType.PeoplePeriodView, personPaste),
					period.StartDate);

			// Add person periods to person 
			personPaste.AddPersonPeriod(period);
		} 

		private void RefreshParentGridRange(int index)
		{
			FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(index);
			Grid.InvalidateRange(GridRangeInfo.Cells(index + PeopleAdminConstants.GridCollectionMapValue,
				PeopleAdminConstants.GridInvalidateRowLeftValue, index + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex));
		}
		 
		private void RefreshChildGrid(int index)
		{
			PeopleWorksheet.StateHolder.GetChildPersonPeriods(index, FilteredPeopleHolder);

			var childGrid = Grid[index + PeopleAdminConstants.GridCollectionMapValue, GridInCellColumnIndex].Control
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
					RenderingAddValue;

				childGrid.Refresh();
			}
		}
		 
		private void DeleteWhenRangeSelected(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
			{
				DeletePersonPeriod(gridRangeInfo.Top, gridRangeInfo.IsRows);
			}
			else
			{
				for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
				{
					if (row != PeopleAdminConstants.GridHeaderIndex)
					{
						DeletePersonPeriod(row, gridRangeInfo.IsRows);
					}
				}
			}
		}
		 
		private void DeletePersonPeriod(int index, bool isRows)
		{
			if (FilteredPeopleHolder.PersonPeriodGridViewCollection[index -
																	PeopleAdminConstants.GridCurrentRowIndexMapValue].ExpandState)
			{
				// Remove child for child grid
				RemoveChild(index, isRows);
			}
			else
			{
				IPersonPeriod currentPersonPeriod = FilteredPeopleHolder.PersonPeriodGridViewCollection[index -
																   PeopleAdminConstants.GridCurrentRowIndexMapValue].Period;

				FilteredPeopleHolder.DeletePersonPeriod(index - PeopleAdminConstants.GridCurrentRowIndexMapValue,
														currentPersonPeriod);

				FilteredPeopleHolder.GetParentPersonPeriodWhenUpdated(index -
																	  PeopleAdminConstants.GridCurrentRowIndexMapValue);

				Grid.InvalidateRange(GridRangeInfo.Cells(index, PeopleAdminConstants.GridInvalidateRowLeftValue,
														 index, ParentGridLastColumnIndex));

			}
		}
		 
		private void DeleteWhenAllSelected()
		{
			// This scenario is used for when user is selecting entire grid using button give top in 
			// that grid.
			for (int i = 1; i <= Grid.RowCount; i++)
			{
				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i - PeopleAdminConstants.GridCollectionMapValue].ExpandState)
				{
					RemoveChild(i, true);
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
		 
		private void LoadChildGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
		{
			BindDropDownGridEvents();

			GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
								PeopleWorksheet.StateHolder.PersonPeriodChildGridData,
								_childGridColumns.Count - PeopleAdminConstants.GridColumnCountMappingValue,
								PeopleWorksheet.StateHolder.CurrentChildName);
		}
		 
		private void InvalidateTabPages()
		{
			foreach (TabPageAdv tab in FilteredPeopleHolder.TabControlPeopleAdmin.TabPages)
			{
				if (tab.Controls.Count > PeopleAdminConstants.ControlStartIndexValue && tab.Controls[PeopleAdminConstants.ControlStartIndexValue].GetType()
																	== typeof(GridControl))
					((GridControl)tab.Controls[PeopleAdminConstants.ControlStartIndexValue]).Invalidate();
			}
		}
		 
		private void HandleChildGridCanCopy(ReadOnlyCollection<PersonPeriodChildModel>
			personPeriodChildCollection, GridRangeInfoList gridRangeInfoList)
		{
			for (int index = gridRangeInfoList.Count; index > 0; index--)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

				if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
				{
					IPersonPeriod personPeriod = personPeriodChildCollection[gridRangeInfo.Top -
						PeopleAdminConstants.GridCollectionMapValue].ContainedEntity;

					AddPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
				}
				else
				{
					for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						IPersonPeriod personPeriod = personPeriodChildCollection[row - PeopleAdminConstants.GridCollectionMapValue].
							ContainedEntity;

						AddPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
					}
				}
			}
		}
		 
		private static void SetBoldProperty<T>(ColumnCellDisplayChangedEventArgs<T> e) where T : IPersonPeriodModel
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}
		 
		private static void SetDataSource<T>(ColumnCellDisplayChangedEventArgs<T> e, object datasource)
			where T : IPersonPeriodModel
		{

			e.QueryCellInfoEventArg.Style.ClearCache();
			e.QueryCellInfoEventArg.Style.DataSource = null;
			e.QueryCellInfoEventArg.Style.DataSource = datasource;
			e.QueryCellInfoEventArg.Style.FormattedText = string.Empty;
		}
		 
		public override void Sort(bool isAscending)
		{
			// Sorts the people period data and invalidate view
			SortPeoplePeriodData(isAscending);
			Invalidate();
		}
		 
		internal override void CreateHeaders()
		{
			CreateParentGridHeaders();
			CreateChildGridHeader();

			// Hide column which is used as a container for grid in cell implementation 
			int pushButtonCol = _gridColumns.IndexOf(_pushButtonColumn);
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

			int length = _gridColumns.Count;
			for (int index = 0; index < length; index++)
			{
				Grid.ColWidths[index] = _gridColumns[index].PreferredWidth + DefaultColumnWidthAddValue;
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
				GridRangeInfoList gridRangeInfoList = Grid.Model.SelectedRanges;
				for (int index = gridRangeInfoList.Count; index > 0; index--)
				{
					GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

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

		internal override void PasteSpecial<T>(object sender, T eventArgs)
		{
			GridRangeInfoList gridRangeInfoList = Grid.Model.Selections.Ranges;

			if (CanCopyRow || CanCopyChildRow)
			{
				for (int index = gridRangeInfoList.Count; index > 0; index--)
				{
					GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

					if (gridRangeInfo.IsTable)
					{
						// This scenario is used for when user is selecting entire grid using button give top in 
						// that grid.
						for (int i = 1; i <= Grid.RowCount; i++)
						{
							CopyAllPersonPeriods(i - PeopleAdminConstants.GridCollectionMapValue);
						}
					}

					else
					{
						if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
						{
							CopyAllPersonPeriods(gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue);
						}
						else
						{
							for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
							{
								CopyAllPersonPeriods(row - PeopleAdminConstants.GridCollectionMapValue);
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

		internal override void CopySpecial<T>(object sender, T eventArgs)
		{
			if (Grid.Model.CurrentCellInfo != null)
			{
				int rowIndex = Grid.CurrentCell.RowIndex;

				if (rowIndex != PeopleAdminConstants.GridHeaderIndex)
				{
					_selectedPersonPeriodCollection.Clear();
					CanCopyRow = false;
					CanCopyChildRow = false;

					if (!FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex - PeopleAdminConstants.GridCollectionMapValue]
						.ExpandState)
					{
						CanCopyRow = true;
						IPerson selectedPerson = FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex -
							PeopleAdminConstants.GridCollectionMapValue].Parent;

						foreach (PersonPeriod period in selectedPerson.PersonPeriodCollection)
						{
							AddPersonPeriodToSelectedPersonPeriodCollection(period);
						}
					}
					else
					{
						CanCopyChildRow = true;

						// child copy processings
						GridControl grid = Grid[Grid.CurrentCell.RowIndex, GridInCellColumnIndex].Control
							as GridControl;

						CopyPersonPeriodFromChildGrid(grid);
					}

				}
			}
		}

		private void CopyPersonPeriodFromChildGrid(GridControl grid)
		{
			if (grid != null)
			{
				var personPeriodChildCollection = grid.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

				if (personPeriodChildCollection != null)
				{
					GridRangeInfoList gridRangeInfoList = grid.Model.SelectedRanges;

					for (int index = gridRangeInfoList.Count; index > 0; index--)
					{
						GridRangeInfo gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

						if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
						{
							IPersonPeriod personPeriod = personPeriodChildCollection
								[gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue].ContainedEntity;

							AddPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
						}
						else
						{
							for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
							{
								IPersonPeriod personPeriod = personPeriodChildCollection[row -
									PeopleAdminConstants.GridCollectionMapValue].ContainedEntity;

								AddPersonPeriodToSelectedPersonPeriodCollection(personPeriod);
							}
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

			if (_currentSelectedPersonPeriods != null && _currentSelectedPersonPeriods[0] != null)
			{
				for (int i = 0; i < _currentSelectedPersonPeriods.Count; i++)
				{
					if (_currentSelectedPersonPeriods[i].Parent != null)
					{
						selectedPersons.Add(_currentSelectedPersonPeriods[i].Parent);
					}
				}
			}
			else
			{
				selectedPersons = GetSelectedPersonsInGrd();
			}

			return selectedPersons;
		}

		private IList<IPerson> GetSelectedPersonsInGrd()
		{
			IList<IPerson> selectedPersons = new List<IPerson>();

			var gridRangeInfoList = Grid.Model.SelectedRanges;

			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - PeopleAdminConstants.GridCollectionMapValue];

				if (gridRangeInfo.Height == PeopleAdminConstants.GridRangeInFOHeightValue)
				{
					selectedPersons.Add(FilteredPeopleHolder.PersonPeriodGridViewCollection
						[gridRangeInfo.Top - PeopleAdminConstants.GridCurrentRowIndexMapValue].Parent);
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
			return selectedPersons;
		}

		internal override void SetSelectedPersons(IList<IPerson> selectedPersons)
		{
			// Selection events will not be raised
			Grid.Model.Selections.Clear(false);

			IList<GridRangeInfo> ranges = new List<GridRangeInfo>();
			foreach (var person in selectedPersons)
			{
				for (int i = 0; i < FilteredPeopleHolder.PersonPeriodGridViewCollection.Count; i++)
				{
					if (FilteredPeopleHolder.PersonPeriodGridViewCollection[i].Parent.Id == person.Id)
					{
						ranges.Add(GridRangeInfo.Row(i + PeopleAdminConstants.GridCollectionMapValue));

					}
				}
			}

            ranges.ForEach(Grid.Selections.Add);
		}

		internal override void DisposeChildGrids()
		{
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				GridRangeInfo gridInfo =
					GridRangeInfo.Cells(rowIndex + PeopleAdminConstants.GridCollectionMapValue, GridInCellColumnIndex,
					rowIndex + PeopleAdminConstants.GridCollectionMapValue, ParentGridLastColumnIndex);

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection.Count == rowIndex) break;

				if (FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState)
				{
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex + PeopleAdminConstants.GridCollectionMapValue, GridInCellColumnIndex]
						.Control as GridControl;
					FilteredPeopleHolder.PersonPeriodGridViewCollection[rowIndex].GridControl = null;
					RemoveChildGrid(gridControl, gridInfo);
					Grid.RowHeights[rowIndex + PeopleAdminConstants.GridCollectionMapValue] = DefaultRowHeight;
				}
			}
		}
	}
}
