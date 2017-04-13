using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common.Controls
{
	public partial class FindAndReplaceForm : BaseDialogForm
	{
		private bool _isOptionsVisible;
		private GridFindReplaceDialogSink _findAndReplaceDialog;
		GridFindReplaceEventArgs _findReplaceEventArguments;
		private GridControl _grid;
		private int _rowCount;
		private IDomainFinder _domainFinder;

		public string SearchText
		{
			get
			{
				if (tabControlAdvMain.SelectedIndex == 0)
				{
					return comboDropDownFindSearchText.Text;
				}
				if (tabControlAdvMain.SelectedIndex == 1)
				{
					return comboDropDownReplaceSearchText.Text;
				}

				return string.Empty;
			}
		}

		public string ReplaceText
		{
			get { return comboDropDownReplaceReplaceText.Text; }
		}

		public bool IsCaseSensitive
		{
			get
			{
				if (tabControlAdvMain.SelectedIndex == 0)
				{
					return checkBoxAdvFindMatchCase.Checked;
				}
				if (tabControlAdvMain.SelectedIndex == 1)
				{
					return checkBoxAdvReplaceMatchCase.Checked;
				}

				return false;
			}
		}

		private void searchFormLoad(object sender, EventArgs e)
		{
			comboBoxAdvFindSearchWithin.Items.Add(UserTexts.Resources.WholeTable);
			comboBoxAdvFindSearchWithin.Items.Add(UserTexts.Resources.ColumnOnly);
			// Sets the selected index
			comboBoxAdvFindSearchWithin.SelectedIndex = 0;

			// Hides the replace funcatinalities
			buttonAdvReplace.Visible = false;
			buttonAdvReplaceAll.Visible = false;

			// Hides find options panels
			panelFindOptions.Visible = false;
			panelReplaceOptions.Visible = false;

			// Sets the text of the options button
			setOptionButtonCaption();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int wmKeydown = 0x100;
			const int wmSyskeydown = 0x104;

			if ((msg.Msg == wmKeydown) || (msg.Msg == wmSyskeydown))
			{
				switch (keyData)
				{
					case Keys.Escape:
						Hide();
						break;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void findAndReplaceFormFormClosing(object sender, FormClosingEventArgs e)
		{
			// Hides the search results table 
			GridRangeInfo cellRange = GridRangeInfo.Cells(1, 0, _grid.RowCount, _grid.ColCount);
			gridListControlSearchResults.ClearCells(cellRange, true);

			// Cancel the event
			e.Cancel = true;
			// Hides the form
			Hide();
		}

		private void buttonAdvFindOptionsClick(object sender, EventArgs e)
		{
			// Sets the isvisibility property of the window
			_isOptionsVisible = (!_isOptionsVisible);
			// Shows the option panel
			panelFindOptions.Visible = _isOptionsVisible;

			// Sets the text of the options button
			setOptionButtonCaption();
		}

		private void buttonAdvReplaceOptionsClick(object sender, EventArgs e)
		{
			// Sets the isvisibility property of the window
			_isOptionsVisible = (!_isOptionsVisible);
			// Shows the option panel
			panelReplaceOptions.Visible = _isOptionsVisible;

			// Sets the text of the options button
			setOptionButtonCaption();
		}

		private void buttonAdvCloseClick(object sender, EventArgs e)
		{
			// Hides the search results table 
			GridRangeInfo cellRange = GridRangeInfo.Cells(1, 0, _grid.RowCount, _grid.ColCount);
			gridListControlSearchResults.ClearCells(cellRange, true);
			// Hides the search dialog box
			Hide();
		}

		private void buttonAdvFindNextClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (!string.IsNullOrEmpty(SearchText))
			{
				// Clear old selection
				_grid.Selections.Clear();
				// Gets the location information
				object locationInfo = GridRangeInfo.Table();

				// Creates the event arguments
				_findReplaceEventArguments = new GridFindReplaceEventArgs(
					SearchText,
					"",
					getSearchOptions(),
					locationInfo);

				// Adds the search texts to the search text list
				addToSearchedList(SearchText);
				// Finds the given text in the grid
				_findAndReplaceDialog.Find(_findReplaceEventArguments);
			}

			Cursor.Current = Cursors.Default;
		}


		private void buttonAdvFindAllClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			setDialogSize();

			showSearchResultsGrid();

			findDataWithinGrid();

			if ((checkBoxAdvFindSearchInPersistance.Checked) &&
				(checkBoxAdvReplaceSearchInPersistance.Checked))
			{
				// Calls the domain finder to search the data inthe domain
				_domainFinder.Find(gridListControlSearchResults, getSearchCriteria());
			}

			Cursor.Current = Cursors.Default;
		}

		private void showSearchResultsGrid()
		{
			gridListControlSearchResults.Visible = true;
		}

		private void setDialogSize()
		{
			const int minWidth = 460;
			const int minHeigth = 467;

			var letWidth = Math.Max(minWidth, Width);
			var letHeight = Math.Max(minHeigth, Height);

			Size = new Size(letWidth, letHeight);
		}

		private void buttonAdvReplaceClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			if ((!string.IsNullOrEmpty(SearchText)) && (!string.IsNullOrEmpty(ReplaceText)))
			{
				object locationInfo = GridRangeInfo.Table();

				_findReplaceEventArguments = new GridFindReplaceEventArgs(
						SearchText,
						ReplaceText,
						getSearchOptions(),
						locationInfo);

				addToSearchedList(SearchText);
				addToReplacesList(ReplaceText);
				_findAndReplaceDialog.Replace(_findReplaceEventArguments);
			}

			Cursor.Current = Cursors.Default;
		}

		private void buttonAdvReplaceAllClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			if ((!string.IsNullOrEmpty(SearchText)) && (!string.IsNullOrEmpty(ReplaceText)))
			{
				// Gets the location information
				object locationInfo = GridRangeInfo.Table();

				// Set event arguments for finds the search texts
				_findReplaceEventArguments = new GridFindReplaceEventArgs(SearchText, ReplaceText, getSearchOptions(),
																		 locationInfo);

				// Adds the search texts to the search text list
				addToSearchedList(SearchText);
				// Adds the replace texts to the replaced text list                
				addToReplacesList(ReplaceText);

				// Replace the text
				_findAndReplaceDialog.ReplaceAll(_findReplaceEventArguments);
			}

			Cursor.Current = Cursors.Default;
		}

		private void gridListControlSearchResultsCellClick(object sender, GridCellClickEventArgs e)
		{
			if (e.RowIndex != 0)
			{
				if ((!string.IsNullOrEmpty(gridListControlSearchResults[e.RowIndex, 2].Text)) &&
					(!string.IsNullOrEmpty(gridListControlSearchResults[e.RowIndex, 3].Text)))
				{
					// Gets row index
					int rowIndex = Convert.ToInt32(gridListControlSearchResults[e.RowIndex, 2].Text,
												   CultureInfo.CurrentCulture);
					// Gets the column index
					int colIndex = Convert.ToInt32(gridListControlSearchResults[e.RowIndex, 3].Text,
												   CultureInfo.CurrentCulture);

					// Clears the earlier selection
					_grid.Selections.Clear();
					// Selects the relevant cell
					_grid.Selections.SelectRange(GridRangeInfo.Cell(rowIndex, colIndex), true);
				}
			}
		}

		private void tabControlAdvMainSelectedIndexChanging(object sender, SelectedIndexChangingEventArgs args)
		{
			if (args.NewSelectedIndex == 0)
			{
				if (!string.IsNullOrEmpty(comboDropDownFindSearchText.Text))
				{
					comboDropDownFindSearchText.Text = comboDropDownReplaceSearchText.Text;
				}

				// Hides the replace funcatinalities
				buttonAdvReplace.Visible = false;
				buttonAdvReplaceAll.Visible = false;

				// Shows the option panel
				panelFindOptions.Visible = _isOptionsVisible;
			}
			else if (args.NewSelectedIndex == 1)
			{
				if (!string.IsNullOrEmpty(comboDropDownFindSearchText.Text))
				{
					comboDropDownReplaceSearchText.Text = comboDropDownFindSearchText.Text;
				}

				// Hides the replace funcatinalities
				buttonAdvReplace.Visible = true;
				buttonAdvReplaceAll.Visible = true;

				// Shows the option panel
				panelReplaceOptions.Visible = _isOptionsVisible;
			}

			// Sets the text of the options button
			setOptionButtonCaption();
		}

		private void checkBoxAdvFindSearchInPersistanceCheckStateChanged(object sender, EventArgs e)
		{
			if (checkBoxAdvFindSearchInPersistance.Checked)
			{
				// Disable the search up functionality
				checkBoxAdvFindSearchUp.Checked = false;
				checkBoxAdvFindSearchUp.Enabled = false;

				// Disable the match whole cell funcationality
				checkBoxAdvFindMatchWholeCell.Checked = false;
				checkBoxAdvFindMatchWholeCell.Enabled = false;

				// Disable the search withing funcationality
				comboBoxAdvFindSearchWithin.SelectedIndex = -1;
				comboBoxAdvFindSearchWithin.Enabled = false;

				// Enable the search in functionality in the repace tab
				checkBoxAdvReplaceSearchInPersistance.Checked = true;
			}
			else
			{
				// Enable the sesrch up and match whole cell funcationality
				checkBoxAdvFindSearchUp.Enabled = true;
				checkBoxAdvFindMatchWholeCell.Enabled = true;

				// Enable the search withing funcationality
				comboBoxAdvFindSearchWithin.SelectedIndex = -1;
				comboBoxAdvFindSearchWithin.Enabled = true;

				// Disable the search in functionality in the repace tab
				checkBoxAdvReplaceSearchInPersistance.Checked = false;
			}
		}

		private void checkBoxAdvReplaceSearchInPersistanceCheckStateChanged(object sender, EventArgs e)
		{
			if (checkBoxAdvReplaceSearchInPersistance.Checked)
			{
				// Disable the search up functionality
				checkBoxAdvReplaceSearchUp.Checked = false;
				checkBoxAdvReplaceSearchUp.Enabled = false;

				// Disable the match whole cell funcationality
				checkBoxAdvReplaceMatchWholeCell.Checked = false;
				checkBoxAdvReplaceMatchWholeCell.Enabled = false;

				// Disable the search withing funcationality
				comboBoxAdvReplaceSearchWithin.Enabled = false;

				// Enable the search in functionality in the find tab
				checkBoxAdvFindSearchInPersistance.Checked = true;
			}
			else
			{
				// Enable the sesrch up and match whole cell funcationality
				checkBoxAdvReplaceSearchUp.Enabled = true;
				checkBoxAdvReplaceMatchWholeCell.Enabled = true;

				// Enable the search withing funcationality
				comboBoxAdvReplaceSearchWithin.Enabled = true;

				// Disable the search in functionality in the Find tab
				checkBoxAdvFindSearchInPersistance.Checked = false;
			}
		}

		public FindAndReplaceForm(FindAndReplaceFunctionality requiredFunctionality, FindOption findOption)
		{
			InitializeComponent();

			// Sets the text depending on the culture selected
			SetTexts();

			// Sets colors for form & controls.
			SetColors();

			switch (requiredFunctionality)
			{
				case FindAndReplaceFunctionality.None:
					tabPageAdvReplace.TabVisible = false;
					tabPageAdvFind.TabVisible = false;
					break;

				case FindAndReplaceFunctionality.All:

					break;

				case FindAndReplaceFunctionality.FindOnly:
					tabPageAdvReplace.Visible = false;
					tabPageAdvReplace.TabVisible = false;
					break;

				case FindAndReplaceFunctionality.ReplaceOnly:
					tabPageAdvFind.TabVisible = false;
					break;
			}

			switch (findOption)
			{
				case FindOption.None:
					break;

				case FindOption.All:
					break;

				case FindOption.WithinGridOnly:
					// Hides the search in persistance check box in the both Find and Repalce tabs
					checkBoxAdvFindSearchInPersistance.Visible = false;
					checkBoxAdvReplaceSearchInPersistance.Visible = false;
					break;

				case FindOption.WithinPersistenceOnly:
					// Sets the funcationlity of the search in persiatance check box of the find tab
					checkBoxAdvFindSearchInPersistance.Checked = true;
					checkBoxAdvFindSearchInPersistance.Enabled = false;

					// Sets the funcationlity of the search in persiatance check box of the replace tab
					checkBoxAdvReplaceSearchInPersistance.Checked = true;
					checkBoxAdvReplaceSearchInPersistance.Enabled = false;

					break;
			}

		}

		protected void SetColors()
		{
			BackColor = ColorHelper.FormBackgroundColor();
		}

		private GridFindTextOptions getSearchOptions()
		{
			var option = GridFindTextOptions.WholeTable;

			if (tabControlAdvMain.SelectedIndex == 0)
			{
				option = getFindOption(option);
			}
			else if (tabControlAdvMain.SelectedIndex == 1)
			{
				option = getReplaceOptions(option);
			}

			return option;
		}

		private GridFindTextOptions getFindOption(GridFindTextOptions option)
		{
			if (checkBoxAdvFindMatchCase.Checked)
				option = GridFindTextOptions.MatchCase;
			if (checkBoxAdvFindMatchWholeCell.Checked)
				option |= GridFindTextOptions.MatchWholeCell;
			if (checkBoxAdvFindSearchUp.Checked)
				option |= GridFindTextOptions.SearchUp;

			switch (comboBoxAdvFindSearchWithin.SelectedIndex)
			{
				case 0: option |= GridFindTextOptions.WholeTable; break;
				case 1: option |= GridFindTextOptions.ColumnOnly; break;
			}

			return option;
		}

		private GridFindTextOptions getReplaceOptions(GridFindTextOptions option)
		{
			if (checkBoxAdvReplaceMatchCase.Checked)
				option = GridFindTextOptions.MatchCase;
			if (checkBoxAdvReplaceMatchWholeCell.Checked)
				option |= GridFindTextOptions.MatchWholeCell;
			if (checkBoxAdvReplaceSearchUp.Checked)
				option |= GridFindTextOptions.SearchUp;

			switch (comboBoxAdvReplaceSearchWithin.SelectedIndex)
			{
				case 0: option |= GridFindTextOptions.WholeTable; break;
				case 1: option |= GridFindTextOptions.ColumnOnly; break;
			}

			return option;
		}

		private void addSearchEntry(string content, int row, int column)
		{
			// Increases the row count
			_rowCount++;

			// Sets the row count
			gridListControlSearchResults.RowCount = _rowCount;

			// Fills the search result table
			gridListControlSearchResults[_rowCount, 1].Text = content;
			gridListControlSearchResults[_rowCount, 2].Text = row.ToString(CultureInfo.CurrentCulture);
			gridListControlSearchResults[_rowCount, 3].Text = column.ToString(CultureInfo.CurrentCulture);

			gridListControlSearchResults[_rowCount, 1].ReadOnly = true;
			gridListControlSearchResults[_rowCount, 2].ReadOnly = true;
			gridListControlSearchResults[_rowCount, 3].ReadOnly = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Grid.GridStyleInfo.set_Text(System.String)")]
		private void configureSearchResultGrid()
		{
			// Configure the search result grid
			gridListControlSearchResults.RowCount = 1;
			gridListControlSearchResults.ColCount = 3;
			gridListControlSearchResults.AutoSize = true;

			gridListControlSearchResults[0, 1].Text = "Content";
			gridListControlSearchResults[0, 2].Text = "Row";
			gridListControlSearchResults[0, 3].Text = "Column";

			gridListControlSearchResults.ColWidths[1] = 150;
		}

		private void addToSearchedList(string txt)
		{
			if (!comboDropDownFindSearchText.Items.Contains(txt))
			{
				comboDropDownFindSearchText.Items.Add(txt);
			}

			if (!comboDropDownReplaceSearchText.Items.Contains(txt))
			{
				comboDropDownReplaceSearchText.Items.Add(txt);
			}
		}

		private void addToReplacesList(string txt)
		{
			if (!comboDropDownReplaceReplaceText.Items.Contains(txt))
			{
				comboDropDownReplaceReplaceText.Items.Add(txt);
			}
		}

		private void setOptionButtonCaption()
		{
			if (_isOptionsVisible)
			{
				buttonAdvFindOptions.Text = UserTexts.Resources.HideOptions;
				buttonAdvReplaceOptions.Text = UserTexts.Resources.HideOptions;
			}
			else
			{
				buttonAdvFindOptions.Text = UserTexts.Resources.ShowOptions;
				buttonAdvReplaceOptions.Text = UserTexts.Resources.ShowOptions;
			}
		}


		private void findDataWithinGrid()
		{

			configureSearchResultGrid();

			// Sets the row count to 0 to avoid the data repeatition
			_rowCount = 0;

			// Clear the cells
			gridListControlSearchResults.RowCount = 0;

			// Gets the search option
			GridFindTextOptions options = getSearchOptions();

			// Holds the Cell range
			GridRangeInfo cellRange = GridRangeInfo.Empty;

			if ((options & GridFindTextOptions.SelectionOnly) != GridFindTextOptions.None)
			{
				cellRange = _grid.Selections.Ranges.ActiveRange;
			}
			else if ((options & GridFindTextOptions.ColumnOnly) != GridFindTextOptions.None)
			{
				cellRange = GridRangeInfo.Col(_grid.CurrentCell.ColIndex);
			}
			else if ((options & GridFindTextOptions.WholeTable) != GridFindTextOptions.None)
			{
				cellRange = GridRangeInfo.Cells(0, 1, _grid.RowCount, _grid.ColCount);
			}

			int startTop = cellRange.Top;
			int startLeft = cellRange.Left;

			while (GridFindReplaceDialogSink.GetNextCell(cellRange, ref startTop, ref startLeft, false, false))
			{
				GridStyleInfo style = _grid[startTop, startLeft];
				GridCellRendererBase renderer = _grid.CellRenderers[style.CellType];

				if (renderer.FindText(SearchText, startTop, startLeft, options, true))
				{
					// Gets the row index and the column index
					int rowIndex;
					int colIndex;
					_grid.CurrentCell.GetCurrentCell(out rowIndex, out colIndex);

					if ((rowIndex != 0) && (colIndex != 0))
					{
						// Holds the cell content
						string cellContent = "Empty";

						if (!string.IsNullOrEmpty(_grid[rowIndex, colIndex].FormattedText))
						{
							// Reads the cell content
							cellContent = _grid[rowIndex, colIndex].FormattedText;
						}

						// Fills the search result table
						string content = String.Format(CultureInfo.CurrentCulture,
													   "{0} : ({1})",
													   cellContent,
													   _grid[0, colIndex].FormattedText);

						// Adds the search texts to the search text list
						addToSearchedList(SearchText);

						if (string.IsNullOrEmpty(SearchText))
						{
							if (string.IsNullOrEmpty(_grid[rowIndex, colIndex].Text))
							{
								// Fills the search result grid
								addSearchEntry(content, rowIndex, colIndex);
							}
						}
						else if ((!string.IsNullOrEmpty(SearchText)) && (!string.IsNullOrEmpty(_grid[rowIndex, colIndex].Text)))
						{
							// Fills the search result grid
							addSearchEntry(content, rowIndex, colIndex);
						}
					}
				}
			}
		}


		private SearchCriteria getSearchCriteria()
		{
			var searchCriteria = new SearchCriteria { SearchText = SearchText, IsCaseSensitive = IsCaseSensitive };

			return searchCriteria;
		}

		public void ConfigureSearchFunctionality(GridControl gridControl, IDomainFinder finder)
		{
			// Sinks the grid with the find and replace dialog sinc object
			_findAndReplaceDialog = new GridFindReplaceDialogSink(gridControl);
			// Sets the grid property of the window
			_grid = gridControl;

			// Sets the focus to the first cell of the grid
			_grid.CurrentCell.MoveTo(0, 0, GridSetCurrentCellOptions.SetFocus);
			// Hides the search results table
			gridListControlSearchResults.Clear(false);

			// Sets the domain finder
			_domainFinder = finder;

			if (finder == null)
			{
				// Disabel the search in persistence check box - find tab
				checkBoxAdvFindSearchInPersistance.Checked = false;
				checkBoxAdvFindSearchInPersistance.Enabled = false;

				// Disabel the search in persistence check box- replace tab
				checkBoxAdvReplaceSearchInPersistance.Checked = false;
				checkBoxAdvReplaceSearchInPersistance.Enabled = false;
			}
			else
			{
				// Enable the search in persistence check box - find tab
				checkBoxAdvFindSearchInPersistance.Checked = false;
				checkBoxAdvFindSearchInPersistance.Enabled = true;

				// Enable the search in persistence check box- replace tab
				checkBoxAdvReplaceSearchInPersistance.Checked = false;
				checkBoxAdvReplaceSearchInPersistance.Enabled = true;
			}
			comboDropDownFindSearchText.Select();
			if (comboDropDownFindSearchText.SelectedIndex > -1)
				comboDropDownFindSearchText.SelectedItem = comboDropDownFindSearchText.Items[comboDropDownFindSearchText.SelectedIndex];

		}

		public void SetActiveFunctionality(FindAndReplaceFunctionality functionality)
		{
			switch (functionality)
			{
				case FindAndReplaceFunctionality.None:
					break;

				case FindAndReplaceFunctionality.All:
					break;

				case FindAndReplaceFunctionality.FindOnly:
					// Sets the find tab selected
					tabControlAdvMain.SelectedIndex = 0;
					break;

				case FindAndReplaceFunctionality.ReplaceOnly:
					// Sets the find tab selected
					tabControlAdvMain.SelectedIndex = 1;
					break;
			}
		}
	}

	public enum FindAndReplaceFunctionality
	{
		/// <summary>
		/// None. 
		/// </summary>
		None,

		/// <summary>
		/// All the functionalities.
		/// </summary>
		All,

		/// <summary>
		/// Find functionality only.
		/// </summary>
		FindOnly,

		/// <summary>
		/// Rerplace functionality only.
		/// </summary>
		ReplaceOnly
	}

	public enum FindOption
	{
		/// <summary>
		/// None.
		/// </summary>
		None,

		/// <summary>
		/// All the option.
		/// </summary>
		All,

		/// <summary>
		/// Find within the content displayed on the grid.
		/// </summary>
		WithinGridOnly,

		/// <summary>
		/// Find in the persistance.
		/// </summary>
		WithinPersistenceOnly
	}

}
