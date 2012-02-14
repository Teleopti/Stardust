﻿using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.PeopleAdmin.MessageBroker;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    public abstract class GridViewBase : IDisposable, IPeopleAdminMessageBroker
    {
        private FilteredPeopleHolder _filteredPeopleHolder;

        protected GridViewBase(GridControl grid, FilteredPeopleHolder filteredPeopleHolder)
        {
            Grid = grid;
            CurrentGrid = grid;
            _filteredPeopleHolder = filteredPeopleHolder;
            Init();
        }

        public void SetFilteredPerson(FilteredPeopleHolder filteredPeopleHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
        }

        private void Init()
        {
            // Show the grid like an Excel Worksheet.
            GridHelper.GridStyle(Grid);
            // Override the column resize style.
            Grid.ResizeColsBehavior = GridResizeCellsBehavior.ResizeSingle;

            Grid.Dock = DockStyle.Fill;
            Grid.Location = new System.Drawing.Point(0, 0);
            Grid.Visible = true;

            Grid.CellModels.Add("HourMinutes", new TimeSpanHourMinutesOrEmptyCellModel(Grid.Model));
			Grid.CellModels.Add("NumericCell", new NumericCellModel(Grid.Model));
            Grid.ReadOnly = !TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications); 
        }

        public bool ReadOnly
        {
            get { return Grid.ReadOnly; }
        }
        public virtual void Invalidate()
        {
        }

        public virtual void Reinitialize()
        {
        }

        public virtual void Sort(bool isAscending)
        {
        }

        internal FilteredPeopleHolder FilteredPeopleHolder { get { return _filteredPeopleHolder; } }

        public virtual void RefreshChildGrids()
        {
        }

        public virtual void RefreshParentGrid()
        {
        }

        public void UnregisterForMessageBrokerEvents()
        {
            StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.
                UnregisterEventSubscription(OnEventMessageHandler);
        }

        internal abstract ViewType Type { get; }

        public GridControl Grid { get; private set; }

        public GridControl CurrentGrid { get; set; }

        public virtual void ClearView()
        {
            RowCount = 0;
            ColCount = 0;
            Grid.ColCount = 0;
            Grid.RowCount = 0;

            Grid.RowHeights.ResetModified();
        }

        public int ColCount { get; set; }

        public int RowCount { get; set; }

        public int ColHeaders
        {
            get { return Grid.Cols.HeaderCount; }
        }

        public int RowHeaders
        {
            get { return Grid.Rows.HeaderCount; }
        }

        public bool IsRightToLeft
        {
            get { return Grid.RightToLeft == RightToLeft.Yes; }
            set
            {
                Grid.RightToLeft = value ? RightToLeft.Yes : RightToLeft.No;
            }
        }

        internal virtual bool ValidCell(int columnIndex, int rowIndex)
        {
            return ValidColumn(columnIndex) && ValidRow(rowIndex);
        }

        internal virtual bool ValidColumn(int columnIndex)
        {
            var ret = false;
            if ((columnIndex != -1) && (ColCount > 0))
                if (columnIndex <= ColCount - 1)
                    ret = true;

            return ret;
        }

        internal virtual bool ValidRow(int rowIndex)
        {
            return rowIndex >= 0;
        }

        internal virtual void CreateHeaders()
        {
        }

        internal virtual void CreateContextMenu()
        {
            if (Grid.ContextMenuStrip != null)
            {
                Grid.ContextMenuStrip.Items.Clear();
            }
        }

        internal virtual void ShowMessage(string message, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(message,
                               caption,
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error,
                               MessageBoxDefaultButton.Button2,
                               (IsRightToLeft) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
        }

        internal virtual bool ValidateBeforeSave()
        {
            //This Valildation need to do with all views in person admin 
            string personName = PeopleAdminHelper.ValidatePeriodIsNotDuplicate(_filteredPeopleHolder);

            if (!String.IsNullOrEmpty(personName))
            {
                ShowMessage(string.Concat((UserTexts.Resources.DuplicatePersonPeriodMessage + "  " + personName), "  "),
                            UserTexts.Resources.PersonPeriodError);
                return false;
            }

            string personNameSc = PeopleAdminHelper.ValidateSchedulePeriodIsNotDuplicate(_filteredPeopleHolder);

            if (!String.IsNullOrEmpty(personNameSc))
            {
                ShowMessage(string.Concat((UserTexts.Resources.DuplicateSchedulePeriodMessage + " " + personNameSc), "  "),
                            UserTexts.Resources.SchedulePeriodError);
                return false;
            }

            string personNamePersonRotation = PeopleAdminHelper.ValidatePersonRotationIsNotDuplicate(_filteredPeopleHolder);

            if (!String.IsNullOrEmpty(personNamePersonRotation))
            {
                ShowMessage(string.Concat((UserTexts.Resources.DuplicatePersonRotationMessage + " " + personNamePersonRotation), "  "),
                            UserTexts.Resources.PersonRotationError);
                return false;
            }

            string personNamePersonAvailability = PeopleAdminHelper.ValidatePersonAvailabilityIsNotDuplicate(_filteredPeopleHolder);

            if (!String.IsNullOrEmpty(personNamePersonAvailability))
            {
                ShowMessage(string.Concat((UserTexts.Resources.DuplicatePersonAvailabilityMessage + " " + personNamePersonAvailability), "  "),
                            UserTexts.Resources.PersonAvailability);
                return false;
            }

            //After all other validations done. need to check duplicate user credentials.
            var conflicts = _filteredPeopleHolder.CheckForDuplicateUserNames();
            if(conflicts.Count > 0)
            {
                using (var conflictForm = new UserCredentialConflicts(conflicts))
                {
                    conflictForm.ShowDialog();
                }
                return false;
            }

            //Hmm, if a bad password has been entered, the user need to set a good one before save
            //Actually we dont change it if its bad, but there could be bad passwords in the db, before the "change" was done.
            if (!_filteredPeopleHolder.CheckBadPasswordPolicy())
            {
                ShowMessage(string.Concat(UserTexts.Resources.PasswordPolicyWarning, "  "), UserTexts.Resources.ErrorMessage);
                return false;
            }
            return true;
        }

        internal virtual void MergeHeaders() { }

        internal virtual void KeyDown(KeyEventArgs e)
        {
            GridHelper.HandleSelectionKeys(Grid, e);
        }

        internal virtual void QueryCellInfo(GridQueryCellInfoEventArgs e)
        {
            if (e.Style.HasPasswordChar) return;
            e.Style.CellTipText = e.Style.FormattedText.Trim().Length == 0 ? null : e.Style.FormattedText;
        }

        internal virtual void ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
        }

        internal virtual void ClipboardCanPaste(object sender, GridCutPasteEventArgs e)
        {
            CurrentGrid = Grid;
        }

        internal virtual void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
        }

        internal virtual void SelectedDateChange(object sender, EventArgs e)
        {
        }

        internal virtual void SelectionChanged(GridSelectionChangedEventArgs e, bool eventCancel)
        {
            CurrentGrid = Grid;
        }

        internal virtual void ClipboardPaste(GridCutPasteEventArgs e)
        {
            PasteFromClipboard(false);
        }
        internal virtual void CellButtonClicked(GridCellButtonClickedEventArgs e)
        {
        }

        internal virtual void DrawCellButton(GridDrawCellButtonEventArgs e)
        {
        }

        protected void PasteFromClipboard(bool insert)
        {
            if (insert) InsertClipHandlerToGrid();

            GridHelper.HandlePaste(Grid, GridClipHandler, GridPasteAction);
        }

        protected static ClipHandler<string> GridClipHandler
        {
            get { return PeopleAdminHelper.ConvertClipboardToClipHandler(); }
        }

        private readonly SimpleTextPasteAction _gridPasteAction = new SimpleTextPasteAction();

        internal SimpleTextPasteAction GridPasteAction
        {
            get { return _gridPasteAction; }
        }

        protected virtual void InsertClipHandlerToGrid() { }

        internal virtual void PrepareView()
        {
        }

        internal void HideRowHeaderColumn()
        {
            Grid.Model.Cols.Hidden[0] = true;
        }

        internal virtual void CellClick(object sender, GridCellClickEventArgs e)
        {

        }

        internal virtual void CellDoubleClick(object sender, GridCellClickEventArgs e)
        {

        }

        internal virtual void AddNewGridRow<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void InsertNewGridRow<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void InsertNewGridColumn<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void DeleteOptionalColumn<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void ChangeOptionalColumn<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void AddNewGridRowFromClipboard<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void InsertNewGridRowFromClipboard<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void DeleteSelectedGridRows<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void CopySpecial<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void PasteSpecial<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void ViewDataSaved<T>(object sender, T eventArgs) where T : EventArgs { }

        internal virtual void TrackerDescriptionChanged<T>(object sender, SelectedItemChangeBaseEventArgs<T> eventArgs) { }

        internal virtual IList<IPerson> GetSelectedPersons()
        {
            return new List<IPerson>();
        }

        internal virtual void SetSelectedPersons(IList<IPerson> selectedPersons)
        {
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Grid.Dispose();
            Grid = null;
            CurrentGrid = null;
        }

        internal virtual void DisposeChildGrids()
        {
        }

        public void RegisterForMessageBrokerEvents(Type type)
        {
            StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription
                (OnEventMessageHandler, type);

        }

        public event EventHandler<EventMessageArgs> OnEventMessageHandlerChanged;

        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
        	var handler = OnEventMessageHandlerChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public virtual void SetView(IShiftCategoryLimitationView view) { }
    }
}
