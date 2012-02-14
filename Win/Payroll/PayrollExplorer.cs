using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Payroll
{
    /// <summary>
    /// Payroll screen
    /// </summary>
    public partial class PayrollExplorer : BaseRibbonForm, 
                                           IExplorerView
    {
        #region Fields - Instance Members

        #region Fields - Private Fields

        /// <summary>
        /// Unit of work instance
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Explorer presenter instance
        /// </summary>
        private readonly IExplorerPresenter _explorerPresenter;

        /// <summary>
        /// DefinitionSetView usercontrol
        /// </summary>
        private UserControl _definitionSetView;

        /// <summary>
        /// DateView usercontrol
        /// </summary>
        private UserControl _dateView;

        /// <summary>
        /// WeekView usercontrol
        /// </summary>
        private UserControl _weekView;

        /// <summary>
        /// Visualize usercontrol
        /// </summary>
        private UserControl _visualizeView;

        /// <summary>
        /// ClipboardAction
        /// </summary>
        private ClipboardOperation _clipboardAction = ClipboardOperation.Copy;

        /// <summary>
        /// Sorting mode
        /// </summary>
        private SortingMode _sortingMode = SortingMode.Ascending;

        /// <summary>
        /// EditControl
        /// </summary>
        private EditControl _editControl;

        /// <summary>
        /// ClipboardControl
        /// </summary>
        private ClipboardControl _clipboardControl;

        /// <summary>
        /// Current selected view
        /// </summary>
        private PayrollViewType _currentView = PayrollViewType.DefinitionSetDropDown;

        #endregion

        #region Fields - Constants

        private const string ADDNEWDEFINITIONSET = "New Definition Set";
        private const string ADDNEWDATERULE = "New Date Rule";
        private const string ADDNEWWEEKDAYRULE = "New Weekday Rule";
        private const string XXX = "xxx";
        private const string XX = "xx";

        #endregion

        #endregion

        #region Properties - Instance Members

        #endregion

        #region  Methods - Instance Members
        
        #region  Constructors

        /// <summary>
        /// Constructor.    
        /// </summary>
        public PayrollExplorer(IUnitOfWork uow)
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                SetRibbonHeaderQuickItemsResourceText();
            }

            InstantiateEditControl();
            InstatntiateClipboardControl();

            datePeriod.SetSelectedDate(TimeZoneHelper.ConvertToUtc(DateTime.Now));
            datePeriod.SelectedDateChanged += datePeriod_SelectedDateChanged;
            ToolStripControlHost hostDatePicker = new ToolStripControlHost(datePeriod);
            tsDatePeriod.Items.Add(hostDatePicker);

            _unitOfWork = uow;
            _explorerPresenter = new ExplorerPresenter(new PayrollHelper(_unitOfWork), this);
            _explorerPresenter.Model.SetDefaultSegment((int) (StateHolder.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.DefaultSegment]));
            _explorerPresenter.Model.SetRightToLeft((base.RightToLeft == RightToLeft.Yes) ? true : false);

            _explorerPresenter.Model.SetSelectedDate(TimeZoneHelper.ConvertToUtc(DateTime.Now));
            
            InstantiateDefinitionSeView();
            InstantiateDateView();
            InstantiateWeekView();
            InstantiateVisualizeView();

            scRight.Panel2.Controls.Add(_weekView);
            _weekView.Dock = DockStyle.Fill;

            SetPermissionOnControls();
        }

        private void datePeriod_SelectedDateChanged(object sender, Domain.Common.CustomEventArgs<DateTime> e)
        {
            _explorerPresenter.Model.SetSelectedDate(TimeZoneHelper.ConvertToUtc(e.Value));
            this.Refresh(PayrollViewType.VisualizeControl);
        }

        #endregion

        #region  Methods
        
        #region  Methods - Private Methods

        /// <summary>
        /// Instantiates the definition se view.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-22
        /// </remarks>
        private void InstantiateDefinitionSeView()
        {
            _explorerPresenter.DefinitionSetPresenter.LoadModel();
            _definitionSetView = ViewBuilder.CreateView(PayrollViewType.DefinitionSetDropDown, this);
            splitContainerAdvVertical.Panel1.Controls.Add(_definitionSetView);
            _definitionSetView.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Instantiates the date view.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        private void InstantiateDateView()
        {
            _dateView = ViewBuilder.CreateView(PayrollViewType.DayControl, this);
        }

        /// <summary>
        /// Instantiates the week view.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        private void InstantiateWeekView()
        {
            _weekView = ViewBuilder.CreateView(PayrollViewType.WeekdayControl, this);
        }

        /// <summary>
        /// Instantiates the visualize view.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        private void InstantiateVisualizeView()
        {
            _visualizeView = ViewBuilder.CreateView(PayrollViewType.VisualizeControl, this);
            scRight.Panel1.Controls.Add(_visualizeView);
            _visualizeView.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Loads the grid view based on selection.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        private void LoadGridViewBasedOnSelection()
        {
            UserControl control = null;
            scRight.Panel2.Controls.Clear();
            if (tsbWeekView.CheckState == CheckState.Checked)
                control = _weekView;
            else if (tsbDayView.CheckState == CheckState.Checked)
                control = _dateView;
            scRight.Panel2.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Instantiates the edit control.
        /// </summary>
        private void InstantiateEditControl()
        {
            _editControl = new EditControl();
            ToolStripControlHost editControlHost = new ToolStripControlHost(this._editControl);
            tsEdit.Items.Add(editControlHost);

            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = ADDNEWDEFINITIONSET, Tag = "New Definition Set" });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = ADDNEWDATERULE, Tag = "New Date Rule" });
            _editControl.NewSpecialItems.Add(new ToolStripButton { Text = ADDNEWWEEKDAYRULE, Tag = "New Weekday Rule" });
            _editControl.NewClicked += (EditControl_NewClicked);
            _editControl.NewSpecialClicked += (EditControl_NewSpecialClicked);
            _editControl.DeleteClicked += (EditControl_DeleteClicked);

        }

        /// <summary>
        /// Instatntiates the clipboard control.
        /// </summary>
        private void InstatntiateClipboardControl()
        {
            _clipboardControl = new ClipboardControl();
            ToolStripControlHost clipboardhost = new ToolStripControlHost(_clipboardControl);
            tsClipboard.Items.Add(clipboardhost);

            _clipboardControl.CutClicked += (ClipboardControl_CutClicked);
            _clipboardControl.CopyClicked += (ClipboardControl_CopyClicked);

            _clipboardControl.PasteSpecialItems.Add(new ToolStripButton() { Text = UserTexts.Resources.Paste });
            _clipboardControl.PasteSpecialItems.Add(new ToolStripButton() { Text = UserTexts.Resources.PasteNew });
            _clipboardControl.PasteSpecialClicked += (ClipboardControl_PasteSpecialClicked);
            _clipboardControl.PasteClicked += (ClipboardControl_PasteClicked);
            _clipboardControl.SetButtonState(ClipboardAction.Paste, false);
        }

        /// <summary>
        /// Gets the common behavior instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        private ICommonBehavior GetCommonBehaviorInstance()
        {
            ICommonBehavior commonBehavior = null;
            object common = null;
            switch (this._currentView)
            {
                case PayrollViewType.DefinitionSetDropDown:
                    common = _definitionSetView;
                    break;
                case PayrollViewType.DayControl:
                    common = _dateView;
                    break;
                case PayrollViewType.WeekdayControl:
                    common = _weekView;
                    break;
                case PayrollViewType.VisualizeControl:
                    common = _visualizeView;
                    break;
            }
            commonBehavior = (ICommonBehavior) common;
            return commonBehavior;
        }

        /// <summary>
        /// Gets the common behavior instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        private ICommonBehavior GetCommonBehaviorInstance(PayrollViewType view)
        {
            ICommonBehavior commonBehavior = null;
            object common = null;
            switch (view)
            {
                case PayrollViewType.DefinitionSetDropDown:
                    common = _definitionSetView;
                    break;
                case PayrollViewType.DayControl:
                    common = _dateView;
                    break;
                case PayrollViewType.WeekdayControl:
                    common = _weekView;
                    break;
                case PayrollViewType.VisualizeControl:
                    common = _visualizeView;
                    break;
            }
            commonBehavior = (ICommonBehavior) common;
            return commonBehavior;
        }
        private IList<ICommonBehavior> GetCommonBehaviorInstances()
        {
            IList<ICommonBehavior> commonBehaviors = new List<ICommonBehavior>();

            commonBehaviors.Add((ICommonBehavior)_weekView);
            commonBehaviors.Add((ICommonBehavior)_dateView);
            commonBehaviors.Add((ICommonBehavior)_visualizeView);

            return commonBehaviors;
        }


        /// <summary>
        /// Sets the ribbon header quick items resource text.
        /// </summary>
        private void SetRibbonHeaderQuickItemsResourceText()
        {
            foreach (ToolStripItem item in ribbonControl.Header.QuickItems)
            {
                string key = item.Text;
                if (key.StartsWith(XXX, StringComparison.OrdinalIgnoreCase))
                {
                    key = key.Substring(3);
                }
                else if (key.StartsWith(XX, StringComparison.OrdinalIgnoreCase))
                {
                    key = key.Substring(2);
                }

                item.Text = UserTexts.Resources.ResourceManager.GetString(key);
            }
        }

        /// <summary>
        /// Sets the permission on controls.
        /// </summary>
        private void SetPermissionOnControls()
        {
            tsbSystemOptions.Enabled =
                AuthorizationService.DefaultService.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
        }
                
        #endregion

        #region  Methods - Public Methods
        
        /// <summary>
        /// Checks for delete.
        /// </summary>
        /// <returns>True is clicked on Yes, else False</returns>
        public bool CheckForDelete()
        {
            DialogResult result = MessageBoxAdv.Show(UserTexts.Resources.DeleteConfirmation, UserTexts.Resources.Delete,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                (RightToLeft == RightToLeft.Yes)
                                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                                    : 0);
            return (result == DialogResult.Yes) ? true : false;
        }

        #endregion

        #region Methods - Protected Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_explorerPresenter.Helper.UnitOfWork.IsDirty())
            {
                DialogResult response = Syncfusion.Windows.Forms.MessageBoxAdv.Show(
                    string.Concat(UserTexts.Resources.DoYouWantToSaveChangesYouMade, "  "),
                    Text,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
                switch (response)
                {
                    case DialogResult.No:
                        break;

                    case DialogResult.Yes:
                        tsbSave.PerformClick();
                        break;

                    default:
                        e.Cancel = true;
                        break;
                }
            }
            if (!e.Cancel)
                _explorerPresenter.Helper.UnitOfWork.Clear();
        }

        #endregion

        #endregion

        #region Events - Instance Members

        /// <summary>
        /// Handles the DeleteClicked event of the EditControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EditControl_DeleteClicked(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().DeleteSelected();
        }

        /// <summary>
        /// Handles the NewSpecialClicked event of the EditControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void EditControl_NewSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            GetCommonBehaviorInstance().AddNew();
        }

        /// <summary>
        /// Handles the NewClicked event of the EditControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EditControl_NewClicked(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().AddNew();
        }

        /// <summary>
        /// Handles the Click event of the toolStripButtonWeekDayView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripButtonWeekDayView_Click(object sender, EventArgs e)
        {
            tsbWeekView.CheckState = CheckState.Checked;
            tsbDayView.CheckState = CheckState.Unchecked;
            _currentView = PayrollViewType.WeekdayControl;
            LoadGridViewBasedOnSelection();
        }

        /// <summary>
        /// Handles the Click event of the toolStripButtonDateView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripButtonDateView_Click(object sender, EventArgs e)
        {
            tsbWeekView.CheckState = CheckState.Unchecked;
            tsbDayView.CheckState = CheckState.Checked;
            _currentView = PayrollViewType.DayControl;
            LoadGridViewBasedOnSelection();
        }

        /// <summary>
        /// Handles the Click event of the tsbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void tsbSave_Click(object sender, EventArgs e)
        {
            try
            {
                _explorerPresenter.SaveAll();    
            }
            catch(OptimisticLockException)
            {
                MessageBoxAdv.Show(string.Concat(UserTexts.Resources.SomeoneElseHaveChanged + "." + UserTexts.Resources.PleaseTryAgainLater, " "),
                    UserTexts.Resources.Message,
                            MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                       (RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
            }
            
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemSortAsc control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripMenuItemSortAsc_Click(object sender, EventArgs e)
        {
            _sortingMode = SortingMode.Ascending;
            GetCommonBehaviorInstance().Sort(SortingMode.Ascending);
        }

        /// <summary>
        /// Handles the Click event of the toolStripMenuItemSortDesc control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripMenuItemSortDesc_Click(object sender, EventArgs e)
        {
            _sortingMode = SortingMode.Descending;
            GetCommonBehaviorInstance().Sort(SortingMode.Descending);
        }

        /// <summary>
        /// Handles the Click event of the tsbSort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
         private void tsbSort_Click(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().Sort(_sortingMode);
        }

        /// <summary>
        /// Handles the Click event of the tsbRename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void tsbRename_Click(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().Rename();
        }

        /// <summary>
        /// Handles the CutClicked event of the ClipboardControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ClipboardControl_CutClicked(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().Cut();
        }

        /// <summary>
        /// Handles the PasteClicked event of the ClipboardControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ClipboardControl_PasteClicked(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().Paste();
        }

        /// <summary>
        /// Handles the PasteSpecialClicked event of the ClipboardControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void ClipboardControl_PasteSpecialClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ClipboardControl_PasteClicked(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the CopyClicked event of the ClipboardControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ClipboardControl_CopyClicked(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance().Copy();
        }

        #endregion

        #endregion

        #region IExplorerView Members

        /// <summary>
        /// Gets the unit of work for the form.
        /// </summary>
        public IUnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        /// <summary>
        /// Gets the explorer presenter.
        /// </summary>
        /// <value>The explorer presenter.</value>
        public IExplorerPresenter ExplorerPresenter
        {
            get
            {
                return _explorerPresenter;
            }
        }

        /// <summary>
        /// Gets the clipboard action.
        /// </summary>
        /// <value>The clipboard action.</value>
        public ClipboardOperation ClipboardActionType
        {
            get
            {
                return _clipboardAction;
            }
        }

        /// <summary>
        /// Sets the selected view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void SetSelectedView(PayrollViewType view)
        {
            _currentView = view;
        }

        /// <summary>
        /// Sets the state of the clipboard control.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        public void SetClipboardControlState(ClipboardOperation action, bool status)
        {
            ClipboardAction clipboardAction = ClipboardAction.Copy;
            switch(action)
            {
                case ClipboardOperation.Cut:
                    clipboardAction = ClipboardAction.Cut;
                    break;
                case ClipboardOperation.Copy:
                    clipboardAction = ClipboardAction.Copy;
                    break;
                case ClipboardOperation.Paste:
                    clipboardAction = ClipboardAction.Paste;
                    break;
            }
            _clipboardAction = action;
            _clipboardControl.SetButtonState(clipboardAction, true);
        }

        /// <summary>
        /// Gets the width of visualize control container.
        /// </summary>
        /// <returns></returns>
        public float GetWidthOfVisualizeControlContainer()
        {
            float panelWidth = ((float)scRight.Panel2.Width - scRight.SplitterWidth);
            return panelWidth; 
        }

        /// <summary>
        /// Refreshes the specified view.
        /// </summary>
        /// <param name="view"></param>
        public void Refresh(PayrollViewType view)
        {
            if (_explorerPresenter == null) 
                return;
            GetCommonBehaviorInstance(view).RefreshView();
        }

        /// <summary>
        /// Refreshes the selected views.
        /// </summary>
        public void RefreshSelectedViews()
        {
            IList<ICommonBehavior> commonBehaviors = GetCommonBehaviorInstances();

            foreach (var behavior in commonBehaviors)
            {
                behavior.Reload();
            }
        }

        #endregion

        /// <summary>
        /// Handles the Resize event of the scRight_Panel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void scRight_Panel1_Resize(object sender, EventArgs e)
        {
            ICommonBehavior commonBehavior = GetCommonBehaviorInstance(PayrollViewType.VisualizeControl);
            if (commonBehavior != null)
            {
                commonBehavior.RefreshView();
            }
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            GetCommonBehaviorInstance(PayrollViewType.VisualizeControl).RefreshView();
        }
    }
}
