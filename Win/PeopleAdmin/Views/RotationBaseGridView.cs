using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	public class
		RotationBaseGridView<TAdapterParent, TAdapterChild, TBaseType, TScheduleType> : DropDownGridViewBase
		where TAdapterParent : IRotationModel<TBaseType, TScheduleType>
		where TAdapterChild : IRotationModel<TBaseType, TScheduleType>
	{
		public RotationBaseGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder,
		                            IList<TAdapterParent> parentAdapterCollection, ViewType viewType,
		                            IToggleManager toggleManager)
			: base(grid, filteredPeopleHolder)
		{
			Init();
			// to show another MonthCalendar (MonthCalendarAdv)
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(grid.Model);
			cellModel.SetNoneButtonText("");
			cellModel.SetTodayButtonText("");
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
			_viewType = viewType;
			_toggleManager = toggleManager;
			_parentAdapterCollection = parentAdapterCollection;
		}

		// This is used to indicate grid in cell column index in parent grid
		private const int _gridInCellColumnIndex = 2;

		//To hold the index of the column which holds the rotation drop down box
		private const int _rotationColumIndex = 4;

		//To hold the index of the column which holds the index of the week dropdown box
		private const int _weekColumIndex = 6;

		//To hold the index of the column which holds the index of the week dropdown box in child grids
		private const int _weekColumnIndexChildGrid = 4;

		private IList<TAdapterParent> _parentAdapterCollection;

		//Parent grid columns.
		private readonly List<ColumnBase<TAdapterParent>> _parentGridColumns = new List<ColumnBase<TAdapterParent>>();

		private ColumnBase<TAdapterParent> _pushButtonColumn;
		private ColumnBase<TAdapterParent> _gridInCellColumn;
		private ColumnBase<TAdapterParent> _personNameColumn;
		private ColumnBase<TAdapterParent> _fromDateColumn;
		private ColumnBase<TAdapterParent> _startWeekColumn;
		private ColumnBase<TAdapterParent> _rotationNameColumn;

		//child grid columns.
		private readonly List<IColumn<TAdapterChild>> _childGridColumns = new List<IColumn<TAdapterChild>>();

		private ColumnBase<TAdapterChild> _childPersonNameColumn;
		private ColumnBase<TAdapterChild> _childFromDateColumn;
		private ColumnBase<TAdapterChild> _childStartWeekColumn;
		private ColumnBase<TAdapterChild> _childRotationNameColumn;

		//menu items
		private ToolStripMenuItem _addNewPersonRotationItem;
		private ToolStripMenuItem _deletePersonRotationMenuItem;
		private ToolStripMenuItem _pasteSpecialPersonPeriodMenuItem;
		private ToolStripMenuItem _copySpecialPersonPeriodMenuItem;
		private ToolStripMenuItem _lookUpMenuIem;

		//Special copy and paste related stuff
		private readonly IList<IPersonRotation> _selectedPersonRotationCollection = new List<IPersonRotation>();
		private readonly IList<IPersonAvailability> _selectedPersonAvailabilityCollection = new List<IPersonAvailability>();

		/// <summary>
		/// The rectangle to hold the push button.
		/// </summary>
		private Rectangle _rect;

		// View Type 
		private ViewType _viewType;
		private readonly IToggleManager _toggleManager;

		internal override ViewType Type
		{
			get { return _viewType; }
		}

		public override int ParentGridLastColumnIndex
		{
			get { return _parentGridColumns.Count; }
		}

		internal override void ChildGridQueryColWidth(object sender, GridRowColSizeEventArgs e)
		{
			// RenderingAddValue - 1 is used for fill column width different with child and parent grid
			PeopleAdminHelper.CreateColWidthForChildGrid(e, ValidColumn(e.Index), _parentGridColumns.Count - 1,
														 RenderingAddValue - 1, Grid.ColWidths[e.Index + 2]);
		}

		internal override void ChildGridQueryRowHeight(object sender, GridRowColSizeEventArgs e)
		{
			PeopleAdminHelper.CreateRowHeightForChildGrid(e);
		}

		internal override void ChildGridQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			PeopleAdminHelper.CreateColumnCountForChildGrid(e, _childGridColumns.Count - 1);
		}

		internal override void ChildGridQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			var personRotationChildCollection = ((GridControl)sender).Tag as ReadOnlyCollection<TAdapterChild>;
			PeopleAdminHelper.CreateRowCountForChildGrid(e, personRotationChildCollection);
		}

		internal override void ChildGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			var gridControl = ((GridControl)sender);

			var personRotationChildCollection = gridControl.Tag as ReadOnlyCollection<TAdapterChild>;
			PeopleWorksheet.StateHolder.CurrentRotationChildName = gridControl.Text;

			if (personRotationChildCollection != null)
				if (ValidCell(e.ColIndex, e.RowIndex, gridControl.RowCount))
				{
					_childGridColumns[e.ColIndex].GetCellInfo(e, personRotationChildCollection);
				}
			base.ChildGridQueryCellInfo(sender, e);
		}

		internal override void ChildGridQuerySaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.ColIndex == _weekColumnIndexChildGrid)
			{
				// test if can be converted to int
				int cellValue;
				if (Int32.TryParse(e.Style.CellValue.ToString(), out cellValue))
				{
					if (cellValue < 1)
						return;
				}
			}
		    var grid = (GridControl) sender;
			var personRotationChildCollection = grid.Tag as ReadOnlyCollection<TAdapterChild>;
			if (personRotationChildCollection != null)
				if (ValidCell(e.ColIndex, e.RowIndex, grid.RowCount))
					_childGridColumns[e.ColIndex].SaveCellInfo(e, personRotationChildCollection);
		}

		internal override void ChildGridClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			ClearTheCollections();
			CanCopyRow = false;
			CanCopyChildRow = false;

			var gridModel = sender as GridModel;
			if (gridModel == null) return;

			var grid = gridModel.ActiveGridView as GridControl;
			if (grid == null) return;
			// child copy processings
			var adapterChildCollection = grid.Tag as ReadOnlyCollection<TAdapterChild>;
			if (adapterChildCollection == null) return;

			CanCopyChildRow = true;

			var gridRangeInfoList = grid.Model.SelectedRanges;
			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - 1];
				if (gridRangeInfo.Height == 1)
				{
					ChildGridCopyHelper(gridRangeInfo.Top - 1, adapterChildCollection);
				}
				else
				{
					for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						ChildGridCopyHelper(row - 1, adapterChildCollection);
					}
				}
			}
		}

		void ParentColumnCellChanged(object sender, ColumnCellChangedEventArgs<TAdapterParent> e)
		{
			e.DataItem.CanBold = true;

			PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _parentGridColumns.Count, Grid);
		}

		void ParentColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterParent> e)
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}

		void ChildColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterChild> e)
		{
			if (e.DataItem.CanBold)
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
		}

		public override void RefreshParentGrid()
		{
			// To overcome rendering issues in message broker
			if (Grid.CurrentCell != null) Grid.CurrentCell.MoveTo(1, _parentGridColumns.IndexOf(_rotationNameColumn) + 1);
		}

		public override void RefreshChildGrids()
		{
			// Updating child adapters

			if (_viewType == ViewType.PersonRotationView)
			{

				IList<PersonRotationModelParent> adaptersWithChildren = FilteredPeopleHolder.
					PersonRotationParentAdapterCollection.Where(s => s.GridControl != null).ToList();


				foreach (PersonRotationModelParent adapter in adaptersWithChildren)
				{
					// This is for overcome rendering issue with message broker
					adapter.GridControl.CurrentCell.MoveTo(1, _childGridColumns.IndexOf(_childRotationNameColumn) + 1);
				}
			}
			if (_viewType == ViewType.PersonAvailabilityView)
			{
				IList<PersonAvailabilityModelParent> adaptersWithChildren = FilteredPeopleHolder.
				   PersonAvailabilityParentAdapterCollection.Where(s => s.GridControl != null).ToList();


				foreach (PersonAvailabilityModelParent adapter in adaptersWithChildren)
				{
					// This is for overcome rendering issue with message broker
					adapter.GridControl.CurrentCell.MoveTo(1, _childGridColumns.IndexOf(_childRotationNameColumn) + 1);
				}
			}

		}

		internal override void CreateHeaders()
		{
			CreateParentGridHeaders();
			CreateChildGridHeaders();

			// Hide column which is used as a container for grid in cell implementation 
			var gridViewColumn = _parentGridColumns.IndexOf(_gridInCellColumn);
			Grid.Cols.Hidden[gridViewColumn] = true;
		}

		internal override void PrepareView()
		{
			ColCount = _parentGridColumns.Count;
			RowCount = _parentAdapterCollection.Count;

			Grid.RowCount = RowCount;
			Grid.ColCount = ColCount - 1;
			Grid.Model.Data.RowCount = RowCount;

			Grid.Cols.HeaderCount = 0;
			Grid.Rows.HeaderCount = 0;

			switch (_viewType)
			{
				case ViewType.PersonRotationView:
					Grid.Name = "PersonRotationView";
					break;

				case ViewType.PersonAvailabilityView:
					Grid.Name = "PersonAvailabilityView";
					break;
			}

			int length = _parentGridColumns.Count;
			for (int index = 0; index < length; index++)
			{
				Grid.ColWidths[index] = _parentGridColumns[index].PreferredWidth + 10;
			}

			//Hack: This is used to overcome rendering issue in period grids;width 
			// of the last column need to increase
			Grid.ColWidths[6] += 2;
            Grid.ColWidths[0] = _parentGridColumns[0].PreferredWidth;


		}

		internal override void MergeHeaders()
		{
			Grid.Model.CoveredRanges.Add(GridRangeInfo.Cells(0, 1, 0, 3));
		}

		public override void Invalidate()
		{
			Grid.Invalidate();
		}

		internal override void CreateContextMenu()
		{
			Grid.ContextMenuStrip = new ContextMenuStrip();

			_addNewPersonRotationItem = new ToolStripMenuItem(Resources.New);
			_addNewPersonRotationItem.Click += AddNewGridRow;
			Grid.ContextMenuStrip.Items.Add(_addNewPersonRotationItem);

			_deletePersonRotationMenuItem = new ToolStripMenuItem(Resources.Delete);
			_deletePersonRotationMenuItem.Click += DeleteSelectedGridRows;
			Grid.ContextMenuStrip.Items.Add(_deletePersonRotationMenuItem);

			_copySpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.CopySpecial);
			_copySpecialPersonPeriodMenuItem.Click += CopySpecial;
			Grid.ContextMenuStrip.Items.Add(_copySpecialPersonPeriodMenuItem);

            _pasteSpecialPersonPeriodMenuItem = new ToolStripMenuItem(Resources.PasteNew);
			_pasteSpecialPersonPeriodMenuItem.Click += PasteSpecial;
			Grid.ContextMenuStrip.Items.Add(_pasteSpecialPersonPeriodMenuItem);

			_lookUpMenuIem = new ToolStripMenuItem(Resources.LookUp);
			_lookUpMenuIem.Click += LookUpMenuIemClick;
			Grid.ContextMenuStrip.Items.Add(_lookUpMenuIem);
		}

		void LookUpMenuIemClick(object sender, EventArgs e)
		{
			SelectedEntity<IAggregateRoot> selectedEntity = null;
			if (_viewType == ViewType.PersonRotationView)
			{
				selectedEntity = new SelectedEntity<IAggregateRoot>
					(PeopleWorksheet.StateHolder.RotationStateHolder.GetCurrentEntity(Grid, GridInCellColumnIndex) as
					 IRotation,
					 Common.Configuration.ViewType.Rotation);
			}
			if (_viewType == ViewType.PersonAvailabilityView)
			{
				selectedEntity = new SelectedEntity<IAggregateRoot>(
					PeopleWorksheet.StateHolder.RotationStateHolder.GetCurrentEntity(Grid, GridInCellColumnIndex) as
					IAvailabilityRotation,
					Common.Configuration.ViewType.Availability);
			}

			if (selectedEntity != null)
			{
				var screen = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager)), selectedEntity);
				screen.Show();
			}
		}

		internal override void AddNewGridRow<T>(object sender, T eventArgs)
		{
			AddPersonRotation();
		}

		internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
		{
			if (Grid.Model.SelectedRanges.Count > 0)
			{
				var gridRangeInfoList = Grid.Model.SelectedRanges;
				for (var index = gridRangeInfoList.Count; index > 0; index--)
				{
					var gridRangeInfo = gridRangeInfoList[index - 1];

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

		public override void Reinitialize()
		{
			Grid.Refresh();
		}

		internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			UpdateLocalAdapterCollection();

			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_parentGridColumns[e.ColIndex].GetCellInfo(e,
														   new ReadOnlyCollection<TAdapterParent>(
															   _parentAdapterCollection));
			}
			base.QueryCellInfo(e);
		}

		internal override void CellButtonClicked(GridCellButtonClickedEventArgs e)
		{
			if (e.ColIndex == 1)
			{
				LoadInnerGrid(e.RowIndex);
			}
		}

		internal override void DrawCellButton(GridDrawCellButtonEventArgs e)
		{
			if (e.Style.CellType == "PushButton")
			{
				var width = 20;
				//int height = 20;
				_rect = new Rectangle(e.Button.Bounds.X, e.Button.Bounds.Y, width, Grid.DefaultRowHeight);
				e.Button.Bounds = _rect;
			}
		}

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (e.ColIndex == _weekColumIndex)
			{
				// test if can be converted to int
				int cellValue;
				if (Int32.TryParse(e.Style.CellValue.ToString(), out cellValue))
				{
					if (cellValue < 1)
						return;
				}
			}

			if (ValidCell(e.ColIndex, e.RowIndex))
				_parentGridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<TAdapterParent>(
																   _parentAdapterCollection));
		}

		internal void PasteSpecial<T>(object sender, T eventArgs)
		{
			var gridRangeInfoList = Grid.Model.Selections.Ranges;

			if (!CanCopyRow && !CanCopyChildRow) return;
			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - 1];


				if (gridRangeInfo.IsTable)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 1; i <= Grid.RowCount; i++)
						CopyAllPersonRotations(i - 1);
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						CopyAllPersonRotations(gridRangeInfo.Top - 1);
					}
					else
					{
						for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
						{
							CopyAllPersonRotations(row - 1);
						}
					}
				}
			}
		}

		internal void CopySpecial<T>(object sender, T eventArgs)
		{
			if (Grid.Model.CurrentCellInfo == null)
			{
				//TODO:Need to implement when person is not selected scenario 
				return;
			}

			var rowIndex = Grid.CurrentCell.RowIndex;

			if (rowIndex == 0) return;

			ClearTheCollections();
			CanCopyRow = false;
			CanCopyChildRow = false;

			if (!_parentAdapterCollection[rowIndex - 1].ExpandState)
			{
				CopyCollapsedPersonRotations(rowIndex - 1);
			}
			else
			{
				CopyExpandedPersonRotations();
			}
		}

		internal override void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
		{
			if (Grid.Model.CurrentCellInfo == null)
			{
				//TODO:Need to implement when person is not selected scenario 
				return;
			}

			int rowIndex = Grid.CurrentCell.RowIndex;
			CanCopyRow = false;
			CanCopyChildRow = false;
			ClearTheCollections();

			if (Type == ViewType.PersonRotationView)
			{
				if (rowIndex == 0) return;

				if (!FilteredPeopleHolder.PersonRotationParentAdapterCollection[rowIndex - 1].ExpandState && FilteredPeopleHolder.PersonRotationParentAdapterCollection[rowIndex - 1].PersonRotation != null)
				{
					CanCopyRow = true;
					PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex - 1);

					foreach (var rotation in PeopleWorksheet.StateHolder.ChildPersonRotationCollection)
					{
						// Parent processings
						if (!_selectedPersonRotationCollection.Contains(rotation))
							_selectedPersonRotationCollection.Add(rotation);
					}
				}
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				if (rowIndex == 0) return;

				if (!FilteredPeopleHolder.PersonAvailabilityParentAdapterCollection[rowIndex - 1].ExpandState)
				{
					CanCopyRow = true;
					PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex - 1);

					foreach (var avail in PeopleWorksheet.StateHolder.ChildPersonAvailabilityCollection)
					{
						// Parent processings
						if (!_selectedPersonAvailabilityCollection.Contains(avail))
							_selectedPersonAvailabilityCollection.Add(avail);
					}
				}
			}

		}

		private void ClearTheCollections()
		{
			_selectedPersonRotationCollection.Clear();
			_selectedPersonAvailabilityCollection.Clear();
		}

		internal override void SelectedDateChange(object sender, EventArgs e)
		{
			// Collaps all expanded rows
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				var gridInfo =
					GridRangeInfo.Cells(rowIndex + 1, _gridInCellColumnIndex, rowIndex + 1, ParentGridLastColumnIndex);

				if (_parentAdapterCollection[rowIndex].ExpandState)
				{
					_parentAdapterCollection[rowIndex].ExpandState = false;

					//// Get child grid and dispose it
					var gridControl = Grid[rowIndex + 1, _gridInCellColumnIndex].Control as GridControl;
					_parentAdapterCollection[rowIndex].GridControl = null;
					GridCreatorDispose(gridControl, gridInfo);

					Grid.RowHeights[rowIndex + 1] = DefaultRowHeight;
				}
			}

			if (Type == ViewType.PersonRotationView)
			{
				// ReCall Person rotations
				FilteredPeopleHolder.RefreshPersonRotations();
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				FilteredPeopleHolder.RefreshPersonAvailabilities();
			}

			Grid.Invalidate();
		}

		internal override void DisposeChildGrids()
		{
			for (int rowIndex = 0; rowIndex < Grid.RowCount; rowIndex++)
			{
				var gridInfo =
					GridRangeInfo.Cells(rowIndex + 1, _gridInCellColumnIndex, rowIndex + 1, ParentGridLastColumnIndex);
				if (_parentAdapterCollection.Count == rowIndex) return;

				if (_parentAdapterCollection[rowIndex].ExpandState)
				{
					_parentAdapterCollection[rowIndex].ExpandState = false;

					// Get child grid and dispose it
					var gridControl = Grid[rowIndex + 1, _gridInCellColumnIndex].Control as GridControl;
					_parentAdapterCollection[rowIndex].GridControl = null;

					GridCreatorDispose(gridControl, gridInfo);
					Grid.RowHeights[rowIndex + 1] = DefaultRowHeight;
				}
			}
		}

		public override void Sort(bool isAscending)
		{
			// Gets the filtered people grid data as a collection
			var personRotationcollection = new List<TAdapterParent>(_parentAdapterCollection);
			int columnIndex = GetColumnIndex();

			// Gets the sort column to sort
			string sortColumn = _parentGridColumns[columnIndex].BindingProperty;
			// Gets the coparer erquired to sort the data
			var comparer = _parentGridColumns[columnIndex].ColumnComparer;

			Grid.CurrentCell.MoveLeft();

			// Dispose the child grids
			DisposeChildGrids();

			if (!string.IsNullOrEmpty(sortColumn))
			{
				// Holds the results of the sorting process
				IList<TAdapterParent> result;
				if (comparer != null)
				{
					// Sorts the person collection in ascending order
					personRotationcollection.Sort(comparer);
					if (!isAscending)
						personRotationcollection.Reverse();

					result = personRotationcollection;
				}
				else
				{
					// Gets the sorted people collection
					result = GridHelper.Sort(
						new Collection<TAdapterParent>(personRotationcollection),
						sortColumn,
						isAscending
						);
				}

				_parentAdapterCollection = result;

				// Sets the filtered list
				//FilteredPeopleHolder.SetSortedPersonRotationFilteredList(result);
				if (Type == ViewType.PersonRotationView)
				{
					FilteredPeopleHolder.SetSortedPersonRotationFilteredList(
						(List<PersonRotationModelParent>)_parentAdapterCollection);
				}
				else if (Type == ViewType.PersonAvailabilityView)
				{
					FilteredPeopleHolder.SetSortedPersonAvailabilityFilteredList(
						(List<PersonAvailabilityModelParent>)_parentAdapterCollection);
				}

				Grid.CurrentCell.MoveRight();

				Invalidate();
			}
		}

		internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
		{
			PasteSpecial(sender, eventArgs);
		}

		private bool IsValidRow()
		{
			return Grid.CurrentCell.RowIndex > 0;
		}

		private TAdapterParent CurrentPersonRotationView
		{
			get { return _parentAdapterCollection[CurrentRowIndex]; }
		}

		private bool IsCurrentRowExpanded()
		{
			return IsValidRow()
				&& CurrentPersonRotationView.ExpandState;
		}

		private int CurrentRowIndex
		{
			get { return Grid.CurrentCell.RowIndex - 1; }
		}

		private int GetColumnIndex()
		{
			int columnIndex;
			if (IsCurrentRowExpanded())
			{
				columnIndex = CurrentPersonRotationView.GridControl.CurrentCell.ColIndex + 2;
			}
			else
			{
				int parentColIndex = gridColumnIndex();
				columnIndex = (parentColIndex < 2) ? (3) : parentColIndex; // TODO: Use constants instead.
			}
			return columnIndex;
		}

		private int gridColumnIndex()
		{
			if (Grid.CurrentCell.ColIndex == -1)
				Grid.CurrentCell.MoveTo(0, 0);
			return Grid.CurrentCell.ColIndex;
		}

		private void Init()
		{
			Grid.CellModels.Add("GridInCell", new GridInCellModel(Grid.Model));
			Grid.CurrentCellChanged += ParentGridCurrentCellChanged;
			Grid.CurrentCellShowingDropDown += ParentGridCurrentCellShowingDropDown;
		}

		private void CreateParentGridHeaders()
		{
			_parentGridColumns.Add(new RowHeaderColumn<TAdapterParent>());

			_pushButtonColumn = new PushButtonColumn<TAdapterParent>(Resources.FullName, "RotationCount");
			_parentGridColumns.Add(_pushButtonColumn);

			_gridInCellColumn = new GridInCellColumn<TAdapterParent>("GridControl");
			_parentGridColumns.Add(_gridInCellColumn);

			_personNameColumn = new ReadOnlyTextColumn<TAdapterParent>("PersonFullName", Resources.Person);
			_personNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
			_parentGridColumns.Add(_personNameColumn);



			//only the person-rotation view has the start week column
			if (Type == ViewType.PersonRotationView)
			{

				_fromDateColumn = new EditableDateOnlyColumnForPeriodGrids<TAdapterParent>("FromDate", Resources.FromDate);
				_fromDateColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
				_fromDateColumn.CellChanged += ParentColumnCellChanged;
				_fromDateColumn.ColumnComparer = new PersonRotationFromDateComparer<TAdapterParent, TBaseType, TScheduleType>();
				_parentGridColumns.Add(_fromDateColumn);


				_rotationNameColumn = new DropDownColumnForPeriodGrids<TAdapterParent, IRotation>("CurrentRotation",
																								 Resources.Rotation,
																								 PeopleWorksheet.StateHolder.AllRotations,
																								 "Name")
				                      	{
				                      		ColumnComparer =
				                      			new PersonRotationCurrentRotationComparer<TAdapterParent, TBaseType, TScheduleType>()
				                      	};

				_rotationNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
				_rotationNameColumn.CellDisplayChanged += RotationNameColumn_CellDisplayChanged;
				_rotationNameColumn.CellChanged += ParentColumnCellChanged;
				_parentGridColumns.Add(_rotationNameColumn);


				_startWeekColumn = new DynamicDropDownColumn<TAdapterParent, int>("StartWeek", Resources.StartWeek, "RotationWeekCount");

				_startWeekColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
				_startWeekColumn.CellChanged += ParentColumnCellChanged;
				_startWeekColumn.ColumnComparer = new PersonRotationStartWeekComparer<TAdapterParent, TBaseType, TScheduleType>();

				_parentGridColumns.Add(_startWeekColumn);
			}

			else if (Type == ViewType.PersonAvailabilityView)
			{
				_fromDateColumn = new EditableDateOnlyColumnForPeriodGrids<TAdapterParent>("FromDate", Resources.FromDate);
				_fromDateColumn.CellChanged += ParentColumnCellChanged;
				_fromDateColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
				_fromDateColumn.ColumnComparer = new PersonAvailabilityFromDateComparer<TAdapterParent, TBaseType, TScheduleType>();
				_parentGridColumns.Add(_fromDateColumn);

				_rotationNameColumn = new DropDownColumnForPeriodGrids<TAdapterParent, IAvailabilityRotation>("CurrentRotation",
																				 Resources.Availability,
																				 PeopleWorksheet.StateHolder.AllAvailabilities,
																				 "Name", false)
				                      	{
				                      		ColumnComparer =
				                      			new PersonAvailabilityCurrentRotationComparer<TAdapterParent, TBaseType, TScheduleType>()
				                      	};
				_rotationNameColumn.CellChanged += ParentColumnCellChanged;
				_rotationNameColumn.CellDisplayChanged += ParentColumn_CellDisplayChanged;
				_rotationNameColumn.CellDisplayChanged += AvailabilityNameColumn_CellDisplayChanged;
				_parentGridColumns.Add(_rotationNameColumn);
			}
		}

		void RotationNameColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterParent> e)
		{
			SetDataSource(e, PeopleWorksheet.StateHolder.AllRotations);
		}

		private static void SetDataSource(ColumnCellDisplayChangedEventArgs<TAdapterParent> e, object databasource)
		{
			e.QueryCellInfoEventArg.Style.ClearCache();
			e.QueryCellInfoEventArg.Style.DataSource = null;
			e.QueryCellInfoEventArg.Style.DataSource = databasource;
			e.QueryCellInfoEventArg.Style.FormattedText = string.Empty;
		}

		void AvailabilityNameColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterParent> e)
		{
			SetDataSource(e, PeopleWorksheet.StateHolder.AllAvailabilities);
		}

		private void CreateChildGridHeaders()
		{
			_childGridColumns.Add(new RowHeaderColumn<TAdapterChild>());

			_childPersonNameColumn = new LineColumn<TAdapterChild>("PersonFullName");
			_childGridColumns.Add(_childPersonNameColumn);

			if (Type == ViewType.PersonRotationView)
			{
				_childFromDateColumn =
					new EditableDateOnlyColumnForPeriodGrids<TAdapterChild>("FromDate", Resources.FromDate);
				_childFromDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
				_childFromDateColumn.CellChanged += ChildColumn_CellChanged;
				_childGridColumns.Add(_childFromDateColumn);

				_childRotationNameColumn =
					new DropDownColumnForPeriodGrids<TAdapterChild, IRotation>("CurrentRotation",
																			  Resources.Rotation,
																			  PeopleWorksheet.StateHolder.AllRotations,
																			  "Name");
				_childRotationNameColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
				_childRotationNameColumn.CellDisplayChanged += ChildRotationNameColumn_CellDisplayChanged;
				_childRotationNameColumn.CellChanged += ChildColumn_CellChanged;
				_childGridColumns.Add(_childRotationNameColumn);

				//only the person-rotation view has the start week column
				_childStartWeekColumn =
					new DynamicDropDownColumn<TAdapterChild, int>("StartWeek", Resources.StartWeek, "RotationWeekCount");
				_childStartWeekColumn.CellChanged += ChildColumn_CellChanged;
				_childStartWeekColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
				_childGridColumns.Add(_childStartWeekColumn);
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				_childFromDateColumn =
					new EditableDateOnlyColumnForPeriodGrids<TAdapterChild>("FromDate", Resources.FromDate);
				_childFromDateColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
				_childFromDateColumn.CellChanged += ChildColumn_CellChanged;
				_childGridColumns.Add(_childFromDateColumn);

				_childRotationNameColumn =
				  new DropDownColumnForPeriodGrids<TAdapterChild, IAvailabilityRotation>("CurrentRotation",
																			Resources.Availability,
																			PeopleWorksheet.StateHolder.AllAvailabilities,
																			"Name", false);
				_childRotationNameColumn.CellDisplayChanged += ChildColumn_CellDisplayChanged;
				_childRotationNameColumn.CellDisplayChanged += ChildAvailabilityNameColumn_CellDisplayChanged;
				_childRotationNameColumn.CellChanged += ChildColumn_CellChanged;
				_childGridColumns.Add(_childRotationNameColumn);

			}
		}

		void ChildAvailabilityNameColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterChild> e)
		{
			SetDataSource(e, PeopleWorksheet.StateHolder.AllAvailabilities);
		}

		private static void SetDataSource(ColumnCellDisplayChangedEventArgs<TAdapterChild> e, object databasource)
		{
			e.QueryCellInfoEventArg.Style.ClearCache();
			e.QueryCellInfoEventArg.Style.DataSource = null;
			e.QueryCellInfoEventArg.Style.DataSource = databasource;
			e.QueryCellInfoEventArg.Style.FormattedText = string.Empty;
		}

		void ChildRotationNameColumn_CellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<TAdapterChild> e)
		{
			SetDataSource(e, PeopleWorksheet.StateHolder.AllRotations);
		}

		void ChildColumn_CellChanged(object sender, ColumnCellChangedEventArgs<TAdapterChild> e)
		{
			e.DataItem.CanBold = true;
			GridControl grid = null;
			if (Grid.CurrentCell.RowIndex > 0)
			{
				if (Type == ViewType.PersonRotationView)
				{
					grid = FilteredPeopleHolder.PersonRotationParentAdapterCollection[Grid.CurrentCell.RowIndex - 1].
						GridControl;
				}
				if (Type == ViewType.PersonAvailabilityView)
				{
					grid =
						FilteredPeopleHolder.PersonAvailabilityParentAdapterCollection[Grid.CurrentCell.RowIndex - 1].
							GridControl;
				}

				PeopleAdminHelper.InvalidateGridRange(e.SaveCellInfoEventArgs.RowIndex, _childGridColumns.Count, grid);
			}
		}

		private void LoadInnerGrid(int rowIndex)
		{
			if (rowIndex != 0)
			{
				// Set grid range for covered ranged
				var gridInfo =
					GridRangeInfo.Cells(rowIndex, _gridInCellColumnIndex, rowIndex, ParentGridLastColumnIndex - 1);

				// Set out of focus form current cell.This helps  to fire save cell info in child grid.
				SetOutOfFocusFromCurrentCell();

				if (!_parentAdapterCollection[rowIndex - 1].ExpandState)
				{
					//loading child grids need to be taken out of the condiotions to make sure that
					//correct child items are loaded to the row, wheather it is expanded or collapsed.

					GridControl grid = _parentAdapterCollection[rowIndex - 1].GridControl;

					GetChildRotations(rowIndex, grid);

					_parentAdapterCollection[rowIndex - 1].ExpandState = true;

					////Remove selected row - New
					Grid.Selections.Remove(GridRangeInfo.Row(rowIndex));

					LoadAbstractGrid(rowIndex, _gridInCellColumnIndex, gridInfo);

					Grid.CurrentCell.MoveTo(rowIndex, GridInCellColumnIndex);
				}
				else
				{
					_parentAdapterCollection[rowIndex - 1].ExpandState = false;
					//_parentAdapterCollection[rowIndex - 1].GridControl = null;

					//// Get child grid and dispose it
					var gridControl = Grid[rowIndex, _gridInCellColumnIndex].Control as GridControl;
					GridCreatorDispose(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;

					PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenAddedOrUpdated(rowIndex -
																											  1);

					Grid.InvalidateRange(gridInfo);
				}
			}
		}

		private static void GetChildRotations(int rowIndex, GridControl grid)
		{
			if (grid != null)
			{
				PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex - 1, grid);
			}
			else
			{
				PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex - 1);
			}
		}

		private void LoadAbstractGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
		{
			BindDropDownGridEvents();

			CreateGrid(rowIndex, columnIndex, gridInfo);
		}

		private void CreateGrid(int rowIndex, int columnIndex, GridRangeInfo gridInfo)
		{
			if (Type == ViewType.PersonRotationView)
			{
				GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
									PeopleWorksheet.StateHolder.PersonRotationChildGridData,
									_childGridColumns.Count - 1,
									PeopleWorksheet.StateHolder.CurrentRotationChildName);
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				GridCreator.GetGrid(Grid, gridInfo, rowIndex, columnIndex,
									PeopleWorksheet.StateHolder.PersonAvailabilityChildGridData,
									_childGridColumns.Count - 1,
									PeopleWorksheet.StateHolder.CurrentRotationChildName);
			}
		}

		private void GridCreatorDispose(GridControl abstractGrid, GridRangeInfo gridRange)
		{
			//int columnIndex,
			Grid.CoveredRanges.Remove(gridRange);
			Grid.Controls.Remove(abstractGrid);
		}

		private void AddPersonRotation()
		{
			if (Grid.Model.CurrentCellInfo == null)
			{
				//TODO:Need to implement when person is not selected scenario 
				return;
			}

			InsertPersonPeriod();
		}

		private void InsertPersonPeriod()
		{
			var gridRangeInfoList = Grid.Model.SelectedRanges;

			for (int index = 0; index < gridRangeInfoList.Count; index++)
			{
				var gridRangeInfo = gridRangeInfoList[index];

				if (gridRangeInfo.IsTable || gridRangeInfo.IsCols)
				{
					// This scenario is used for when user is selecting entire grid using button give top in 
					// that grid.
					for (int i = 1; i <= Grid.RowCount; i++)
					{
						AddPersonRotation(i);
					}
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						AddPersonRotation(gridRangeInfo.Top);
					}
					else
					{
						for (int row = gridRangeInfo.Top; row <= gridRangeInfo.Bottom; row++)
						{
							AddPersonRotation(row);
						}
					}
				}
			}
		}

		private void AddPersonRotation(int rowIndex)
		{
			if (rowIndex == 0)
				return;

			PeopleWorksheet.StateHolder.RotationStateHolder.AddPersonRotation(rowIndex - 1);

			GridControl grid = _parentAdapterCollection[rowIndex - 1].GridControl;
			GetChildRotations(rowIndex, grid);

			if (_parentAdapterCollection[rowIndex - 1].ExpandState)
			{
				var childGrid = Grid[rowIndex, _gridInCellColumnIndex].Control as CellEmbeddedGrid;
				if (childGrid != null)
				{
					SetChildGridTagProperty(childGrid);
					// Merging name column's all cellsA
					childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, childGrid.RowCount, 1));
					//increase the parent grids row height bya a single row
					Grid.RowHeights[rowIndex] += DefaultRowHeight;
					childGrid.Refresh();
					Grid.RefreshRange(GridRangeInfo.Cells(rowIndex, _gridInCellColumnIndex - 1, rowIndex,
						ParentGridLastColumnIndex));
				}
			}
			else
			{
				PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenAddedOrUpdated(rowIndex - 1);
				UpdateLocalAdapterCollection();

				var gridInfo =
					GridRangeInfo.Cells(rowIndex, _gridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);

				Grid.InvalidateRange(gridInfo);

				////Fix for bug 6891, this makes the rows expand automatically when you add a new rotation
				//var adapterParent = _parentAdapterCollection[rowIndex - 1] as PersonRotationModelParent;

				//if (adapterParent != null)
				//{
				//    if (adapterParent.RotationCount > 1)
				//    {
				//        LoadInnerGrid(rowIndex);
				//    }
				//}
			}
		}

		private void SetChildGridTagProperty(CellEmbeddedGrid childGrid)
		{
			if (Type == ViewType.PersonRotationView)
			{
				childGrid.Tag = PeopleWorksheet.StateHolder.PersonRotationChildGridData;
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				childGrid.Tag = PeopleWorksheet.StateHolder.PersonAvailabilityChildGridData;
			}
		}

		private void RemoveChild(int rowIndex, bool isDeleteAll)
		{
			var childGrid = Grid[rowIndex, _gridInCellColumnIndex].Control as CellEmbeddedGrid;
			if (childGrid != null)
			{
				if (isDeleteAll)
				{
					PersonRotationDeleteAllChildren(rowIndex, childGrid);
				}
				else
				{
					var gridRangeInfoList = childGrid.Model.SelectedRanges.GetRowRanges(GridRangeInfoType.Cells | GridRangeInfoType.Rows);
					for (var index = gridRangeInfoList.Count; index > 0; index--)
					{
						var gridRangeInfo = gridRangeInfoList[index - 1];
						var top = gridRangeInfo.Top;
						var bottom = gridRangeInfo.Bottom;
						if (top == 0) continue;
						if (gridRangeInfo.Height == 1)
						{
							PersonRotationChildDelete(rowIndex, top - 1, childGrid);
						}
						else
						{
							for (var row = bottom; row >= top; row--)
							{
								PersonRotationChildDelete(rowIndex, row - 1, childGrid);
							}
						}
					}
				}
			}
		}

		private void PersonRotationDeleteAllChildren(int rowIndex, CellEmbeddedGrid childGrid)
		{
			PeopleWorksheet.StateHolder.RotationStateHolder.DeleteAllChildPersonRotations(rowIndex - 1);

			var personRotationChildCollection = childGrid.Tag as ReadOnlyCollection<TAdapterChild>;

			if (personRotationChildCollection != null)
			{
				IList<TAdapterChild> personRotationCollection =
					new List<TAdapterChild>(personRotationChildCollection);
				personRotationCollection.Clear();

				var gridInfo = GridRangeInfo.Cells(rowIndex, _gridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);

				childGrid.Tag = new ReadOnlyCollection<TAdapterChild>(personRotationCollection);
				childGrid.RowCount = personRotationCollection.Count;
				childGrid.Invalidate();

				// remove child grid
				_parentAdapterCollection[rowIndex - 1].ExpandState = false;
				_parentAdapterCollection[rowIndex - 1].GridControl = null;

				//// Get child grid and dispose it
				var gridControl = Grid[rowIndex, _gridInCellColumnIndex].Control as GridControl;
				GridCreatorDispose(gridControl, gridInfo);

				Grid.RowHeights[rowIndex] = DefaultRowHeight;

				PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenDeleted(rowIndex - 1);
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void PersonRotationChildDelete(int rowIndex, int childPersonPeriodIndex, CellEmbeddedGrid childGrid)
		{
			PeopleWorksheet.StateHolder.RotationStateHolder.SetChildrenPersonRotationCollection(childGrid.Tag);
			PeopleWorksheet.StateHolder.RotationStateHolder.DeletePersonRotation(rowIndex - 1, childPersonPeriodIndex);

			var personRotationChildCollection = childGrid.Tag as ReadOnlyCollection<TAdapterChild>;

			if (personRotationChildCollection != null)
			{
				string personName = _parentAdapterCollection[rowIndex - 1].PersonFullName;
				IList<TAdapterChild> rotationCollection =
					new List<TAdapterChild>(personRotationChildCollection);
				rotationCollection.RemoveAt(childPersonPeriodIndex);

				GridRangeInfo gridInfo =
					GridRangeInfo.Cells(rowIndex, _gridInCellColumnIndex - 1, rowIndex, ParentGridLastColumnIndex);


				if (rotationCollection.Count > 0 && childPersonPeriodIndex == 0)
				{
					rotationCollection[0].PersonFullName = personName;
				}

				childGrid.Tag = new ReadOnlyCollection<TAdapterChild>(rotationCollection);
				childGrid.RowCount = rotationCollection.Count;
				childGrid.Invalidate();

				if (childGrid.RowCount == 0)
					Grid.RowHeights[rowIndex] = DefaultRowHeight + RenderingAddValue;
				else
					Grid.RowHeights[rowIndex] = childGrid.RowCount * DefaultRowHeight + RenderingAddValue;

				if (rotationCollection.Count == 0 || rotationCollection.Count == 1)
				{
					// remove child grid
					_parentAdapterCollection[rowIndex - 1].ExpandState = false;
					_parentAdapterCollection[rowIndex - 1].GridControl = null;

					PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenDeleted(gridInfo.Top - 1);

					//// Get child grid and dispose it
					var gridControl = Grid[rowIndex, _gridInCellColumnIndex].Control as GridControl;
					GridCreatorDispose(gridControl, gridInfo);

					Grid.RowHeights[rowIndex] = DefaultRowHeight;
				}
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void CopyAllPersonRotations(int rowIndex)
		{
			if (rowIndex < 0) return;

			var personPaste = _parentAdapterCollection[rowIndex].Person;
			var isParent = !_parentAdapterCollection[rowIndex].ExpandState;

			if (personPaste != null)
			{
				if (CanCopyRow)
				{
					CopyAllParentRows(rowIndex, personPaste);
				}

				if (CanCopyChildRow)
				{
					CopyAllChildRows(rowIndex, personPaste);
				}

				//Lets refill the child person rotation collection
				PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex);
				UpdateLocalAdapterCollection();

				if (isParent)
					RefreshParentGridRange(rowIndex);
				else
					RefreshChildGrid(rowIndex);
			}
		}

		private void CopyAllChildRows(int rowIndex, IPerson personPaste)
		{
			if (Type == ViewType.PersonRotationView)
			{
				foreach (IPersonRotation personRotation in _selectedPersonRotationCollection)
				{
					AddRotationWhenPasteSpecial(rowIndex, personPaste, personRotation);
				}
			}
			else
			{
				foreach (IPersonAvailability personAvailability in _selectedPersonAvailabilityCollection)
				{
					AddAvailabilityWhenPasteSpecial(rowIndex, personPaste, personAvailability);
				}
			}
		}

		private void CopyAllParentRows(int rowIndex, IPerson personPaste)
		{
			PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex);

			if (Type == ViewType.PersonRotationView && _selectedPersonRotationCollection.Count > 0)
			{
				foreach (IPersonRotation personRotation in _selectedPersonRotationCollection)
				{
					if (!PeopleWorksheet.StateHolder.ChildPersonRotationCollection.Contains(personRotation))
					{
						AddRotationWhenPasteSpecial(rowIndex, personPaste, personRotation);
					}
				}
			}
			else if (Type == ViewType.PersonAvailabilityView && _selectedPersonAvailabilityCollection.Count > 0)
			{
				foreach (IPersonAvailability personAvailability in _selectedPersonAvailabilityCollection)
				{
					if (!PeopleWorksheet.StateHolder.ChildPersonAvailabilityCollection.Contains(personAvailability))
					{
						AddAvailabilityWhenPasteSpecial(rowIndex, personPaste, personAvailability);
					}
				}
			}
		}

		private void AddAvailabilityWhenPasteSpecial(int rowIndex, IPerson personPaste, IPersonAvailability personAvailability)
		{
			IPersonAvailability newPersonAvail =
				new PersonAvailability(personPaste, personAvailability.Availability,
				PeriodDateService.GetValidPeriodDate(PeriodDateDictionaryBuilder.GetDateOnlyDictionary
				(FilteredPeopleHolder, _viewType, personPaste), personAvailability.StartDate));
			WorksheetStateHolder.AddPersonAvailability(newPersonAvail, rowIndex, FilteredPeopleHolder);
		}

		private void AddRotationWhenPasteSpecial(int rowIndex, IPerson personPaste, IPersonRotation personRotation)
		{
			var newPersonRotation =
				new PersonRotation(personPaste, personRotation.Rotation,
				PeriodDateService.GetValidPeriodDate(PeriodDateDictionaryBuilder.GetDateOnlyDictionary
					(FilteredPeopleHolder, _viewType, personPaste), personRotation.StartDate),
								   personRotation.StartDay);

			WorksheetStateHolder.AddPersonRotation(newPersonRotation, rowIndex, FilteredPeopleHolder);
		}

		private void CopyExpandedPersonRotations()
		{
			CanCopyChildRow = true;

			// child copy processings
			var grid = Grid[Grid.CurrentCell.RowIndex, _gridInCellColumnIndex].Control as GridControl;
			if (grid == null) return;

			CopyExpandedItems(grid);
		}

		private void CopyExpandedItems(GridControl grid)
		{
			if (Type == ViewType.PersonRotationView)
			{
				CopyExpandedPersonRotations(grid);
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				CopyExpandedPersonAvailabilities(grid);
			}
		}

		private void CopyExpandedPersonRotations(GridControl grid)
		{
			var personRotationChildCollection = grid.Tag as ReadOnlyCollection<PersonRotationModelChild>;
			if (personRotationChildCollection == null) return;

			var gridRangeInfoList = grid.Model.SelectedRanges;
			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - 1];
				if (gridRangeInfo.Height == 1)
				{
					var personRotation = personRotationChildCollection[gridRangeInfo.Top - 1].PersonRotation;
					if (!_selectedPersonRotationCollection.Contains(personRotation))
						_selectedPersonRotationCollection.Add(personRotation);
				}
				else
				{
					for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						var personRotation = personRotationChildCollection[row - 1].PersonRotation;
						if (!_selectedPersonRotationCollection.Contains(personRotation))
							_selectedPersonRotationCollection.Add(personRotation);
					}
				}
			}
		}

		private void CopyExpandedPersonAvailabilities(GridControl grid)
		{
			var personAvailabilityChildCollection = grid.Tag as ReadOnlyCollection<PersonAvailabilityModelChild>;
			if (personAvailabilityChildCollection == null) return;

			var gridRangeInfoList = grid.Model.SelectedRanges;
			for (var index = gridRangeInfoList.Count; index > 0; index--)
			{
				var gridRangeInfo = gridRangeInfoList[index - 1];
				if (gridRangeInfo.Height == 1)
				{
					var personAvailability =
						personAvailabilityChildCollection[gridRangeInfo.Top - 1].PersonRotation;
					if (!_selectedPersonAvailabilityCollection.Contains(personAvailability))
						_selectedPersonAvailabilityCollection.Add(personAvailability);
				}
				else
				{
					for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						var personRotation = personAvailabilityChildCollection[row - 1].PersonRotation;
						if (!_selectedPersonAvailabilityCollection.Contains(personRotation))
							_selectedPersonAvailabilityCollection.Add(personRotation);
					}
				}
			}
		}

		private void CopyCollapsedPersonRotations(int rowIndex)
		{
			CanCopyRow = true;

			PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(rowIndex);

			if (Type == ViewType.PersonRotationView)
			{
				CopyCollapsedPersonRotations();
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				CopyCollapsedPersonAvailabilities();
			}
		}

		private void CopyCollapsedPersonRotations()
		{
			//Lets copy all the items in the parent item into the list
			foreach (IPersonRotation personRotation in PeopleWorksheet.StateHolder.ChildPersonRotationCollection)
			{
				//Parent processings
				if (!_selectedPersonRotationCollection.Contains(personRotation))
					_selectedPersonRotationCollection.Add(personRotation);
			}
		}

		private void CopyCollapsedPersonAvailabilities()
		{
			//Lets copy all the items in the parent item into the list
			foreach (IPersonAvailability personAvail in PeopleWorksheet.StateHolder.ChildPersonAvailabilityCollection)
			{
				//Parent processings
				if (!_selectedPersonAvailabilityCollection.Contains(personAvail))
					_selectedPersonAvailabilityCollection.Add(personAvail);
			}
		}

		private void UpdateLocalAdapterCollection()
		{
			if (Type == ViewType.PersonRotationView)
			{
				_parentAdapterCollection =
					(IList<TAdapterParent>)FilteredPeopleHolder.PersonRotationParentAdapterCollection;
			}
			else if (Type == ViewType.PersonAvailabilityView)
			{
				_parentAdapterCollection =
					(IList<TAdapterParent>)FilteredPeopleHolder.PersonAvailabilityParentAdapterCollection;
			}
		}

		private void RefreshParentGridRange(int index)
		{
			PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenAddedOrUpdated(index);
			Grid.InvalidateRange(
				GridRangeInfo.Cells(index + 1, _gridInCellColumnIndex - 1, index + 1, ParentGridLastColumnIndex));
		}

		private void RefreshChildGrid(int index)
		{
			PeopleWorksheet.StateHolder.RotationStateHolder.GetChildPersonRotations(index);

			var childGrid = Grid[index + 1, _gridInCellColumnIndex].Control as CellEmbeddedGrid;
			if (childGrid != null)
			{
				if (Type == ViewType.PersonRotationView)
				{
					childGrid.Tag = PeopleWorksheet.StateHolder.PersonRotationChildGridData;
					// Merging name column's all cells
					childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, childGrid.RowCount + 2, 1));

					Grid.RowHeights[index + 1] = PeopleWorksheet.StateHolder.PersonRotationChildGridData.Count * 20 + 2;
				}
				else if (Type == ViewType.PersonAvailabilityView)
				{
					childGrid.Tag = PeopleWorksheet.StateHolder.PersonAvailabilityChildGridData;

					// Merging name column's all cells
					childGrid.Model.CoveredRanges.Add(GridRangeInfo.Cells(1, 1, childGrid.RowCount + 2, 1));

					Grid.RowHeights[index + 1] = PeopleWorksheet.StateHolder.PersonAvailabilityChildGridData.Count * 20 +
												 2;
				}

				childGrid.Refresh();
			}
		}

		private void DeleteWhenRangeSelected(GridRangeInfo gridRangeInfo)
		{
			if (gridRangeInfo.Height == 1)
			{
				DeleteRotations(gridRangeInfo, gridRangeInfo.Top);
			}
			else
			{
				for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
				{
					if (row != 0)
					{
						DeleteRotations(gridRangeInfo, row);
					}
				}
			}
		}

		private void DeleteRotations(GridRangeInfo gridRangeInfo, int index)
		{
			// Child list remove
			if (_parentAdapterCollection[index - 1].ExpandState)
			{
				RemoveChild(index, gridRangeInfo.IsRows);
			}
			else
			{
				PeopleWorksheet.StateHolder.RotationStateHolder.DeletePersonRotation(index - 1);
				GridRangeInfo gridInfo =
				GridRangeInfo.Cells(index, _gridInCellColumnIndex - 2, index,
									ParentGridLastColumnIndex);
				PeopleWorksheet.StateHolder.RotationStateHolder.GetParentPersonRotationWhenDeleted(index - 1);
				UpdateLocalAdapterCollection();
				Grid.InvalidateRange(gridInfo);
			}
		}

		private void DeleteWhenAllSelected()
		{
			// This scenario is used for when user is selecting entire grid using button give top in 
			// that grid.
			for (var i = 1; i <= Grid.RowCount; i++)
			{
				if (_parentAdapterCollection[i - 1].ExpandState)
				{
					RemoveChild(i, true);
				}
				else
				{
					PeopleWorksheet.StateHolder.RotationStateHolder.DeleteAllPersonRotation(i - 1);

					PeopleWorksheet.StateHolder.GetParentPersonRotationWhenDeleted(i - 1, FilteredPeopleHolder);
					UpdateLocalAdapterCollection();
					Grid.InvalidateRange(
						GridRangeInfo.Cells(i, _gridInCellColumnIndex - 1, i, ParentGridLastColumnIndex));
				}
			}
		}

		private void ParentGridCurrentCellChanged(object sender, EventArgs e)
		{
			var cc = Grid.CurrentCell;
			if (cc.ColIndex == _rotationColumIndex)
			{
				cc.ConfirmChanges(false);

				//Reset the slave value so the user must change it...
				Grid[cc.RowIndex, _weekColumIndex].Text = "";
			}
		}


		private void ParentGridCurrentCellShowingDropDown(object sender, EventArgs e)
		{
			var cc = Grid.CurrentCell;
			if (cc.ColIndex == _weekColumIndex)
			{
				var cr = cc.Renderer as GridComboBoxCellRenderer;
				if (cr != null)
				{
					//    DataView dv = new DataView(slaveComboTable);
					//    dv.RowFilter = string.Format("[masterId] = '{0}'", this.gridDataBoundGrid1[cc.RowIndex, MasterColumn].Text);
					//    ((GridComboBoxListBoxPart)cr.ListBoxPart).DataSource = dv;
				}
			}
		}


		internal override IList<IPerson> GetSelectedPersons()
		{
			IList<IPerson> selectedPersons = new List<IPerson>();

			var gridRangeInfoList = Grid.Model.SelectedRanges;
			for (int index = gridRangeInfoList.Count; index > 0; index--)
			{
				GridRangeInfo gridRangeInfo = gridRangeInfoList[index - 1];
				if (gridRangeInfo.Height == 1)
				{
					selectedPersons.Add(_parentAdapterCollection[gridRangeInfo.Top - 1].Person);
				}
				else
				{
					for (int row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
					{
						if (row > 0)
						{
							selectedPersons.Add(
								_parentAdapterCollection[row - 1].Person);
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

			var ranges = new List<GridRangeInfo>();
			foreach (var person in selectedPersons)
			{
				for (var i = 0; i < _parentAdapterCollection.Count; i++)
				{
					if (_parentAdapterCollection[i].Person.Id == person.Id)
					{
						ranges.Add(GridRangeInfo.Row(i + 1));
					}
				}
			}

            ranges.ForEach(Grid.Selections.Add);
		}

		private void ChildGridCopyHelper(int index, IList<TAdapterChild> adapterChildCollection)
		{
			var baseType = adapterChildCollection[index].PersonRotation;

			if (_viewType == ViewType.PersonAvailabilityView)
			{
				var personAvailability = baseType as IPersonAvailability;

				if ((personAvailability != null) &&
					(!_selectedPersonAvailabilityCollection.Contains(personAvailability)))
					_selectedPersonAvailabilityCollection.Add(personAvailability);
			}
			else
			{
				var personRotation = baseType as IPersonRotation;

				if ((personRotation != null) && (!_selectedPersonRotationCollection.Contains(personRotation)))
					_selectedPersonRotationCollection.Add(personRotation);
			}
		}
	}
}