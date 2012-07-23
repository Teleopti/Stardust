﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Cursors=System.Windows.Forms.Cursors;
using KeyEventArgs=System.Windows.Forms.KeyEventArgs;

namespace Teleopti.Ccc.Win.Meetings
{
    public partial class AddressBookView : BaseRibbonForm, IAddressBookView
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
        private bool _IsRequired = true;

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
            }
        }

        public AddressBookView(AddressBookViewModel addressBookViewModel, DateOnly startDate) : this()
        {
            // Builds people list grid view.
            CreateColumns();

            _presenter = new AddressBookPresenter(this, addressBookViewModel, startDate);
            _presenter.Initialize();
        }

        private void buttonAdvRequired_Click(object sender, EventArgs e)
        {
            _presenter.AddRequiredParticipants(_gridHelper.FindSelectedItems());
        }

        private void buttonAdvOptional_Click(object sender, EventArgs e)
        {
            _presenter.AddOptionalParticipants(_gridHelper.FindSelectedItems());
        }

        private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            InvokeParticipantSelectedEvent();
        }

        private void textBoxExtFilterCriteria_Enter(object sender, EventArgs e)
        {
            AcceptButton = buttonAdvGo;
        }

        private void textBoxExtFilterCriteria_Leave(object sender, EventArgs e)
        {
            AcceptButton = buttonAdvOK;
        }

        private void buttonAdvGo_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

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

        private void gridControlPeople_CellClick(object sender, GridCellClickEventArgs e)
        {
            if ((gridControlPeople.CurrentCell.ColIndex > 0) && (gridControlPeople.CurrentCell.RowIndex == 0))
            {
                Sort(gridControlPeople.CurrentCell.ColIndex);
            }
        }

        private void dateTimePickerAdvtDate_ValueChanged(object sender, EventArgs e)
        {
            _presenter.SetCurrentDate(new DateOnly(dateTimePickerAdvtDate.Value));
        }

        /// <summary>
        /// Sets the colors.
        /// </summary>
        protected void SetColors()
        {
            BackColor = ColorHelper.ControlPanelColor;
            GridHelper.GridStyle(gridControlPeople);

            gridControlPeople.BackColor = ColorHelper.GridControlGridInteriorColor();
            gridControlPeople.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            dateTimePickerAdvtDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
        }

        /// <summary>
        /// Creates columns for the grid.
        /// </summary>
        private void CreateColumns()
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
            _gridHelper = new SFGridColumnGridHelper<ContactPersonViewModel>(
                gridControlPeople,
                new ReadOnlyCollection<SFGridColumnBase<ContactPersonViewModel>>(_gridColumns),
                personViewDataList
                );

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
                    toolStripStatusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.People);
                    break;
                case 1:
                    toolStripStatusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.Person);
                    break;
                default:
                    toolStripStatusLabelMessage.Text = string.Format(CultureInfo.CurrentUICulture, _statusText, count, UserTexts.Resources.People);
                    break;
            }

            // Invalidates grid for updates.
            gridControlPeople.Invalidate();
        }

        private void InvokeParticipantSelectedEvent()
        {
        	var handler = ParticipantsSelected;
            if (handler != null)
            {
                // Creates the event args.
                var e = new AddressBookParticipantSelectionEventArgs(_presenter.AddressBookViewModel);

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

            List<ContactPersonViewModel> personViewDataList =
                iSort.Sort(selectedColumn,new ReadOnlyCollection<ContactPersonViewModel>(_presenter.AddressBookViewModel.PersonModels), mode).ToList();

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
            dateTimePickerAdvtDate.Value = startDate;
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
            int caretPositon = textBoxExtRequiredParticipant.SelectionStart;
            _presenter.ParseRequiredParticipants(textBoxExtRequiredParticipant.Text);
            textBoxExtRequiredParticipant.Select(Math.Min(textBoxExtRequiredParticipant.Text.Length, caretPositon), 0);
        }

        private void textBoxExtOptionalParticipant_TextChanged(object sender, EventArgs e)
        {
            int caretPositon = textBoxExtRequiredParticipant.SelectionStart;
            _presenter.ParseOptionalParticipants(textBoxExtOptionalParticipant.Text);
            textBoxExtRequiredParticipant.Select(Math.Min(textBoxExtRequiredParticipant.Text.Length, caretPositon), 0);
        }

        private void gridControlPeople_CellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            if (_IsRequired)
            {
                _presenter.AddRequiredParticipants(_gridHelper.FindSelectedItems());

            }
            else
            {
                _presenter.AddOptionalParticipants(_gridHelper.FindSelectedItems());
            }
        }

        private void textBoxExtRequiredParticipant_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsValidKey(e.KeyCode,e.Modifiers))
                e.SuppressKeyPress = true;
        }

        private static bool IsValidKey(Keys key, Keys modifiers)
        {
            return (key == Keys.Down ||
                    key == Keys.Up ||
                    key == Keys.Left ||
                    key == Keys.Right ||
                    key == Keys.Return ||
                    key == Keys.Back ||
                    key == Keys.Delete ||
                    key == Keys.Home ||
                    key == Keys.End ||
                    key == Keys.Tab ||
                    modifiers == Keys.Control);
        }

        private void textBoxExtOptionalParticipant_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsValidKey(e.KeyCode, e.Modifiers))
                e.SuppressKeyPress = true;
        }

        private void textBoxExtRequiredParticipant_Click(object sender, EventArgs e)
        {
            _IsRequired = true;
        }

        private void textBoxExtOptionalParticipant_Click(object sender, EventArgs e)
        {
            _IsRequired = false;
        }
    }
}
