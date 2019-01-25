using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;
using GridConstructor = Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views.GridConstructor;
using ViewType = Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views.ViewType;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin
{
    public partial class PeopleWorksheet : BaseRibbonForm
    {
        private PeopleAdminFilterPanel _peopleAdminFilterPanel;

        // Instantiates the find and replace form
        private FindAndReplaceForm _findAndReplaceForm =
            new FindAndReplaceForm(FindAndReplaceFunctionality.FindOnly, FindOption.All);

        //Holds the domain finder object - by default people domain finder
        private IDomainFinder _domainFinder;
        private EditControl _editControl;
        private ClipboardControl _clipboardControl;
        private GridConstructor _gridConstructor;
        private GridConstructor _panelConstructor;
        private FilteredPeopleHolder _filteredPeopleHolder;
        private IEventAggregator _globalEventAggregator;
		
		private static WorksheetStateHolder _stateHolder;
        private TabControlAdv _tabControlPeopleAdmin;
        private readonly bool _readOnly;
        private ILifetimeScope _container;
        private IToggleManager _toggleManager;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;
		private Form _mainWindow;
        private DateNavigateControl _dateNavigatePeriods;
	    private static int _numberOfOpened = 0;
		private readonly IConfigReader _configReader;

		protected PeopleWorksheet()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                ColorHelper.SetRibbonQuickAccessTexts(peopleRibbon);
            }
            // Set colors, themes on controls.
            SetColors();
            setPermissionOnControls();

            _readOnly = !PrincipalAuthorization.Current().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public PeopleWorksheet(WorksheetStateHolder state, FilteredPeopleHolder filteredPeopleHolder, IEventAggregator globalEventAggregator, IComponentContext componentContext, Form mainWindow)
            : this()
        {
            if (filteredPeopleHolder == null) throw new ArgumentNullException(nameof(filteredPeopleHolder));
            _mainWindow = mainWindow;
            _filteredPeopleHolder = filteredPeopleHolder;
            _globalEventAggregator = globalEventAggregator;
			var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
            _container = lifetimeScope.BeginLifetimeScope();
            _toggleManager = componentContext.Resolve<IToggleManager>();

            _tabControlPeopleAdmin = tabControlPeopleAdmin;
            _filteredPeopleHolder.TabControlPeopleAdmin = _tabControlPeopleAdmin;

			_businessRuleConfigProvider = componentContext.Resolve<IBusinessRuleConfigProvider>();
			_configReader = componentContext.Resolve<IConfigReader>();

			_gridConstructor = new GridConstructor(filteredPeopleHolder, _toggleManager, _businessRuleConfigProvider, _configReader);
            _panelConstructor = new GridConstructor(filteredPeopleHolder, _toggleManager, _businessRuleConfigProvider, _configReader);
            _domainFinder = new PeopleDomainFinder(filteredPeopleHolder);
            shiftCategoryLimitationView.SetState(filteredPeopleHolder, _gridConstructor);

            _dateNavigatePeriods = new DateNavigateControl();
            _dateNavigatePeriods.SetSelectedDate(_filteredPeopleHolder.SelectedDate);
            _dateNavigatePeriods.SelectedDateChanged += dateNavigatePeriodsSelectedDateChanged;

            var hostDatePicker = new ToolStripControlHost(_dateNavigatePeriods);
            toolStripDatePicker.Items.Add(hostDatePicker);

            _stateHolder = state;

            _gridConstructor.GridViewChanging += gridConstructorGridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructorGridViewChanged;
            _panelConstructor.GridViewChanged += panelConstructorGridViewChanged;

            // Clear cache available.
            _gridConstructor.FlushCache();
            _panelConstructor.FlushCache();

            // Show the default view as General View.
            _gridConstructor.BuildGridView(ViewType.EmptyView);

            //initialize filter popup
            _peopleAdminFilterPanel = new PeopleAdminFilterPanel(_filteredPeopleHolder, this, _container) { Location = new Point(229, 130) };
            _peopleAdminFilterPanel.BringToFront();
            _peopleAdminFilterPanel.Leave += peopleAdminFilterPanelLeave;
            Controls.Add(_peopleAdminFilterPanel);
            _peopleAdminFilterPanel.Visible = false;

            instantiateEditControl();
            instantiateClipboardControl();
            loadTrackerDescriptions();
            toolStripComboBoxExTrackerDescription.Enabled = false;


            Cursor.Current = Cursors.Default;
            toolStripButtonMainSave.Enabled = !_readOnly;
	        _numberOfOpened++;
        }

        private void unregisterEventsForFormKill()
        {
            _gridConstructor.GridViewChanging -= gridConstructorGridViewChanging;
            _gridConstructor.GridViewChanged -= gridConstructorGridViewChanged;
            _panelConstructor.GridViewChanged -= panelConstructorGridViewChanged;
            _peopleAdminFilterPanel.Leave -= peopleAdminFilterPanelLeave;

            _editControl.NewClicked -= (editControlNewClicked);
            _editControl.NewSpecialClicked -= (editControlNewSpecialClicked);
            _editControl.DeleteClicked -= (editControlDeleteClicked);
            _clipboardControl.CutClicked -= (clipboardControlCutClicked);
            _clipboardControl.CopyClicked -= (clipboardControlCopyClicked);
            _clipboardControl.PasteSpecialClicked -= (clipboardControlPasteSpecialClicked);
            _clipboardControl.PasteClicked -= (clipboardControlPasteClicked);

            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.GeneralView));
            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.PeoplePeriodView));
            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.SchedulePeriodView));
            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.PersonRotationView));
            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.PersonAvailabilityView));
            unregisterViewEvents(_gridConstructor.FindGrid(ViewType.PersonalAccountGridView));
            unregisterViewEvents(_panelConstructor.FindGrid(ViewType.RolesView));
            unregisterViewEvents(_panelConstructor.FindGrid(ViewType.EmptyView));
            unregisterViewEvents(_panelConstructor.FindGrid(ViewType.ExternalLogOnView));
            unregisterViewEvents(_panelConstructor.FindGrid(ViewType.SkillsView));
        }

        private void unregisterViewEvents(GridViewBase view)
        {
            if (view == null) return;
            // unBind events for the grid.
            view.Grid.QueryCellInfo -= gridWorksheetQueryCellInfo;
            view.Grid.SaveCellInfo -= gridWorksheetSaveCellInfo;
            view.Grid.ClipboardPaste -= gridWorksheetClipboardPaste;
            view.Grid.SelectionChanged -= gridWorksheetSelectionChanged;
            view.Grid.KeyDown -= gridWorksheetKeyDown;
						view.Grid.MouseDown -= gridWorksheetMouseDown;

			// Periods specific events.
			view.Grid.CellButtonClicked -= gridWorksheetCellButtonClicked;
            view.Grid.DrawCellButton -= gridWorksheetDrawCellButton;
            view.Grid.CellClick -= gridWorksheetCellClick;
            view.Grid.CellDoubleClick -= gridWorksheetCellDoubleClick;
            view.Grid.ClipboardCanCopy -= gridWorksheetClipboardCanCopy;
            view.Grid.ClipboardCanPaste -= gridClipboardCanPaste;
        }
        protected void SetColors()
        {
        }

        private void setPermissionOnControls()
        {
            var optionPagePermission = PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
            backStageButton3.Enabled = optionPagePermission;

            toolStripButtonContract.Enabled = optionPagePermission;
            toolStripButtonContractSchedule.Enabled = optionPagePermission;
            toolStripButtonPartTimePercentage.Enabled = optionPagePermission;
		}

        private void dateNavigatePeriodsSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
        {
            if (_gridConstructor.View == null)
                return;

            _filteredPeopleHolder.SelectedDate = e.Value;
            _gridConstructor.View.SelectedDateChange(sender, e);
            _panelConstructor.View.Invalidate();
            refilterOnTracker();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int wmKeydown = 0x100;
            const int wmSyskeydown = 0x104;

            if ((msg.Msg == wmKeydown) || (msg.Msg == wmSyskeydown))
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.F:
                        configureAndShowFindAndReplaceForm();
                        break;

                    case Keys.Control | Keys.N:
                        handleControlNCmdKey();
                        break;

                    case Keys.Escape:
                        _findAndReplaceForm.Hide();
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void handleControlNCmdKey()
        {
            if (toolStripExEdit.Enabled)
            {
                var viewType = _gridConstructor.View.Type;

                ToolStripItem theButton = null;
                switch (viewType)
                {
                    case ViewType.GeneralView:
                        theButton = _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewPerson);
						if(theButton != null)
							theButton.Enabled = PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPerson);
						break;
                    case ViewType.PeoplePeriodView:
                        theButton = _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewPersonPeriod);
                        break;
                    case ViewType.SchedulePeriodView:
                        theButton = _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewSchedulePeriod);
                        break;
                    case ViewType.PersonRotationView:
                        theButton = _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewPersonRotation);
                        break;
                    case ViewType.PersonalAccountGridView:
                        theButton = _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewPersonAccount);
                        break;
                    case ViewType.PersonAvailabilityView:
                        theButton =
                            _editControl.NewSpecialItems.FirstOrDefault(c => c.Text == UserTexts.Resources.NewPersonAvailability);
                        break;
                }

                if (theButton != null)
                {
                    var theEvent = new ToolStripItemClickedEventArgs(theButton);
                    editControlNewSpecialClicked(theButton, theEvent);
                }
            }
        }

        private void panelConstructorGridViewChanged(object sender, EventArgs e)
        {
            InParameter.NotNull("sender", sender);

            var view = (GridViewBase)sender;

            if (!_panelConstructor.IsCached)
            {
                view.Grid.ResetVolatileData();

                // Bind events for the grid.
                view.Grid.QueryCellInfo += gridPanelQueryCellInfo;
                view.Grid.SaveCellInfo += gridPanelSaveCellInfo;
                view.Grid.KeyDown += gridPanelKeyDown;
            }
        }

        private void gridPanelQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
			_panelConstructor?.View.QueryCellInfo(e);
			e.Handled = true;
        }

        private void gridPanelSaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (_readOnly) return;
            _panelConstructor.View.SaveCellInfo(sender, e);

            _gridConstructor.View.Invalidate();
            _panelConstructor.View.Invalidate();

            e.Handled = true;
        }

        private void gridPanelKeyDown(object sender, KeyEventArgs e)
        {
            _panelConstructor.View.KeyDown(e);
        }

        private void gridConstructorGridViewChanging(object sender, EventArgs e)
        {
            splitContainerWorksheet.Panel1.Controls.Clear();
        }

        private void gridConstructorGridViewChanged(object sender, EventArgs e)
        {
            InParameter.NotNull(nameof(sender), sender);

            var view = (GridViewBase)sender;

            if (!_gridConstructor.IsCached)
            {
                view.Grid.ResetVolatileData();

                // Bind events for the grid.
                view.Grid.QueryCellInfo += gridWorksheetQueryCellInfo;
                view.Grid.SaveCellInfo += gridWorksheetSaveCellInfo;
                view.Grid.ClipboardPaste += gridWorksheetClipboardPaste;
                view.Grid.SelectionChanged += gridWorksheetSelectionChanged;
                view.Grid.KeyDown += gridWorksheetKeyDown;
								view.Grid.MouseDown += gridWorksheetMouseDown;

                // Periods specific events.
                view.Grid.CellButtonClicked += gridWorksheetCellButtonClicked;
                view.Grid.DrawCellButton += gridWorksheetDrawCellButton;
                view.Grid.CellClick += gridWorksheetCellClick;
                view.Grid.CellDoubleClick += gridWorksheetCellDoubleClick;
                view.Grid.ClipboardCanCopy += gridWorksheetClipboardCanCopy;
                view.Grid.ClipboardCanPaste += gridClipboardCanPaste;
            }

            splitContainerWorksheet.Panel1.Controls.Add(view.Grid);

            toolStripButtonGeneral.Checked = (sender is GeneralGridView);
            toolStripButtonPeoplePeriods.Checked = (sender is PeoplePeriodGridView);
            toolStripButtonSchedulePeriods.Checked = (sender is SchedulePeriodGridView);
            toolStripButtonPersonAccounts.Checked = (sender is PersonalAccountGridView);
            toolStripButtonPersonAvailability.Checked =
                (sender is RotationBaseGridView<PersonAvailabilityModelParent,
                                PersonAvailabilityModelChild, IPersonAvailability, IAvailabilityRotation>);

            toolStripButtonPersonRotation.Checked = (sender is RotationBaseGridView<PersonRotationModelParent,
                        PersonRotationModelChild, IPersonRotation, IRotation>);


            toolStripComboBoxExTrackerDescription.Enabled = false;

            if (_gridConstructor.CurrentView == ViewType.PersonalAccountGridView)
            {
                toolStripComboBoxExTrackerDescription.Enabled = true;
            }

            toolStripButtonClosePreviousPeriod.Enabled = false;

            if (_gridConstructor.CurrentView == ViewType.PersonalAccountGridView || _gridConstructor.CurrentView == ViewType.SchedulePeriodView)
            {
                toolStripButtonClosePreviousPeriod.Enabled = true;
            }

            if (_gridConstructor.CurrentView == ViewType.PersonalAccountGridView)
            {
                getPersonAccountAbsenceType();
            }

            if (_gridConstructor.CurrentView == ViewType.GeneralView)
            {
                ((GeneralGridView)view).ResetChangeLogonDataCheck();
            }
        }

		private void gridWorksheetMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				GridRangeInfo rangeInfo = _gridConstructor.View.Grid.PointToRangeInfo(e.Location);
				if (!_gridConstructor.View.Grid.Selections.Ranges.AnyRangeIntersects(rangeInfo))
				{
					_gridConstructor.View.Grid.Selections.Clear(true);
					_gridConstructor.View.Grid.CurrentCell.MoveTo(rangeInfo, GridSetCurrentCellOptions.None);
				}
			}
		}

		private IAbsence getPersonAccountAbsenceType()
        {
            IAbsence currentType = null;

            if (toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem != null)
            {
                currentType = (IAbsence)toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem;
            }

            _filteredPeopleHolder.SelectedPersonAccountAbsenceType = currentType;
            return currentType;
        }

        private void refreshAfterMessageBroker()
        {
            _gridConstructor.View.Grid.ResetVolatileData();
            _gridConstructor.View.Grid.Invalidate();

            _gridConstructor.View.RefreshParentGrid();

            _gridConstructor.View.RefreshChildGrids();
        }

        private void gridClipboardCanPaste(object sender, GridCutPasteEventArgs e)
        {
            _gridConstructor.View.ClipboardCanPaste(sender, e);
            e.Handled = true;
        }

        public static WorksheetStateHolder StateHolder => _stateHolder;
		
		private void gridWorksheetKeyDown(object sender, KeyEventArgs e)
        {
            _gridConstructor.View.KeyDown(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void gridWorksheetSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            try
            {
                bool eventCancel = false;
                _gridConstructor.View.SelectionChanged(e, eventCancel);
                if (!eventCancel)
                    foreach (TabPageAdv tab in tabControlPeopleAdmin.TabPages)
                    {
                        //if (tab.Controls.Count > 0 && tab.Controls[0].GetType() == typeof(GridControl))
                        //    ((GridControl)tab.Controls[0]).Invalidate();

                        foreach (Control control in tab.Controls)
                        {
                            if (control.GetType() == typeof(GridControl))
                                ((GridControl)control).Invalidate();

                            foreach (Control con in control.Controls)
                            {
                                if (con.GetType() == typeof(GridControl))
                                    ((GridControl)con).Invalidate();
                            }
                        }
                    }

                if (_gridConstructor.View is PersonalAccountGridView)
                {
                    toolStripButtonClosePreviousPeriod.Enabled = (((PersonalAccountGridView)_gridConstructor.View).ExpandedGridSelection == false);
                }

            }
            catch (DataSourceException exception)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(exception);
                FormKill();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is DataSourceException)
                {
                    DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                    FormKill();
                }
            }
        }

        private void gridWorksheetSaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            _gridConstructor.View.SaveCellInfo(sender, e);
            e.Handled = true;
        }

        private void gridWorksheetCellClick(object sender, GridCellClickEventArgs e)
        {
            _gridConstructor.View.CellClick(sender, e);
        }

        private void gridWorksheetCellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            _gridConstructor.View.CellDoubleClick(sender, e);
        }

        private void gridWorksheetClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            _gridConstructor.View.ClipboardPaste(e);
            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void gridWorksheetQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            try
            {

                if (_gridConstructor.View == null)
                    return;
                _gridConstructor.View.QueryCellInfo(e);
                e.Handled = true;
            }
            catch (DataSourceException exception)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(exception);
                FormKill();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is DataSourceException)
                {
                    DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                    FormKill();
                }
            }
        }

        private void gridWorksheetClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            if (_gridConstructor.View.Grid.CurrentCell != null)
            {
                var currentRow = _gridConstructor.View.Grid.CurrentCell.RowIndex;
                var currentCol = _gridConstructor.View.Grid.CurrentCell.ColIndex;
                refreshCellFocus(currentRow, currentCol);
            }
            _gridConstructor.View.ClipboardCanCopy(sender, e);
            e.Handled = true;
        }

        private void refreshCellFocus(int currentRow, int currentCol)
        {
            _gridConstructor.View.Grid.CurrentCell.MoveTo(0, 0);
            _gridConstructor.View.Grid.CurrentCell.MoveTo(currentRow, currentCol);
        }

        private void save()
        {
            if (_readOnly) return;
			if (_toggleManager.IsEnabled(Toggles.WFM_Clear_Data_After_Leaving_Date_47768) 
				&& _filteredPeopleHolder.FilteredPeopleGridData.FirstOrDefault(y => y.IsTerminalDateChanged) != null)
			{
				DialogResult response = ShowYesNoMessage(UserTexts.Resources.DoYouWantToSaveChangesWithTerminalDate, Text);

				if(response.Equals(DialogResult.No))
				{
					return;
				}
				_filteredPeopleHolder.ClearIsTerminalDateChanged();
			}
			Cursor.Current = Cursors.WaitCursor;

            //Set current cell out of focus to make changes reflect to the data.
            setCurrentCellOutOfFocus();
            if (KillMode) return;
            try
            {
                // Add Person rotations and Availability to Repository
                _filteredPeopleHolder.AddRootsToRepository();

                if (!_filteredPeopleHolder.GetUnitOfWork.IsDirty())
                {
                    if (!_gridConstructor.View.ValidateBeforeSave())
                        return;

                    _filteredPeopleHolder.ResetBoldProperty();
                    _gridConstructor.View.Invalidate();
                    _filteredPeopleHolder.PersistTenantData();
                    return;
                }

                if (!_gridConstructor.View.ValidateBeforeSave())
                {
                    return;
                }

                persist();
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
                return;
            }
            if (KillMode) return;

            //Clear validate user credential collection.
            _filteredPeopleHolder.ValidateUserCredentialsCollection.Clear();

            //Clear
            _filteredPeopleHolder.ValidatePasswordPolicy.Clear();

			//View data saved.
			_gridConstructor.View.ViewDataSaved(_gridConstructor.View, new EventArgs());

            //Refresh grid control
            _gridConstructor.View.Invalidate();

            Cursor.Current = Cursors.Default;
        }

		private void notifySaveChanges()
        {
            var handler = PeopleWorksheetSaved;
            if (handler != null)
                handler(this, EventArgs.Empty);

            _globalEventAggregator.GetEvent<PeopleSaved>().Publish("");
        }

        private void notifyForceClose()
        {
            var handler = PeopleWorksheetForceClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void persist()
        {
            try
            {
                _filteredPeopleHolder.PersistAll();

                //Reset Add new records nold behaviour.
                _filteredPeopleHolder.ResetBoldProperty();

                notifySaveChanges();
            }
            catch (TooManyActiveAgentsException e)
            {
                string explanation;
                if (e.LicenseType.Equals(LicenseType.Seat))
                {
                    explanation = String.Format(CultureInfo.CurrentCulture, UserTexts.Resources.YouHaveTooManySeats,
                                 e.NumberOfLicensed);
                }
                else
                {
                    explanation = String.Format(CultureInfo.CurrentCulture, UserTexts.Resources.YouHaveTooManyActiveAgents,
                                  e.NumberOfAttemptedActiveAgents, e.NumberOfLicensed);
                }

                ShowErrorMessage(explanation, UserTexts.Resources.ErrorMessage);
                FormKill();
            }
            catch (OptimisticLockException)
            {
                ShowInformationMessage(string.Concat(
                    UserTexts.Resources.SomeoneElseHaveChanged, ". ",
                    UserTexts.Resources.YourChangesWillBeDiscarded, Environment.NewLine,
                    UserTexts.Resources.PleaseTryAgainLater),
                                        UserTexts.Resources.ErrorMessage);
                FormKill();
            }
            catch (ConstraintViolationException)
            {
                ShowInformationMessage(string.Concat(
                    UserTexts.Resources.SomeoneElseHaveChanged, ". ",
                    UserTexts.Resources.YourChangesWillBeDiscarded, Environment.NewLine,
                    UserTexts.Resources.PleaseTryAgainLater),
                                        UserTexts.Resources.ErrorMessage);
                FormKill();
            }
        }

        private void gridWorksheetCellButtonClicked(object sender, GridCellButtonClickedEventArgs e)
        {
            _gridConstructor.View.CellButtonClicked(e);
        }

        private void gridWorksheetDrawCellButton(object sender, GridDrawCellButtonEventArgs e)
        {
            _gridConstructor.View.DrawCellButton(e);
        }

        private void toolStripButtonContractClick(object sender, EventArgs e)
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    using (var optionDialog = new OptionDialog(new OptionCore(new PeopleSettingPagesProvider())))
                    {
                        optionDialog.SetUnitOfWork(uow);
                        optionDialog.Page(typeof(ContractControl));
                        DialogResult response = optionDialog.ShowDialog(this);
                        if (response == DialogResult.OK)
                        {
                            // TODO: Refresh grid data?
                        }
                    }
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
                FormKill(); // bugfix, otherwise the form does not close
            }
        }

        private void toolStripButtonContractScheduleClick(object sender, EventArgs e)
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    using (var optionDialog = new OptionDialog(new OptionCore(new PeopleSettingPagesProvider())))
                    {
                        optionDialog.SetUnitOfWork(uow);
                        optionDialog.Page(typeof(ContractScheduleControl));
                        DialogResult response = optionDialog.ShowDialog(this);
                        if (response == DialogResult.OK)
                        {
                            // TODO: Refresh grid data?
                        }
                    }
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
                FormKill(); // bugfix, otherwise the form does not close
            }
        }

        private void toolStripButtonPartTimePercentageClick(object sender, EventArgs e)
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    using (var optionDialog = new OptionDialog(new OptionCore(new PeopleSettingPagesProvider())))
                    {
                        optionDialog.SetUnitOfWork(uow);
                        optionDialog.Page(typeof(PartTimePercentageControl));
                        DialogResult response = optionDialog.ShowDialog(this);
                        if (response == DialogResult.OK)
                        {
                            // TODO: Refresh grid data?
                        }
                    }
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
                FormKill(); // bugfix, otherwise the form does not close
            }
        }

        public void RefreshView(FilteredPeopleHolder filteredPeopleHolder)
        {
            var type = _gridConstructor.View.GetType();
            var viewType = _gridConstructor.View.Type;
            var panelViewType = _panelConstructor.View.Type;

            _gridConstructor.GridViewChanging -= gridConstructorGridViewChanging;
            _gridConstructor.GridViewChanged -= gridConstructorGridViewChanged;

            _gridConstructor.View.ClearView();
            _gridConstructor.FlushCache();
            _filteredPeopleHolder = filteredPeopleHolder;
            _filteredPeopleHolder.TabControlPeopleAdmin = _tabControlPeopleAdmin;

            _gridConstructor = new GridConstructor(filteredPeopleHolder, _toggleManager, _businessRuleConfigProvider, _configReader);

            _panelConstructor.GridViewChanged -= panelConstructorGridViewChanged;
            _panelConstructor.View.ClearView();
            _panelConstructor.FlushCache();

            _peopleAdminFilterPanel.Visible = false;
            _gridConstructor.GridViewChanging += gridConstructorGridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructorGridViewChanged;

            switch (viewType)
            {
                case ViewType.PersonRotationView:
                    StateHolder.LoadRotationStateHolder(ViewType.PersonRotationView, _filteredPeopleHolder);
                    break;
                case ViewType.PersonAvailabilityView:
                    StateHolder.LoadRotationStateHolder(ViewType.PersonAvailabilityView, _filteredPeopleHolder);
                    break;
                case ViewType.PeoplePeriodView:
                    loadPeopleAdminReferences();
                    break;
            }

            _filteredPeopleHolder.LoadRuleSetBag();
            _filteredPeopleHolder.LoadBudgetGroup();
            _gridConstructor.BuildGridView(viewType);

            //why isn't this in the panel?
            if (type == typeof(SchedulePeriodGridView))
                _gridConstructor.View.SetView(shiftCategoryLimitationView);

            shiftCategoryLimitationView.SetState(filteredPeopleHolder, _gridConstructor);

            loadTrackerDescriptions();
            _filteredPeopleHolder.ResetRolesViewAdapterCollection();
            _filteredPeopleHolder.ResetPersonSkillAdapterCollection();
            _filteredPeopleHolder.ResetExternalLogOnAdapterCollection();

            _gridConstructor.View.SelectedDateChange(null, null);
            //löjligt men...
            _gridConstructor.View.SetSelectedPersons(_gridConstructor.View.GetSelectedPersons());

            _panelConstructor = new GridConstructor(filteredPeopleHolder, _toggleManager, _businessRuleConfigProvider, _configReader);
            _panelConstructor.GridViewChanged += panelConstructorGridViewChanged;
            _panelConstructor.BuildGridView(panelViewType);

            tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdminSelectedIndexChanged;
            tabControlPeopleAdmin.TabPages.Clear();
            if (viewType == ViewType.PeoplePeriodView)
            {
                var tabPage = GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.PersonSkill);
                tabPage.Dock = DockStyle.Fill;
                tabControlPeopleAdmin.TabPages.Add(tabPage);
                // Creates external log on grid view.
                var externalLogOnTabPage = new TabPageAdv(UserTexts.Resources.ExternalLogOn) { Dock = DockStyle.Fill };
                tabControlPeopleAdmin.TabPages.Add(externalLogOnTabPage);
            }
            if (viewType == ViewType.GeneralView)
            {
                var tabPage = GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.Roles);
                tabPage.Dock = DockStyle.Fill;
                tabControlPeopleAdmin.TabPages.Add(tabPage);
            }

            tabControlPeopleAdmin.SelectedIndexChanged += tabControlPeopleAdminSelectedIndexChanged;
        }

        private void peopleAdminFilterPanelLeave(object sender, EventArgs e)
        {
            _peopleAdminFilterPanel.Visible = false;
        }

        /// Handles the click event of the Find and replace menu item of the find and replace tool strip.
        private void toolStripButtonFindClick(object sender, EventArgs e)
        {
            // Sets the active tab
            _findAndReplaceForm.SetActiveFunctionality(FindAndReplaceFunctionality.FindOnly);
            // Configures the find and replace form
            _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);

            // Shows the find and replace form

            _findAndReplaceForm.Show();
            _findAndReplaceForm.Focus();
        }

        private void toolStripMenuItemSortAscClick(object sender, EventArgs e)
        {
            sort(true);
        }

        private void toolStripMenuItemSortDescClick(object sender, EventArgs e)
        {
            sort(false);
        }

        private void loadTrackerDescriptions()
        {
            if (_filteredPeopleHolder.FilteredAbsenceCollection.Count > 0)
            {
                toolStripComboBoxExTrackerDescription.Items.Clear();

                toolStripComboBoxExTrackerDescription.ComboBox.DisplayMember = "Name";

                for (int index = 0;
                     index < _filteredPeopleHolder.FilteredAbsenceCollection.Count;
                     index++)
                {
                    // Binds the scenarions to th scenario drop down list
                    toolStripComboBoxExTrackerDescription.Items.Add(
                        _filteredPeopleHolder.FilteredAbsenceCollection[index]);
                }

                toolStripComboBoxExTrackerDescription.SelectedIndex = 0;
            }
        }

        private void sort(bool isAscending)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Sots the data
			var sorting = _gridConstructor.View.Sort(isAscending).ToArray();
			_filteredPeopleHolder.SetSortedPeople(sorting);
            _gridConstructor.Sort(sorting);
            Cursor.Current = Cursors.Default;
        }

        private enum ClipboardItem
        {
            General,
            PersonPeriod,
            SchedulePeriod,
            PersonRotation,
            PersonAccount,
            PersonAvailability,
            Paste,
            PasteNew
        }

        private void instantiateEditControl()
        {
            _editControl = new EditControl();
            var editControlHost = new ToolStripControlHost(_editControl);
            toolStripExEdit.Items.Add(editControlHost);

            //Create general toolstrip button seperately to set CTRL + N behaviour.
            var generalToolStripButton = new ToolStripButton
            {
                Text = UserTexts.Resources.NewPerson,
                Tag = ClipboardItem.General.ToString()
            }
            ;
            _editControl.NewSpecialItems.Add(generalToolStripButton);

            _editControl.NewSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.NewPersonPeriod,
                Tag = ClipboardItem.PersonPeriod.ToString()
            });
            _editControl.NewSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.NewSchedulePeriod,
                Tag = ClipboardItem.SchedulePeriod.ToString()
            });
            _editControl.NewSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.NewPersonRotation,
                Tag = ClipboardItem.PersonRotation.ToString()
            });
            _editControl.NewSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.NewPersonAccount,
                Tag = ClipboardItem.PersonAccount.ToString()
            });
            _editControl.NewSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.NewPersonAvailability,
                Tag = ClipboardItem.PersonAvailability.ToString()
            });

            _editControl.NewClicked += (editControlNewClicked);
            _editControl.NewSpecialClicked += (editControlNewSpecialClicked);

            _editControl.DeleteClicked += (editControlDeleteClicked);

            //Set generalToolStripButton to add new person when press CTRL + N.
            //SetShortcut(generalToolStripButton, ((Keys.Control | Keys.N)));
            toolStripExEdit.Enabled = PrincipalAuthorization.Current().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        private static ClipboardItem getClipboarditem(ToolStripItemClickedEventArgs e)
        {
            return (ClipboardItem)Enum.Parse(typeof(ClipboardItem), (string)e.ClickedItem.Tag);
        }

        private void instantiateClipboardControl()
        {
            _clipboardControl = new ClipboardControl();
            var clipboardhost = new ToolStripControlHost(_clipboardControl);
            toolStripExClipboard.Items.Add(clipboardhost);

            _clipboardControl.CutClicked += clipboardControlCutClicked;

            _clipboardControl.CopyClicked += clipboardControlCopyClicked;

            _clipboardControl.PasteSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.Paste,
                Tag = ClipboardItem.Paste.ToString()
            });
            _clipboardControl.PasteSpecialItems.Add(new ToolStripButton
            {
                Text = UserTexts.Resources.PasteNew,
                Tag = ClipboardItem.PasteNew.ToString()
            });

            _clipboardControl.PasteSpecialClicked += clipboardControlPasteSpecialClicked;
            _clipboardControl.PasteClicked += clipboardControlPasteClicked;
            toolStripExClipboard.Enabled = PrincipalAuthorization.Current().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        private void setToolStripsToPreferredSize()
        {
            toolStripViews.Size = toolStripViews.PreferredSize;
            toolStripExEdit.Size = toolStripExEdit.PreferredSize;
            toolStripExClipboard.Size = toolStripExClipboard.PreferredSize;
            toolStripFilter.Size = toolStripFilter.PreferredSize;
            toolStripExEditing.Size = toolStripExEditing.PreferredSize;
            toolStripDatePicker.Size = toolStripDatePicker.PreferredSize;
            toolStripExSettings.Size = toolStripExSettings.PreferredSize;
        }

        private void validatePersistance(cancelSave cancelSave)
        {
            if (_readOnly) return;
            //Set current cell out of focus to make changes reflect to the data.
            setCurrentCellOutOfFocus();

            // Add Person rotations and Availability to Repository
            _filteredPeopleHolder.AddRootsToRepository();

            if (_filteredPeopleHolder.GetUnitOfWork.IsDirty())
            {
                DialogResult response = ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade, Text);
                switch (response)
                {
                    case DialogResult.No:
                        _filteredPeopleHolder.ResetBoldProperty();
                        break;

                    case DialogResult.Yes:
                        if (!_gridConstructor.View.ValidateBeforeSave())
                        {
                            cancelSave.Cancel = true;
                            return;
                        }
                        notifyForceClose();
                        // Save changes and close.
                        persist();
                        break;
                    default:
                        // Do not close the form.
                        cancelSave.Cancel = true;
                        break;
                }
            }
        }

        private class cancelSave
        {
            public bool Cancel { get; set; }
        }

        private void setCurrentCellOutOfFocus()
        {
            //------Temp solution to set out of focus from current cell.
            _gridConstructor.View.Grid.Focus();
            int rowIndex = (_gridConstructor.View.Grid.Model.CurrentCellInfo == null)
                                ? 1
                                : _gridConstructor.View.Grid.Model.CurrentCellInfo.RowIndex;
            _gridConstructor.View.Grid.CurrentCell.MoveTo(rowIndex, 0, GridSetCurrentCellOptions.BeginEndUpdate);
            _gridConstructor.View.Grid.Selections.Clear(false);
            //------
        }

        public event EventHandler PeopleWorksheetSaved;
        public event EventHandler PeopleWorksheetForceClose;

        private void toolStripButtonFilterPeopleClick(object sender, EventArgs e)
        {
            var cancelSave = new cancelSave();

            try
            {
                validatePersistance(cancelSave);

                if (cancelSave.Cancel)
                    return;

	            if (KillMode)
		            return;

	            //Refresh grid control
                _gridConstructor.View.Invalidate();

                _peopleAdminFilterPanel.Refresh();

                //Set peopleAdminFilterPanel control cisibility
                _peopleAdminFilterPanel.Visible = (!_peopleAdminFilterPanel.Visible);

                //Check for constrains.
                if (_peopleAdminFilterPanel.Visible)
                {
                    disposeAllChildGrids();
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
            }
        }

        private void disposeAllChildGrids()
        {
            //Disposes child grids of the all the views
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.GeneralView));
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.PeoplePeriodView));
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.SchedulePeriodView));
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonRotationView));
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonAvailabilityView));
            disposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonalAccountGridView));
            disposeChildGrids(_panelConstructor.FindGrid(ViewType.RolesView));
            disposeChildGrids(_panelConstructor.FindGrid(ViewType.EmptyView));
            disposeChildGrids(_panelConstructor.FindGrid(ViewType.ExternalLogOnView));
            disposeChildGrids(_panelConstructor.FindGrid(ViewType.SkillsView));
        }

        private void toolStripButtonGeneralClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _editControl.SetButtonState(EditAction.Delete,
                PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.DeletePerson));
            IList<IPerson> selectedPersons = _gridConstructor.View.GetSelectedPersons();

            // Construct general information grid.
            if (_gridConstructor.View.Type != ViewType.GeneralView)
            {
                _gridConstructor.BuildGridView(ViewType.GeneralView);
                if (KillMode) return;
                _gridConstructor.View.Grid.Cursor = Cursors.WaitCursor;

                _gridConstructor.View.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.Grid.RowCount = _gridConstructor.View.RowCount;

                //Setup for F1
                setupHelpContext(_gridConstructor.View.Grid);

                _gridConstructor.View.Invalidate();

                // Sets the domain finder
                _domainFinder = new PeopleDomainFinder(_filteredPeopleHolder);
                // Configures the find and replace form
                _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);

                if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword))
                {
                    togglePanelContent(false);
                    splitContainerWorksheet.Panel2Collapsed = false;
                    splitContainerWorksheet.SplitterDistance = splitContainerWorksheet.Width - 300;
                    // Clears all tab pages and recreate.
                    tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdminSelectedIndexChanged;
                    tabControlPeopleAdmin.TabPages.Clear();

                    // Creates skills grid view.
                    _panelConstructor.BuildGridView(ViewType.RolesView);
                    TabPageAdv rolesTabPage =
                        GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.Roles);

                    //Setup for F1
                    setupHelpContext(_panelConstructor.View.Grid);

                    rolesTabPage.Dock = DockStyle.Fill;
                    tabControlPeopleAdmin.TabPages.Add(rolesTabPage);
                    Cursor.Current = Cursors.WaitCursor;
                }
                else
                {
                    splitContainerWorksheet.Panel2Collapsed = true;

                    _panelConstructor.BuildGridView(ViewType.EmptyView);
                    _panelConstructor.View.Invalidate();

                    // Clear tab pages.
                    tabControlPeopleAdmin.TabPages.Clear();
                }
                _gridConstructor.View.SetSelectedPersons(selectedPersons);
                _gridConstructor.View.Grid.Cursor = Cursors.Default;
                Cursor.Current = Cursors.Default;
            }
        }

        private void setCorrectView(ViewType mainView, ViewType panelView, bool togglePanelContent, bool ensureRightPanelVisible, int rightPanelWidth)
        {
            _editControl.SetButtonState(EditAction.Delete, true);
            if (_gridConstructor.View.Type != mainView)
            {
                Cursor.Current = Cursors.WaitCursor;
                _gridConstructor.View.Grid.Cursor = Cursors.WaitCursor;
                _editControl.PanelItem.Enabled = true;
                IList<IPerson> selectedPersons = _gridConstructor.View.GetSelectedPersons();
                _gridConstructor.View.Grid.CurrentCell.MoveTo(_gridConstructor.View.Grid.CurrentCell.RowIndex, 0);
                _gridConstructor.BuildGridView(mainView);
                _gridConstructor.View.Grid.Cursor = Cursors.WaitCursor;
                _gridConstructor.View.SetView(shiftCategoryLimitationView);
                setupHelpContext(_gridConstructor.View.Grid);
                _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);
                this.togglePanelContent(togglePanelContent);

                splitContainerWorksheet.Panel2Collapsed = !ensureRightPanelVisible;
                if (ensureRightPanelVisible)
                {
                    splitContainerWorksheet.SplitterDistance = splitContainerWorksheet.Width - rightPanelWidth;
                }

                _panelConstructor.BuildGridView(panelView);
                setupHelpContext(_panelConstructor.View.Grid);

                _gridConstructor.View.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.Grid.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.SelectedDateChange(null, null);
                _gridConstructor.View.SetSelectedPersons(selectedPersons);
                _gridConstructor.View.Grid.Cursor = Cursors.Default;
                Cursor.Current = Cursors.Default;
            }
        }

        private void toolStripButtonPeoplePeriodsClick(object sender, EventArgs e)
        {

            if ((StateHolder.ContractCollection.Count == 0) || (StateHolder.PartTimePercentageCollection.Count == 0)
                || (StateHolder.ContractScheduleCollection.Count == 0) ||
                (_filteredPeopleHolder.SiteTeamAdapterCollection.Count == 0))
            {
                ShowWarningMessage(UserTexts.Resources.PersonPeriodCouldNotLoad, Text);
                return;
            }

            if (_gridConstructor.View.Type != ViewType.PeoplePeriodView)
            {
                _domainFinder = new PeoplePeriodDomainFinder(_filteredPeopleHolder);

                setCorrectView(ViewType.PeoplePeriodView, ViewType.SkillsView, false, true, 300);

                // Clears all tab pages and recreate.
                tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdminSelectedIndexChanged;
                tabControlPeopleAdmin.TabPages.Clear();

                var skillsTabPage = GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.PersonSkill);
                skillsTabPage.Dock = DockStyle.Fill;
                tabControlPeopleAdmin.TabPages.Add(skillsTabPage);

                // Creates external log on grid view.
                var externalLogOnTabPage = new TabPageAdv(UserTexts.Resources.ExternalLogOn) { Dock = DockStyle.Fill };
                tabControlPeopleAdmin.TabPages.Add(externalLogOnTabPage);
                tabControlPeopleAdmin.SelectedIndexChanged += tabControlPeopleAdminSelectedIndexChanged;
            }
        }

        private void toolStripButtonSchedulePeriodsClick(object sender, EventArgs e)
        {
            try
            {
                if (_gridConstructor.View.Type != ViewType.SchedulePeriodView)
                {
                    _domainFinder = new PeopleSchedulePeriodDomainFinder(_filteredPeopleHolder);

                    setCorrectView(ViewType.SchedulePeriodView, ViewType.EmptyView, true, true, shiftCategoryLimitationView.ViewWidth());

                    if (KillMode)
                        return;

                    tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdminSelectedIndexChanged;
                    tabControlPeopleAdmin.TabPages.Clear();
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
            }
        }

        private void togglePanelContent(bool showShiftCategoryView)
        {
            if (!showShiftCategoryView)
                shiftCategoryLimitationView.SendToBack();
            else
                shiftCategoryLimitationView.BringToFront();
        }

        private void toolStripButtonPersonRotationClick(object sender, EventArgs e)
        {
            if (StateHolder.AllRotations.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.RotationsCouldNotBeLoaded, Text);
                return;
            }

            StateHolder.LoadRotationStateHolder(ViewType.PersonRotationView, _filteredPeopleHolder);
            _domainFinder = new PersonRotationDomainFinder(_filteredPeopleHolder);
            setCorrectView(ViewType.PersonRotationView, ViewType.EmptyView, false, false, 0);
        }

        private void toolStripButtonPersonAccountsClick(object sender, EventArgs e)
        {
            if (_filteredPeopleHolder.FilteredAbsenceCollection.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.AbsenceCouldNotBeLoaded, Text);
                return;
            }
            _domainFinder = new PersonAccountDomainFinder(_filteredPeopleHolder);
            setCorrectView(ViewType.PersonalAccountGridView, ViewType.EmptyView, false, false, 0);
        }

        private void toolStripButtonPersonAvailabilityClick(object sender, EventArgs e)
        {
            if (StateHolder.AllAvailabilities.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.AvailabilitiesCouldNotBeLoaded, Text);
                return;
            }

            StateHolder.LoadRotationStateHolder(ViewType.PersonAvailabilityView, _filteredPeopleHolder);
            _domainFinder = new PersonAvailabilityDomainFinder(_filteredPeopleHolder);
            setCorrectView(ViewType.PersonAvailabilityView, ViewType.EmptyView, false, false, 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void editControlNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (getClipboarditem(e))
                {
                    case ClipboardItem.General:
                        toolStripButtonGeneral.PerformClick();
                        break;
                    case ClipboardItem.PersonPeriod:
                        toolStripButtonPeoplePeriods.PerformClick();
                        break;
                    case ClipboardItem.SchedulePeriod:
                        toolStripButtonSchedulePeriods.PerformClick();
                        break;
                    case ClipboardItem.PersonRotation:
                        toolStripButtonPersonRotation.PerformClick();
                        break;
                    case ClipboardItem.PersonAccount:
                        toolStripButtonPersonAccounts.PerformClick();
                        break;
                    case ClipboardItem.PersonAvailability:
                        toolStripButtonPersonAvailability.PerformClick();
                        break;
                }
                _gridConstructor.View.AddNewGridRow<EventArgs>(sender, e);
            }
            catch (Exception ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
            }
        }

        private void editControlNewClicked(object sender, EventArgs e)
        {
            _gridConstructor.View.AddNewGridRow(sender, e);
        }

        private void editControlDeleteClicked(object sender, EventArgs e)
        {
            if ((_gridConstructor.View.Grid.Model.SelectedRanges == null) ||
                (_gridConstructor.View.Grid.Model.SelectedRanges.Count == 0))
            {
                return;
            }
            _gridConstructor.View.DeleteSelectedGridRows(sender, e);
        }

        private void clipboardControlCutClicked(object sender, EventArgs e)
        {
            if ((_gridConstructor.View.CurrentGrid.Model.SelectedRanges == null) ||
                (_gridConstructor.View.CurrentGrid.Model.SelectedRanges.Count == 0))
            {
                return;
            }

            if (_gridConstructor.View.Grid.CutPaste.CanCut())
                _gridConstructor.View.Grid.CutPaste.Cut();
        }

        private void clipboardControlPasteClicked(object sender, EventArgs e)
        {
            bool canPaste = _gridConstructor.View.CurrentGrid.CutPaste.CanPaste();
            if (canPaste) _gridConstructor.View.CurrentGrid.CutPaste.Paste();
        }

        private void clipboardControlPasteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (getClipboarditem(e))
            {
                case ClipboardItem.Paste:
                    bool canPaste = _gridConstructor.View.CurrentGrid.CutPaste.CanPaste();
                    if (canPaste) _gridConstructor.View.CurrentGrid.CutPaste.Paste();
                    break;
                case ClipboardItem.PasteNew:
                    _gridConstructor.View.AddNewGridRowFromClipboard<EventArgs>(sender, e);
                    break;
            }
        }

        private void clipboardControlCopyClicked(object sender, EventArgs e)
        {
            if (_gridConstructor.View.CurrentGrid.Model.SelectedRanges == null ||
                _gridConstructor.View.CurrentGrid.Model.SelectedRanges.Count == 0)
            {
                return;
            }
            bool canCopy = _gridConstructor.View.CurrentGrid.CutPaste.CanCopy();
            if (canCopy) _gridConstructor.View.CurrentGrid.CutPaste.Copy();
        }

        private void tabControlPeopleAdminSelectedIndexChanged(object sender, EventArgs e)
        {

            //if tab control is collapsed then return. 
            if (splitContainerWorksheet.Panel2Collapsed) return;

            if (_panelConstructor.View.Type != ViewType.ExternalLogOnView)
            {
                if (tabControlPeopleAdmin.SelectedTab.Controls.Count == 0)
                {
                    _panelConstructor.BuildGridView(ViewType.ExternalLogOnView);
                    GridConstructor.WrapWithTabPageExternal(_panelConstructor.View.Grid, tabControlPeopleAdmin.SelectedTab, tableLayoutPanel1);
                    //Setup for F1
                    setupHelpContext(_panelConstructor.View.Grid);
                }
                else
                {
                    _panelConstructor.BuildGridView(ViewType.ExternalLogOnView);
                    _panelConstructor.View.RowCount = _filteredPeopleHolder.ExternalLogOnViewAdapterCollection.Count;
                    _panelConstructor.View.Grid.RowCount = _panelConstructor.View.RowCount;
                    _panelConstructor.View.Invalidate();
                    setupHelpContext(_panelConstructor.View.Grid);
                }

                panel1.Visible = true;
            }
            else
            {
                panel1.Visible = false;
                _panelConstructor.BuildGridView(ViewType.SkillsView);
                _panelConstructor.View.RowCount = _filteredPeopleHolder.PersonSkillViewAdapterCollection.Count;
                _panelConstructor.View.Grid.RowCount = _panelConstructor.View.RowCount;
                _panelConstructor.View.Invalidate();
            }
        }

        private void peopleWorksheetFormClosing(object sender, FormClosingEventArgs e)
        {
            if (KillMode)
                return;
            if (_readOnly) return;
            var cancelSave = new cancelSave();
            validatePersistance(cancelSave);
            notifyForceClose();
            e.Cancel = cancelSave.Cancel;
        }

        private void peopleWorksheetFormClosed(object sender, FormClosedEventArgs e)
        {
			_numberOfOpened--;
			_mainWindow.Activate();
            unregisterEventsForFormKill();

            disposeAllChildGrids();
            if (_filteredPeopleHolder != null)
            {
                _filteredPeopleHolder.Dispose();
                _filteredPeopleHolder = null;
            }
            if (_findAndReplaceForm != null)
            {
                _findAndReplaceForm.Dispose();
                _findAndReplaceForm = null;
            }
            if (_peopleAdminFilterPanel != null)
            {
                _peopleAdminFilterPanel.Dispose();
                _peopleAdminFilterPanel = null;
            }
	        if (_stateHolder != null && _numberOfOpened == 0)
	        {
		        _stateHolder.Dispose();
		        _stateHolder = null;
	        }
	        if (_gridConstructor != null)
            {
                _gridConstructor.Dispose();
                _gridConstructor = null;
            }
            if (_panelConstructor != null)
            {
                _panelConstructor.Dispose();
                _panelConstructor = null;
            }

	        backStageButton1.Click -= backStageButton1Click;
	        backStageButton2.Click -= backStageButton2Click;
	        backStageButton3.Click -= backStageButton3Click;
	        backStageButton4.Click -= backStageButton4Click;
	        toolStripButtonMainSave.Click -= toolStripButtonMainSaveClick;
	        toolStripButtonFilterPeople.Click -= toolStripButtonFilterPeopleClick;
	        toolStripButtonGeneral.Click -= toolStripButtonGeneralClick;
	        toolStripButtonPeoplePeriods.Click -= toolStripButtonPeoplePeriodsClick;
	        toolStripButtonSchedulePeriods.Click -= toolStripButtonSchedulePeriodsClick;
	        toolStripButtonPersonRotation.Click -= toolStripButtonPersonRotationClick;
	        toolStripButtonPersonAccounts.Click -= toolStripButtonPersonAccountsClick;
	        toolStripButtonPersonAvailability.Click -= toolStripButtonPersonAvailabilityClick;
	        toolStripButtonFind.Click -= toolStripButtonFindClick;
	        toolStripMenuItemSortAsc.Click -= toolStripMenuItemSortAscClick;
	        toolStripMenuItemSortDesc.Click -= toolStripMenuItemSortDescClick;
	        toolStripButtonClosePreviousPeriod.Click -=toolStripButtonClosePreviousPeriodClick;
	        toolStripButtonContract.Click -= toolStripButtonContractClick;
	        toolStripButtonContractSchedule.Click -= toolStripButtonContractScheduleClick;
	        toolStripButtonPartTimePercentage.Click -= toolStripButtonPartTimePercentageClick;

	        toolStripComboBoxExTrackerDescription.SelectedIndexChanged -= toolStripComboBoxExTrackerDescriptionSelectedIndexChanged;

			_toggleManager = null;
            _clipboardControl = null;
            _container = null;
            _mainWindow = null;
            _domainFinder = null;
            _globalEventAggregator = null;
            _panelConstructor = null;
            tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdminSelectedIndexChanged;
            tabControlPeopleAdmin.TabPages.Clear();
	        if (_tabControlPeopleAdmin != null)
	        {
				_tabControlPeopleAdmin.Dispose();
		        _tabControlPeopleAdmin = null;
	        }
            _editControl = null;
            if (shiftCategoryLimitationView != null)
            {
                shiftCategoryLimitationView.Dispose();
                shiftCategoryLimitationView = null;
            }
            splitContainerWorksheet.Panel1.Controls.Clear();
            splitContainerWorksheet.Panel2.Controls.Clear();
            splitContainerWorksheet = null;
            _dateNavigatePeriods.SelectedDateChanged -= dateNavigatePeriodsSelectedDateChanged;
            _dateNavigatePeriods = null;
            toolStripDatePicker.Items.Clear();
            toolStripDatePicker = null;

            peopleRibbon = null;
        }

        private void peopleWorksheetShown(object sender, EventArgs e)
        {
            //Set toolStrips to preferred Size.
            setToolStripsToPreferredSize();

            _peopleAdminFilterPanel.BringToFront();

            // Shows default view.
            if (KillMode) return;
            toolStripButtonGeneral.PerformClick();

            //Load all other views relavant references.
            if (KillMode) return;
            loadPeopleAdminReferences();
            _filteredPeopleHolder.LoadRuleSetBag();
            _filteredPeopleHolder.LoadBudgetGroup();
            peopleRibbon.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
            _editControl.ToolStripButtonDelete.Enabled =
               PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.DeletePerson);
        }

        private void loadPeopleAdminReferences()
        {
            // Load all contract schedules (to show on the combo boxes).
            StateHolder.LoadContractSchedules(_filteredPeopleHolder);

            // Load all part-time percentages (to show on the combo boxes).
            StateHolder.LoadPartTimePercentages(_filteredPeopleHolder);

            //Load all contracts available (to show on the combo boxes).
            StateHolder.LoadContracts(_filteredPeopleHolder);
        }

        private void toolStripComboBoxExTrackerDescriptionSelectedIndexChanged(object sender, EventArgs e)
        {
            refilterOnTracker();
        }

        private void refilterOnTracker()
        {
            if (_gridConstructor.CurrentView == ViewType.PersonalAccountGridView)
            {
                IAbsence currentType = getPersonAccountAbsenceType();

                var trackerTypeChangeEventArg = new SelectedItemChangeBaseEventArgs<IAbsence>(currentType);
                _gridConstructor.View.TrackerDescriptionChanged(this, trackerTypeChangeEventArg);
            }
        }

        private void disposeChildGrids(GridViewBase view)
		{
			view?.DisposeChildGrids();
		}

        private void configureAndShowFindAndReplaceForm()
        {
            if (_findAndReplaceForm == null)
            {
                _findAndReplaceForm = new FindAndReplaceForm(FindAndReplaceFunctionality.All,
                                                                  FindOption.All);
            }

            switch (_gridConstructor.CurrentView)
            {
                case ViewType.GeneralView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PeopleDomainFinder(_filteredPeopleHolder));
                    break;

                case ViewType.PeoplePeriodView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PeoplePeriodDomainFinder(_filteredPeopleHolder));
                    break;

                case ViewType.SchedulePeriodView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PeopleSchedulePeriodDomainFinder(_filteredPeopleHolder));
                    break;

                case ViewType.PersonRotationView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PersonRotationDomainFinder(_filteredPeopleHolder));
                    break;

                case ViewType.PersonalAccountGridView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PersonAccountDomainFinder(_filteredPeopleHolder));
                    break;

                case ViewType.PersonAvailabilityView:
                    _findAndReplaceForm.ConfigureSearchFunctionality(
                        _gridConstructor.View.Grid, new PersonAvailabilityDomainFinder(_filteredPeopleHolder));
                    break;
            }

            _findAndReplaceForm.Size = new Size(460, 345);
            _findAndReplaceForm.Show();
            _findAndReplaceForm.Focus();
        }

        private void setupHelpContext(GridControl grid)
        {
            RemoveControlHelpContext(grid);
            AddControlHelpContext(grid);
            grid.Focus();
        }

        private void externalFilteringTextBoxTextChanged(object sender, EventArgs e)
        {
            var externalPersonList = _filteredPeopleHolder.ExternalLogOnViewAdapterCollection;
            var filterValue = externalFilteringTextBox.Text;

            var cultureInfo = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
            var lowerSearchText = filterValue.ToLower(cultureInfo);
            IList<ExternalLogOnModel> filteredPersons = (from externalLogOn in externalPersonList
                                                         where
                                                         externalLogOn.ExternalLogOn.AcdLogOnName.ToLower(cultureInfo).Contains(lowerSearchText) ||
                                                         externalLogOn.ExternalLogOn.AcdLogOnName.ToLower(cultureInfo).Replace(",", "").Contains(lowerSearchText) ||
                                                         externalLogOn.ExternalLogOn.AcdLogOnName.ToLower(cultureInfo).Contains(lowerSearchText)
                                                         select externalLogOn).ToList();

            _filteredPeopleHolder.SetFilteredExternalLogOnCollection(filteredPersons);
            _panelConstructor.View.PrepareView();
            _panelConstructor.View.Invalidate();
        }

        private void toolStripButtonClosePreviousPeriodClick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            if (_gridConstructor.View.Type == ViewType.PersonalAccountGridView)
            {
                DateOnly selectedDate = _filteredPeopleHolder.SelectedDate;
                IAbsence selectedAbsence = null;
                if (toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem != null)
                    selectedAbsence = (IAbsence)toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem;

                ReadOnlyCollection<IPersonAccountModel> selectedPersonAccountModels =
                    ((PersonalAccountGridView)_gridConstructor.View).GetSelectedPersonAccounts;

                IPersonAccountCloser personAccountCloser = new PersonAccountCloser();

                foreach (IPersonAccountModel personAccountModel in selectedPersonAccountModels)
                {
                    IPersonAccountCollection personAccountCollection = personAccountModel.Parent;
                    personAccountCloser.ClosePersonAccount(personAccountCollection, selectedAbsence, selectedDate);
                }
                refreshAfterMessageBroker();
            }

            if (_gridConstructor.View.Type == ViewType.SchedulePeriodView)
            {
                //Call Closemethod for scheduleperiod
                ((SchedulePeriodGridView)_gridConstructor.View).OnClosePreviousPeriod();

                _gridConstructor.View.Grid.Refresh();
                _gridConstructor.View.RefreshChildGrids();

            }

            Cursor = Cursors.Default;
        }

        private void backStageButton1Click(object sender, EventArgs e)
        {
            save();
        }

        private void backStageButton2Click(object sender, EventArgs e)
        {
            Close();
        }

        private void backStageButton3Click(object sender, EventArgs e)
        {
            var toggleManager = _container.Resolve<IToggleManager>();
            try
            {
                var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(toggleManager, _businessRuleConfigProvider, _configReader)));
                settings.Show();
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
            }
        }

        private void backStageButton4Click(object sender, EventArgs e)
        {
            if (!CloseAllOtherForms(this)) return;

            Close();

            ////this canceled
            if (Visible)
                return;
            Application.Exit();
        }

        private void backStageButton4VisibleChanged(object sender, EventArgs e)
        {
            backStageButton4.Location = new Point(0, 154);
        }

        private void toolStripButtonMainSaveClick(object sender, EventArgs e)
        {
            save();
        }
    }
}
