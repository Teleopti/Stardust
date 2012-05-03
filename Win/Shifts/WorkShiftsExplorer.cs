﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
    public partial class WorkShiftsExplorer : BaseRibbonForm, IExplorerView
    {
        private EditControl _editControl;
        private ClipboardControl _clipboardControl;
        private NavigationView _navigationView;
        private GeneralView _generalView;
        private VisualizeGridView _visualizeView;
        private IDictionary<ShiftCreatorViewType, ToolStripButton> _viewButtonDictionary;
        private readonly IEventAggregator _eventAggregator;
        private readonly IExternalExceptionHandler _externalExceptionHandler = new ExternalExceptionHandler();

        public WorkShiftsExplorer(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdv1);
            }
            setPermissionOnControls();
            if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
                Icon = Resources.shifts;
        }

        public IExplorerPresenter Presenter { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;

            Presenter.Model.SetRightToLeft((base.RightToLeft == RightToLeft.Yes) ? true : false);
            Presenter.Model.SetSelectedView(ShiftCreatorViewType.RuleSet);

            _navigationView = new NavigationView(Presenter, _eventAggregator);
            _generalView = new GeneralView(Presenter, _eventAggregator);
            _visualizeView = new VisualizeGridView(Presenter, _eventAggregator);

            _navigationView.Refresh += delegate
            {
                _visualizeView.RefreshView();
                var amountList = Presenter.VisualizePresenter.RuleSetAmounts();
                _generalView.Amounts(amountList);
                _generalView.RefreshView();
            };

            _viewButtonDictionary = new Dictionary<ShiftCreatorViewType, ToolStripButton>();
            _viewButtonDictionary.Add(ShiftCreatorViewType.General, toolStripButtonGeneral);
            _viewButtonDictionary.Add(ShiftCreatorViewType.Activities, toolStripButtonCombined);
            _viewButtonDictionary.Add(ShiftCreatorViewType.Limitation, toolStripButtonLimitations);
            _viewButtonDictionary.Add(ShiftCreatorViewType.DateExclusion, toolStripButtonDateExclusion);
            _viewButtonDictionary.Add(ShiftCreatorViewType.WeekdayExclusion, toolStripButtonWeekdayExclusion);

            createAndAddViews();
            instantiateEditControl();
            instantiateClipboardControl();

            toolStripButtonGeneral.Checked = true;


            tsClipboard.Size = tsClipboard.PreferredSize;
            tcEdit.Size = tcEdit.PreferredSize;
            tcShiftBags.Size = tcShiftBags.PreferredSize;
            tcShiftBags.Width = tcShiftBags.Text.Length*8 > tcShiftBags.PreferredSize.Width ? tcShiftBags.Text.Length*8  : tcShiftBags.PreferredSize.Width;
            tcRename.Size = tcRename.PreferredSize;
            tcViews.Size = tcViews.PreferredSize;

        }

        private void createAndAddViews()
        {
            splitContainerAdvVertical.Panel1.Controls.Add(_navigationView);
            _navigationView.Dock = DockStyle.Fill;
            splitContainerAdvHorizontal.Panel1.Controls.Add(_visualizeView);
            _visualizeView.Dock = DockStyle.Fill;
            splitContainerAdvHorizontal.Panel2.Controls.Add(_generalView);
            _generalView.Dock = DockStyle.Fill;
        }

        private void setPermissionOnControls()
        {
            toolStripButtonSystemOptions.Enabled = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
        }

        private void workShiftsExplorerFormClosing(object sender, FormClosingEventArgs e)
        {
            Presenter.Validate();
            if (!Presenter.CheckForUnsavedData())
            {
                e.Cancel = true;
                return;
            }

            _editControl.NewClicked -= (editControlNewClicked);
            _editControl.NewSpecialClicked -= (editControlNewSpecialClicked);
            _editControl.DeleteClicked -= (editControlDeleteClicked);
            _editControl.NewSpecialItems.Clear();
            _editControl.Dispose();
            _editControl = null;
        }

        private void splitContainerAdvHorizontalPanel1Resize(object sender, EventArgs e)
        {
            if(_visualizeView != null)
                _visualizeView.RefreshView();
        }

        private void instantiateEditControl()
        {
            _editControl = new EditControl();
            var editControlHost = new ToolStripControlHost(_editControl);
            tcEdit.Items.Add(editControlHost);

            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewRuleSet, Tag = ShiftCreatorViewType.RuleSet });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewRuleSetBag, Tag = ShiftCreatorViewType.RuleSetBag });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewActivity, Tag = ShiftCreatorViewType.Activities });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewLimitation, Tag = ShiftCreatorViewType.Limitation });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = UserTexts.Resources.NewDateExclusion, Tag = ShiftCreatorViewType.DateExclusion });
            _editControl.NewClicked += (editControlNewClicked);
            _editControl.NewSpecialClicked += (editControlNewSpecialClicked);

            _editControl.DeleteClicked += (editControlDeleteClicked);
            
        }

        private void editControlDeleteClicked(object sender, EventArgs e)
        {
            switch (Presenter.Model.SelectedView)
            {
                case ShiftCreatorViewType.RuleSet:
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.Delete();
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                case ShiftCreatorViewType.WeekdayExclusion:
                    _generalView.Delete();
                    break;
            }
        }

        private void editControlNewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch((ShiftCreatorViewType) e.ClickedItem.Tag)
            {
                case ShiftCreatorViewType.General: 
                case ShiftCreatorViewType.RuleSet: 
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.ChangeGridView((ShiftCreatorViewType)e.ClickedItem.Tag);
                    _navigationView.Add();
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                    _generalView.ChangeGridView((ShiftCreatorViewType)e.ClickedItem.Tag);
                    SelectToolStripItem((ShiftCreatorViewType)e.ClickedItem.Tag);
                    _generalView.Add();
                    break;
            }
        }

        private void editControlNewClicked(object sender, EventArgs e)
        {
            switch (Presenter.Model.SelectedView)
            {
                case ShiftCreatorViewType.General:
                case ShiftCreatorViewType.RuleSet:
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.ChangeGridView(Presenter.Model.SelectedView);
                    _navigationView.Add();
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                    _generalView.ChangeGridView(Presenter.Model.SelectedView);
                    SelectToolStripItem(Presenter.Model.SelectedView);
                    _generalView.Add();
                    break;
            }
        }

        private void instantiateClipboardControl()
        {
            _clipboardControl = new ClipboardControl();
            var clipboardhost = new ToolStripControlHost(_clipboardControl);
            tsClipboard.Items.Add(clipboardhost);

            _clipboardControl.CutClicked += (clipboardControlCutClicked);
            _clipboardControl.CopyClicked += (clipboardControlCopyClicked);
            _clipboardControl.PasteClicked += (clipboardControlPasteClicked);
            _clipboardControl.SetButtonState(ClipboardAction.Paste, false);
        }

        private void clipboardControlCutClicked(object sender, EventArgs e)
        {
            _externalExceptionHandler.AttemptToUseExternalResource(Clipboard.Clear);
            switch (Presenter.Model.SelectedView)
            {
                case ShiftCreatorViewType.RuleSet:
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.Cut();
                    _clipboardControl.SetButtonState(ClipboardAction.Paste, true);
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                    _generalView.Cut();
                    _clipboardControl.SetButtonState(ClipboardAction.Paste, true);
                    break;
            }
        }

        private void clipboardControlPasteClicked(object sender, EventArgs e)
        {
            switch (Presenter.Model.SelectedView)
            {
                case ShiftCreatorViewType.RuleSet:
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.Paste();
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                    _generalView.Paste();
                    break;
            }
        }

        private void clipboardControlCopyClicked(object sender, EventArgs e)
        {
            _externalExceptionHandler.AttemptToUseExternalResource(Clipboard.Clear);
            switch (Presenter.Model.SelectedView)
            {
                case ShiftCreatorViewType.RuleSet:
                case ShiftCreatorViewType.RuleSetBag:
                    _navigationView.Copy();
                    _clipboardControl.SetButtonState(ClipboardAction.Paste, true);
                    break;
                case ShiftCreatorViewType.Activities:
                case ShiftCreatorViewType.Limitation:
                case ShiftCreatorViewType.DateExclusion:
                    _generalView.Copy();
                    _clipboardControl.SetButtonState(ClipboardAction.Paste, true);
                    break;
            }
        }

        private void toolStripButtonTemplateViewClick(object sender, EventArgs e)
        {
            loadView(ShiftCreatorViewType.General);
        }

        private void toolStripButtonLimiterViewClick(object sender, EventArgs e)
        {
            loadView(ShiftCreatorViewType.Limitation);
        }

        private void toolStripButtonCombinedClick(object sender, EventArgs e)
        {
            loadView(ShiftCreatorViewType.Activities);
        }

        private void toolStripButtonSaveClick(object sender, EventArgs e)
        {
            if (validateGrid())
            {
                Cursor.Current = Cursors.WaitCursor;
                persist();
                toolStripButtonRefresh.PerformClick();
                Cursor.Current = Cursors.Default;
            }
        }

        private void toolStripButtonHelpClick(object sender, EventArgs e)
        {
            ShowHelp(true);
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        private bool validateGrid()
        {
            _generalView.ReflectEnteredValues();
            bool status = Presenter.Validate();
            if (!status)
            {
                _generalView.Refresh();
                MessageBox.Show(
                    string.Concat(UserTexts.Resources.ShiftCreatorValidationErrorMessage, "  "),
                                Text,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0
                    );
                
            }
            return status;
        }

        private void toolStripButtonRefreshClick(object sender, EventArgs e)
        {
            if (validateGrid())
            {
                Presenter.VisualizePresenter.LoadModelCollection();
                _visualizeView.RefreshView();
                var amountList = Presenter.VisualizePresenter.RuleSetAmounts();
                _generalView.Amounts(amountList);
                _generalView.RefreshView();
            }
        }

        private void toolStripButtonCloseExitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripAssignRuleSetClick(object sender, EventArgs e)
        {
            _navigationView.AssignRuleSet();
        }

        private void toolStripButtonDateExclusionClick(object sender, EventArgs e)
        {
            loadView(ShiftCreatorViewType.DateExclusion);
        }

        private void toolStripButtonWeekdayExclusionClick(object sender, EventArgs e)
        {
            loadView(ShiftCreatorViewType.WeekdayExclusion);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void toolStripButtonSystemOptionsClick(object sender, EventArgs e)
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

        private void toolStripButtonExitSystemClick(object sender, EventArgs e)
        {
            if (!CloseAllOtherForms(this))
                return; // a form was canceled

            Close();
            ////this canceled
            if (Visible)
                return;
            Application.Exit();
        }

        private void persist()
        {
            Presenter.Persist();
            
        }

        public void SetupHelpContextForGrid(GridControlBase grid)
        {
            RemoveControlHelpContext(grid);
            AddControlHelpContext(grid);
            grid.Focus();
        }

        public bool AskForDelete()
        {
            DialogResult result = MessageBox.Show(UserTexts.Resources.AreYouSureYouWantToDelete, UserTexts.Resources.Delete,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0);
            return (result == DialogResult.Yes) ? true : false;
        }


        public bool CheckForSelectedRuleSet()
        {
            if (Presenter.Model.FilteredRuleSetCollection == null)
            {
                MessageBox.Show(UserTexts.Resources.YouHaveToSelectAtLeastOneRuleSet,UserTexts.Resources.NoRuleSetSelected,
                                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                    (RightToLeft == RightToLeft.Yes)
                                        ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                        : 0);
                return false;
            }
            return true;
        }

        public void SetVisualGridDrawingInfo()
        {
            int widthToAdd = _visualizeView.GetFirstColumnWidth();
            float panelWidth = (float)splitContainerAdvHorizontal.Panel1.Width - splitContainerAdvHorizontal.SplitterWidth - 10;
            float columnCount = Presenter.Model.VisualizeGridColumnCount;

            float widthPerHour = ((panelWidth - widthToAdd) / columnCount);
            float pixelSize = (widthPerHour / 60);

            double width = Presenter.Model.VisualizeGridColumnCount * pixelSize * 60;
            var newWidth = (int)width;
            Presenter.Model.SetVisualColumnWidth(newWidth);
            Presenter.Model.SetWidthPerHour(widthPerHour);
            Presenter.Model.SetWidthPerPixel(pixelSize);
        }

        public void SelectToolStripItem(ShiftCreatorViewType view)
        {
            ToolStripButton selectedButton = null;
            switch(view)
            {
                case ShiftCreatorViewType.General:
                    selectedButton = toolStripButtonGeneral;
                    break;
                case ShiftCreatorViewType.Activities:
                    selectedButton = toolStripButtonCombined;
                    break;
                case ShiftCreatorViewType.Limitation:
                    selectedButton = toolStripButtonLimitations;
                    break;
                case ShiftCreatorViewType.DateExclusion:
                    selectedButton = toolStripButtonDateExclusion;
                    break;
                case ShiftCreatorViewType.WeekdayExclusion:
                    selectedButton = toolStripButtonWeekdayExclusion;
                    break;
            }
            if (selectedButton == null) return;
            selectedButton.Checked = true;
            IList<ToolStripButton> buttons = (from p in _viewButtonDictionary where p.Value.Text != selectedButton.Text select p.Value).ToList();
            foreach (ToolStripButton button in buttons)
                button.Checked = false;
        }

        public void RefreshChildViews()
        {
            _navigationView.RefreshView();
            _generalView.RefreshView();
            _visualizeView.RefreshView();
        }

        public void RefreshActivityGridView()
        {
            _generalView.RefreshView();
        }

        public new void AddControlHelpContext(Control control)
        {
            base.AddControlHelpContext(control);
        }

        private void toolStripButtonRenameClick(object sender, EventArgs e)
        {
            _navigationView.Rename();
        }

        private void shiftCreatorKeyUp(object sender, KeyEventArgs e)
        {
            _visualizeView.RefreshView();
        }

        private void loadView(ShiftCreatorViewType view)
        {
            _generalView.ChangeGridView(view);
            Presenter.Model.SetSelectedView(view);
            SelectToolStripItem(view);
        }

        public void ShowDataSourceException(DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                   UserTexts.Resources.Shifts,
                                                                    UserTexts.Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        public void Show(IExplorerPresenter explorerPresenter)
        {
            Presenter = explorerPresenter;
            Show();
        }

        public void ForceRefreshNavigationView()
        {
            _navigationView.ForceRefresh();
        }

        public void UpdateNavigationViewTreeIcons()
        {
            _navigationView.UpdateTreeIcons();
        }
    }
}
