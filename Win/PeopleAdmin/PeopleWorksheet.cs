using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.PeopleAdmin.Views;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;
using GridConstructor = Teleopti.Ccc.Win.PeopleAdmin.Views.GridConstructor;
using ViewType = Teleopti.Ccc.Win.PeopleAdmin.Views.ViewType;

namespace Teleopti.Ccc.Win.PeopleAdmin
{
    public partial class PeopleWorksheet : BaseRibbonForm
    {       
        private readonly PeopleAdminFilterPanel _peopleAdminFilterPanel;

        // Instantiates the find and replace form
        private FindAndReplaceForm _findAndReplaceForm =
            new FindAndReplaceForm(FindAndReplaceFunctionality.FindOnly, FindOption.All);

        //Holds the domain finder object - by default people domain finder
        private IDomainFinder _domainFinder;
        private readonly DateNavigateControl _dateNavigatePeriods;
        private EditControl _editControl;
        private ClipboardControl _clipboardControl;
        private GridConstructor _gridConstructor;
        private GridConstructor _panelConstructor;
        private FilteredPeopleHolder _filteredPeopleHolder;
        private readonly IEventAggregator _globalEventAggregator;

        private static WorksheetStateHolder _stateHolder;
        private TabControlAdv _tabControlPeopleAdmin;
        private bool _readOnly;
        private ILifetimeScope _container;
        
        protected PeopleWorksheet()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                ColorHelper.SetRibbonQuickAccessTexts(ribbonControlWorksheet);
            }
            // Set colors, themes on controls.
            SetColors();
            SetPermissionOnControls();
            if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
                Icon = Resources.people;
            _readOnly = !PrincipalAuthorization.Instance().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public PeopleWorksheet(WorksheetStateHolder state, FilteredPeopleHolder filteredPeopleHolder, IEventAggregator globalEventAggregator, IComponentContext componentContext)
            : this()
        {
            if (filteredPeopleHolder == null) throw new ArgumentNullException("filteredPeopleHolder");
            _filteredPeopleHolder = filteredPeopleHolder;
            _globalEventAggregator = globalEventAggregator;
  //          _componentContext = componentContext;
            var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
            _container = lifetimeScope.BeginLifetimeScope();
            
            _tabControlPeopleAdmin = tabControlPeopleAdmin;
            _filteredPeopleHolder.TabControlPeopleAdmin = _tabControlPeopleAdmin;

            _gridConstructor = new GridConstructor(filteredPeopleHolder);
            _panelConstructor = new GridConstructor(filteredPeopleHolder);
            _domainFinder = new PeopleDomainFinder(filteredPeopleHolder);
            shiftCategoryLimitationView.SetState(filteredPeopleHolder, _gridConstructor);

            _dateNavigatePeriods = new DateNavigateControl();
            _dateNavigatePeriods.SetSelectedDate(_filteredPeopleHolder.SelectedDate);
            _dateNavigatePeriods.SelectedDateChanged += dateNavigatePeriods_SelectedDateChanged;

            var hostDatePicker = new ToolStripControlHost(_dateNavigatePeriods);
            toolStripDatePicker.Items.Add(hostDatePicker);

            _stateHolder = state;

            _gridConstructor.GridViewChanging += gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructor_GridViewChanged;
            _panelConstructor.GridViewChanged += panelConstructor_GridViewChanged;
            toolStripButtonMainNew.Click += toolStripButtonMainNew_Click;

            // Clear cache available.
            _gridConstructor.FlushCache();
            _panelConstructor.FlushCache();

            // Show the default view as General View.
            _gridConstructor.BuildGridView(ViewType.EmptyView);

            //initialize filter popup
            _peopleAdminFilterPanel = new PeopleAdminFilterPanel(_filteredPeopleHolder, this, _container) { Location = new Point(229, 130) };
            _peopleAdminFilterPanel.BringToFront();
            _peopleAdminFilterPanel.Leave += peopleAdminFilterPanel_Leave;
            Controls.Add(_peopleAdminFilterPanel);
            _peopleAdminFilterPanel.Visible = false;

            InstantiateEditControl();
            InstantiateClipboardControl();
            LoadTrackerDescriptions();
            toolStripComboBoxExTrackerDescription.Enabled = false;


            Cursor.Current = Cursors.Default;
            toolStripButtonMainSave.Enabled = !_readOnly;
        }

