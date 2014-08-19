using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools.Enums;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.MessageBroker.Events;
using log4net;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.Controls.Chart;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.Common.Controls.ToolStripGallery;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Rows;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{

    public partial class Forecaster : BaseRibbonForm, IFinishWorkload
    {
        private ILog _logger = LogManager.GetLogger(typeof (Forecaster));

        private ISkill _skill;
        private IMultisiteSkill _multisiteSkill;
        private readonly IList<AbstractDetailView> _detailViews = new List<AbstractDetailView>();
        private IScenario _scenario;
        private readonly bool _longterm;
        private readonly IDictionary<string, TeleoptiGridControl> _gridCollection = new Dictionary<string, TeleoptiGridControl>();
        private readonly DateOnlyPeriod _dateTimePeriod;

        private ISkillDayCalculator _skillDayCalculator;
        private GridRow _currentSelectedGridRow;
        private ChartControl _chartControl;
        private GridChartManager _gridChartManager;
        private ZoomButtonsEventArgs _zoomButtonsEventArgs;
        private DateOnly _currentLocalDate;
        private DateTime _currentLocalDateTime;
        private SplitterManager _splitterManager;
        private bool _noChangesRightNow;

        private ClipboardControl _clipboardControl;

        private readonly ZoomButtons _zoomButtons;
        private readonly GridViewInChartButtons _zoomButtonsChart;
        private readonly DateNavigateControl _timeNavigationControl;//it was moved and renamed..
        private readonly GridRowInChartSettingButtons _gridrowInChartSetting;
		  private readonly IToggleManager _toggleManager;
        private ForecasterSettings _currentForecasterSettings;
        private ForecasterChartSetting _skillChartSetting;
        private ForecasterChartSetting _workloadChartSetting;
        private ITaskOwnerGrid _currentSelectedGrid;
        private bool _showGraph = true;
        private bool _showSkillView = true;
        private bool _userWantsToCloseForecaster;
        private IUnsavedDaysInfo _unsavedSkillDays = new UnsavedDaysInfo();
        private readonly IDirtyForecastDayContainer _dirtyForecastDayContainer = new DirtyForecastDayContainer();
        private bool _forceClose;

        #region Private

        /// <summary>
        /// Sets the colors.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-27
        /// </remarks>
        private void SetColors()
        {
            Color ribbonContextTabColor = ColorHelper.RibbonContextTabColor();
            for (int i = 0; i < ribbonControlAdv1.TabGroups.Count; i++)
            {
                ribbonControlAdv1.TabGroups[i].Color = ribbonContextTabColor;
            }
        }

        /// <summary>
        /// Sets the permissions on the form controls.
        /// <remarks>
        /// Created By: niclash
        /// Created Date: 10-30-2008
        /// </remarks>
        /// </summary>
        private void SetPermissionOnControls()
        {
            toolStripButtonSystemOptions.Enabled =
               PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
        }

        private void addChart()
        {
            _chartControl = new ChartControl();
            _gridChartManager = new GridChartManager(_chartControl, _skill.SkillType.DisplayTimeSpanAsMinutes);
            _gridChartManager.Create();
            _chartControl.Dock = DockStyle.Fill;
            splitContainer2.Panel1.Controls.Add(_chartControl);

            _zoomButtonsEventArgs = new ZoomButtonsEventArgs
                                        {
                                            Interval = _currentForecasterSettings.ChartInterval,
                                            Target = _currentForecasterSettings.TemplateTarget
                                        };

            reloadChart();
        }

        
        private void Save(Func<bool> callback)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<Func<bool>>(Save), callback);
                return;
            }

            exitEditMode();

            showProgressBar();

            DisableAllControlsExceptCancelLoadButton();
            if (!backgroundWorkerSave.IsBusy)
                backgroundWorkerSave.RunWorkerAsync(callback);
        }

        private void showProgressBar()
        {
            toolStripProgressBarMain.Value = 0;
            toolStripProgressBarMain.Maximum = _dirtyForecastDayContainer.Size;
            toolStripProgressBarMain.Visible = true;

            toolStripStatusLabelInfo.Text = UserTexts.Resources.Save;
            toolStripStatusLabelInfo.Visible = true;
        }

        private void exitEditMode()
        {
            var currentSkillView = GetCurrentSkillDetailView();
            if (currentSkillView != null)
            {
                var currentGrid = currentSkillView.CurrentGrid as GridControl;
                if (currentGrid != null)
                {
                    currentGrid.Model.EndEdit();
                }
            }

            var currentWorkloadView = GetCurrentWorkloadDetailView();
            if (currentWorkloadView != null)
            {
                var currentGrid = currentWorkloadView.CurrentGrid as GridControl;
                if (currentGrid != null)
                {
                    currentGrid.Model.EndEdit();
                }
            }
        }

		private void ChoppedSave()
		{
            ChoppedSaveSkillDays(_dirtyForecastDayContainer.DirtyChildSkillDays);
            ChoppedSaveSkillDays(_dirtyForecastDayContainer.DirtySkillDays);
			ChoppedSaveMultisiteDays();
		}

    	private void ChoppedSaveSkillDays(ICollection<ISkillDay> dirtyList)
    	{
    	    var dirtySkillDays = new List<ISkillDay>();
            dirtySkillDays.AddRange(dirtyList);
            foreach (var skillDay in dirtySkillDays)
    		{
    			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
    			{
    				try
    				{
    					var skillDayRepository = new SkillDayRepository(uow);
						skillDayRepository.Add(skillDay);
    					uow.PersistAll();
                        removeSkillDayFromDirtyList(skillDay);
    				}
                    catch (OptimisticLockException)
    				{
                        AddUnsavedDay(skillDay.CurrentDate);
    				}
                    catch (ConstraintViolationException)
                    {
                        AddUnsavedDay(skillDay.CurrentDate);
                    }
    			}
                reportSavingProgress(1);
			}
    	}

        private void removeSkillDayFromDirtyList(ISkillDay skillDay)
        {
            if (_dirtyForecastDayContainer.DirtySkillDays.Contains(skillDay))
                _dirtyForecastDayContainer.DirtySkillDays.Remove(skillDay);
            if (_dirtyForecastDayContainer.DirtyChildSkillDays.Contains(skillDay))
                _dirtyForecastDayContainer.DirtyChildSkillDays.Remove(skillDay);
        }

        private void removeMultisiteDayFromDirtyList(IMultisiteDay multisiteDay)
        {
            if (_dirtyForecastDayContainer.DirtyMultisiteDays.Contains(multisiteDay))
                _dirtyForecastDayContainer.DirtyMultisiteDays.Remove(multisiteDay);
        }

        private void ChoppedSaveMultisiteDays()
		{
            var dirtyMultisiteDays = new List<IMultisiteDay>();
            dirtyMultisiteDays.AddRange(_dirtyForecastDayContainer.DirtyMultisiteDays);
            foreach (var multisiteDay in dirtyMultisiteDays)
			{
			    using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
                    try
                    {
                        var multisiteDayRepository = new MultisiteDayRepository(uow);
                        multisiteDayRepository.Add(multisiteDay);
                        uow.PersistAll();
                        removeMultisiteDayFromDirtyList(multisiteDay);
                    }
                    catch (OptimisticLockException)
                    {
                        AddUnsavedDay(multisiteDay.MultisiteDayDate);
                    }
                    catch (ConstraintViolationException)
                    {
                        AddUnsavedDay(multisiteDay.MultisiteDayDate);
                    }
				}
			    reportSavingProgress(1);
			}
		}

        private void reportSavingProgress(int step)
        {
            if (backgroundWorkerSave.IsBusy)
            {
                backgroundWorkerSave.ReportProgress(step);
            }
        }

        private void AddUnsavedDay(DateOnly localCurrentDate)
        {
            var unsavedSkillDay = new UnsavedDayInfo(localCurrentDate, _scenario);
            if (!_unsavedSkillDays.Contains(unsavedSkillDay))
                _unsavedSkillDays.Add(unsavedSkillDay);
        }

    	private void InformUserOfUnsavedData()
    	{
            if (_unsavedSkillDays.Count == 0) return;
    		
    		OptimisticLockExceptionInformation();
    	}

    	private void OptimisticLockExceptionInformation()
        {
            var unsavedSkillDays = _unsavedSkillDays.UnsavedDaysOrderedByDate;
    		var messageBox =
    			new MessageBoxWithListView(
    				UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot + UserTexts.Resources.TheDaysListedBelowWillNotBeSavedDot, UserTexts.Resources.Warning,
    				unsavedSkillDays);
			MarkUnsavedDays();
    		messageBox.ShowDialog();
        }

    	private void MarkUnsavedDays()
    	{
            _skillDayCalculator.InvokeDatesUnsaved(_unsavedSkillDays);
    		foreach (var detailView in _detailViews)
    		{
    			detailView.RefreshCurrentTab();
    		}
    	}

    	private bool ValidateForm()
        {
            try
            {
                if (_skillDayCalculator != null)
                    _skillDayCalculator.CheckRestrictions();
            }
            catch (ValidationException validationException)
            {
                ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, validationException.Message), UserTexts.Resources.ValidationError);
                return false;
            }
            return true;
        }

        private DialogResult AskToCommitChanges()
        {
			DialogResult result = ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade, UserTexts.Resources.Save);
            return result;
        }

        #region backgroundworker

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Cancel) return;

        	StatisticHelper statHelper;
        	using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
        	{

        	    _currentForecasterSettings = new PersonalSettingDataRepository(unitOfWork).FindValueByKey("Forecaster",
        	                                                                                              new ForecasterSettings
        	                                                                                                  ());

				unitOfWork.Reassociate(_skill);
				if (IsMultisiteSkill)
				{
					foreach (var childSkill in _multisiteSkill.ChildSkills)
					{
						unitOfWork.Reassociate(childSkill);
					}
				}
				foreach (IWorkload workload in _skill.WorkloadCollection)
				{
					unitOfWork.Reassociate(workload);
					if (e.Cancel) return;
				}

				backgroundWorker1.ReportProgress(5, UserTexts.Resources.DataSourceInitialized);
				statHelper = new StatisticHelper(
					new RepositoryFactory(),
					unitOfWork);
				statHelper.StatusChanged += _statHelper_StatusChanged;
				LoadSkillDays(unitOfWork, statHelper);

        	    _skillChartSetting = new ForecasterChartSetting(TemplateTarget.Skill);
				_skillChartSetting.Initialize();
				_workloadChartSetting = new ForecasterChartSetting(TemplateTarget.Workload);
				_workloadChartSetting.Initialize();

				_skillDayCalculator.DistributeStaff(); //Does the initial fixing of distributed demand
			}
        	backgroundWorker1.ReportProgress(1, UserTexts.Resources.Done);

            statHelper.StatusChanged -= _statHelper_StatusChanged;
		}

        private void LoadSkillDays(IUnitOfWork unitOfWork, StatisticHelper statHelper)
        {
            var periodToLoad = SkillDayCalculator.GetPeriodToLoad(_dateTimePeriod);
            IList<ISkillDay> skillDays = statHelper.LoadStatisticData(periodToLoad, _skill, _scenario,
                                                                      _longterm);
            if (IsMultisiteSkill)
            {
                backgroundWorker1.ReportProgress(5, UserTexts.Resources.MultisiteSkillLoading);
                var multisiteDays = MultisiteHelper.LoadMultisiteDays(
                    periodToLoad, _multisiteSkill, _scenario,
                    new MultisiteDayRepository(unitOfWork), false).ToList();
                var multisiteCalculator = new MultisiteSkillDayCalculator(_multisiteSkill, skillDays, multisiteDays,
                                                                          _dateTimePeriod);

                foreach (var childSkill in _multisiteSkill.ChildSkills)
                {
                    multisiteCalculator.SetChildSkillDays(childSkill,
                                                          statHelper.LoadStatisticData(periodToLoad, childSkill,
                                                                                       _scenario, _longterm));
                }
					
                _skillDayCalculator = multisiteCalculator;
            }
            else
            {
                _skillDayCalculator = new SkillDayCalculator(_skill, skillDays, _dateTimePeriod);
            }
        }

        private void EnableOrDisableGridControls(bool enable)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<bool>(EnableOrDisableGridControls), enable);
			}
			else
			{
				foreach (var gridControl in _gridCollection.Values)
				{
					gridControl.Enabled = enable;
				}
				Cursor = !enable ? Cursors.WaitCursor : Cursors.Default;
			}
		}

        void _statHelper_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            backgroundWorker1.ReportProgress(5, UserTexts.Resources.DataSourceLoading);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var text = (string)e.UserState;
            if (toolStripStatusLabelInfo.Text.Equals(UserTexts.Resources.CancelLoading))
                text = UserTexts.Resources.CancelLoading;

            toolStripStatusLabelInfo.Text = text;
            toolStripProgressBarMain.PerformStep();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Cancelled) return;
            
            if (datasourceExceptionOccurred(e.Error))
            {
                _forceClose = true;
                Close();
                return;
            }

            throwIfExceptionOccurred(e);

            LoadDisplayOptionsFromSetting();

            try
            {
                initializeSkillWorkload();

                reloadScenarioMenuItems();
            }
            catch (DataSourceException dataSourceException)
            {
                if (datasourceExceptionOccurred(dataSourceException))
                {
                    _forceClose = true;
                    Close();
                    return;
                }
                throw;
            }
            
            //Listen to future changes
            EntityEventAggregator.EntitiesNeedsRefresh += MainScreen_EntitiesNeedsRefresh;
            _chartControl.ChartRegionMouseEnter += _chartControl_ChartRegionMouseEnter;
            toolStripProgressBarMain.Step++;
            _chartControl.ChartRegionClick += _chartControl_ChartRegionClick;
            toolStripProgressBarMain.Visible = false;
            toolStripStatusLabelInfo.Visible = false;
            toolStripTabItemMultisite.Visible = false;

            if(_userWantsToCloseForecaster)
            {
                BeginInvoke(new MethodInvoker(Close));
                return;
            }

        	var multisiteSkillDayCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
			if (multisiteSkillDayCalculator != null)
			{
				foreach (var multisiteDay in multisiteSkillDayCalculator.MultisiteDays)
				{
					multisiteDay.ValueChanged += multisiteDay_ValueChanged;
					multisiteDay.MultisiteSkillDay.StaffRecalculated += skillDay_StaffRecalculated;
					foreach(var childSkillDay in multisiteDay.ChildSkillDays)
					{
						childSkillDay.StaffRecalculated += childSkillDay_StaffRecalculated;
					}
				}
			}
			else
			{
				foreach (var skillDay in _skillDayCalculator.SkillDays)
				{
					skillDay.StaffRecalculated += skillDay_StaffRecalculated;
				}
			}

            EnableAllControlsExceptCancelLoadButton();

        	_detailViews.First().CurrentDay = _dateTimePeriod.StartDate;
			LogPointOutput.LogInfo("Forecast.LoadAndOpenSkill", "Completed");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private static void throwIfExceptionOccurred(RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Exception ex = new Exception("Background thread exception", e.Error);
                throw ex;
            }
        }

        private bool datasourceExceptionOccurred(Exception exception)
        {
            if (exception != null)
            {
                var dataSourceException = exception as DataSourceException;
                if (dataSourceException == null)
                    return false;

                using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
                {
                    view.ShowDialog(this);
                }

                return true;
            }
            return false;
        }

    	private void childSkillDay_StaffRecalculated(object sender, EventArgs e)
    	{
			var skillDay = sender as ISkillDay;
			if (skillDay == null) return;
            if (skillDay.Scenario.Equals(_scenario))
                _dirtyForecastDayContainer.DirtyChildSkillDays.Add(skillDay);
    	}

    	private void multisiteDay_ValueChanged(object sender, EventArgs e)
    	{
    		var multisiteDay = sender as IMultisiteDay;
			if (multisiteDay == null) return;
            if (multisiteDay.Scenario.Equals(_scenario))
                _dirtyForecastDayContainer.DirtyMultisiteDays.Add(multisiteDay);
    	}

    	private void skillDay_StaffRecalculated(object sender, EventArgs e)
    	{
			var skillDay = sender as ISkillDay;
			if (skillDay == null) return;
            if (skillDay.Scenario.Equals(_scenario))
                _dirtyForecastDayContainer.DirtySkillDays.Add(skillDay);
    	}

    	private void initializeSkillWorkload()
        {
            loadAllDetailViews();
            reassociateSkills();
    	    finalizeLoadedDetailViews();
        }

        private void reloadDetailViews()
        {
            loadAllDetailViews();
            finalizeLoadedDetailViews();
        }

        private void finalizeLoadedDetailViews()
        {
            toolStripStatusLabelInfo.Text = UserTexts.Resources.TemplatesLoaded;
            toolStripProgressBarMain.Step++;
            splitContainer2.Panel1.Controls.Remove(_chartControl);
            addChart();
            toolStripStatusLabelInfo.Text = UserTexts.Resources.ChartInitialized;
            toolStripProgressBarMain.Step++;

            toolStripProgressBarMain.Step++;

            Text = string.Concat(UserTexts.Resources.TeleoptiRaptorColonForecaster, " ",
                                 _dateTimePeriod.StartDate.ToShortDateString(CultureInfo.CurrentUICulture), " - ",
								 _dateTimePeriod.EndDate.ToShortDateString(CultureInfo.CurrentUICulture), " ",
								 _skill.Name, " | ",
                                 UserTexts.Resources.ScenarioColon, " ",
                                 _scenario.Description.Name);
            SetGridOpeningGridViews();
            Cursor = Cursors.Default;
        }

        private void reassociateSkills()
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(_skill);
                if (IsMultisiteSkill)
                {
                    foreach (var childSkill in _multisiteSkill.ChildSkills)
                    {
                        unitOfWork.Reassociate(childSkill);
                    }
                }
                foreach (IWorkload workload in _skill.WorkloadCollection)
                {
                    unitOfWork.Reassociate(workload);
                }
                LazyLoadingManager.Initialize(_skillDayCalculator.Skill);
                LazyLoadingManager.Initialize(_skillDayCalculator.Skill.SkillType);
                LazyLoadingManager.Initialize(_skillDayCalculator.Skill.WorkloadCollection);
                foreach (TemplateTarget templateTarget in Enum.GetValues(typeof (TemplateTarget)))
                {
                    CreateTemplateGalleryRibbonBar(templateTarget, false, false);
                }
            }
        }

        private void loadAllDetailViews()
        {
            loadWorkloadDetailViews();
            toolStripStatusLabelInfo.Text = UserTexts.Resources.WorkloadDaysLoaded;
            toolStripProgressBarMain.Step++;

            if (IsMultisiteSkill)
            {
                loadMultisiteSkillDetailViews();
            }
            else
            {
                loadSkillDetailView();
            }

            RefreshTabs();

            toolStripStatusLabelInfo.Text = UserTexts.Resources.SkillDaysLoaded;
            toolStripProgressBarMain.Step++;
        }

        private void DisableAllControlsExceptCancelLoadButton()
        {
            officeDropDownButtonSaveToScenario.Enabled = false;
            toolStripButtonForecastWorkflow.Enabled = false;
            toolStripButtonSave2.Enabled = false;
            toolStripTabItemChart.Enabled = false;
        	ControlBox = false;

            EnableOrDisableGridControls(false);
            ribbonControlAdv1.Enabled = false;
        }

        private void EnableAllControlsExceptCancelLoadButton()
        {
            officeDropDownButtonSaveToScenario.Enabled = true;
            toolStripButtonForecastWorkflow.Enabled = true;
            toolStripButtonSave2.Enabled = true;
            toolStripTabItemChart.Enabled = true;
        	ControlBox = true;

            EnableOrDisableGridControls(true);
            ribbonControlAdv1.Enabled = true;
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                LoadSkill(_skill);
            }
            catch (DataSourceException dataSourceException)
            {
                if (datasourceExceptionOccurred(dataSourceException))
                {
                    _forceClose = true;
                    Close();
                    return;
                }
                throw;
            }
            
            if (_skill.WorkloadCollection.Count() == 0)
            {
                Close();
                return;
            }

            Cursor = Cursors.WaitCursor;
            DisableAllControlsExceptCancelLoadButton();

            initializeTexts();
            initializeEvents();
            SetPermissionOnControls();
            SetToolStripsToPreferredSize();

            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;

            backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// Some events are not shown in a controls event list, initialize them here.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-18
        /// </remarks>
        private void initializeEvents()
        {
            toolStripTextBoxNewScenario.GotFocus += toolStripTextBoxNewScenario_GotFocus;
            toolStripTextBoxNewScenario.LostFocus += toolStripTextBoxNewScenario_LostFocus;
        }

        /// <summary>
        /// Initializes texts that wont load automatically.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-12-18
        /// </remarks>
        private void initializeTexts()
        {
            officeDropDownButtonSaveToScenario.DropDownText = UserTexts.Resources.SaveAsScenario;
            toolStripTextBoxNewScenario.Text = "(" + UserTexts.Resources.NewScenario + ")";
        }

	    private void reloadScenarioMenuItems()
	    {
		    using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
		    {
			    IScenarioRepository scenarioRepository = new ScenarioRepository(unitOfWork);
			    IList<IScenario> scenarios = scenarioRepository.FindAllSorted();

			    flowLayoutExportToScenario.ContainerControl.Controls.Clear();
			    foreach (var scenario in scenarios)
			    {
				    if (_scenario.Description.Name == scenario.Description.Name) continue;
				    var button = new ButtonAdv
				    {
					    Text = scenario.Description.Name,
					    Width = 300,
					    Height = 80,
					    Appearance = ButtonAppearance.Metro,
					    UseVisualStyle = true,
					    Tag = scenario,
						 BackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))))
				    };

				    button.Font.ChangeToBold();
				    button.Click += scenarioMenuItem_Click;
				    flowLayoutExportToScenario.ContainerControl.Controls.Add(button);
			    }
		    }
	    }

	    private void scenarioMenuItem_Click(object sender, EventArgs e)
	    {
		    var scenario = (IScenario) ((ButtonAdv) sender).Tag;
			 SaveForecastToScenario(scenario);
	    }

	    private void SaveForecastToScenario(IScenario scenario)
        {
            Cursor = Cursors.WaitCursor;

            EntityEventAggregator.EntitiesNeedsRefresh -= MainScreen_EntitiesNeedsRefresh;

            initializeProgressBarBeforeSaveForecastToScenario();

            _dirtyForecastDayContainer.Clear();

            if (!backgroundWorkerSave.IsBusy)
                backgroundWorkerSave.RunWorkerAsync(scenario);
        }

        private void saveForecastToScenarioCommand_ProgressReporter(object sender, CustomEventArgs<int> e)
        {
            reportSavingProgress(e.Value);
        }

        private void initializeProgressBarBeforeSaveForecastToScenario()
        {
            toolStripProgressBarMain.Value = 0;
            int daysToSave = 0;
            daysToSave += _skillDayCalculator.VisibleSkillDays.Count();
            var skillDayCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
            if (skillDayCalculator != null)
            {
                daysToSave += skillDayCalculator.MultisiteDays.Count();
                foreach (var childSkill in _multisiteSkill.ChildSkills)
                {
                    daysToSave += skillDayCalculator.GetVisibleChildSkillDays(childSkill).Count();
                }
            }
            toolStripProgressBarMain.Maximum = daysToSave;
            toolStripProgressBarMain.Visible = true;

            toolStripStatusLabelInfo.Text = UserTexts.Resources.Save;
            toolStripStatusLabelInfo.Visible = true;

            DisableAllControlsExceptCancelLoadButton();
        }

        private void _chartControl_ChartRegionClick(object sender, ChartRegionMouseEventArgs e)
        {
            int column = (int)Math.Round(GridChartManager.GetIntervalValueForChartPoint(_chartControl, e.Point));

            _currentLocalDate = new DateOnly(_gridChartManager.GetDateByColumn(column, _currentLocalDate.Date));
            _timeNavigationControl.SetSelectedDate(_currentLocalDate);
        }

        private void MainScreen_EntitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
        {
           if (e.EntityStatus == DomainUpdateType.Delete)
            {
                foreach (var workload in _skill.WorkloadCollection)
                {
                    if (e.UpdatedIds.Contains(workload.Id.Value))
                    {
                        string warningMessage = String.Format(CultureInfo.CurrentCulture, UserTexts.Resources.WorkloadWindowAlreadyOpen, workload.Name);
                        ShowWarningMessage(warningMessage, UserTexts.Resources.WarningMessageTitle);
                        return;
                    }
                }
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, EntitiesUpdatedEventArgs>(MainScreen_EntitiesNeedsRefresh), sender, e);
                return;
            }
            if (typeof(IWorkload).IsAssignableFrom(e.EntityType))
            {
                CreateTemplateGalleryRibbonBar(TemplateTarget.Workload, true, true);
            }
            else if (typeof(IMultisiteSkill).IsAssignableFrom(e.EntityType) &&
                e.UpdatedIds.Contains(_skill.Id.Value))
            {
                CreateTemplateGalleryRibbonBar(TemplateTarget.Multisite, true, true);
                CreateTemplateGalleryRibbonBar(TemplateTarget.Skill, true, true);
            }
            else if (typeof(ISkill).IsAssignableFrom(e.EntityType) ||
                     typeof(IChildSkill).IsAssignableFrom(e.EntityType))
            {
                CreateTemplateGalleryRibbonBar(TemplateTarget.Skill, true, true);
            }
            else if (typeof(ISkillDay).IsAssignableFrom(e.EntityType))
            {
                RefreshSkillDays(e.UpdatedIds);
            }
        }

        private void RefreshSkillDays(IEnumerable<Guid> guidList)
        {
            bool currentSkillInvolved = false;
            bool currentPeriodInvolved = false;

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillDayRepository skillDayRepository = new SkillDayRepository(unitOfWork);
                foreach (Guid guid in guidList)
                {
                    if (currentPeriodInvolved && currentSkillInvolved) break;

                    ISkillDay skillDay = skillDayRepository.Get(guid);
                    if (skillDay == null) continue;

                    if (_skill.Equals(skillDay.Skill))
                    {
                        currentSkillInvolved = true;
                    }
                    if (_multisiteSkill != null && _multisiteSkill.ChildSkills.Any(s => s.Equals(skillDay.Skill)))
                    {
                        currentSkillInvolved = true;
                    }
                    if (_skillDayCalculator.VisiblePeriod.Contains(skillDay.CurrentDate)) currentPeriodInvolved = true;
                }
            }

            if (currentSkillInvolved && currentPeriodInvolved)
            {
                reloadCurrentData();
            }
        }

        private void reloadCurrentData()
        {
            toolStripProgressBarMain.Visible = true;
            toolStripStatusLabelInfo.Text = UserTexts.Resources.LoadingThreeDots;
            officeDropDownButtonSaveToScenario.Enabled = false;

            EntityEventAggregator.EntitiesNeedsRefresh -= MainScreen_EntitiesNeedsRefresh;
            _chartControl.ChartRegionMouseEnter -= _chartControl_ChartRegionMouseEnter;
            _chartControl.ChartRegionClick -= _chartControl_ChartRegionClick;

            LoadSkill(_skill);
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
            backgroundWorker1.RunWorkerAsync();
        }

        private void detailView_ValuesChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(detailView_ValuesChanged),sender,e);
                return;
            }
            RefreshTabs();
        }

        public void RefreshTabs()
        {
            foreach (AbstractDetailView detailView in _detailViews)
            {
                detailView.RefreshCurrentTab();
                detailView.RefreshIntradayBehindCurrentTab();
            }
            if (_gridChartManager != null) _gridChartManager.ReloadChart();
        }

        #endregion

        #region Public

        /// <summary>
        /// Initializes a new instance of the <see cref="Forecaster"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        protected Forecaster()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                SetTexts();
                ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdv1);
            }

            SetColors();
            RibbonTemplatePanelsClose();
	        ribbonControlAdv1.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
            Application.DoEvents();

            WindowState = FormWindowState.Maximized;
            toolStripStatusLabelInfo.Text = UserTexts.Resources.Initializing;
            toolStripProgressBarMain.Value = 0;
            toolStripProgressBarMain.Step++;
        }

        /// <summary>
        /// Sets the grid opening grid views.
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-09
        /// </remarks>
        private void SetGridOpeningGridViews()
        {
            SetGridZoomLevel(TemplateTarget.Workload, _currentForecasterSettings.WorkingInterval);
            SetGridZoomLevel(TemplateTarget.Skill, _currentForecasterSettings.WorkingIntervalSkill);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Forecaster"/> class.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="longterm">if set to <c>true</c> [longterm].</param>
	    /// <param name="toggleManager"></param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
	    public Forecaster(ISkill skill, DateOnlyPeriod dateTimePeriod, IScenario scenario, bool longterm, IToggleManager toggleManager)
            : this()
        {
		    _toggleManager = toggleManager;
            _dateTimePeriod = dateTimePeriod;

            _zoomButtons = new ZoomButtons();
            _zoomButtons.ZoomChanged += buttons_ZoomChanged;
            ToolStripControlHost host3 = new ToolStripControlHost(_zoomButtons);
            toolStripExZoomBtns.Items.Add(host3);

            _zoomButtonsChart = new GridViewInChartButtons();
            _zoomButtonsChart.ZoomChanged += _zoomButtonsChart_ZoomChanged;
            ToolStripControlHost host4 = new ToolStripControlHost(_zoomButtonsChart);
            toolStripExChartViews.Items.Add(host4);

            _timeNavigationControl = new DateNavigateControl();
            _timeNavigationControl.SetAvailableTimeSpan(_dateTimePeriod);
            _timeNavigationControl.SelectedDateChanged += _timeNavigationControl_SelectedDateChanged;
            ToolStripControlHost hostDatepicker = new ToolStripControlHost(_timeNavigationControl);
            toolStripExDatePicker.Items.Add(hostDatepicker);

            SetUpClipboard();

            _gridrowInChartSetting = new GridRowInChartSettingButtons();
            ToolStripControlHost chartsetteinghost = new ToolStripControlHost(_gridrowInChartSetting);
            toolStripExGridRowInChartButtons.Items.Add(chartsetteinghost);
            _gridrowInChartSetting.SetButtons();

            _gridrowInChartSetting.LineInChartSettingsChanged += _gridlinesInChartSettings_LineInChartSettingsChanged;
            _gridrowInChartSetting.LineInChartEnabledChanged += _gridrowInChartSetting_LineInChartEnabledChanged;

            _skill = skill;
            _scenario = scenario;
            _longterm = longterm;
            _currentLocalDate = dateTimePeriod.StartDate;
            _currentLocalDateTime = _currentLocalDate;

            ribbonControlAdv1.TabGroups[0].Name = UserTexts.Resources.Templates;
            toolStripTabItemMultisite.Visible = false;
            toolStripTabItemSkill.Visible = true;
            toolStripTabItemWorkload.Visible = true;
            ribbonControlAdv1.TabGroups[0].Visible = true;

            toolStripStatusLabelInfo.Text = UserTexts.Resources.SkillLoaded;
            toolStripProgressBarMain.Step++;
        }

        private void LoadDisplayOptionsFromSetting()
        {
            SplitterManager.ShowGraph = _currentForecasterSettings.ShowGraph;
            toolStripButtonShowGraph.Checked = _currentForecasterSettings.ShowGraph;
            _showGraph = _currentForecasterSettings.ShowGraph;
            SplitterManager.ShowSkillView = _currentForecasterSettings.ShowSkillView;
            toolStripButtonShowSkillView.Checked = _currentForecasterSettings.ShowSkillView;
            _showSkillView = _currentForecasterSettings.ShowSkillView;
        }

        private void LoadSkill(ISkill skill)
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var skillRepository = new SkillRepository(uow);
                _multisiteSkill = _skill as IMultisiteSkill;
                if (_multisiteSkill == null)
                    _skill = skillRepository.LoadSkill(skill);
                else
                {
                    _skill = skillRepository.LoadMultisiteSkill(skill);
                    _multisiteSkill = _skill as IMultisiteSkill;
                }
            }
        }

        private void SetUpClipboard()
        {
            _clipboardControl = new ClipboardControl();
            ToolStripControlHost hostClipboardControl = new ToolStripControlHost(_clipboardControl);
            toolStripEx1.Items.Add(hostClipboardControl);
            var copySpecialButton = new ToolStripButton {Text = UserTexts.Resources.CopySpecial, Tag = "special"};
            copySpecialButton.Click += (s, e) => OperateOnActiveGridControl(GridHelper.CopySelectedValuesAndHeadersToPublicClipboard);
            _clipboardControl.CopySpecialItems.Add(copySpecialButton);
            _clipboardControl.CutClicked += (s, e) => OperateOnActiveGridControl(x => x.CutPaste.Cut());
            _clipboardControl.CopyClicked += (s, e) => OperateOnActiveGridControl(x => x.CutPaste.Copy());
            _clipboardControl.PasteClicked += (s, e) => OperateOnActiveGridControl(x => x.CutPaste.Paste());
        }

        private void OperateOnActiveGridControl(Action<GridControl> operation)
        {
            var theGrid = new ColorHelper().GetActiveControl(ActiveControl) as GridControl;
            if (theGrid != null)
                operation.Invoke(theGrid);
        }

        private void _gridrowInChartSetting_LineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
        {
            _gridChartManager.UpdateChartSettings(_currentSelectedGrid, _currentSelectedGridRow, _gridrowInChartSetting, e.Enabled);
            if (_gridChartManager.CurrentGrid != _currentSelectedGrid)
            {
                _gridChartManager.ReloadChart();
            }
        }

        private void _gridlinesInChartSettings_LineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
        {
            _gridChartManager.UpdateChartSettings(_currentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis, e.LineColor);
        }

        /// <summary>
        /// Handles the ZoomChanged event of the<see cref="ZoomButtons">gridzoom control</see>
        /// 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ZoomButtonsEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-05
        /// </remarks>
        private void buttons_ZoomChanged(object sender, ZoomButtonsEventArgs e)
        {
            SetGridZoomLevel(e.Target, e.Interval);
            if (e.Target == TemplateTarget.Workload)
            {
                _currentForecasterSettings.WorkingInterval = e.Interval;
            }
            else
            {
                _currentForecasterSettings.WorkingIntervalSkill = e.Interval;
            }
        }

        private void SetGridZoomLevel(TemplateTarget target, WorkingInterval workingInterval)
        {
            if (_zoomButtons != null) _zoomButtons.CheckButton(target, workingInterval);
            foreach (AbstractDetailView detailView in _detailViews)
            {
                //Since there is either a skill or a multisite skill tab it works
                if ((detailView.TargetType == target) ||
                    (detailView.TargetType == TemplateTarget.Multisite && target == TemplateTarget.Skill))
                {
                    detailView.ShowTab(workingInterval);
                    return;
                }
            }
        }

        private void _zoomButtonsChart_ZoomChanged(object sender, ZoomButtonsEventArgs e)
        {
            e.GridKey = string.Empty;
            SetChartZoomLevel(e);
            _zoomButtonsEventArgs = e;
            _currentForecasterSettings.ChartInterval = _zoomButtonsEventArgs.Interval;
            _currentForecasterSettings.TemplateTarget = _zoomButtonsEventArgs.Target;
        }

        private void SetChartZoomLevel(ZoomButtonsEventArgs e)
        {
            string name;
            string type;
            Guid id;
            if (e.Target == TemplateTarget.Workload)
            {
                type = UserTexts.Resources.Workload;
                name = ((WorkloadDetailView)GetCurrentWorkloadDetailView()).Workload.Name;
                id =  ((WorkloadDetailView)GetCurrentWorkloadDetailView()).Workload.Id.GetValueOrDefault();
            }
            else //multisite how do i work it?
            {
                type = UserTexts.Resources.Skill;
                ISkill skill = null;
                AbstractDetailView undeterminedDetailView = GetCurrentSkillDetailView();
                SkillDetailView skillDetailView = undeterminedDetailView as SkillDetailView;
                MultisiteSkillDetailView multisiteSkillDetailView = undeterminedDetailView as MultisiteSkillDetailView;
                ChildSkillDetailView childSkillDetailView = undeterminedDetailView as ChildSkillDetailView;
                if (skillDetailView != null)
                {
                    skill = skillDetailView.Skill;
                }
                if (multisiteSkillDetailView != null)
                {
                    skill = multisiteSkillDetailView.Skill;
                }
                if (childSkillDetailView != null)
                {
                    skill = childSkillDetailView.Skill;
                }
                if (skill != null)
                {
                    name = skill.Name;
                    id = skill.Id.GetValueOrDefault();
                }
                else
                {
                    name = string.Empty;
                    id = Guid.Empty;
                }
            }
            if (string.IsNullOrEmpty(e.GridKey))
                e.GridKey = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", id, e.Target, e.Interval);

            string chartName = string.Format(CultureInfo.CurrentCulture, "{0} - {1} - {2}", type, name, LanguageResourceHelper.TranslateEnumValue(e.Interval));
            if (e.Interval == WorkingInterval.Intraday)
            {
                chartName = string.Format(CultureInfo.CurrentCulture, "{0} {1}", chartName, _currentLocalDate.ToShortDateString());
            }
            TeleoptiGridControl gridControl;
            if (_gridCollection.TryGetValue(e.GridKey, out gridControl))
                _gridChartManager.ReloadChart((ITaskOwnerGrid)gridControl, chartName);

            if (_zoomButtonsChart != null) _zoomButtonsChart.CheckSingleButton(e.Target, e.Interval);
        }

        private void SetToolStripsToPreferredSize()
        {
            toolStripExChartViews.Size = toolStripExChartViews.PreferredSize;
            toolStripExCurrentChart.Size = toolStripExCurrentChart.PreferredSize;
            toolStripExDatePicker.Size = toolStripExDatePicker.PreferredSize;
            toolStripExGridRowInChartButtons.Size = toolStripExGridRowInChartButtons.PreferredSize;
            toolStripExNumber.Size = toolStripExNumber.PreferredSize;
            toolStripExOutput.Size = toolStripExOutput.PreferredSize;
            toolStripExWorkflow.Size = toolStripExWorkflow.PreferredSize;
            toolStripExZoomBtns.Size = toolStripExZoomBtns.PreferredSize;
        }

        private void reloadChart()
        {
            SetChartZoomLevel(_zoomButtonsEventArgs);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is multisite skill.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is multisite skill; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-24
        /// </remarks>
        private bool IsMultisiteSkill
        {
            get { return (_multisiteSkill != null); }
        }

        private void loadWorkloadDetailViews()
        {
            tabControlWorkloads.TabPages.Clear();

            _detailViews.Clear();
            _gridCollection.Clear();

            foreach (IWorkload workload in _skill.WorkloadCollection)
            {
                TabPageAdv tabPage = ColorHelper.CreateTabPage(workload.Name, workload.Description);
                WorkloadDetailView workloadDetailView = new WorkloadDetailView(
                    (SkillDayCalculator)_skillDayCalculator, workload, _workloadChartSetting);
                workloadDetailView.Name = "Workload";
                InitializeDetailView(workloadDetailView);
                tabControlWorkloads.TabPages.Add(tabPage);
                tabPage.Controls.Add(workloadDetailView);
                foreach (KeyValuePair<string, TeleoptiGridControl> pair in workloadDetailView.GridCollection)
                {
                    _gridCollection.Add(pair.Key, pair.Value);
                    pair.Value.Refresh();
                    pair.Value.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table()); //Cheat to make the merge work as expected
                }
            }
        }

        private void loadMultisiteSkillDetailViews()
        {
            MultisiteSkillDayCalculator multisiteCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
            tabControlAdvMultisiteSkill.TabPages.Clear();
            TabPageAdv tabPage = ColorHelper.CreateTabPage(_multisiteSkill.Name, _multisiteSkill.Description);
            MultisiteSkillDetailView multisiteSkillDetailView = new MultisiteSkillDetailView(multisiteCalculator, _skillChartSetting);
            multisiteSkillDetailView.Name = "MultiSkill";
            InitializeDetailView(multisiteSkillDetailView);
            tabControlAdvMultisiteSkill.TabPages.Add(tabPage);
            tabPage.Controls.Add(multisiteSkillDetailView);

            foreach (IChildSkill childSkill in _multisiteSkill.ChildSkills)
            {
                tabPage = ColorHelper.CreateTabPage(childSkill.Name, childSkill.Description);
                
                ChildSkillDetailView childSkillDetailView = new ChildSkillDetailView(multisiteCalculator, childSkill, _skillChartSetting);
				childSkillDetailView.Name = "MultiSkill";
                InitializeDetailView(childSkillDetailView);
                tabControlAdvMultisiteSkill.TabPages.Add(tabPage);
                tabPage.Controls.Add(childSkillDetailView);

                //Add the grids to the grid collection
                foreach (KeyValuePair<string, TeleoptiGridControl> pair in childSkillDetailView.GridCollection)
                {
                    _gridCollection.Add(pair.Key, pair.Value);
                    pair.Value.Refresh();
                    pair.Value.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table()); //Cheat to make the merge work as expected
                }
            }

            multisiteCalculator.InitializeChildSkills();

            //Add the grids to the grid collection
            try
            {
                foreach (KeyValuePair<string, TeleoptiGridControl> pair in multisiteSkillDetailView.GridCollection)
                {
                    _gridCollection.Add(pair.Key, pair.Value);
                    pair.Value.Refresh();
                    pair.Value.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table()); //Cheat to make the merge work as expected
                }
            }
            catch (ArgumentException)
            {
                ShowWarningMessage(UserTexts.Resources.ChildSkillWithSameNameAsParentSkillWarning, UserTexts.Resources.WarningMessageTitle);
                _userWantsToCloseForecaster = true;
            }
        }

        #endregion

        private void detailView_WorkingIntervalChanged(object sender, WorkingIntervalChangedEventArgs e)
        {
            if (_noChangesRightNow) return;
            _noChangesRightNow = true;

            AbstractDetailView myabstractDetailView = sender as AbstractDetailView;
            ShowTabGroup(myabstractDetailView.TargetType, myabstractDetailView.CurrentWorkingInterval);
            if (myabstractDetailView is MultisiteSkillDetailView)
                ShowTabGroup(TemplateTarget.Multisite, myabstractDetailView.CurrentWorkingInterval); //Both skill and multisite should be shown
            if (myabstractDetailView is ChildSkillDetailView)
                ShowTabGroup(TemplateTarget.Multisite, WorkingInterval.Custom); //Never show multisite template tab for child skill detail views

            foreach (var detailView in _detailViews)
            {

                //No need to do more if it's the same detail view
                if (detailView == myabstractDetailView) continue;
                detailView.CurrentDay = e.NewStartDate;
                if (e.NewTimeOfDay != TimeSpan.Zero)
                {
                    detailView.CurrentTimeOfDay = e.NewTimeOfDay;
                }
                if (detailView.TargetType != myabstractDetailView.TargetType) continue;
                detailView.ShowTab(e.NewWorkingInterval);
            }

            _currentLocalDate = new DateOnly(e.NewStartDate);
            _currentLocalDateTime = e.NewStartDate.Date.Add(e.NewTimeOfDay);
            _timeNavigationControl.SetSelectedDate(_currentLocalDate);
            _noChangesRightNow = false;

            reloadChart();
        }

        private IForecastTemplateOwner GetRefreshedAggregateRoot(TemplateTarget templateTarget, bool refresh)
        {
            IForecastTemplateOwner root = null;
            switch (templateTarget)
            {
                case TemplateTarget.Skill:
                    AbstractDetailView detailView = GetCurrentSkillDetailView();
                    root = (ISkill)detailView.GetType().GetProperty("Skill")
                        .GetValue(detailView, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
                    break;
                case TemplateTarget.Multisite:
                    AbstractDetailView multisiteDetailView = GetCurrentSkillDetailView() as MultisiteSkillDetailView;
                    if (multisiteDetailView == null)
                    {
                        multisiteDetailView = GetCurrentSkillDetailView() as ChildSkillDetailView;
                    }
                    if (multisiteDetailView != null)
                    {
                        root = (ISkill)multisiteDetailView.GetType().GetProperty("Skill")
                        .GetValue(multisiteDetailView, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
                    }
                    break;
                case TemplateTarget.Workload:
                    WorkloadDetailView workloadDetailView = GetCurrentWorkloadDetailView() as WorkloadDetailView;
                    if (workloadDetailView != null) root = workloadDetailView.Workload;
                    break;
            }
            if (root != null && refresh) ForecastingTemplateRefresher.RefreshRoot(root);

            return root;
        }

        private TeleoptiToolStripGallery GetTemplateToolStripGallery(TemplateTarget templateTarget)
        {
            TeleoptiToolStripGallery toolStripGallery = null;
            switch (templateTarget)
            {
                case TemplateTarget.Skill:
                    toolStripGallery = teleoptiToolStripGallerySkill;
                    break;
                case TemplateTarget.Workload:
                    toolStripGallery = teleoptiToolStripGalleryWorkload;
                    break;
                case TemplateTarget.Multisite:
                    toolStripGallery = teleoptiToolStripGalleryMultisiteSkill;
                    break;
            }
            return toolStripGallery;
        }

        private void ShowTabGroup(TemplateTarget templateTarget, WorkingInterval workingInterval)
        {
            bool visible = (workingInterval == WorkingInterval.Day);
            var toolStrip = GetTemplateToolStripGallery(templateTarget);
            if (toolStrip == null) return;

            toolStrip.ParentRibbonTab.Visible = visible;
        }

        private AbstractDetailView GetCurrentWorkloadDetailView()
        {
            return tabControlWorkloads.SelectedTab.Controls[0] as AbstractDetailView;
        }

        private AbstractDetailView GetCurrentSkillDetailView()
        {
            return tabControlAdvMultisiteSkill.SelectedTab.Controls[0] as AbstractDetailView;
        }

        private void RibbonTemplatePanelsClose()
        {
            foreach (var tabGroup in ribbonControlAdv1.TabGroups.OfType<ToolStripTabGroup>())
            {
                tabGroup.Visible = false;
            }
        }

        private void loadSkillDetailView()
        {
            tabControlAdvMultisiteSkill.ItemSize = new Size(1, 0);
            tabControlAdvMultisiteSkill.TabPages.Clear();
            TabPageAdv tabPage = ColorHelper.CreateTabPage(_skill.Name, _skill.Description);
            SkillDetailView skillDetailView = new SkillDetailView((SkillDayCalculator)_skillDayCalculator, _skill, _skillChartSetting);
            skillDetailView.Name = "Skill";
            InitializeDetailView(skillDetailView);
            tabControlAdvMultisiteSkill.TabPages.Add(tabPage);
            tabPage.Controls.Add(skillDetailView);

            //UnitOfWork.Reassociate(_skillDayCalculator.VisibleSkillDays);

            //Add the grids to the grid collection
            foreach (KeyValuePair<string, TeleoptiGridControl> pair in skillDetailView.GridCollection)
            {
                _gridCollection.Add(pair.Key, pair.Value);
                pair.Value.Refresh();
                pair.Value.Model.MergeCells.EvaluateMergeCells(GridRangeInfo.Table()); //Cheat to make the merge work as expected
            }
        }

        private void InitializeDetailView(AbstractDetailView detailView)
        {
            detailView.Dock = DockStyle.Fill;
            detailView.WorkingIntervalChanged += detailView_WorkingIntervalChanged;
            detailView.TemplateSelected += detailView_TemplateSelected;
            detailView.ValuesChanged += detailView_ValuesChanged;
            detailView.CellClicked += detailView_CellClicked;
            _detailViews.Add(detailView);
        }

        private void detailView_TemplateSelected(object sender, TemplateEventArgs e)
        {
            SelectTemplateInToolstrip(GetTemplateToolStripGallery(e.TemplateTarget), e.TemplateName);
        }
        #region date navigation
        private void _timeNavigationControl_SelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
        {
            _currentLocalDate = e.Value;
            _currentLocalDateTime = e.Value.Date;
            foreach (AbstractDetailView item in _detailViews)
            {
                item.CurrentDay = _currentLocalDate;
                item.CurrentTimeOfDay = _currentLocalDateTime.TimeOfDay;
            }
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.S:
                        btnSave_click(this, EventArgs.Empty);
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region ribbon events

        private void btnSave_click(object sender, EventArgs e)
        {
            if (ValidateForm())
                Save(null);
        }

        private void toolStripButtonForecastWorkflow_Click(object sender, EventArgs e)
        {
            IWorkload workload = GetRefreshedAggregateRoot(TemplateTarget.Workload, false) as Workload;
            if (workload == null) return;
            if (CheckToClose())
            {
                workload = getWorkload(workload);
                using (var workflow = new ForecastWorkflow(workload, _scenario, _dateTimePeriod,
                                                                 _skillDayCalculator.SkillDays.ToList(), this))
                {
                    workflow.ShowDialog(this);
                }
            }
        }

        private static IWorkload getWorkload(IWorkload workload)
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var repository = new RepositoryFactory().CreateWorkloadRepository(unitOfWork);
                workload = repository.Get(workload.Id.GetValueOrDefault());
                LazyLoadingManager.Initialize(workload.Skill);
                LazyLoadingManager.Initialize(workload.Skill.SkillType);
                LazyLoadingManager.Initialize(workload.TemplateWeekCollection);
                foreach (var template in workload.TemplateWeekCollection.Values)
                {
                    LazyLoadingManager.Initialize(template.OpenHourList);
                }
                LazyLoadingManager.Initialize(workload.QueueSourceCollection);
            }
            return workload;
        }

        private void toolStripButtonIncreaseDecimals_Click(object sender, EventArgs e)
        {
            _currentForecasterSettings.NumericCellVariableDecimals++;
            foreach (KeyValuePair<string, TeleoptiGridControl> keyValuePair in _gridCollection)
            {
                keyValuePair.Value.ChangeNumberOfDecimals(1);
            }
        }

        private void toolStripButtonDecreaseDecimals_Click(object sender, EventArgs e)
        {
            if (_currentForecasterSettings.NumericCellVariableDecimals < 1)
                return;

            _currentForecasterSettings.NumericCellVariableDecimals--;
            foreach (KeyValuePair<string, TeleoptiGridControl> keyValuePair in _gridCollection)
            {
                keyValuePair.Value.ChangeNumberOfDecimals(-1);
            }
        }

        #endregion

        #region form closing

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_forceClose) return;

            if(backgroundWorker1.IsBusy)
            {
                //Since it doesn't seem to be possible to do cancel onm this we just need to make the user wait for the complete load
                e.Cancel = true;
                _userWantsToCloseForecaster = true;
                backgroundWorker1.ReportProgress(1, UserTexts.Resources.CancelLoading);
                return;

            }

            base.OnFormClosing(e);
            if (CheckToClose())
            {
                SetDisplaySettings();

                try
                {
                    SaveSettings();
                }
                catch (DataSourceException dataSourceException)
                {
                    _logger.Error("An error occurred when trying to save settings for forecaster.", dataSourceException);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void SaveSettings()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var repository = new PersonalSettingDataRepository(uow);
                repository.PersistSettingValue(_currentForecasterSettings);
                _skillChartSetting.SaveSettings(repository);
                _workloadChartSetting.SaveSettings(repository);
                uow.PersistAll();
            }
        }

        private void SaveDisplaySettings()
        {
            SetDisplaySettings();
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var repository = new PersonalSettingDataRepository(uow);
                repository.PersistSettingValue(_currentForecasterSettings);
                uow.PersistAll();
            }
        }

        private void SetDisplaySettings()
        {
            _currentForecasterSettings.ShowGraph = _showGraph;
            _currentForecasterSettings.ShowSkillView = _showSkillView;
        }


        /// <summary>
        /// Handles the Click event of the toolStripButtonExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-04-17
        /// </remarks>
        private void toolStripButtonExit_Click(object sender, EventArgs e)
        {
            SaveDisplaySettings();
            if (!CloseAllOtherForms(this))
                return; // a form was canceled

            Close();
            ////this canceled
            if (Visible)
                return;
            Application.Exit();
        }
        #endregion

        #region Chart interactivity
        /// <summary>
        /// Handles the CellClicked event of the detailView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-08
        /// changed by:ostenp
        /// removed all referense to the chartsettingbuttons since they now are an own usercontrol
        /// </remarks>
        void detailView_CellClicked(object sender, EventArgs e)
        {
            AbstractDetailView abstractDetailView = sender as AbstractDetailView;

            if (abstractDetailView != null)
            {
                IChartSeriesSetting chartSeriesSettings = abstractDetailView.CurrentGridRow.ChartSeriesSettings;
                _currentSelectedGridRow = abstractDetailView.CurrentGridRow;
                _currentSelectedGrid = abstractDetailView.CurrentGrid;
                if (chartSeriesSettings != null)
                {
                    _gridrowInChartSetting.Enabled = true;
                    _gridrowInChartSetting.SetButtons(chartSeriesSettings.Enabled, chartSeriesSettings.AxisLocation, chartSeriesSettings.SeriesType, chartSeriesSettings.Color);
                }
                else
                {
                    _gridrowInChartSetting.SetButtons(false, AxisLocation.Right, ChartSeriesDisplayType.Line, ColorHelper.ChartSettingsDisabledColor);
                    _gridrowInChartSetting.Enabled = false;
                }
            }
        }

        private void toolStripButtonPrint_Click(object sender, EventArgs e)
        {
            if (_chartControl == null) return;

            using (var pPrintDialog = new PrintDialog {Document = _chartControl.PrintDocument})
            {
                _chartControl.PrintDocument.DefaultPageSettings.Landscape = true;
                //this will check if the platform is 64bit or not
                var sArchType = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                if (sArchType != null && sArchType.Contains("64"))
                    pPrintDialog.UseEXDialog = true;

                if (pPrintDialog.ShowDialog() == DialogResult.OK)
                {
                    _chartControl.PrintColorMode = pPrintDialog.PrinterSettings.SupportsColor
                        ? ChartPrintColorMode.Color
                        : ChartPrintColorMode.GrayScale;
                    _chartControl.PrintDocument.Print();
                }
            }
        }


        private void toolStripButtonPrintPreview_Click(object sender, EventArgs e)
        {
            if (_chartControl == null) return;

            using (var pPrintPreviewDialog = new PrintPreviewDialog())
            {
                _chartControl.PrintDocument.DefaultPageSettings.Landscape = true;
                pPrintPreviewDialog.Document = _chartControl.PrintDocument;
                try
                {
                    pPrintPreviewDialog.ShowDialog();
                }
                catch (System.Drawing.Printing.InvalidPrinterException exceptionInvalidPrinterException)
                {
                    ShowInformationMessage(exceptionInvalidPrinterException.Message, UserTexts.Resources.Forecasts);
                }
            }
        }

        private void tabControlWorkloads_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPageAdv tabPage = ((TabControlAdv)sender).SelectedTab;
            if (tabPage == null) return;

            CreateTemplateGalleryRibbonBar(TemplateTarget.Workload, false, false);

            AbstractDetailView detailView = tabPage.Controls[0] as AbstractDetailView;
            triggerDetailViewWorkingIntervalChanged(detailView);

            _zoomButtonsEventArgs.GridKey = string.Empty;
            reloadChart();
        }

        private void triggerDetailViewWorkingIntervalChanged(AbstractDetailView detailView)
        {
            if (detailView == null) return;

            detailView_WorkingIntervalChanged(detailView, new WorkingIntervalChangedEventArgs
                                                             {
                                                                 NewStartDate = detailView.CurrentDay,
                                                                 NewTimeOfDay = detailView.CurrentTimeOfDay,
                                                                 NewWorkingInterval = detailView.CurrentWorkingInterval
                                                             });
        }

        private void tabControlAdvMultisiteSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPageAdv tabPage = ((TabControlAdv)sender).SelectedTab;
            if (tabPage == null) return;

            //Recreate template list
            CreateTemplateGalleryRibbonBar(TemplateTarget.Multisite, false, false);
            CreateTemplateGalleryRibbonBar(TemplateTarget.Skill, false, false);

            AbstractDetailView detailView = tabPage.Controls[0] as AbstractDetailView;
            triggerDetailViewWorkingIntervalChanged(detailView);

            _zoomButtonsEventArgs.GridKey = string.Empty;
            reloadChart();
        }

        private void _chartControl_ChartRegionMouseEnter(object sender, ChartRegionMouseEventArgs e)
        {
            GridChartManager.SetChartToolTip(e.Region, _chartControl);
        }

        #endregion

        #region Handle Template in ToolStripGallery

        private void teleoptiToolStripGallerySkill_ItemClicked(object sender, Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Tag as bool? == true) return;
            SetupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Skill);
        }

        private void teleoptiToolStripGalleryWorkload_ItemClicked(object sender, Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Tag as bool? == true) return;
            SetupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Workload);
        }

        private void teleoptiToolStripGalleryMultisiteSkill_ItemClicked(object sender, Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Tag as bool? == true) return;
            SetupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Multisite);
        }

        private void SetupGalleryItem(ContextMenuStrip contextMenu, string templateName, TemplateTarget templateTarget)
        {
            var templateInfo = new TemplateEventArgs();
            templateInfo.TemplateTarget = templateTarget;
            templateInfo.TemplateName = templateName;

            ToolStripItem itemEdit = new ToolStripMenuItem();
            itemEdit.Text = UserTexts.Resources.Edit;
            itemEdit.Tag = templateInfo;
            itemEdit.Click += itemEdit_Click;
            contextMenu.Items.Add(itemEdit);

            ToolStripItem itemRemove = new ToolStripMenuItem();
            itemRemove.Text = UserTexts.Resources.Delete;
            itemRemove.Tag = templateInfo;
            itemRemove.Click += itemRemove_Click;
            contextMenu.Items.Add(itemRemove);

            ToolStripItem itemRename = new ToolStripMenuItem();
            itemRename.Text = UserTexts.Resources.Rename;
            itemRename.Tag = templateInfo;
            itemRename.Click += itemRename_Click;
            contextMenu.Items.Add(itemRename);
        }

        private void itemRemove_Click(object sender, EventArgs e)
        {
            ToolStripItem selectedItem = sender as ToolStripItem;
            if (selectedItem == null) return;

            TemplateEventArgs templateInfo = selectedItem.Tag as TemplateEventArgs;
            if (templateInfo == null) return;

        	AttemptDatabaseConnectionDependentAction(() =>
        	    DeleteTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
        		);
		}

		private bool AttemptDatabaseConnectionDependentAction(System.Action action)
		{
			try
			{
				action.Invoke();
				return true;
			}
			catch (DataSourceException dataSourceException)
			{
			    if (datasourceExceptionOccurred(dataSourceException))
			        return false;
			    throw;
			}
		}

        private void itemEdit_Click(object sender, EventArgs e)
        {
            ToolStripItem selectedItem = sender as ToolStripItem;
            if (selectedItem == null) return;

            TemplateEventArgs templateInfo = selectedItem.Tag as TemplateEventArgs;
            if (templateInfo == null) return;

			AttemptDatabaseConnectionDependentAction(() =>
				EditTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
				);
        }


        private void itemRename_Click(object sender, EventArgs e)
        {
            ToolStripItem selectedItem = sender as ToolStripItem;
            if (selectedItem == null) return;

            TemplateEventArgs templateInfo = selectedItem.Tag as TemplateEventArgs;
            if (templateInfo == null) return;

			AttemptDatabaseConnectionDependentAction(() =>
				RenameTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
				);
        }

        private void teleoptiToolStripGallerySkill_GalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
        {
            ApplyTemplate(TemplateTarget.Skill, e.GalleryItem.Text);
        }

        private void teleoptiToolStripGalleryWorkload_GalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
        {
            ApplyTemplate(TemplateTarget.Workload, e.GalleryItem.Text);
        }

        private void teleoptiToolStripGalleryMultisiteSkill_GalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
        {
            ApplyTemplate(TemplateTarget.Multisite, e.GalleryItem.Text);
        }

        private void ApplyTemplate(TemplateTarget templateTarget, string templateName)
        {
            switch (templateTarget)
            {
                case TemplateTarget.Workload:
                    GetCurrentWorkloadDetailView().SetTemplate(templateName, templateTarget);
                    break;
                case TemplateTarget.Skill:
                case TemplateTarget.Multisite:
                    GetCurrentSkillDetailView().SetTemplate(templateName, templateTarget);
                    break;
            }
        }

        private void ResetTemplate(TemplateTarget templateTarget)
        {
            switch (templateTarget)
            {
                case TemplateTarget.Workload:
                    GetCurrentWorkloadDetailView().ResetTemplates(templateTarget);
                    break;
                case TemplateTarget.Skill:
                case TemplateTarget.Multisite:
                    GetCurrentSkillDetailView().ResetTemplates(templateTarget);
                    break;
            }
        }
        private void ResetLongterm()
        {
            GetCurrentWorkloadDetailView().ResetLongterm();
        }

        private static void SelectTemplateInToolstrip(TeleoptiToolStripGallery toolStrip, string templateName)
        {
            if (toolStrip == null) return;
            toolStrip.SetCheckedItem(toolStrip.Items.OfType<ToolStripGalleryItem>().FirstOrDefault(t => t.Text == templateName));
        }

        private void toolStripButtonCreateNewMultisiteTemplate_Click(object sender, EventArgs e)
        {
            AbstractDetailView detailView;
            EditMultisiteDayTemplate editTemplate = null;
            var success = AttemptDatabaseConnectionDependentAction(() =>
                                                                       {
                                                                           detailView = GetCurrentSkillDetailView();
                                                                           MultisiteSkill multisiteSkill =
                                                                               detailView.GetType().GetProperty("Skill")
                                                                                   .GetValue(detailView,
                                                                                             BindingFlags.Instance |
                                                                                             BindingFlags.Public, null,
                                                                                             null,
                                                                                             CultureInfo.
                                                                                                 InvariantCulture) as
                                                                               MultisiteSkill;
                                                                           if (multisiteSkill == null) return;

                                                                           editTemplate = new EditMultisiteDayTemplate(multisiteSkill);
                                                                       });
            if (!success)
                return;
            
            if (DialogResult.OK == editTemplate.ShowDialog(this))
            {
                //If this works, ask the user if she would like to apply the template right away!
                if (
                    ShowConfirmationMessage(UserTexts.Resources.ApplyNewTemplateQuestion, UserTexts.Resources.Template) ==
                    DialogResult.Yes)
                    ApplyTemplate(TemplateTarget.Multisite, editTemplate.TemplateName);
            }
			editTemplate.Dispose();
        }

        private void toolStripBtnCreateSkillTemplate_Click(object sender, EventArgs e)
        {
        	AbstractDetailView detailView;
        	EditSkillDayTemplate editTemplate = null;
        	var success = AttemptDatabaseConnectionDependentAction(() =>
        	                                                      	{
																		detailView = GetCurrentSkillDetailView();
																		editTemplate = new EditSkillDayTemplate(
																				(Skill)detailView.GetType().GetProperty("Skill")
																					.GetValue(detailView, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture)
																			);
																	});
            if (!success)
                return;

            if (DialogResult.OK == editTemplate.ShowDialog(this))
            {
                //If this works, ask the user if she would like to apply the template right away!
                if (
                    ShowConfirmationMessage(UserTexts.Resources.ApplyNewTemplateQuestion, UserTexts.Resources.Template) ==
                    DialogResult.Yes)
                    ApplyTemplate(TemplateTarget.Skill, editTemplate.TemplateName);
            }
			editTemplate.Dispose();
        }

        private void toolStripButtonCreateNewTemplate_Click(object sender, EventArgs e)
        {
        	//TODO! Need a way to get open hours from current workload day
        	//For now I'm using the default times (8-17)
        	IList<TimePeriod> openHours = new List<TimePeriod>();
        	EditWorkloadDayTemplate editTemplate = null;
        	var success =
        		AttemptDatabaseConnectionDependentAction(() =>
        		                                         editTemplate =
        		                                         new EditWorkloadDayTemplate(
        		                                         	((WorkloadDetailView) GetCurrentWorkloadDetailView()).Workload,
        		                                         	openHours));

        	if (!success)
        		return;
        	if (DialogResult.OK == editTemplate.ShowDialog(this))
        	{
        		//If this works, ask the user if she would like to apply the template right away!
        		if (
        			ShowConfirmationMessage(UserTexts.Resources.ApplyNewTemplateQuestion, UserTexts.Resources.Template) ==
        			DialogResult.Yes)
        			ApplyTemplate(TemplateTarget.Workload, editTemplate.TemplateName);
        	}
			editTemplate.Dispose();
        }

    	private void CreateTemplateGalleryRibbonBar(TemplateTarget templateTarget, bool refresh, bool force)
        {
            TeleoptiToolStripGallery galleryControl = GetTemplateToolStripGallery(templateTarget);
            if (galleryControl == null) return;

            if (tabControlWorkloads.SelectedTab == null || tabControlAdvMultisiteSkill.SelectedTab == null) return;

            IForecastTemplateOwner rootEntity;
            var templateList = GetTemplates(templateTarget, refresh, out rootEntity);

            if (force == false &&
                galleryControl.Tag == rootEntity) return;

            galleryControl.Items.Clear();
            galleryControl.ImageScaling = ToolStripItemImageScaling.None;

            //Ugly thing for making the gallery to show the days in correct order in RTL bug:5187
            //This will be rported to Syncfusion (2008-11-19)
            if (RightToLeft != RightToLeft.Yes)
            {
                foreach (DayOfWeek dayOfWeek in DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture))
                {
                    if (templateList.Count > 0)
                    {
                        IForecastDayTemplate template = templateList[(int) dayOfWeek];
                        galleryControl.Items.Add(GetGalleryItem(template));
                    }
                }
            }
            else
            {
                //Loop'em backwards!!!
                for (int i = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture).Count - 1; i >= 0; i--)
                {
                    if (templateList.Count > 0)
                    {
                        IForecastDayTemplate template = templateList[i];
                        galleryControl.Items.Add(GetGalleryItem(template));
                    }
                }
            }

            var sortedTemplates =
                from t in templateList
                orderby t.Value.Name
                select t;

            foreach (var template in sortedTemplates.Where(i => !i.Value.DayOfWeek.HasValue))
            {
                galleryControl.Items.Add(GetGalleryItem(template.Value));
            }

            galleryControl.Tag = rootEntity;
        }

        private ToolStripGalleryItem GetGalleryItem(IForecastDayTemplate template)
        {
            var galleryItem = new ToolStripGalleryItem();
            galleryItem.Text = template.Name;
            galleryItem.Tag = template.DayOfWeek.HasValue; //Locked => true if standard day template
            galleryItem.ImageTransparentColor = Color.Magenta;

            if (template.DayOfWeek.HasValue)
            {
                galleryItem.Image = imageList1.Images[(int)template.DayOfWeek];
            }
            else
            {
                galleryItem.Image = imageList1.Images[7];
            }

            return galleryItem;
        }

        private void RemoveDayTemplate(TemplateTarget target, string templateName)
        {
            IForecastTemplateOwner rootEntity = GetRefreshedAggregateRoot(target, false);

            IEnumerable<IRootChangeInfo> changedRoots;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IForecastTemplateOwner templateOwnerOriginal = ForecastingTemplateRefresher.LoadNewInstance(rootEntity,
                                                                                                            uow);
                templateOwnerOriginal.RemoveTemplate(target, templateName);

                changedRoots = uow.PersistAll();
            }
            EntityEventAggregator.TriggerEntitiesNeedRefresh(null, changedRoots);
        }

        private void EditTemplate(TemplateTarget target, string templateName)
        {
            IForecastTemplateOwner rootEntity = GetRefreshedAggregateRoot(target, false);
            IForecastDayTemplate template = rootEntity.TryFindTemplateByName(target, templateName);
            if (template == null) return;

            switch (target)
            {
                case TemplateTarget.Multisite:
                    EditMultisiteDayTemplate editMultisiteTemplate = new EditMultisiteDayTemplate((MultisiteDayTemplate)template);
                    editMultisiteTemplate.Show(this);
                    break;
                case TemplateTarget.Workload:
                    EditWorkloadDayTemplate editWorkloadTemplate = new EditWorkloadDayTemplate((WorkloadDayTemplate)template);
                    editWorkloadTemplate.Show(this);
                    break;
                case TemplateTarget.Skill:
                    EditSkillDayTemplate editSkillTemplate = new EditSkillDayTemplate((ISkillDayTemplate)template);
                    editSkillTemplate.Show(this);
                    break;
            }
        }

        // get this from metadata? asks KlasM. RogerK and RobinK replies that it is ok to hardcode for now 
        private const int templateNameMaxLength = 50;

        private void RenameTemplate(TemplateTarget target, string originalTemplateName)
        {
            var ctrl = new PromptTextBox(new RenameTemplateTag { Target = target, OriginalTemplateName = originalTemplateName },
                originalTemplateName, UserTexts.Resources.Template, templateNameMaxLength, ValidateWorkloadRenameName);
            ctrl.SetHelpId("NameForecastTemplate");
            ctrl.NameThisView += ctrl_RenameTemplate;
            ctrl.ShowDialog(this);
		}

	    private bool ValidateWorkloadRenameName(string newName)
	    {
		    return
			    ((WorkloadDetailView) GetCurrentWorkloadDetailView()).Workload.TemplateWeekCollection
			                                                         .All(t => t.Value.Name.ToUpperInvariant() != newName.ToUpperInvariant());
	    }

	    private class RenameTemplateTag
        {
            public TemplateTarget Target { get; set; }
            public string OriginalTemplateName { get; set; }
        }

        private void ctrl_RenameTemplate(object sender, CustomEventArgs<TupleItem> e)
        {
            RenameTemplateTag templateTag = (RenameTemplateTag)e.Value.ValueMember;
            string newName = e.Value.Text;			
            if (newName != templateTag.OriginalTemplateName)
            {
                try
                {
                    SaveTemplateWithNewName(newName, templateTag);
                }
                catch (DataSourceException dataSourceException)
                {
                    datasourceExceptionOccurred(dataSourceException);
                }
            }
        }

        private void SaveTemplateWithNewName(string newName, RenameTemplateTag templateTag)
        {
            IForecastTemplateOwner rootEntity = GetRefreshedAggregateRoot(templateTag.Target, false);
			
            IEnumerable<IRootChangeInfo> changedRoots;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IForecastTemplateOwner templateOwnerOriginal =
                    ForecastingTemplateRefresher.LoadNewInstance(rootEntity,
                                                                 uow);
                IForecastDayTemplate template =
                    templateOwnerOriginal.TryFindTemplateByName(templateTag.Target,
                                                                templateTag.OriginalTemplateName);
				if (template == null) return;
                template.Name = newName;

                changedRoots = uow.PersistAll();
            }
            EntityEventAggregator.TriggerEntitiesNeedRefresh(null, changedRoots);
            RefreshTabs();
        }

        private void DeleteTemplate(TemplateTarget target, string templateName)
        {
            RemoveDayTemplate(target, templateName);
            RefreshTabs();
        }

        private IDictionary<int, IForecastDayTemplate> GetTemplates(TemplateTarget templateTarget, bool refresh, out IForecastTemplateOwner rootEntity)
        {
            rootEntity = GetRefreshedAggregateRoot(templateTarget, refresh);
            IDictionary<int, IForecastDayTemplate> result = null;
            if (rootEntity != null)
            {
                result = rootEntity.GetTemplates(templateTarget).OrderBy(i => i.Key).ToDictionary(k => k.Key,
                                                                                            v => v.Value);
            }
            return result;
        }

        private void toolStripButtonResetMultisiteSkillTemplates_Click(object sender, EventArgs e)
        {
            DisableAllControlsExceptCancelLoadButton();

            backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Multisite);
        }

        private void toolStripButtonResetWorkloadTemplates_Click(object sender, EventArgs e)
        {
            DisableAllControlsExceptCancelLoadButton();

            backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Workload);
        }

        private void toolStripButtonResetSkillTemplates_Click(object sender, EventArgs e)
        {
            DisableAllControlsExceptCancelLoadButton();

            backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Skill);
        }

        #endregion

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            SaveDisplaySettings();
            Close();
        }

        private void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            ViewBase.ShowHelp(this,false);
        }

        private bool CheckToClose()
        {
            if (!IsDirtyListEmpty())
			{
				switch (AskToCommitChanges())
				{
					case DialogResult.Yes:
						if (!ValidateForm()) //Validation is only done if the user would like to save and something is dirty
                            return false;
				        showProgressBar();
				        try
				        {
                            ChoppedSave();
				        }
				        catch (DataSourceException ex)
				        {
                            //transaction time out, t.ex, table locked by another.
                            datasourceExceptionOccurred(ex);
                            return false;
				        }
                        InformUserOfUnsavedData();
                        if (_unsavedSkillDays.Count > 0)
                            return false;
				        break;
				    case DialogResult.No:
						break;
					case DialogResult.Cancel:
						return false;
				}
			}
        	return true;
        }

        private bool IsDirtyListEmpty()
    	{
            return _dirtyForecastDayContainer.IsEmpty();
    	}

    	private void toolStripButtonSystemOptions_Click(object sender, EventArgs e)
        {
            try
            {
					var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager)));
                settings.Show();
            }
            catch (DataSourceException ex)
            {
                DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
            }
        }

        private void toolStripButtonShowGraph_Click(object sender, EventArgs e)
        {
            toolStripButtonShowGraph.Checked = !toolStripButtonShowGraph.Checked;
            SplitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
            _showGraph = toolStripButtonShowGraph.Checked;
            splitContainer2.Panel1.Controls.Remove(_chartControl);
            addChart();
        }
        private void toolStripButtonShowSkillView_Click(object sender, EventArgs e)
        {
            toolStripButtonShowSkillView.Checked = !toolStripButtonShowSkillView.Checked;
            SplitterManager.ShowSkillView = toolStripButtonShowSkillView.Checked;
            _showSkillView = toolStripButtonShowSkillView.Checked;
        }
        private SplitterManager SplitterManager
        {
            get
            {
                if (_splitterManager == null)
                {
                    _splitterManager = new SplitterManager
                                           {
                                               MainSplitter = splitContainer2,
                                               WorkSkillSplitter = splitContainerWorkloadSkill
                                           };
                }
                return _splitterManager;
            }
        }

        #region toolStripTextBoxNewScenarioEvents

        private void toolStripTextBoxNewScenario_TextChanged(object sender, EventArgs e)
        {
            if (toolStripTextBoxNewScenario.Text == "(" + UserTexts.Resources.NewScenario + ")")
            {
                toolStripTextBoxNewScenario.Tag = "";
            }
            else
            {
                toolStripTextBoxNewScenario.Tag = toolStripTextBoxNewScenario.Text;
            }
        }

        private void toolStripTextBoxNewScenario_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (!string.IsNullOrEmpty(toolStripTextBoxNewScenario.Text))
                {
                    // Create new Scenario here
                    string scenarioName = toolStripTextBoxNewScenario.Text;
                    IScenario newScenario = new Scenario(scenarioName);
                    using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        IScenarioRepository scenarioRepository = new ScenarioRepository(unitOfWork);
                        scenarioRepository.Add(newScenario);
                        unitOfWork.PersistAll();
                    }
                    SaveForecastToScenario(newScenario);
                }
            }
        }

        void toolStripTextBoxNewScenario_LostFocus(object sender, EventArgs e)
        {
            Font italicFont = new Font(toolStripTextBoxNewScenario.Font.FontFamily, toolStripTextBoxNewScenario.Font.SizeInPoints, FontStyle.Italic);
            toolStripTextBoxNewScenario.Font = italicFont;
            toolStripTextBoxNewScenario.Text = "(" + UserTexts.Resources.NewScenario + ")";
        }

        void toolStripTextBoxNewScenario_GotFocus(object sender, EventArgs e)
        {
            string scenarioName = (string)toolStripTextBoxNewScenario.Tag;
            if (string.IsNullOrEmpty(scenarioName))
            {
                toolStripTextBoxNewScenario.Text = "";
            }
            else
            {
                toolStripTextBoxNewScenario.Text = (string)toolStripTextBoxNewScenario.Tag;
            }
            Font regularFont = new Font(toolStripTextBoxNewScenario.Font.FontFamily, toolStripTextBoxNewScenario.Font.SizeInPoints, FontStyle.Regular);
            toolStripTextBoxNewScenario.Font = regularFont;
            //toolStripTextBoxNewScenario.Font.ChangeToRegular(); // Doesn't work... /Henry
        }
        #endregion

        private void UnhookEvents()
        {
            _zoomButtons.ZoomChanged -= buttons_ZoomChanged;
            _zoomButtonsChart.ZoomChanged -= _zoomButtonsChart_ZoomChanged;
            _timeNavigationControl.SelectedDateChanged -= _timeNavigationControl_SelectedDateChanged;
            _gridrowInChartSetting.LineInChartSettingsChanged -= _gridlinesInChartSettings_LineInChartSettingsChanged;
            _gridrowInChartSetting.LineInChartEnabledChanged -= _gridrowInChartSetting_LineInChartEnabledChanged;
            if (_chartControl != null)
            {
                _chartControl.ChartRegionClick -= _chartControl_ChartRegionClick;
                _chartControl.ChartRegionMouseEnter -= _chartControl_ChartRegionMouseEnter;
            }
            backgroundWorker1.RunWorkerCompleted -= backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.DoWork -= backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged -= backgroundWorker1_ProgressChanged;
            EntityEventAggregator.EntitiesNeedsRefresh -= MainScreen_EntitiesNeedsRefresh;
        }

        private void ReleaseManagedResources()
        {
            foreach (AbstractDetailView detailView in _detailViews)
            {
                detailView.WorkingIntervalChanged -= detailView_WorkingIntervalChanged;
                detailView.TemplateSelected -= detailView_TemplateSelected;
                detailView.ValuesChanged -= detailView_ValuesChanged;
                detailView.CellClicked -= detailView_CellClicked;
                detailView.Dispose();
            }
            _detailViews.Clear();
            _clipboardControl.Dispose();
            _clipboardControl = null;
            if (_gridChartManager != null)
                _gridChartManager.Dispose();

            if (_chartControl != null)
            {
                _chartControl.Dispose();
                _chartControl = null;
            }
        }

        private void toolStripButtonLongtermWorkloadTemplates_Click(object sender, EventArgs e)
        {
            ResetLongterm();
        }

        private void backgroundWorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
            var scenario = e.Argument as IScenario;
            if(scenario != null)
            {
                var command = new SaveForecastToScenarioCommand(_skill, _skillDayCalculator, _dateTimePeriod);
                command.ProgressReporter += saveForecastToScenarioCommand_ProgressReporter;
                e.Result = command.Execute(scenario);
                command.ProgressReporter -= saveForecastToScenarioCommand_ProgressReporter;
            }
            else ChoppedSave();
        }

        private void backgroundWorkerSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var progressBarIncrement = toolStripProgressBarMain.Value;
            if (progressBarIncrement + 1 >= toolStripProgressBarMain.Minimum && progressBarIncrement + 1 <= toolStripProgressBarMain.Maximum)
                toolStripProgressBarMain.Value++;
        }

        private void backgroundWorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Error == null)
            {
                var result = e.Result as SaveForecastToScenarioCommandResult;
                if (result != null)
                {
                    _unsavedSkillDays = result.UnsavedDaysInfo;
                    _scenario = result.NewScenario;
                    _skillDayCalculator = result.SkillDayCalculator;

                    EntityEventAggregator.EntitiesNeedsRefresh += MainScreen_EntitiesNeedsRefresh;

                    reloadDetailViews();
                    reloadScenarioMenuItems();

                    Cursor = Cursors.Default;
                }
            }
            
            toolStripProgressBarMain.Visible = false;
            toolStripStatusLabelInfo.Visible = false;

            if (!datasourceExceptionOccurred(e.Error))
            {
                throwIfExceptionOccurred(e);
            }

            EnableAllControlsExceptCancelLoadButton();
            
            InformUserOfUnsavedData();
        }

        private void backgroundWorkerApplyStandardTemplates_DoWork(object sender, DoWorkEventArgs e)
        {
            ResetTemplate((TemplateTarget)e.Argument);
        }

        private void backgroundWorkerApplyStandardTemplates_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;

            toolStripProgressBarMain.Visible = false;
            toolStripStatusLabelInfo.Visible = false;
            
            throwIfExceptionOccurred(e);

            EnableAllControlsExceptCancelLoadButton();

            toolStripProgressBarMain.Visible = false; 
        }

        public void ReloadForecaster()
        {
            reloadCurrentData();
            //throw new NotImplementedException();
        }

		  private void backStageButtonSave_Click(object sender, EventArgs e)
		  {
			  if (ValidateForm())
				  Save(null);
		  }

		  private void backStageButtonClose_Click(object sender, EventArgs e)
		  {
			  SaveDisplaySettings();
			  Close();
		  }

		  private void backStageButtonOptions_Click(object sender, EventArgs e)
		  {
			  try
			  {
				  var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager)));
				  settings.Show();
			  }
			  catch (DataSourceException ex)
			  {
				  DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			  }
		  }

		  private void backStageButton4_Click(object sender, EventArgs e)
		  {
			  SaveDisplaySettings();
			  if (!CloseAllOtherForms(this))
				  return; // a form was canceled

			  Close();
			  ////this canceled
			  if (Visible)
				  return;
			  Application.Exit();
		  }
    }

    public interface IFinishWorkload
    {
        void ReloadForecaster();
    }

    internal interface IDirtyForecastDayContainer
    {
        HashSet<ISkillDay> DirtySkillDays { get; }
        HashSet<ISkillDay> DirtyChildSkillDays { get; }
        HashSet<IMultisiteDay> DirtyMultisiteDays { get; }
        bool IsEmpty();
        int Size { get; }
        void Clear();
    }

    public class DirtyForecastDayContainer : IDirtyForecastDayContainer
    {
        private readonly HashSet<ISkillDay> _dirtySkillDays;
        private readonly HashSet<ISkillDay> _dirtyChildSkillDays;
        private readonly HashSet<IMultisiteDay> _dirtyMultisiteDays;

        public DirtyForecastDayContainer()
        {
            _dirtySkillDays = new HashSet<ISkillDay>();
            _dirtyChildSkillDays = new HashSet<ISkillDay>();
            _dirtyMultisiteDays = new HashSet<IMultisiteDay>();
        }

        public HashSet<ISkillDay> DirtySkillDays
        {
            get { return _dirtySkillDays; }
        }

        public HashSet<ISkillDay> DirtyChildSkillDays
        {
            get { return _dirtyChildSkillDays; }
        }

        public HashSet<IMultisiteDay> DirtyMultisiteDays
        {
            get { return _dirtyMultisiteDays; }
        }

        public bool IsEmpty()
        {
            return Size == 0;
        }

        public int Size
        {
            get { return _dirtySkillDays.Count + _dirtyChildSkillDays.Count + _dirtyMultisiteDays.Count; }
        }

        public void Clear()
        {
            _dirtySkillDays.Clear();
            _dirtyChildSkillDays.Clear();
            _dirtyMultisiteDays.Clear();
        }
    }
}
