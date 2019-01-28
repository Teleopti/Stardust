using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public class GeneralGridView : GridViewBase
	{
		internal override ViewType Type => ViewType.GeneralView;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_addNewPersonMenuItem?.Dispose();
				_addNewPersonFromClipboardMenuItem?.Dispose();
				_deleteSelectedPeopleMenuItem?.Dispose();
				_currentSelectedPersons = null;
			}
			_workflowControlSetColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_firstDayOfWeekColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_terminationColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_languageColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_uiCultureColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_timeZoneColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_identityUserColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_applicationUserColumn.CellDisplayChanged -= applicationUsercolumnCellDisplayChanged;
			_applicationPasswordColumn.CellDisplayChanged -= applicationPasswordColumnCellDisplayChanged;
			_roleColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_firstNameColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_lastNameColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_emailColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_employeeColumn.CellDisplayChanged -= columnCellDisplayChanged;
			_noteColumn.CellDisplayChanged -= columnCellDisplayChanged;

			_workflowControlSetColumn = null;
			_firstDayOfWeekColumn = null;
			_terminationColumn = null;
			_languageColumn = null;
			_uiCultureColumn = null;
			_timeZoneColumn = null;
			_identityUserColumn = null;
			_applicationUserColumn = null;
			_applicationPasswordColumn = null;
			_roleColumn = null;
			_firstNameColumn = null;
			_lastNameColumn = null;
			_emailColumn = null;
			_employeeColumn = null;
			_noteColumn = null;

			_gridColumns.Clear();

			base.Dispose(disposing);
		}

		private readonly List<ColumnBase<PersonGeneralModel>> _gridColumns = new List<ColumnBase<PersonGeneralModel>>();
		private bool _hasRights;

		private ColumnBase<PersonGeneralModel> _firstNameColumn;
		private ColumnBase<PersonGeneralModel> _lastNameColumn;
		private ColumnBase<PersonGeneralModel> _emailColumn;
		private ColumnBase<PersonGeneralModel> _employeeColumn;
		private ColumnBase<PersonGeneralModel> _noteColumn;
		private ColumnBase<PersonGeneralModel> _terminationColumn;
		private ColumnBase<PersonGeneralModel> _languageColumn;
		private ColumnBase<PersonGeneralModel> _uiCultureColumn;
		private ColumnBase<PersonGeneralModel> _timeZoneColumn;
		private ColumnBase<PersonGeneralModel> _identityUserColumn;
		private ColumnBase<PersonGeneralModel> _applicationUserColumn;
		private ColumnBase<PersonGeneralModel> _applicationPasswordColumn;
		private ColumnBase<PersonGeneralModel> _roleColumn;
		private ColumnBase<PersonGeneralModel> _workflowControlSetColumn;
		private ColumnBase<PersonGeneralModel> _firstDayOfWeekColumn;

		private ToolStripMenuItem _addNewPersonMenuItem;
		private ToolStripMenuItem _addNewPersonFromClipboardMenuItem;
		private ToolStripMenuItem _deleteSelectedPeopleMenuItem;
		private IList<IPerson> _currentSelectedPersons;


		public GeneralGridView(GridControl grid, FilteredPeopleHolder filteredPeopleHolder)
			: base(grid, filteredPeopleHolder)
		{
			var cellModel = new GridDropDownMonthCalendarAdvCellModel(grid.Model);
			cellModel.HideNoneButton();
			cellModel.HideTodayButton();
			if (!grid.CellModels.ContainsKey(GridCellModelConstants.CellTypeDatePickerCell))
				grid.CellModels.Add(GridCellModelConstants.CellTypeDatePickerCell, cellModel);
			if (!grid.CellModels.ContainsKey(GridCellModelConstants.CellTypeDropDownCultureCell))
				grid.CellModels.Add(GridCellModelConstants.CellTypeDropDownCultureCell, new DropDownCultureCellModel(grid.Model));
			if (!grid.CellModels.ContainsKey(GridCellModelConstants.CellTypeDropDownCellModel))
				grid.CellModels.Add(GridCellModelConstants.CellTypeDropDownCellModel, new DropDownCellModel(grid.Model));
		}

		public override IEnumerable<Tuple<IPerson, int>> Sort(bool isAscending)
		{
			// Sorts the people data
			var result = sortPeopleData(isAscending);

			
			return result;
		}

		public override void PerformSort(IEnumerable<Tuple<IPerson, int>> order)
		{
			if (!(order?.Any() ?? false)) return;

			var result = from x in FilteredPeopleHolder.FilteredPeopleGridData
				join y in order
					on x.ContainedEntity equals y.Item1
					into a
				from b in a.DefaultIfEmpty(new Tuple<IPerson, int>(null, int.MaxValue))
				orderby b.Item2
				select x;

					 // Sets the filtered list
			FilteredPeopleHolder.SetSortedPeopleFilteredList(result.ToList());

			// refresh the grid view to get affect the sorted data
			Invalidate();
		}

		private int getColumnIndex()
		{
			return Math.Max(Grid.CurrentCell.ColIndex, 0); // this is to avoid -1, that would cause chrash
		}

		private IEnumerable<Tuple<IPerson, int>> sortPeopleData(bool isAscending)
		{
			// Gets the filtered people grid data as a collection
			var personcollection = FilteredPeopleHolder.FilteredPeopleGridData.ToList();

			int columnIndex = getColumnIndex();

			// Gets the sort column to sort
			string sortColumn = _gridColumns[columnIndex].BindingProperty;
			// Gets the coparer erquired to sort the data
			IComparer<PersonGeneralModel> comparer = _gridColumns[columnIndex].ColumnComparer;

			if (string.IsNullOrEmpty(sortColumn))
			{
				return Enumerable.Empty<Tuple<IPerson, int>>();
			}

// Holds the results of the sorting process
			IList<PersonGeneralModel> result;

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
					new Collection<PersonGeneralModel>(personcollection),
					sortColumn, isAscending);
			}

			return result.Select((t, i) => new Tuple<IPerson, int>(t.ContainedEntity, i));
		}
		
		internal override void CreateHeaders()
		{
			_hasRights = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword);
			_gridColumns.Add(new RowHeaderColumn<PersonGeneralModel>());

			createUserInfoHeaders();
			createPermissionInformationHeaders();
			createLanguageInfoHeaders();

			_firstDayOfWeekColumn = new DropDownColumn<PersonGeneralModel, DayOfWeekDisplay>("FirstDayOfWeek", Resources.WorkWeekStartsAt,
									DayOfWeekDisplay.ListOfDayOfWeek, "DisplayName");
			_firstDayOfWeekColumn.CellDisplayChanged += columnCellDisplayChanged;
			_firstDayOfWeekColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_firstDayOfWeekColumn);

			_workflowControlSetColumn = new DropDownColumn<PersonGeneralModel, IWorkflowControlSet>("WorkflowControlSet", Resources.WorkflowControlSet,
									PeopleWorksheet.StateHolder.WorkflowControlSetCollection, "Name");
			_workflowControlSetColumn.CellDisplayChanged += columnCellDisplayChanged;
			_workflowControlSetColumn.CellChanged += columnCellChanged;
			// Sets the comparer for the sorting
			_workflowControlSetColumn.ColumnComparer = new WorkflowControlSetComparer();
			_gridColumns.Add(_workflowControlSetColumn);

			//add optional columns.
			populateOptionalColumns();

			_terminationColumn = new NullableDateOnlyColumn<PersonGeneralModel>("TerminalDate", Resources.TerminalDate);
			_terminationColumn.CellDisplayChanged += columnCellDisplayChanged;
			_terminationColumn.CellChanged += columnCellChanged;
			// Sets the comparer for the sorting
			_terminationColumn.ColumnComparer = new PeopleAdminTerminalDateComparer();
			_gridColumns.Add(_terminationColumn);
		}

		private void createLanguageInfoHeaders()
		{
			_languageColumn = new DropDownCultureColumn<PersonGeneralModel, Culture>("LanguageInfo", Resources.Language,
																					  PeopleWorksheet.StateHolder.CultureCollection, "DisplayName");
			_languageColumn.CellDisplayChanged += columnCellDisplayChanged;
			_languageColumn.CellChanged += columnCellChanged;
			// Sets the comparer for the sorting
			_languageColumn.ColumnComparer = new PeopleAdminLanguageComparer();
			_gridColumns.Add(_languageColumn);

			_uiCultureColumn = new DropDownCultureColumn<PersonGeneralModel, Culture>("CultureInfo", Resources.Format,
																						PeopleWorksheet.StateHolder.UiCultureCollection, "DisplayName");
			_uiCultureColumn.CellDisplayChanged += columnCellDisplayChanged;
			_uiCultureColumn.CellChanged += columnCellChanged;
			// Sets the comparer for the sorting
			_uiCultureColumn.ColumnComparer = new PeopleAdminCultureComparer();
			_gridColumns.Add(_uiCultureColumn);

			_timeZoneColumn = new DropDownColumn<PersonGeneralModel, TimeZoneInfo>("TimeZoneInformation", Resources.TimeZone,
																					TimeZoneInfo.GetSystemTimeZones(), "DisplayName");

			_timeZoneColumn.CellDisplayChanged += columnCellDisplayChanged;
			_timeZoneColumn.CellChanged += columnCellChanged;
			// Sets the comparer for the sorting
			_timeZoneColumn.ColumnComparer = new PeopleAdminTimeZoneInfoComparer();
			_gridColumns.Add(_timeZoneColumn);
		}

		private void createPermissionInformationHeaders()
		{
			if (_hasRights)
				_identityUserColumn = new EditableTextColumn<PersonGeneralModel>("LogOnName", 100, Resources.LogOn);
			else
				_identityUserColumn = new ReadOnlyTextColumn<PersonGeneralModel>("LogOnName", Resources.LogOn);
			_identityUserColumn.CellDisplayChanged += columnCellDisplayChanged;
			_identityUserColumn.CellChanged += logOnNameColumnCellChanged;
			_gridColumns.Add(_identityUserColumn);

			if (_hasRights)
				_applicationUserColumn = new EditableTextColumn<PersonGeneralModel>("ApplicationLogOnName", 50, Resources.ApplicationLogInName);
			else
				_applicationUserColumn = new ReadOnlyTextColumn<PersonGeneralModel>("ApplicationLogOnName", Resources.ApplicationLogInName);
			_applicationUserColumn.CellDisplayChanged += applicationUsercolumnCellDisplayChanged;
			_applicationUserColumn.CellChanged += applicationLogOnNameColumnCellChanged;
			_gridColumns.Add(_applicationUserColumn);

			_applicationPasswordColumn = new EditablePasswordColumn<PersonGeneralModel>("Password", 20, Resources.Password, !_hasRights);
			_applicationPasswordColumn.CellDisplayChanged += applicationPasswordColumnCellDisplayChanged;
			_applicationPasswordColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_applicationPasswordColumn);

			if (_hasRights)
			{
				var column = new ReadOnlyCollectionColumn<PersonGeneralModel>("Roles", Resources.Roles, 200);
				_roleColumn = column;
			}
			else
			{
				var column = new ReadOnlyTextColumn<PersonGeneralModel>("Roles", Resources.Roles, 200);
				_roleColumn = column;
			}

			_roleColumn.ColumnComparer = new PeopleAdminApplicationRoleComparer();
			_roleColumn.CellDisplayChanged += columnCellDisplayChanged;
			_roleColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_roleColumn);
		}

		private void createUserInfoHeaders()
		{
			_firstNameColumn = new EditableNotEmptyTextColumn<PersonGeneralModel>("FirstName", 25, Resources.FirstName, true);
			_firstNameColumn.CellDisplayChanged += columnCellDisplayChanged;
			_firstNameColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_firstNameColumn);

			_lastNameColumn = new EditableNotEmptyTextColumn<PersonGeneralModel>("LastName", 25, Resources.LastName, true);
			_lastNameColumn.CellDisplayChanged += columnCellDisplayChanged;
			_lastNameColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_lastNameColumn);

			_emailColumn = new EditableEmailColumn<PersonGeneralModel>("Email", 50, Resources.Email);
			_emailColumn.CellDisplayChanged += columnCellDisplayChanged;
			_emailColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_emailColumn);

			_employeeColumn = new EditableTextColumn<PersonGeneralModel>("EmployeeNumber", 50, Resources.EmployeeNo);
			_employeeColumn.CellDisplayChanged += columnCellDisplayChanged;
			_employeeColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_employeeColumn);

			_noteColumn = new EditableTextColumn<PersonGeneralModel>("Note", 1024, Resources.Note);
			_noteColumn.CellDisplayChanged += columnCellDisplayChanged;
			_noteColumn.CellChanged += columnCellChanged;
			_gridColumns.Add(_noteColumn);
		}

		private void applicationPasswordColumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonGeneralModel> e)
		{
			if (e.DataItem.CanBold)
			{
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
			}
			if (!e.DataItem.IsValid)
			{
				e.QueryCellInfoEventArg.Style.BackColor = Color.Red;
				e.QueryCellInfoEventArg.Style.CellTipText = Resources.PasswordPolicyWarning;
				if (!FilteredPeopleHolder.ValidatePasswordPolicy.Contains(e.DataItem))
					FilteredPeopleHolder.ValidatePasswordPolicy.Add(e.DataItem);
			}
			else
			{
				e.QueryCellInfoEventArg.Style.BackColor = Color.White;
				e.QueryCellInfoEventArg.Style.CellTipText = "";
				FilteredPeopleHolder.ValidatePasswordPolicy.Remove(e.DataItem);
			}
		}

		private static void columnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonGeneralModel> e)
		{
			if (e.DataItem.CanBold)
			{
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
			}
		}

		private static void applicationUsercolumnCellDisplayChanged(object sender, ColumnCellDisplayChangedEventArgs<PersonGeneralModel> e)
		{
			if (e.DataItem.CanBold)
			{
				e.QueryCellInfoEventArg.Style.Font.Bold = true;
			}

			if (!e.DataItem.logonDataCanBeChanged())
			{
				e.QueryCellInfoEventArg.Style.ReadOnly = true;
				e.QueryCellInfoEventArg.Style.BackColor = Color.Silver;
			}
			else
			{
				e.QueryCellInfoEventArg.Style.ReadOnly = false;
				e.QueryCellInfoEventArg.Style.ResetBackColor();
			}
		}

		private void columnCellChanged(object sender, ColumnCellChangedEventArgs<PersonGeneralModel> e)
		{
			e.DataItem.CanBold = true;
			refreshCell(e.SaveCellInfoEventArgs.RowIndex);
		}

		private void applicationLogOnNameColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonGeneralModel> e)
		{
			IPerson changedPerson = e.DataItem.ContainedEntity;
			if (!FilteredPeopleHolder.ValidateUserCredentialsCollection.Contains(changedPerson))
				FilteredPeopleHolder.ValidateUserCredentialsCollection.Add(changedPerson);

			e.DataItem.CanBold = true;
			refreshCell(e.SaveCellInfoEventArgs.RowIndex);
		}

		private void logOnNameColumnCellChanged(object sender, ColumnCellChangedEventArgs<PersonGeneralModel> e)
		{
			IPerson changedPerson = e.DataItem.ContainedEntity;
			if (!FilteredPeopleHolder.ValidateUserCredentialsCollection.Contains(changedPerson))
				FilteredPeopleHolder.ValidateUserCredentialsCollection.Add(changedPerson);

			if (e.SaveCellInfoEventArgs.Style.ReadOnly) e.DataItem.LogOnName = string.Empty;
			else
			{
				e.DataItem.CanBold = true;
				refreshCell(e.SaveCellInfoEventArgs.RowIndex);
			}
		}

		private void populateOptionalColumns()
		{
			ConfigureGridForDynamicColumns();
		}

		private void refreshCell(int row)
		{
			Grid.RefreshRange(GridRangeInfo.Cells(row, 1, row, _gridColumns.Count));
		}

		private int gridFirstColumn()
		{
			var found = false;
			foreach (var column in _gridColumns)
			{
				if (found)
				{
					if (column.GetType() != typeof(RowHeaderColumn<PersonGeneralModel>))
					{
						return _gridColumns.IndexOf(column);
					}
				}

				if (column.GetType() == typeof(RowHeaderColumn<PersonGeneralModel>))
					found = true;
			}

			return 0;
		}

		internal override void CreateContextMenu()
		{
			Grid.ContextMenuStrip = new ContextMenuStrip();

			var addPersonPermission =
				PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPerson);
			var deletePersonPermission =
				PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.DeletePerson);
			_addNewPersonMenuItem = new ToolStripMenuItem(Resources.New);
			if(addPersonPermission)
				_addNewPersonMenuItem.Click += AddNewGridRow;
			_addNewPersonMenuItem.Enabled = addPersonPermission;
			Grid.ContextMenuStrip.Items.Add(_addNewPersonMenuItem);

			_addNewPersonFromClipboardMenuItem = new ToolStripMenuItem(Resources.PasteNew);
			if (addPersonPermission)
				_addNewPersonFromClipboardMenuItem.Click += AddNewGridRowFromClipboard;
			_addNewPersonFromClipboardMenuItem.Enabled = addPersonPermission;
			Grid.ContextMenuStrip.Items.Add(_addNewPersonFromClipboardMenuItem);

			_deleteSelectedPeopleMenuItem = new ToolStripMenuItem(Resources.Delete);
			if(deletePersonPermission)
				_deleteSelectedPeopleMenuItem.Click += DeleteSelectedGridRows;
			_deleteSelectedPeopleMenuItem.Enabled = deletePersonPermission;
			//_deleteSelectedPeopleMenuItem.ShortcutKeys = Keys.Delete;
			Grid.ContextMenuStrip.Items.Add(_deleteSelectedPeopleMenuItem);

		}

		internal override void PrepareView()
		{
			//FilteredPeopleHolder.ReassociateOptionalColumnCollection();
			ColCount = _gridColumns.Count;
			RowCount = FilteredPeopleHolder.FilteredPeopleGridData.Count;

			Grid.ColCount = ColCount - 1;
			Grid.RowCount = RowCount;

			Grid.Cols.HeaderCount = 0;
			Grid.Rows.HeaderCount = 0;

			Grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			//Set note filed width to PreferredWidth
			Grid.ColWidths[0] = _gridColumns[0].PreferredWidth;
			Grid.ColWidths[5] = _gridColumns[5].PreferredWidth + 10;
			Grid.ColWidths[13] = _gridColumns[13].PreferredWidth;

			Grid.Name = "GeneralView";
		}


		public override void Invalidate()
		{
			Grid.Invalidate();
		}

		internal override void SelectionChanged(GridSelectionChangedEventArgs e, bool eventCancel)
		{
			int rangeLength = Grid.Model.SelectedRanges.Count;
			if (rangeLength == 0) { eventCancel = true; return; }

			IList<IPerson> list = new List<IPerson>();
			IList<PersonGeneralModel> gridDataList = new List<PersonGeneralModel>();
			for (var rangeIndex = 0; rangeIndex < rangeLength; rangeIndex++)
			{
				var rangeInfo = Grid.Model.SelectedRanges[rangeIndex];

				var top = 1; // This is to skip if Range is Empty.
				var length = 0;

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

				if (top > 0)
					for (var index = top - 1; index < length; index++)
					{
						gridDataList.Add(FilteredPeopleHolder.FilteredPeopleGridData[index]);
						list.Add(FilteredPeopleHolder.FilteredPeopleGridData[index].ContainedEntity);

					}
			}
			_currentSelectedPersons = list;
			FilteredPeopleHolder.SetRoleGridViewDataByPersons(new ReadOnlyCollection<IPerson>(list),
				new ReadOnlyCollection<PersonGeneralModel>(gridDataList));
		}

		internal override void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].SaveCellInfo(e, new ReadOnlyCollection<PersonGeneralModel>(FilteredPeopleHolder.FilteredPeopleGridData));
			}
		}

		internal override void QueryCellInfo(GridQueryCellInfoEventArgs e)
		{
			if (ValidCell(e.ColIndex, e.RowIndex))
			{
				_gridColumns[e.ColIndex].GetCellInfo(e, new ReadOnlyCollection<PersonGeneralModel>(FilteredPeopleHolder.FilteredPeopleGridData));
			}
			base.QueryCellInfo(e);
		}

		private void addPerson()
		{
			var colIndex = (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.ColIndex;
			Grid.CurrentCell.MoveTo(Grid.RowCount, colIndex, GridSetCurrentCellOptions.SetFocus);

			insertPerson();
		}

		private void insertPerson()
		{
			var rowIndex = (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.RowIndex;
			var colIndex = (Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.ColIndex;
			if (colIndex == 0) colIndex = 1;

			PeopleWorksheet.StateHolder.AddAndSavePerson(rowIndex, FilteredPeopleHolder);
			RowCount = FilteredPeopleHolder.FilteredPeopleGridData.Count;
			Grid.RowCount = RowCount;
			Grid.CurrentCell.MoveTo(rowIndex + 1, colIndex, GridSetCurrentCellOptions.NoSetFocus);
			Grid.Invalidate();
		}

		private void addFromClipBoard()
		{
			var colIndex = (Grid.Model.CurrentCellInfo != null) ? Grid.Model.CurrentCellInfo.ColIndex : 1;

			Grid.CurrentCell.MoveTo(Grid.RowCount, colIndex, GridSetCurrentCellOptions.SetFocus);
			InsertClipHandlerToGrid();
		}

		private void deleteWhenAllSelected()
		{
			for (var i = Grid.RowCount; i >= 1; i--)
			{
				PeopleWorksheet.StateHolder.DeletePersonByIndex(i, FilteredPeopleHolder);
			}

			PeopleWorksheet.StateHolder.TobeDeleteFromGridDataAfterRomove(FilteredPeopleHolder);
		}

		protected override void InsertClipHandlerToGrid()
		{
			PeopleWorksheet.StateHolder.InsertFromClipHandler((Grid.Model.CurrentCellInfo == null) ? 1 : Grid.Model.CurrentCellInfo.RowIndex,
				GridClipHandler, FilteredPeopleHolder);

			var colIndex = (Grid.Model.CurrentCellInfo != null) ? Grid.Model.CurrentCellInfo.ColIndex : 1;
			var rowIndex = (Grid.Model.CurrentCellInfo != null) ? Grid.Model.CurrentCellInfo.RowIndex : 1;

			RowCount = FilteredPeopleHolder.FilteredPeopleGridData.Count;
			Grid.RowCount = RowCount;

			var currentColIndex = colIndex;
			if (currentColIndex < gridFirstColumn())
				currentColIndex = gridFirstColumn();
			Grid.Selections.Clear();
			Grid.CurrentCell.MoveTo(rowIndex + 1, currentColIndex, GridSetCurrentCellOptions.SetFocus);

			var pasteAction = new SimpleTextPasteAction();
			GridHelper.HandlePaste(Grid, GridClipHandler, pasteAction);
			Grid.Invalidate();

		}

		private void deletePersons()
		{
		    if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.DeletePerson))
		        return;

			if (Grid.Model.SelectedRanges.Count <= 0) return;

			var result = ViewBase.ShowConfirmationMessage(Resources.AreYouSureYouWantToDelete, Resources.AreYouSureYouWantToDelete);
			if (result != DialogResult.Yes) return;

			var gridRangeInfos = revertIfNecessary(Grid.Model.SelectedRanges);
			foreach (var gridRangeInfo in gridRangeInfos)
			{
				if (gridRangeInfo.IsTable)
				{
					deleteWhenAllSelected();
				}
				else
				{
					if (gridRangeInfo.Height == 1)
					{
						PeopleWorksheet.StateHolder.DeletePersonByIndex(gridRangeInfo.Top, FilteredPeopleHolder);
						PeopleWorksheet.StateHolder.TobeDeleteFromGridDataAfterRomove(FilteredPeopleHolder);
					}
					else
					{
						for (var row = gridRangeInfo.Bottom; row >= gridRangeInfo.Top; row--)
						{
							PeopleWorksheet.StateHolder.DeletePersonByIndex(row, FilteredPeopleHolder);
						}
						PeopleWorksheet.StateHolder.TobeDeleteFromGridDataAfterRomove(FilteredPeopleHolder);
					}
				}
			}

			RowCount = FilteredPeopleHolder.FilteredPeopleGridData.Count;
			Grid.RowCount = RowCount;
			Grid.Invalidate();
		}

		private static IEnumerable<GridRangeInfo> revertIfNecessary(IEnumerable gridRangeInfoList)
		{
			var gridRangeInfos = gridRangeInfoList.Cast<GridRangeInfo>().ToList();

			return (from info in gridRangeInfos orderby info.Top descending select info).ToList();
		}

		internal override void AddNewGridRow<T>(object sender, T eventArgs)
		{
			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPerson))
				return;
			addPerson();
		}

		internal override void AddNewGridRowFromClipboard<T>(object sender, T eventArgs)
		{
			addFromClipBoard();
		}

		internal override void DeleteSelectedGridRows<T>(object sender, T eventArgs)
		{
			deletePersons();
		}

		internal override IList<IPerson> GetSelectedPersons()
		{
			return _currentSelectedPersons ?? new List<IPerson>();
		}

		internal override void SetSelectedPersons(IList<IPerson> selectedPersons)
		{
			// Selection events will not be raised
			Grid.Selections.Clear(false);

			var range = GridRangeInfo.Empty;
			foreach (var person in selectedPersons)
			{
				for (var i = 0; i < FilteredPeopleHolder.FilteredPeopleGridData.Count; i++)
				{
					if (FilteredPeopleHolder.FilteredPeopleGridData[i].Id == person.Id)
					{
						range = range.UnionRange(GridRangeInfo.Row(i + 1));
					}
				}
			}
			Grid.Selections.Add(range);
		}

		public void ConfigureGridForDynamicColumns()
		{
			const int optionalValueLengthLimit = 255;
			var optionalColumnCollection = FilteredPeopleHolder.OptionalColumnCollection;

			foreach (var t in optionalColumnCollection)
			{
				var optionalColumn = new OptionalColumn<PersonGeneralModel>(t.Name, optionalValueLengthLimit, t.Name);
				optionalColumn.CellChanged += columnCellChanged;
				optionalColumn.CellDisplayChanged += columnCellDisplayChanged;
				_gridColumns.Add(optionalColumn);
			}
		}

		public void ResetChangeLogonDataCheck()
		{
			foreach (var model in FilteredPeopleHolder.FilteredPeopleGridData)
			{
				model.ResetLogonDataCheck();
			}
		}
	}
}