        private void unregisterEventsForFormKill()
        {
            _gridConstructor.GridViewChanging -= gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged -= gridConstructor_GridViewChanged;
            _panelConstructor.GridViewChanged -= panelConstructor_GridViewChanged;
        }

        protected void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
            ColorHelper.SetTabControlTheme(tabControlPeopleAdmin);
            BrushInfo coloredBrush = ColorHelper.ControlGradientPanelBrush();
            splitContainerWorksheet.BackgroundColor = coloredBrush;
            splitContainerWorksheet.Panel1.BackgroundColor = coloredBrush;
            splitContainerWorksheet.Panel2.BackgroundColor = coloredBrush;
            Color ribbonContextTabColor = ColorHelper.RibbonContextTabColor();
            int tabGroupsCount = ribbonControlWorksheet.TabGroups.Count;
            for (int i = 0; i < tabGroupsCount; i++)
            {
                ribbonControlWorksheet.TabGroups[i].Color = ribbonContextTabColor;
            }
        }

        private void SetPermissionOnControls()
        {
            toolStripButtonSystemOptions.Enabled =
                PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
        	toolStripTabItem2.Visible = toolStripButtonSystemOptions.Enabled;
        }

        private void dateNavigatePeriods_SelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
        {
            if (_gridConstructor.View == null)
                return;

            _filteredPeopleHolder.SelectedDate = e.Value;
            _gridConstructor.View.SelectedDateChange(sender, e);
			_panelConstructor.View.Invalidate();
            RefilterOnTracker();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.F:
                        ConfigureAndShowFindAndReplaceForm();
                        break;

                    case Keys.Control | Keys.S:
		                toolStripButtonMainSave_MouseUp(this, null);
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
				    EditControl_NewSpecialClicked(theButton, theEvent);
			    }
		    }
	    }

	    private void panelConstructor_GridViewChanged(object sender, EventArgs e)
        {
            InParameter.NotNull("sender", sender);

            var view = (GridViewBase)sender;

            if (!_panelConstructor.IsCached)
            {
                view.Grid.ResetVolatileData();

                // Bind events for the grid.
                view.Grid.QueryCellInfo += gridPanel_QueryCellInfo;
                view.Grid.SaveCellInfo += gridPanel_SaveCellInfo;
                view.Grid.KeyDown += gridPanel_KeyDown;
            }
        }

        private void gridPanel_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            _panelConstructor.View.QueryCellInfo(e);
            e.Handled = true;
        }

        private void gridPanel_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if(_readOnly) return;
            _panelConstructor.View.SaveCellInfo(sender, e);

            _gridConstructor.View.Invalidate();
            _panelConstructor.View.Invalidate();

            e.Handled = true;
        }

        private void gridPanel_KeyDown(object sender, KeyEventArgs e)
        {
            _panelConstructor.View.KeyDown(e);
        }

        private void gridConstructor_GridViewChanging(object sender, EventArgs e)
        {
            splitContainerWorksheet.Panel1.Controls.Clear();
        }

        private void gridConstructor_GridViewChanged(object sender, EventArgs e)
        {

            InParameter.NotNull("sender", sender);

            GridViewBase view = (GridViewBase)sender;

            if (!_gridConstructor.IsCached)
            {
                view.Grid.ResetVolatileData();

                // Bind events for the grid.
                view.Grid.QueryCellInfo += gridWorksheet_QueryCellInfo;
                view.Grid.SaveCellInfo += gridWorksheet_SaveCellInfo;
                view.Grid.ClipboardPaste += gridWorksheet_ClipboardPaste;
                view.Grid.SelectionChanged += gridWorksheet_SelectionChanged;
                view.Grid.KeyDown += gridWorksheet_KeyDown;

                // Periods specific events.
                view.Grid.CellButtonClicked += gridWorksheet_CellButtonClicked;
                view.Grid.DrawCellButton += gridWorksheet_DrawCellButton;
                view.Grid.CellClick += gridWorksheet_CellClick;
                view.Grid.CellDoubleClick += gridWorksheet_CellDoubleClick;
                view.Grid.ClipboardCanCopy += gridWorksheet_ClipboardCanCopy;
                view.Grid.ClipboardCanPaste += Grid_ClipboardCanPaste;
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

        private IAbsence getPersonAccountAbsenceType()
        {
            IAbsence currentType = null;

            if (toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem != null)
            {
                currentType = (IAbsence) toolStripComboBoxExTrackerDescription.ComboBox.SelectedItem;
            }

            _filteredPeopleHolder.SelectedPersonAccountAbsenceType = currentType;
            return currentType;
        }

        private void RefreshAfterMessageBroker()
        {
            _gridConstructor.View.Grid.ResetVolatileData();
            _gridConstructor.View.Grid.Invalidate();

            _gridConstructor.View.RefreshParentGrid();

            _gridConstructor.View.RefreshChildGrids();
        }

        private void Grid_ClipboardCanPaste(object sender, GridCutPasteEventArgs e)
        {
            _gridConstructor.View.ClipboardCanPaste(sender, e);
            e.Handled = true;
        }

        public static WorksheetStateHolder StateHolder
        {
            get { return _stateHolder; }
        }


        private void gridWorksheet_KeyDown(object sender, KeyEventArgs e)
        {
            _gridConstructor.View.KeyDown(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void gridWorksheet_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
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

        private void gridWorksheet_SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            _gridConstructor.View.SaveCellInfo(sender, e);
            e.Handled = true;
        }

        private void gridWorksheet_CellClick(object sender, GridCellClickEventArgs e)
        {
            _gridConstructor.View.CellClick(sender, e);
        }

        private void gridWorksheet_CellDoubleClick(object sender, GridCellClickEventArgs e)
        {
            _gridConstructor.View.CellDoubleClick(sender, e);
        }

        private void gridWorksheet_ClipboardPaste(object sender, GridCutPasteEventArgs e)
        {
            _gridConstructor.View.ClipboardPaste(e);
            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void gridWorksheet_QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
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
                if (ex.InnerException != null && ex.InnerException is DataSourceException)
                {
                    DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                    FormKill();
                }
            }
        }



        private void gridWorksheet_ClipboardCanCopy(object sender, GridCutPasteEventArgs e)
        {
            if (_gridConstructor.View.Grid.CurrentCell != null)
            {
                var currentRow = _gridConstructor.View.Grid.CurrentCell.RowIndex;
                var currentCol = _gridConstructor.View.Grid.CurrentCell.ColIndex;
                RefreshCellFocus(currentRow, currentCol);
            }
            _gridConstructor.View.ClipboardCanCopy(sender, e);
            e.Handled = true;
        }

        private void RefreshCellFocus(int currentRow, int currentCol)
        {
            _gridConstructor.View.Grid.CurrentCell.MoveTo(0, 0);
            _gridConstructor.View.Grid.CurrentCell.MoveTo(currentRow, currentCol);
        }

		private void toolStripButtonMainSave_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (_readOnly) return;
		
			Cursor.Current = Cursors.WaitCursor;

			//Set current cell out of focus to make changes reflect to the data.
			SetCurrentCellOutOfFocus();
			if (KillMode) return;
			try
			{
				// Add Person rotations and Availability to Repository
				_filteredPeopleHolder.AddRootsToRepository();

                if (!_filteredPeopleHolder.GetUnitOfWork.IsDirty())
				{
					_filteredPeopleHolder.ResetBoldProperty();
					_gridConstructor.View.Invalidate();
					return;
				}

				if (!_gridConstructor.View.ValidateBeforeSave())
                {
					return;
                }

				Persist();
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

        private void Persist()
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

        private void gridWorksheet_CellButtonClicked(object sender, GridCellButtonClickedEventArgs e)
        {
            _gridConstructor.View.CellButtonClicked(e);
        }

        private void gridWorksheet_DrawCellButton(object sender, GridDrawCellButtonEventArgs e)
        {
            _gridConstructor.View.DrawCellButton(e);
        }

        private void toolStripButtonMainHelp_Click(object sender, EventArgs e)
        {
            ViewBase.ShowHelp(this,false);
        }
        private void toolStripButtonContract_Click(object sender, EventArgs e)
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

        private void toolStripButtonContractSchedule_Click(object sender, EventArgs e)
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

        private void toolStripButtonPartTimePercentage_Click(object sender, EventArgs e)
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

            _gridConstructor.GridViewChanging -= gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged -= gridConstructor_GridViewChanged;

            _gridConstructor.View.ClearView();
            _gridConstructor.FlushCache();
            _filteredPeopleHolder = filteredPeopleHolder;
            _filteredPeopleHolder.TabControlPeopleAdmin = _tabControlPeopleAdmin;

            _gridConstructor = new GridConstructor(filteredPeopleHolder);

            _panelConstructor.GridViewChanged -= panelConstructor_GridViewChanged;
            _panelConstructor = new GridConstructor(filteredPeopleHolder);
            _panelConstructor.GridViewChanged += panelConstructor_GridViewChanged;
            _panelConstructor.BuildGridView(panelViewType);

            _peopleAdminFilterPanel.Visible = false;
            _gridConstructor.GridViewChanging += gridConstructor_GridViewChanging;
            _gridConstructor.GridViewChanged += gridConstructor_GridViewChanged;

            switch (viewType)
            {
                case ViewType.PersonRotationView:
                    StateHolder.LoadRotationStateHolder(ViewType.PersonRotationView, _filteredPeopleHolder);
                    break;
                case ViewType.PersonAvailabilityView:
                    StateHolder.LoadRotationStateHolder(ViewType.PersonAvailabilityView, _filteredPeopleHolder);
                    break;
                case ViewType.PeoplePeriodView:
                    LoadPeopleAdminReferences();
                    break;
            }

            _filteredPeopleHolder.LoadRuleSetBag();
            _filteredPeopleHolder.LoadBudgetGroup();
            _gridConstructor.BuildGridView(viewType);

            //why isn't this in the panel?
            if (type == typeof(SchedulePeriodGridView))
                _gridConstructor.View.SetView(shiftCategoryLimitationView);

			shiftCategoryLimitationView.SetState(filteredPeopleHolder, _gridConstructor);

            LoadTrackerDescriptions();
            _filteredPeopleHolder.ResetRolesViewAdapterCollection();
            _filteredPeopleHolder.ResetPersonSkillAdapterCollection();
            _filteredPeopleHolder.ResetExternalLogOnAdapterCollection();

            _gridConstructor.View.SelectedDateChange(null, null);
            //löjligt men...
            _gridConstructor.View.SetSelectedPersons(_gridConstructor.View.GetSelectedPersons());
        }

        private void peopleAdminFilterPanel_Leave(object sender, EventArgs e)
        {
            _peopleAdminFilterPanel.Visible = false;
        }

        /// Handles the click event of the Find and replace menu item of the find and replace tool strip.
        private void toolStripButtonFind_Click(object sender, EventArgs e)
        {
            // Sets the active tab
            _findAndReplaceForm.SetActiveFunctionality(FindAndReplaceFunctionality.FindOnly);
            // Configures the find and replace form
            _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);

            // Shows the find and replace form
			
            _findAndReplaceForm.Show();
			_findAndReplaceForm.Focus();
        }

        private void toolStripMenuItemSortAsc_Click(object sender, EventArgs e)
        {
            Sort(true);
        }


        private void toolStripMenuItemSortDesc_Click(object sender, EventArgs e)
        {
            Sort(false);
        }

        private void toolStripButtonSystemOptions_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider()));
                settings.Show();
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
            }
        }

        private void toolStripButtonSystemExit_Click(object sender, EventArgs e)
        {
            if (!CloseAllOtherForms(this))
                return; // a form was canceled

            Close();
            ////this canceled
            if (Visible)
                return;
            Application.Exit();
        }

        /// <summary>
        /// Loads the tracker descriptions combo box on the ribbon bar.
        /// </summary>
        private void LoadTrackerDescriptions()
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


        private void Sort(bool isAscending)
        {
            Cursor.Current = Cursors.WaitCursor;

            // Sots the data
            _gridConstructor.View.Sort(isAscending);
            Cursor.Current = Cursors.Default;
        }

        #region Methods

        #region Clipboard related ribbon controls

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

        private void InstantiateEditControl()
        {
            _editControl = new EditControl();
            ToolStripControlHost editControlHost = new ToolStripControlHost(_editControl);
            toolStripExEdit.Items.Add(editControlHost);

            //Create general toolstrip button seperately to set CTRL + N behaviour.
            ToolStripButton generalToolStripButton = new ToolStripButton
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

            _editControl.NewClicked += (EditControl_NewClicked);
            _editControl.NewSpecialClicked += (EditControl_NewSpecialClicked);

            _editControl.DeleteClicked += (EditControl_DeleteClicked);

            //Set generalToolStripButton to add new person when press CTRL + N.
            //SetShortcut(generalToolStripButton, ((Keys.Control | Keys.N)));
            toolStripExEdit.Enabled = PrincipalAuthorization.Instance().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        private static ClipboardItem GetClipboarditem(ToolStripItemClickedEventArgs e)
        {
            return (ClipboardItem)Enum.Parse(typeof(ClipboardItem), (string)e.ClickedItem.Tag);
        }

        private void InstantiateClipboardControl()
        {
            _clipboardControl = new ClipboardControl();
            ToolStripControlHost clipboardhost = new ToolStripControlHost(_clipboardControl);
            toolStripExClipboard.Items.Add(clipboardhost);

            _clipboardControl.CutClicked += (ClipboardControl_CutClicked);

            _clipboardControl.CopyClicked += (ClipboardControl_CopyClicked);

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

            _clipboardControl.PasteSpecialClicked += (ClipboardControl_PasteSpecialClicked);
            _clipboardControl.PasteClicked += (ClipboardControl_PasteClicked);
            //_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
            toolStripExClipboard.Enabled = PrincipalAuthorization.Instance().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
        }

        #endregion

        #region Helper methods

        private void SetToolStripsToPreferredSize()
        {
            toolStripViews.Size = toolStripViews.PreferredSize;
            toolStripExEdit.Size = toolStripExEdit.PreferredSize;
            toolStripExClipboard.Size = toolStripExClipboard.PreferredSize;
            toolStripFilter.Size = toolStripFilter.PreferredSize;
            toolStripExEditing.Size = toolStripExEditing.PreferredSize;
            toolStripDatePicker.Size = toolStripDatePicker.PreferredSize;
            toolStripEx1.Size = toolStripEx1.PreferredSize;
        }

        private void ValidatePersistance(CancelSave cancelSave)
        {
            if (_readOnly) return;
            //Set current cell out of focus to make changes reflect to the data.
            SetCurrentCellOutOfFocus();

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
                        Persist();
                        break;
                    default:
                        // Do not close the form.
                        cancelSave.Cancel = true;
                        break;
                }
            }
        }

        private class CancelSave
        {
            public bool Cancel { get; set; }
        }

        private void SetCurrentCellOutOfFocus()
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

        private void AddNew()
        {
            _stateHolder.AddAndSavePerson(_filteredPeopleHolder.FilteredPersonCollection.Count, _filteredPeopleHolder);
            int rowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
            _gridConstructor.View.Grid.RowCount = rowCount;
            _gridConstructor.View.Invalidate();
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler PeopleWorksheetSaved;
        public event EventHandler PeopleWorksheetForceClose;


        #region Ribbon bar button clicks

        private void toolStripButtonFilterPeople_Click(object sender, EventArgs e)
        {
            var cancelSave = new CancelSave();

            try
            {
                ValidatePersistance(cancelSave);

                if (cancelSave.Cancel)
                    return;

                //Refresh grid control
                _gridConstructor.View.Invalidate();

                _peopleAdminFilterPanel.Refresh();

                //Set peopleAdminFilterPanel control cisibility
                _peopleAdminFilterPanel.Visible = (_peopleAdminFilterPanel.Visible) ? false : true;

                //Check for constrains.
                if (_peopleAdminFilterPanel.Visible)
                {
                    DisposeAllChildGrids();
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
                return;
            }
        }

        private void DisposeAllChildGrids()
        {
            //Disposes child grids of the all the views
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.GeneralView));
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.PeoplePeriodView));
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.SchedulePeriodView));
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonRotationView));
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonAvailabilityView));
            DisposeChildGrids(_gridConstructor.FindGrid(ViewType.PersonalAccountGridView));
        }

        private void toolStripButtonGeneral_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            IList<IPerson> selectedPersons = _gridConstructor.View.GetSelectedPersons();

            // Construct general information grid.
            if (_gridConstructor.View.Type != ViewType.GeneralView)
            {
                _gridConstructor.BuildGridView(ViewType.GeneralView);
				if (KillMode) return;

                _gridConstructor.View.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.Grid.RowCount = _gridConstructor.View.RowCount;

                //Setup for F1
                SetupHelpContext(_gridConstructor.View.Grid);

                _gridConstructor.View.Invalidate();

                // Sets the domain finder
                _domainFinder = new PeopleDomainFinder(_filteredPeopleHolder);
                // Configures the find and replace form
                _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);

                if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword))
                {
                    TogglePanelContent(false);
                    splitContainerWorksheet.Panel2Collapsed = false;
                    splitContainerWorksheet.SplitterDistance = splitContainerWorksheet.Width - 300;

                    // Clears all tab pages and recreate.
                    tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdmin_SelectedIndexChanged;
                    tabControlPeopleAdmin.TabPages.Clear();

                    // Creates skills grid view.
                    _panelConstructor.BuildGridView(ViewType.RolesView);
                    TabPageAdv rolesTabPage =
                        GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.Roles);

                    //Setup for F1
                    SetupHelpContext(_panelConstructor.View.Grid);

                    rolesTabPage.Dock = DockStyle.Fill;
                    tabControlPeopleAdmin.TabPages.Add(rolesTabPage);
                    //tabControlPeopleAdmin.SelectedIndexChanged += tabControlPeopleAdmin_SelectedIndexChanged;
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
                Cursor.Current = Cursors.Default;
            }
        }

        private void SetCorrectView(ViewType mainView, ViewType panelView, bool togglePanelContent, bool ensureRightPanelVisible, int rightPanelWidth)
        {

            if (_gridConstructor.View.Type != mainView)
            {
                Cursor.Current = Cursors.WaitCursor;

                IList<IPerson> selectedPersons = _gridConstructor.View.GetSelectedPersons();
                _gridConstructor.View.Grid.CurrentCell.MoveTo(_gridConstructor.View.Grid.CurrentCell.RowIndex, 0);
                _gridConstructor.BuildGridView(mainView);
                _gridConstructor.View.SetView(shiftCategoryLimitationView);
                SetupHelpContext(_gridConstructor.View.Grid);
                _findAndReplaceForm.ConfigureSearchFunctionality(_gridConstructor.View.Grid, _domainFinder);
                TogglePanelContent(togglePanelContent);

                splitContainerWorksheet.Panel2Collapsed = !ensureRightPanelVisible;
                if (ensureRightPanelVisible)
                {
                    splitContainerWorksheet.SplitterDistance = splitContainerWorksheet.Width - rightPanelWidth;
                }

                _panelConstructor.BuildGridView(panelView);
                SetupHelpContext(_panelConstructor.View.Grid);

                _gridConstructor.View.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.Grid.RowCount = _filteredPeopleHolder.FilteredPeopleGridData.Count;
                _gridConstructor.View.SelectedDateChange(null, null);
                _gridConstructor.View.SetSelectedPersons(selectedPersons);
                Cursor.Current = Cursors.Default;
            }

        }

        private void toolStripButtonPeoplePeriods_Click(object sender, EventArgs e)
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

                SetCorrectView(ViewType.PeoplePeriodView, ViewType.SkillsView, false, true, 300);

                // Clears all tab pages and recreate.
                tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdmin_SelectedIndexChanged;
                tabControlPeopleAdmin.TabPages.Clear();

                var skillsTabPage = GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, UserTexts.Resources.PersonSkill);
                skillsTabPage.Dock = DockStyle.Fill;
                tabControlPeopleAdmin.TabPages.Add(skillsTabPage);

                // Creates external log on grid view.
                var externalLogOnTabPage = new TabPageAdv(UserTexts.Resources.ExternalLogOn) { Dock = DockStyle.Fill };
                tabControlPeopleAdmin.TabPages.Add(externalLogOnTabPage);
                tabControlPeopleAdmin.SelectedIndexChanged += tabControlPeopleAdmin_SelectedIndexChanged;
            }
        }

        private void toolStripButtonSchedulePeriods_Click(object sender, EventArgs e)
        {
            try
            {
                if (_gridConstructor.View.Type != ViewType.SchedulePeriodView)
                {
                    _domainFinder = new PeopleSchedulePeriodDomainFinder(_filteredPeopleHolder);

                    SetCorrectView(ViewType.SchedulePeriodView, ViewType.EmptyView, true, true, shiftCategoryLimitationView.ViewWidth());

                    if (KillMode)
                        return;

                    tabControlPeopleAdmin.SelectedIndexChanged -= tabControlPeopleAdmin_SelectedIndexChanged;
                    tabControlPeopleAdmin.TabPages.Clear();
                }
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
            }
        }

        private void TogglePanelContent(bool showShiftCategoryView)
        {
            if (!showShiftCategoryView)
                shiftCategoryLimitationView.SendToBack();
            else
                shiftCategoryLimitationView.BringToFront();
        }

        private void toolStripButtonPersonRotation_Click(object sender, EventArgs e)
        {
            if (StateHolder.AllRotations.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.RotationsCouldNotBeLoaded,Text);
                return;
            }

            StateHolder.LoadRotationStateHolder(ViewType.PersonRotationView, _filteredPeopleHolder);
            _domainFinder = new PersonRotationDomainFinder(_filteredPeopleHolder);
            SetCorrectView(ViewType.PersonRotationView, ViewType.EmptyView, false, false, 0);
        }

        private void toolStripButtonPersonAccounts_Click(object sender, EventArgs e)
        {
            if (_filteredPeopleHolder.FilteredAbsenceCollection.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.AbsenceCouldNotBeLoaded, Text);
                return;
            }
            _domainFinder = new PersonAccountDomainFinder(_filteredPeopleHolder);
            SetCorrectView(ViewType.PersonalAccountGridView, ViewType.EmptyView, false, false, 0);
        }

        private void toolStripButtonPersonAvailability_Click(object sender, EventArgs e)
        {
            if (StateHolder.AllAvailabilities.Count <= 0)
            {
                ShowWarningMessage(UserTexts.Resources.AvailabilitiesCouldNotBeLoaded, Text);
                return;
            }

            StateHolder.LoadRotationStateHolder(ViewType.PersonAvailabilityView, _filteredPeopleHolder);
            _domainFinder = new PersonAvailabilityDomainFinder(_filteredPeopleHolder);
            SetCorrectView(ViewType.PersonAvailabilityView, ViewType.EmptyView, false, false, 0);
        }

        private void toolStripButtonMainClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Clipboard handling ribbon events

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void EditControl_NewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                //IList<IPerson> selectedPersons = gridConstructor.View.GetSelectedPersons();
                switch (GetClipboarditem(e))
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
                //gridConstructor.View.SetSelectedPersons(selectedPersons);
                _gridConstructor.View.AddNewGridRow<EventArgs>(sender, e);
            }
            catch (Exception ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
                FormKill();
            }
        }

        private void EditControl_NewClicked(object sender, EventArgs e)
        {
            _gridConstructor.View.AddNewGridRow(sender, e);
        }

        private void EditControl_DeleteClicked(object sender, EventArgs e)
        {
            if ((_gridConstructor.View.Grid.Model.SelectedRanges == null) ||
                (_gridConstructor.View.Grid.Model.SelectedRanges.Count == 0))
            {
                return;
            }
            _gridConstructor.View.DeleteSelectedGridRows(sender, e);
        }

        private void ClipboardControl_CutClicked(object sender, EventArgs e)
        {
            if ((_gridConstructor.View.CurrentGrid.Model.SelectedRanges == null) ||
                (_gridConstructor.View.CurrentGrid.Model.SelectedRanges.Count == 0))
            {
                return;
            }

            if (_gridConstructor.View.Grid.CutPaste.CanCut())
                _gridConstructor.View.Grid.CutPaste.Cut();
        }

        private void ClipboardControl_PasteClicked(object sender, EventArgs e)
        {
            bool canPaste = _gridConstructor.View.CurrentGrid.CutPaste.CanPaste();
            if (canPaste) _gridConstructor.View.CurrentGrid.CutPaste.Paste();
        }

        private void ClipboardControl_PasteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (GetClipboarditem(e))
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

        private void ClipboardControl_CopyClicked(object sender, EventArgs e)
        {
            if ((_gridConstructor.View.CurrentGrid.Model.SelectedRanges == null) ||
                (_gridConstructor.View.CurrentGrid.Model.SelectedRanges.Count == 0))
            {
                return;
            }
            bool canCopy = _gridConstructor.View.CurrentGrid.CutPaste.CanCopy();
            if (canCopy) _gridConstructor.View.CurrentGrid.CutPaste.Copy();
        }

        #endregion

        #region Tab Control events

        private void tabControlPeopleAdmin_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if tab control is collapsed then return. 
            if (splitContainerWorksheet.Panel2Collapsed) return;

            if (_panelConstructor.View.Type != ViewType.ExternalLogOnView)
            {
                if (tabControlPeopleAdmin.SelectedTab.Controls.Count == 0)
                {
                    _panelConstructor.BuildGridView(ViewType.ExternalLogOnView);
                    //GridConstructor.WrapWithTabPage(_panelConstructor.View.Grid, tabControlPeopleAdmin.SelectedTab);
                    GridConstructor.WrapWithTabPageExternal(_panelConstructor.View.Grid, tabControlPeopleAdmin.SelectedTab, tableLayoutPanel1);
                    //Setup for F1
                    SetupHelpContext(_panelConstructor.View.Grid);
                }
                else
                {
                    _panelConstructor.BuildGridView(ViewType.ExternalLogOnView);
                    _panelConstructor.View.RowCount = _filteredPeopleHolder.ExternalLogOnViewAdapterCollection.Count;
                    _panelConstructor.View.Grid.RowCount = _panelConstructor.View.RowCount;
                    _panelConstructor.View.Invalidate();
                    SetupHelpContext(_panelConstructor.View.Grid);
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

        #endregion

        #region Form close events

        private void PeopleWorksheet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (KillMode)
                return;
            if(_readOnly) return;
            var cancelSave = new CancelSave();
            ValidatePersistance(cancelSave);
            notifyForceClose();
            e.Cancel = cancelSave.Cancel;
        }

        private void PeopleWorksheet_FormClosed(object sender, FormClosedEventArgs e)
        {
            unregisterEventsForFormKill();

            DisposeAllChildGrids();
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
        }

        #endregion

        private void toolStripButtonMainNew_Click(object sender, EventArgs e)
        {
            AddNew();
        }

        #endregion

        #region Message broker related items

        private void PeopleWorksheet_Shown(object sender, EventArgs e)
        {
            //Set toolStrips to preferred Size.
            SetToolStripsToPreferredSize();

            _peopleAdminFilterPanel.BringToFront();
			
			// Shows default view.
			if(KillMode) return;
			toolStripButtonGeneral.PerformClick();
            
            //Load all other views relavant references.
			if (KillMode) return;
            LoadPeopleAdminReferences();
            _filteredPeopleHolder.LoadRuleSetBag();
            _filteredPeopleHolder.LoadBudgetGroup();
            
        }

        private void LoadPeopleAdminReferences()
        {
            // Load all contract schedules (to show on the combo boxes).
            StateHolder.LoadContractSchedules(_filteredPeopleHolder);

            // Load all part-time percentages (to show on the combo boxes).
            StateHolder.LoadPartTimePercentages(_filteredPeopleHolder);

            //Load all contracts available (to show on the combo boxes).
            StateHolder.LoadContracts(_filteredPeopleHolder);
        }

        private void toolStripComboBoxExTrackerDescription_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefilterOnTracker();
        }

        private void RefilterOnTracker()
        {
            if (_gridConstructor.CurrentView == ViewType.PersonalAccountGridView)
            {
                IAbsence currentType = getPersonAccountAbsenceType();

                var trackerTypeChangeEventArg = new SelectedItemChangeBaseEventArgs<IAbsence>(currentType);
                _gridConstructor.View.TrackerDescriptionChanged(this, trackerTypeChangeEventArg);
            }
        }

        #endregion

        #region Methods

        private static void DisposeChildGrids(GridViewBase view)
        {
            if (view != null)
            {
                view.DisposeChildGrids();
            }
        }

        private void ConfigureAndShowFindAndReplaceForm()
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

                default:
                    break;
            }


            _findAndReplaceForm.Size = new Size(460, 345);
            _findAndReplaceForm.Show();
            _findAndReplaceForm.Focus();
        }

        private void SetupHelpContext(GridControl grid)
        {
            RemoveControlHelpContext(grid);
            AddControlHelpContext(grid);
            grid.Focus();
        }

        #endregion

        private void ExternalFilteringTextBoxTextChanged(object sender, EventArgs e)
        {
            var externalPersonList = _filteredPeopleHolder.ExternalLogOnViewAdapterCollection;
            var filterValue = externalFilteringTextBox.Text;

            var cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
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

        private void ToolStripButtonClosePreviousPeriodClick(object sender, EventArgs e)
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
                RefreshAfterMessageBroker();
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

    }
}
