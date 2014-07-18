using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Ccc.WpfControls.Controls.Notes;
using Teleopti.Interfaces.Domain;
using Cursors = System.Windows.Forms.Cursors;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.Win.Intraday
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class IntradayView : BaseRibbonForm, IIntradayView
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
    	private readonly ISendCommandToSdk _sendCommandToSdk;
        private readonly IToggleManager _toggleManager;

        private DateNavigateControl _timeNavigationControl;
        private GridRowInChartSettingButtons _gridrowInChartSetting;
        private IntradayViewContent _intradayViewContent;
        private ToolStripGalleryItem _previousClickedGalleryItem;
        private DateTime _lastUpdateUtc;
        private bool _forceClose;
        private readonly IntradaySettingManager _settingManager;

		public IntradayView(IEventAggregator eventAggregator, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, ISendCommandToSdk sendCommandToSdk, IToggleManager toggleManager)
        {
            _eventAggregator = eventAggregator;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
			_sendCommandToSdk = sendCommandToSdk;
		    _toggleManager = toggleManager;

		    var authorization = PrincipalAuthorization.Instance();

			InitializeComponent();
            if (DesignMode) return;

            ribbonControlAdv1.Enabled = false;
            toolStripExDatePicker.Visible = false;
            SetTexts();
            ControlBox = false;
            StartProgress();
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.LoadingFormThreeDots);
            if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
                Icon = Properties.Resources.intraday;

            _settingManager = new IntradaySettingManager();

			if (authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayReForecasting))
				toolStripExChangeForecast.Visible = true;
        }

        public IntradayPresenter Presenter { get; set; }

        public void StartProgress()
        {
            _eventAggregator.GetEvent<IntradayLoadProgress>().Subscribe(ReportProgress);
        }

        private void ReportProgress(string obj)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(ReportProgress), obj);
                return;
            }
            updateProgress(obj);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
			if (Scenario != null) Text = Text + " - " + Scenario.Description.Name;
            Cursor = Cursors.AppStarting;
            try
            {
                setupMainFunctions();
            }
            catch (DataSourceException dataSourceException)
            {
                if (dataSourceExceptionOccurred(dataSourceException))
                {
                    _forceClose = true;
                    Close();
                    return;
                }
                throw;
            }

            backgroundWorkerLoadControls.RunWorkerAsync();
        }

        public void FinishProgress()
        {
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.ReadyThreeDots);
            _eventAggregator.GetEvent<IntradayLoadProgress>().Unsubscribe(ReportProgress);
            ribbonControlAdv1.Enabled = true;
            toolStripExDatePicker.Visible = true;
            Cursor = Cursors.Default;
            ControlBox = true;
        }

        private void updateProgress(string progressStep)
        {
            Refresh();
            toolStripStatusLabelLastUpdate.Text = progressStep;
            statusStripExLastUpdate.Refresh();   
        }

        private void initializeChartSettings()
        { 
            _gridrowInChartSetting = new GridRowInChartSettingButtons();
            var chartSettingHost = new ToolStripControlHost(_gridrowInChartSetting);
            toolStripExGridRowInChartButtons.Items.Add(chartSettingHost);
            _gridrowInChartSetting.SetButtons();

            _gridrowInChartSetting.LineInChartSettingsChanged += gridlinesInChartSettings_LineInChartSettingsChanged;
            _gridrowInChartSetting.LineInChartEnabledChanged += gridrowInChartSetting_LineInChartEnabledChanged;
        }

        private void initializeNavigationControl()
        {
            _timeNavigationControl = new DateNavigateControl();
            _timeNavigationControl.SelectedDateChanged += timeNavigationControl_SelectedDateChanged;
            var hostDatepicker = new ToolStripControlHost(_timeNavigationControl);
            toolStripExDatePicker.Items.Add(hostDatepicker);
        }

        private void addContentControl()
        {
            gradientPanelContent.Visible = false;
            _intradayViewContent.Visible = false;
            gradientPanelContent.Controls.Clear();
            gradientPanelContent.Controls.Add(_intradayViewContent);
            _intradayViewContent.Dock = DockStyle.Fill;
            gradientPanelContent.Visible = true;
            _intradayViewContent.Visible = true;

            _settingManager.LoadCurrentDockingState();

        }

        private void AuthorizeFunctions()
        {
            toolStripTabItemChart.Visible = Presenter.EarlyWarningEnabled;
            toolStripTabItemChart.Enabled = Presenter.EarlyWarningEnabled;
            toolStripStatusLabelLastUpdate.Visible = Presenter.RealTimeAdherenceEnabled;
        }

        private void SetupIntradayDate()
        {
            _timeNavigationControl.SetAvailableTimeSpan(Presenter.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
        }

        private void InitializeGallery()
        {
            teleoptiToolStripGalleryViews.ParentRibbonTab.CheckState = CheckState.Indeterminate;
            teleoptiToolStripGalleryViews.ParentRibbonTab.Checked = teleoptiToolStripGalleryViews.ParentRibbonTab.Visible;
            teleoptiToolStripGalleryViews.Items.Clear();
        }

        private void FinalizeGallery()
        {
            _settingManager.Remove("xxDefaultView");
            
            FillGallery();

            teleoptiToolStripGalleryViews.ItemClicked += teleoptiToolStripGalleryViews_ItemClicked;
            teleoptiToolStripGalleryViews.GalleryItemClicked += teleoptiToolStripGalleryViews_GalleryItemClicked;
        }

        private void FillGallery()
        {
            teleoptiToolStripGalleryViews.Items.Clear();
            //Place the default first, needs refactoring?!?
            AddDefaultItem();

            foreach (IntradaySetting setting in _settingManager.IntradaySettings)
            {
                var item = new ToolStripGalleryItem();
                item.Text = setting.Name;
                item.Tag = setting;
                item.Image = imageListLayouts.Images[1];
                if (setting.Name == "xxDefaultIntradaySetting")//Default
                    continue;

                teleoptiToolStripGalleryViews.Items.Add(item);
                if (setting.Name.Equals(_settingManager.CurrentIntradaySetting.Name))
                    teleoptiToolStripGalleryViews.SetCheckedItem(item);
            }
           
        }

        private void AddDefaultItem()
        {
            var defaultItem = new ToolStripGalleryItem();
            var defaultSetting = _settingManager.DefaultSetting();
            defaultItem.Text = LanguageResourceHelper.Translate(defaultSetting.Name);
            defaultItem.Tag = defaultSetting;
            defaultItem.Image = imageListLayouts.Images[0];
            teleoptiToolStripGalleryViews.Items.Add(defaultItem);
        }

        #region events
        private void presenter_ExternalAgentStateReceived(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new EventHandler(presenter_ExternalAgentStateReceived), sender, e);
            else
            {
                if ((int) _lastUpdateUtc.TimeOfDay.TotalSeconds == (int) DateTime.UtcNow.TimeOfDay.TotalSeconds)
                    return; //To avoid updates that doesn't change the content

                _lastUpdateUtc = DateTime.UtcNow;
                toolStripStatusLabelLastUpdate.Text = string.Format(CultureInfo.CurrentCulture,
                                                                    Resources.LastUpdateColonParameter0,
                                                                    TimeZoneHelper.ConvertFromUtc(DateTime.UtcNow).
                                                                        ToLongTimeString());
            }
        }

        private void timeNavigationControl_SelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
        {
            Presenter.IntradayDate = e.Value;
        }

        private void toolStripButtonNewView_Click(object sender, EventArgs e)
        {
        	_settingManager.UpdatePreviousDockingState();
        	var ctrl = new PromptTextBox(new object(), "", Resources.View, 50);
        	ctrl.NameThisView += ctrl_NameThisView;
        	ctrl.ShowDialog(this);
        }

    	private void teleoptiToolStripGalleryViews_GalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
        {	
            if (!((IntradaySetting)_previousClickedGalleryItem.Tag).Name.Equals(_settingManager.DefaultSetting().Name)) 
                _settingManager.UpdatePreviousDockingState();

            var view = ((IntradaySetting)e.GalleryItem.Tag).Name;
            _settingManager.LoadDockingState(view);
            
            teleoptiToolStripGalleryViews.SetCheckedItem(e.GalleryItem);
            _intradayViewContent.SelectChartView(view);
            _previousClickedGalleryItem = e.GalleryItem;
            _intradayViewContent.Refresh();
        }

    	private void teleoptiToolStripGalleryViews_ItemClicked(object sender, Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Tag as bool? == true) return;
            SetupGalleryItem(e);
            e.ContextMenuStrip.Items[0].Enabled = !isDefaultView();

            _intradayViewContent.SelectChartView(((IntradaySetting)e.ClickedItem.Tag).Name);
        }

    	private void SetupGalleryItem(Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs e)
        {
            var item = new TupleItem(e.ClickedItem.Text, e.ClickedItem.Tag);

            ToolStripItem itemRemove = new ToolStripMenuItem();
            itemRemove.Text = Resources.Delete;
            itemRemove.Tag = item;
            itemRemove.Click += itemRemove_Click;
            e.ContextMenuStrip.Items.Add(itemRemove);
        }

    	private void itemRemove_Click(object sender, EventArgs e)
        {
			var toolstripItem = ((ToolStripItem) sender);
			toolstripItem.Click -= itemRemove_Click;
			
			var item = (TupleItem)toolstripItem.Tag;
			var deletedItemIndex = teleoptiToolStripGalleryViews.Items.IndexOf(FindGalleryItemByName(item.Text));
           
            _settingManager.Remove(item.Text);
			
            FillGallery();
            toolStripExLayouts.Refresh();

			var previousOne = deletedItemIndex < 1 ? defaultView() : teleoptiToolStripGalleryViews.Items[deletedItemIndex - 1];
            
            var name = ((IntradaySetting) previousOne.Tag).Name;
            _settingManager.LoadDockingState(name);

			teleoptiToolStripGalleryViews.SetCheckedItem(previousOne);
            _intradayViewContent.SelectChartView(name);
			_previousClickedGalleryItem = previousOne;
            _intradayViewContent.Refresh();
        }

        private void ctrl_NameThisView(object sender, CustomEventArgs<TupleItem> e)
        {
			var prompt = sender as PromptTextBox;
			if (prompt != null)
			{
				prompt.NameThisView -= ctrl_NameThisView;
				prompt.Dispose();
			}

            try
            {
                var newSetting = _settingManager.DefaultSetting();
                newSetting.Name = e.Value.Text;
                _settingManager.Persist(newSetting.Name);
            }
            catch (DataSourceException dataSourceException)
            {
                if (dataSourceExceptionOccurred(dataSourceException))
                    return;
            }

            _intradayViewContent.SelectChartView(e.Value.Text);
            FillGallery();
            var item = FindGalleryItemByName(e.Value.Text);
            _previousClickedGalleryItem = item;
            toolStripExLayouts.Refresh();
        }

		private ToolStripGalleryItem FindGalleryItemByName(string name)
    	{
    		foreach (var item in teleoptiToolStripGalleryViews.Items.Cast<ToolStripGalleryItem>().Where(item => ((IntradaySetting)item.Tag).Name.Equals(name)))
				return item;
			return teleoptiToolStripGalleryViews.Items[0];
    	}

    	private void gridlinesInChartSettings_LineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
        {
            _intradayViewContent.LineInChartSettingsChanged(sender, e);
        }

        private void gridrowInChartSetting_LineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
        {
            _intradayViewContent.LineInChartEnabledChanged(e, _gridrowInChartSetting);
        }
       
        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            // a form was canceled
            if (!CloseAllOtherForms(this))
                return; 

            Close();
            //this canceled
            if (Visible)
                return;
            Application.Exit();
        }

        private void IntradayView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_forceClose) return;

            bool cancelClosing;
            try
            {
                cancelClosing = Presenter.CheckIfUserWantsToSaveUnsavedData();
            }
            catch (DataSourceException dataSourceException)
            {
                dataSourceExceptionOccurred(dataSourceException);
                cancelClosing = true;
            }

            e.Cancel = cancelClosing;
            if (e.Cancel || _intradayViewContent == null) return;
            var view = ((IntradaySetting) teleoptiToolStripGalleryViews.CheckedItem.Tag).Name;

            try
            {
                _settingManager.Persist(view);
            }
            catch (DataSourceException)
            {
                //Suppress save of settings...
            }

            _intradayViewContent.Visible = false;
            _intradayViewContent.Close();
        }

        private void toolStripButtonQuickAccessSave_Click(object sender, EventArgs e)
        {
            try
            {
                Presenter.Save();
            }
            catch (DataSourceException dataSourceException)
            {
                dataSourceExceptionOccurred(dataSourceException);
            }
        }

        #endregion

        public void DisableSave()
        {
            toolStripButtonQuickAccessSave.Enabled = false;
        }

        #region pass to control

        public void RefreshPerson(IPerson person)
        {
            _intradayViewContent.RefreshPerson(person);
        }

        public void UpdateShiftEditor(IList<IScheduleDay> scheduleParts)
        {
            _intradayViewContent.UpdateShiftEditor(scheduleParts);
        }

        public void ToggleSchedulePartModified(bool enable)
        {
            _intradayViewContent.ToggleSchedulePartModified(enable);
        }

        public void DrawSkillGrid()
        {
            if (_intradayViewContent!=null)
                _intradayViewContent.DrawSkillGrid(true);
        }

        public ISkill SelectedSkill
        {
            get { return _intradayViewContent.SelectedSkill; }
        }

        public void RefreshRealTimeScheduleControls()
        {
            _intradayViewContent.RefreshRealTimeScheduleControls();
        }

        public void SelectSkillTab(ISkill skill)
        {
            _intradayViewContent.SelectSkillTab(skill);
        }

        public void SetupSkillTabs()
        {
            _intradayViewContent.SetupSkillTabs();
        }

        public void EnableSave()
        {
            toolStripButtonQuickAccessSave.Enabled = true;
        }

        public void UpdateFromEditor()
        {
            NotesEditor.RemoveFocus();
            _intradayViewContent.UpdateFromEditor();
        }

        public void ReloadScheduleDayInEditor(IPerson person)
        {
            _intradayViewContent.ReloadScheduleDayInEditor(person);
        }

        public void ShowBackgroundDataSourceError()
        {
            statusStripButtonServerUnavailable.Visible = true;
        }

        public void HideBackgroundDataSourceError()
        {
            statusStripButtonServerUnavailable.Visible = false;
        }

	    public IScenario Scenario { get; set; }

	    public void SetChartButtons(bool enabled, AxisLocation location, ChartSeriesDisplayType type, Color color)
        {
            _gridrowInChartSetting.SetButtons(enabled, location, type, color);
        }
        #endregion

        private void backgroundWorkerLoadControls_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.Current.Regional.UICulture;
            Presenter.Initialize();
        }

        private void backgroundWorkerLoadControls_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Disposing || IsDisposed) return;
            if (InvokeRequired)
            {
                MethodInvoker updateGui = () => backgroundWorkerLoadControls_RunWorkerCompleted(sender, e);
                BeginInvoke(updateGui);
                return;
            }
            if (e.Error != null)
            {
                if (e.Error is DefaultStateGroupException)
                {
                    ShowErrorMessage(Resources.RtaStateGroupsMustBeConfigured, Resources.ErrorMessage);
                    Close();
                    return;
                }

                if (dataSourceExceptionOccurred(e.Error as DataSourceException))
                {
                    _forceClose = true;
                    Close();
                    return;
                }
                throw e.Error;
            }

            _intradayViewContent = new IntradayViewContent(Presenter, this, _eventAggregator, Presenter.SchedulerStateHolder, _settingManager, _overriddenBusinessRulesHolder, _toggleManager);
            _intradayViewContent.RightToLeft = RightToLeft; //To solve error with wrong dock labels running RTL
            _timeNavigationControl.SetSelectedDate(Presenter.IntradayDate);
            Presenter.ExternalAgentStateReceived += presenter_ExternalAgentStateReceived;
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.LoadingLayoutsThreeDots);
            InitializeGallery();
            toolStripTabItemHome.Checked = true;
            toolStripExLayouts.Size = toolStripExLayouts.PreferredSize;

            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.InitializingContentControlThreeDots);

            try
            {
                addContentControl();
            }
            catch (DataSourceException dse)
            {
                if (dataSourceExceptionOccurred(dse))
                {
                    _forceClose = true;
                    Close();
                    return;
                }
            }
            
            FinalizeGallery();
            selectDefaultView();
            FinishProgress();
        }

        private void selectDefaultView()
        {
            var selectedItem = defaultView();
            
            foreach (ToolStripGalleryItem item in teleoptiToolStripGalleryViews.Items)
            {
                if (item.Tag == _settingManager.CurrentIntradaySetting)
                {
                    selectedItem = item;
                }
            }

            _previousClickedGalleryItem = selectedItem;
            teleoptiToolStripGalleryViews.SetCheckedItem(selectedItem);
        }

        private bool isDefaultView()
        {
            return defaultView().Equals(teleoptiToolStripGalleryViews.CheckedItem);
        }

        private ToolStripGalleryItem defaultView()
        {
            return teleoptiToolStripGalleryViews.Items[0];
        }

        private void setupMainFunctions()
        {
            initializeNavigationControl();

            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.InitializingChartSettingsThreeDots);
            initializeChartSettings();

            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.LoadingPeopleTreeDots);
            
            SetupIntradayDate();
            AuthorizeFunctions();
        }

        private void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            ViewBase.ShowHelp(this,false);
        }

        private void toolStripButtonResetLayout_Click(object sender, EventArgs e)
        {
            _settingManager.ResetDockingState();
        }

        private bool dataSourceExceptionOccurred(DataSourceException exception)
        {
            if (exception == null) return false;

            using (var view = new SimpleExceptionHandlerView(exception, Resources.OpenTeleoptiCCC, Resources.ServerUnavailable))
            {
                view.ShowDialog(this);
            }
            return true;
        }

        private void statusStripButtonServerUnavailable_Click(object sender, EventArgs e)
        {
            statusStripButtonServerUnavailable.Enabled = false;
            Presenter.RetryHandlingMessages();
            statusStripButtonServerUnavailable.Enabled = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || msg.Msg == WM_SYSKEYDOWN)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.S:
                        toolStripButtonQuickAccessSave_Click(this, EventArgs.Empty);
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

		private void toolStripButtonChangeForecastClick(object sender, EventArgs e)
		{
			var skills = new AvailableSkillWithPreselectedSkill(_intradayViewContent.SelectedSkill,
			                                                    Presenter.SchedulerStateHolder.SchedulingResultState
			                                                             .VisibleSkills);
		    var models = new ReforecastModelCollection();
            using (var pages = new ReforecastWizardPages(models))
            {
                var reforecastPages = PropertyPagesHelper.GetReforecastFilePages(skills);
                pages.Initialize(reforecastPages);
                using (var wizard = new WizardNoRoot<ReforecastModelCollection>(pages))
                {
                    if (wizard.ShowDialog(this) != DialogResult.OK) return;
                    var dto = new RecalculateForecastOnSkillCollectionCommandDto
                                  {
									WorkloadOnSkillSelectionDtos = new List<WorkloadOnSkillSelectionDto>(),
									ScenarioId = Presenter.RequestedScenario.Id.GetValueOrDefault()
								  };
                    foreach (var model in models.ReforecastModels)
                    {
                        dto.WorkloadOnSkillSelectionDtos.Add(
                            new WorkloadOnSkillSelectionDto
                                {
                                    SkillId = model.Skill.Id.GetValueOrDefault(),
                                    WorkloadId =
                                        model.Workload.Select(w => w.Id.GetValueOrDefault()).
                                        ToList(),
                                   
                                });
                    }
                    _sendCommandToSdk.ExecuteCommand(dto);
                }
            }
		}

    }
}
