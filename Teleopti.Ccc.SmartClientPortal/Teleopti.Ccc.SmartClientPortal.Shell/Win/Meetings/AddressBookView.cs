using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;

using Cursors=System.Windows.Forms.Cursors;
using KeyEventArgs=System.Windows.Forms.KeyEventArgs;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
	public partial class AddressBookView : BaseDialogForm, IAddressBookView
	{
		private readonly string _statusText = UserTexts.Resources.ZeroOneFoundDot;

		private SFGridColumnGridHelper<ContactPersonViewModel> _gridHelper;
		private SFGridColumnBase<ContactPersonViewModel> _firstNameColumn;
		private SFGridColumnBase<ContactPersonViewModel> _lastNameColumn;
		private SFGridColumnBase<ContactPersonViewModel> _employmentNumberColumn;
		private SFGridColumnBase<ContactPersonViewModel> _teamColumn;
		private SFGridColumnBase<ContactPersonViewModel> _siteColumn;
		private SFGridColumnBase<ContactPersonViewModel> _skillColumn;
		private SFGridColumnBase<ContactPersonViewModel> _emailColumn;


		private readonly List<SFGridColumnBase<ContactPersonViewModel>> _gridColumns =
			new List<SFGridColumnBase<ContactPersonViewModel>>();

		private readonly AddressBookPresenter _presenter;
		private bool _isRequired = true;
		private LastSelectionWas _lastSelectionWas;

		public event EventHandler<AddressBookParticipantSelectionEventArgs> ParticipantsSelected;

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressBookView"/> class.
		/// </summary>
		/// <remarks>
		/// Created by: Aruna Priyankara Wickrama
		/// Created date: 8/5/2008
		/// </remarks>
		protected AddressBookView()
		{
			InitializeComponent();
			
			SetColors();
			if (!DesignMode)
			{
				SetTexts();
				dateTimePickerAdvtDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);
				dateTimePickerAdvtDate.ValueChanged += dateTimePickerAdvtDateValueChanged;
			}
		}

		public AddressBookView(AddressBookViewModel addressBookViewModel, DateOnly startDate) : this()
		{
			// Builds people list grid view.
			createColumns();

			_presenter = new AddressBookPresenter(this, addressBookViewModel, startDate);
			_presenter.Initialize();
		}

		private void buttonAdvRequiredClick(object sender, EventArgs e)
		{
			_presenter.AddRequiredParticipants(_gridHelper.FindSelectedItems());
		}

		private void buttonAdvOptionalClick(object sender, EventArgs e)
		{
			_presenter.AddOptionalParticipants(_gridHelper.FindSelectedItems());
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			InvokeParticipantSelectedEvent();
		}

		private void textBoxExtFilterCriteriaEnter(object sender, EventArgs e)
		{
			AcceptButton = buttonAdvGo;
		}

		private void textBoxExtFilterCriteriaLeave(object sender, EventArgs e)
		{
			AcceptButton = buttonAdvOK;
		}

		private void buttonAdvGoClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			if (string.IsNullOrEmpty(textBoxExtFilterCriteria.Text))
				PrepareGridView(_presenter.AddressBookViewModel.PersonModels);
			// Restores origin list to refilter.
			_gridHelper.RestoreToOriginalList();
			var date = new DateOnly(dateTimePickerAdvtDate.Value);
			var toRemove = new List<ContactPersonViewModel>();
			// Keeps the filterred & removes the rest.
			foreach (var contactPersonView in _gridHelper.SourceList)
			{
				// we must set the date on the original list of cource
				contactPersonView.CurrentDate = date;
				bool filterOut = _presenter.IsVisible(contactPersonView);
				if (!filterOut) toRemove.Add(contactPersonView);
			}
			foreach (var contactPersonView in toRemove)
			{
				_gridHelper.SourceList.Remove(contactPersonView);
			}
			// Refreshes the grid.
			RefreshGrid();
			Cursor = Cursors.Default;
		}

		private void gridControlPeopleCellClick(object sender, GridCellClickEventArgs e)
		{
			if ((gridControlPeople.CurrentCell.ColIndex > 0) && (gridControlPeople.CurrentCell.RowIndex == 0))
			{
				Sort(gridControlPeople.CurrentCell.ColIndex);
			}
		}

		private void dateTimePickerAdvtDateValueChanged(object sender, EventArgs e)
		{
			_presenter.SetCurrentDate(new DateOnly(dateTimePickerAdvtDate.Value));
		}

		protected void SetColors()
		{
			
			//gridControlPeople.BackColor = ColorHelper.GridControlGridInteriorColor();
			//gridControlPeople.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
		}

		private void createColumns()
		{
			gridControlPeople.Rows.HeaderCount = 0;
			gridControlPeople.Cols.HeaderCount = 0;

			_gridColumns.Add(new SFGridRowHeaderColumn<ContactPersonViewModel>("")); // Grid must have a Header column.

			_firstNameColumn = new SFGridEditableTextColumn<ContactPersonViewModel>("FirstName", 30, UserTexts.Resources.FirstName);
			_gridColumns.Add(_firstNameColumn);

			_lastNameColumn = new SFGridEditableTextColumn<ContactPersonViewModel>("LastName", 30, UserTexts.Resources.LastName);
			_gridColumns.Add(_lastNameColumn);

			_employmentNumberColumn = new SFGridEditableTextColumn<ContactPersonViewModel>("EmploymentNumber", 14, UserTexts.Resources.EmployeeNo);
			_gridColumns.Add(_employmentNumberColumn);

			// Site column
			_siteColumn =
				new SFGridEditableTextColumn<ContactPersonViewModel>("SiteBelong.Description.Name", 30, UserTexts.Resources.Site)
					{
						SortCompare = delegate(ContactPersonViewModel left, ContactPersonViewModel right)
										  {
											  int result;

											  if ((left.SiteBelong == null) && (right.SiteBelong == null)) result = 0;
											  else if (left.SiteBelong == null) result = -1;
											  else if (right.SiteBelong == null) result = 1;
											  else
												  result = string.Compare(left.SiteBelong.Description.Name,
																		  right.SiteBelong.Description.Name,
																		  StringComparison.CurrentCulture);

											  return result;
										  }
					};
			_gridColumns.Add(_siteColumn);


			// Team column
			_teamColumn =
				new SFGridEditableTextColumn<ContactPersonViewModel>("TeamBelong.Description.Name", 30, UserTexts.Resources.Team)
					{
						SortCompare = delegate(ContactPersonViewModel left, ContactPersonViewModel right)
										  {
											  int result;

											  if ((left.TeamBelong == null) && (right.TeamBelong == null)) result = 0;
											  else if (left.TeamBelong == null) result = -1;
											  else if (right.TeamBelong == null) result = 1;
											  else
												  result = string.Compare(left.TeamBelong.Description.Name,
																		  right.TeamBelong.Description.Name,
																		  StringComparison.CurrentCulture);

											  return result;
										  }
					};
			_gridColumns.Add(_teamColumn);

			_skillColumn = new SFGridEditableTextColumn<ContactPersonViewModel>("Skills", 120, UserTexts.Resources.Skill)
							   {
								   SortCompare = delegate(ContactPersonViewModel left, ContactPersonViewModel right)
													 {
														 int result;

														 if (string.IsNullOrEmpty(left.Skills) &&
															 string.IsNullOrEmpty(right.Skills)) result = 0;
														 else if (string.IsNullOrEmpty(left.Skills)) result = -1;
														 else if (string.IsNullOrEmpty(right.Skills)) result = 1;
														 else
															 result = string.Compare(left.Skills,
																					 right.Skills,
																					 StringComparison.CurrentCulture);

														 return result;
													 }
							   };
			_gridColumns.Add(_skillColumn);

			_emailColumn = new SFGridEditableTextColumn<ContactPersonViewModel>("Email", 120, UserTexts.Resources.Email)
							   {
								   SortCompare = delegate(ContactPersonViewModel left, ContactPersonViewModel right)
													 {
														 return string.Compare(left.Email,
																			   right.Email,
																			   StringComparison.CurrentCulture);
													 }
							   };
			_gridColumns.Add(_emailColumn);
		}

		public void PrepareGridView(IList<ContactPersonViewModel> personViewDataList)
		{
			gridControlPeople.BeginUpdate(BeginUpdateOptions.SynchronizeScrollBars);
			if (_gridHelper == null)
				_gridHelper = new SFGridColumnGridHelper<ContactPersonViewModel>(
					gridControlPeople,
					new ReadOnlyCollection<SFGridColumnBase<ContactPersonViewModel>>(_gridColumns),
					personViewDataList,false);

			_gridHelper.SetSourceList(personViewDataList);

			// Overrides grid styles.
			gridControlPeople.ActivateCurrentCellBehavior = GridCellActivateAction.None;
			gridControlPeople.ListBoxSelectionMode = SelectionMode.MultiExtended;
			gridControlPeople.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			gridControlPeople.EndUpdate();
		}

		public string GetSearchCriteria()
		{
			return textBoxExtFilterCriteria.Text;
		}

		private void RefreshGrid()
		{
			int count = _gridHelper.SourceList.Count;
			//removed the assingment in the gridControlPeople.RowCount to do this either the grid.QueryRowCount event should be extended and e.Size is provided
			//or grid.refresh is called.
			gridControlPeople.Refresh();
		   
			// Displays filterred count on the status.
			switch (count)
			{
				case 0:
					statusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.People);
					break;
				case 1:
					statusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.Person);
					break;
				default:
					statusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.People);
					break;
			}
		}

		private void InvokeParticipantSelectedEvent()
		{
			var handler = ParticipantsSelected;
			if (handler != null)
			{
				// Creates the event args.
				var e = new AddressBookParticipantSelectionEventArgs();

				// Invokes the participant seleceted event
				handler.Invoke(this, e);
			}
		}

		public void Sort(int columnIndex)
		{
			ISortColumn<ContactPersonViewModel> selectedColumn = _gridColumns[columnIndex];
			SortingModes mode = ((_gridColumns[columnIndex].IsAscending == null) ||
								 (!(bool)_gridColumns[columnIndex].IsAscending))
									? SortingModes.Ascending
									: SortingModes.Descending;

			ISort<ContactPersonViewModel> iSort = new SortingBase<ContactPersonViewModel>();
			var personList = string.IsNullOrEmpty(textBoxExtFilterCriteria.Text)
								 ? _presenter.AddressBookViewModel.PersonModels
								 : _gridHelper.SourceList;
			List<ContactPersonViewModel> personViewDataList =
				iSort.Sort(selectedColumn, new ReadOnlyCollection<ContactPersonViewModel>(personList), mode).ToList();

			SetSortingStatus(columnIndex);
			PrepareGridView(personViewDataList);
		}

		public void SetSortingStatus(int columnIndex)
		{
			for (int index = 0; index < _gridColumns.Count; index++)
			{
				if (index == columnIndex)
				{
					_gridColumns[index].IsAscending = ((_gridColumns[index].IsAscending == null) ||
													   (!(bool)_gridColumns[index].IsAscending))
														  ? true
														  : false;
				}
				else
				{
					_gridColumns[index].IsAscending = null;
				}
			}
		}

		public void SetCurrentDate(DateOnly startDate)
		{
			dateTimePickerAdvtDate.Value = startDate.Date;
		}

		public void SetRequiredParticipants(string requiredParticipants)
		{
			textBoxExtRequiredParticipant.Text = requiredParticipants;
		}

		public void SetOptionalParticipants(string optionalParticipants)
		{
			textBoxExtOptionalParticipant.Text = optionalParticipants;
		}

		public void PerformSearch()
		{
			buttonAdvGo.PerformClick();
		}

		private void textBoxExtRequiredParticipant_TextChanged(object sender, EventArgs e)
		{
			//_presenter.ParseRequiredParticipants(textBoxExtRequiredParticipant.Text);
		}

		private void textBoxExtOptionalParticipant_TextChanged(object sender, EventArgs e)
		{
			//_presenter.ParseOptionalParticipants(textBoxExtOptionalParticipant.Text);
		}

		private void gridControlPeople_CellDoubleClick(object sender, GridCellClickEventArgs e)
		{
			if (gridControlPeople.CurrentCell.RowIndex < 1) return;

			if (_isRequired)
			{
				_presenter.AddRequiredParticipants(_gridHelper.FindSelectedItems());

			}
			else
			{
				_presenter.AddOptionalParticipants(_gridHelper.FindSelectedItems());
			}
		}

		private void textBoxExtRequiredParticipant_MouseUp(object sender, MouseEventArgs e)
		{
			_isRequired = true;
			if (e.Button.Equals(MouseButtons.Right))
				textBoxExtRequiredParticipant.Select();
			TextBoxNameExtender.GetSelected(textBoxExtRequiredParticipant);   
		}

		private void textBoxExtOptionalParticipant_MouseUp(object sender, MouseEventArgs e)
		{
			_isRequired = false;
			if (e.Button.Equals(MouseButtons.Right))
				textBoxExtOptionalParticipant.Select();
			TextBoxNameExtender.GetSelected(textBoxExtOptionalParticipant);
		}

		private void textBoxExtFilterCriteria_MouseUp(object sender, MouseEventArgs e)
		{
			if(e.Button.Equals(MouseButtons.Right))
				textBoxExtFilterCriteria.Select();
		}
		
		private void textBoxExtRequiredParticipant_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
			{
				var textBox = ((TextBoxBase)ActiveControl);
				var first = textBox.Text.Substring(0, textBox.SelectionStart);
				var selectedIndexes = TextBoxNameExtender.SelectedIndexes(textBox);
				_presenter.RemoveIndexesRequired(selectedIndexes);
				textBox.Select(first.Length, 0);
				TextBoxNameExtender.GetSelected(textBox);
			}
			else
			{
				_lastSelectionWas = TextBoxNameExtender.KeyDown((TextBoxBase)ActiveControl, e, _lastSelectionWas, true);	
			}
			
			e.SuppressKeyPress = true;
		}

		private void textBoxExtOptionalParticipant_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
			{
				var textBox = ((TextBoxBase)ActiveControl);
				var first = textBox.Text.Substring(0, textBox.SelectionStart);
				var selectedIndexes = TextBoxNameExtender.SelectedIndexes(textBox);
				_presenter.RemoveIndexesOptional(selectedIndexes);
				textBox.Select(first.Length, 0);
				TextBoxNameExtender.GetSelected(textBox);
			}
			else
			{
				_lastSelectionWas = TextBoxNameExtender.KeyDown((TextBoxBase)ActiveControl, e, _lastSelectionWas, true);
			}


			e.SuppressKeyPress = true;
		}

		private void contextMenuStripEx1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (ActiveControl.GetType() == typeof(TextBoxBase))
				TextBoxNameExtender.UpdateContextMenu((TextBoxBase)ActiveControl, contextMenuStripEx1);
			else
				contextMenuStripEx1.Close();
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TextBoxNameExtender.CutItem((TextBoxBase)ActiveControl);
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TextBoxNameExtender.CopyItem((TextBoxBase)ActiveControl);
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var addSemiColon = !ActiveControl.Name.Equals("textBoxExtFilterCriteria");
			TextBoxNameExtender.PasteItem((TextBoxBase)ActiveControl, addSemiColon);
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TextBoxNameExtender.DeleteItem((TextBoxBase)ActiveControl);
		}

		private void tableLayoutPanelConfirmButtons_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}
