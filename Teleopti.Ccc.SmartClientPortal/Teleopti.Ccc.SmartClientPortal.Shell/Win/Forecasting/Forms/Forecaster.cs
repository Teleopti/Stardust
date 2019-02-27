using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.ToolStripGallery;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;
using Teleopti.Ccc.Win.Forecasting;
using Teleopti.Ccc.Win.Forecasting.Forms;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;
using ToolStripItemClickedEventArgs = Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class Forecaster : BaseRibbonForm, IFinishWorkload
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(Forecaster));

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
		private Form _mainWindow;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly IConfigReader _configReader;
		private readonly IStatisticHelper _statisticHelper;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;

		#region Private


		private void setColors()
		{
			var ribbonContextTabColor = ColorHelper.RibbonContextTabColor();
			for (int i = 0; i < ribbonControlAdv1.TabGroups.Count; i++)
			{
				ribbonControlAdv1.TabGroups[i].Color = ribbonContextTabColor;
			}
		}

		private void setPermissionOnControls()
		{
			backStageButtonOptions.Enabled =
				 PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
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


		private void save(Func<bool> callback)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<Func<bool>>(save), callback);
				return;
			}

			exitEditMode();

			showProgressBar();

			disableAllControlsExceptCancelLoadButton();
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
			var currentSkillView = getCurrentSkillDetailView();
			if (currentSkillView != null)
			{
				var currentGrid = currentSkillView.CurrentGrid as GridControl;
				if (currentGrid != null)
				{
					currentGrid.Model.EndEdit();
				}
			}

			var currentWorkloadView = getCurrentWorkloadDetailView();
			if (currentWorkloadView != null)
			{
				var currentGrid = currentWorkloadView.CurrentGrid as GridControl;
				if (currentGrid != null)
				{
					currentGrid.Model.EndEdit();
				}
			}
		}

		private void choppedSave()
		{
			choppedSaveSkillDays(_dirtyForecastDayContainer.DirtyChildSkillDays);
			choppedSaveSkillDays(_dirtyForecastDayContainer.DirtySkillDays);
			choppedSaveMultisiteDays();
		}

		private void choppedSaveSkillDays(IEnumerable<ISkillDay> dirtyList)
		{
			var dirtySkillDays = new List<ISkillDay>();
			dirtySkillDays.AddRange(dirtyList);
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				foreach (var skillDay in dirtySkillDays)
				{
					try
					{
						var skillDayRepository = new SkillDayRepository(uow);
						skillDayRepository.Add(skillDay);
						
						removeSkillDayFromDirtyList(skillDay);
					}
					catch (OptimisticLockException)
					{
						addUnsavedDay(skillDay.CurrentDate);
					}
					catch (ConstraintViolationException)
					{
						addUnsavedDay(skillDay.CurrentDate);
					}
					reportSavingProgress(1);
				}
				uow.PersistAll();
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

		private void choppedSaveMultisiteDays()
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
						addUnsavedDay(multisiteDay.MultisiteDayDate);
					}
					catch (ConstraintViolationException)
					{
						addUnsavedDay(multisiteDay.MultisiteDayDate);
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

		private void addUnsavedDay(DateOnly localCurrentDate)
		{
			var unsavedSkillDay = new UnsavedDayInfo(localCurrentDate, _scenario);
			if (!_unsavedSkillDays.Contains(unsavedSkillDay))
				_unsavedSkillDays.Add(unsavedSkillDay);
		}

		private void informUserOfUnsavedData()
		{
			if (_unsavedSkillDays.Count == 0) return;

			optimisticLockExceptionInformation();
		}

		private void optimisticLockExceptionInformation()
		{
			var unsavedSkillDays = _unsavedSkillDays.UnsavedDaysOrderedByDate;
			var messageBox =
				new MessageBoxWithListView(
					UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot + UserTexts.Resources.TheDaysListedBelowWillNotBeSavedDot, UserTexts.Resources.Warning,
					unsavedSkillDays);
			markUnsavedDays();
			messageBox.ShowDialog();
		}

		private void markUnsavedDays()
		{
			_skillDayCalculator.InvokeDatesUnsaved(_unsavedSkillDays);
			foreach (var detailView in _detailViews)
			{
				detailView.RefreshCurrentTab();
			}
		}

		private bool validateForm()
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

		private DialogResult askToCommitChanges()
		{
			DialogResult result = ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade, UserTexts.Resources.Save);
			return result;
		}

		private void backgroundWorker1DoWork(object sender, DoWorkEventArgs e)
		{
			if (e.Cancel) return;

			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_currentForecasterSettings = new PersonalSettingDataRepository(unitOfWork).FindValueByKey("Forecaster", new ForecasterSettings());
				
				unitOfWork.Reassociate(_skill);
				if (isMultisiteSkill)
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
				_statisticHelper.StatusChanged += statHelperStatusChanged;
				loadSkillDays(unitOfWork, _statisticHelper);

				_skillChartSetting = new ForecasterChartSetting(TemplateTarget.Skill);
				_skillChartSetting.Initialize();
				_workloadChartSetting = new ForecasterChartSetting(TemplateTarget.Workload);
				_workloadChartSetting.Initialize();

				_skillDayCalculator.DistributeStaff(); //Does the initial fixing of distributed demand
			}
			backgroundWorker1.ReportProgress(1, UserTexts.Resources.Done);

			_statisticHelper.StatusChanged -= statHelperStatusChanged;
		}

		private void loadSkillDays(IUnitOfWork unitOfWork, IStatisticHelper statHelper)
		{
			var periodToLoad = SkillDayCalculator.GetPeriodToLoad(_dateTimePeriod);
			var skillDays = statHelper.LoadStatisticData(periodToLoad, _skill, _scenario, _longterm);
			if (isMultisiteSkill)
			{
				backgroundWorker1.ReportProgress(5, UserTexts.Resources.MultisiteSkillLoading);
				var multisiteDays = MultisiteHelper.LoadMultisiteDays(periodToLoad, _multisiteSkill, _scenario,
					new MultisiteDayRepository(unitOfWork), false).ToList();
				var multisiteCalculator = new MultisiteSkillDayCalculator(_multisiteSkill, skillDays, multisiteDays, _dateTimePeriod);
				
				foreach (var childSkill in _multisiteSkill.ChildSkills)
				{
					multisiteCalculator.SetChildSkillDays(childSkill, statHelper.LoadStatisticData(periodToLoad, childSkill, _scenario, _longterm));
				}

				_skillDayCalculator = multisiteCalculator;
			}
			else
			{
				_skillDayCalculator = new SkillDayCalculator(_skill, skillDays, _dateTimePeriod);
			}
		}

		private void enableOrDisableGridControls(bool enable)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<bool>(enableOrDisableGridControls), enable);
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

		void statHelperStatusChanged(object sender, StatusChangedEventArgs e)
		{
			backgroundWorker1.ReportProgress(5, UserTexts.Resources.DataSourceLoading);
		}

		private void backgroundWorker1ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var text = (string)e.UserState;
			if (toolStripStatusLabelInfo.Text.Equals(UserTexts.Resources.CancelLoading))
				text = UserTexts.Resources.CancelLoading;

			toolStripStatusLabelInfo.Text = text;
			toolStripProgressBarMain.PerformStep();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private void backgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

			loadDisplayOptionsFromSetting();

			try
			{
				initializeSkillWorkload();

				loadScenarioMenuItems();
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
			EntityEventAggregator.EntitiesNeedsRefresh += mainScreenEntitiesNeedsRefresh;
			_chartControl.ChartRegionMouseEnter += chartControlChartRegionMouseEnter;
			toolStripProgressBarMain.Step++;
			_chartControl.ChartRegionClick += chartControlChartRegionClick;
			toolStripProgressBarMain.Visible = false;
			toolStripStatusLabelInfo.Visible = false;
			toolStripTabItemMultisite.Visible = false;

			if (_userWantsToCloseForecaster)
			{
				BeginInvoke(new MethodInvoker(Close));
				return;
			}

			var multisiteSkillDayCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
			if (multisiteSkillDayCalculator != null)
			{
				foreach (var multisiteDay in multisiteSkillDayCalculator.MultisiteDays)
				{
					multisiteDay.ValueChanged += multisiteDayValueChanged;
					multisiteDay.MultisiteSkillDay.StaffRecalculated += skillDayStaffRecalculated;
					foreach (var childSkillDay in multisiteDay.ChildSkillDays)
					{
						childSkillDay.StaffRecalculated += childSkillDayStaffRecalculated;
					}
				}
			}
			else
			{
				foreach (var skillDay in _skillDayCalculator.SkillDays)
				{
					skillDay.StaffRecalculated += skillDayStaffRecalculated;
				}
			}

			enableAllControlsExceptCancelLoadButton();

			_detailViews.First().CurrentDay = _dateTimePeriod.StartDate;
			LogPointOutput.LogInfo("Forecast.LoadAndOpenSkill", "Completed");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private static void throwIfExceptionOccurred(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
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

		private void childSkillDayStaffRecalculated(object sender, EventArgs e)
		{
			var skillDay = sender as ISkillDay;
			if (skillDay == null) return;
			if (skillDay.Scenario.Equals(_scenario) && _dateTimePeriod.Contains(skillDay.CurrentDate))
				_dirtyForecastDayContainer.DirtyChildSkillDays.Add(skillDay);
		}

		private void multisiteDayValueChanged(object sender, EventArgs e)
		{
			var multisiteDay = sender as IMultisiteDay;
			if (multisiteDay == null) return;
			if (multisiteDay.Scenario.Equals(_scenario) && _dateTimePeriod.Contains(multisiteDay.MultisiteDayDate))
				_dirtyForecastDayContainer.DirtyMultisiteDays.Add(multisiteDay);
		}

		private void skillDayStaffRecalculated(object sender, EventArgs e)
		{
			var skillDay = sender as ISkillDay;
			if (skillDay == null) return;
			if (skillDay.Scenario.Equals(_scenario) && _dateTimePeriod.Contains(skillDay.CurrentDate))
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
			setGridOpeningGridViews();
			Cursor = Cursors.Default;
		}

		private void reassociateSkills()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				unitOfWork.Reassociate(_skill);
				if (isMultisiteSkill)
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
				foreach (TemplateTarget templateTarget in Enum.GetValues(typeof(TemplateTarget)))
				{
					createTemplateGalleryRibbonBar(templateTarget, false, false);
				}
			}
		}

		private void loadAllDetailViews()
		{
			loadWorkloadDetailViews();
			toolStripStatusLabelInfo.Text = UserTexts.Resources.WorkloadDaysLoaded;
			toolStripProgressBarMain.Step++;

			if (isMultisiteSkill)
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

		private void disableAllControlsExceptCancelLoadButton()
		{
			officeDropDownButtonSaveToScenario.Enabled = false;
			toolStripButtonForecastWorkflow.Enabled = false;
			toolStripButtonSave2.Enabled = false;
			toolStripTabItemChart.Enabled = false;
			ControlBox = false;

			enableOrDisableGridControls(false);
			ribbonControlAdv1.Enabled = false;
		}

		private void enableAllControlsExceptCancelLoadButton()
		{
			officeDropDownButtonSaveToScenario.Enabled = true;
			toolStripButtonForecastWorkflow.Enabled = true;
			toolStripButtonSave2.Enabled = true;
			toolStripTabItemChart.Enabled = true;
			ControlBox = true;

			enableOrDisableGridControls(true);
			ribbonControlAdv1.Enabled = true;
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				loadSkill(_skill);
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

			if (!_skill.WorkloadCollection.Any())
			{
				Close();
				return;
			}

			Cursor = Cursors.WaitCursor;
			disableAllControlsExceptCancelLoadButton();

			initializeTexts();
			initializeEvents();
			setPermissionOnControls();
			setToolStripsToPreferredSize();

			backgroundWorker1.WorkerSupportsCancellation = true;
			backgroundWorker1.WorkerReportsProgress = true;
			backgroundWorker1.RunWorkerCompleted += backgroundWorker1RunWorkerCompleted;
			backgroundWorker1.DoWork += backgroundWorker1DoWork;
			backgroundWorker1.ProgressChanged += backgroundWorker1ProgressChanged;

			backgroundWorker1.RunWorkerAsync();
		}

		private void initializeEvents()
		{
			toolStripTextBoxNewScenario.GotFocus += toolStripTextBoxNewScenarioGotFocus;
			toolStripTextBoxNewScenario.LostFocus += toolStripTextBoxNewScenarioLostFocus;
		}

		private void initializeTexts()
		{
			officeDropDownButtonSaveToScenario.DropDownText = UserTexts.Resources.SaveAsScenario;
			toolStripTextBoxNewScenario.Text = "(" + UserTexts.Resources.NewScenario + ")";
		}

		private void loadScenarioMenuItems()
		{
			if (flowLayoutExportToScenario.ContainerControl.Controls.Count > 0) return;

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IScenarioRepository scenarioRepository = ScenarioRepository.DONT_USE_CTOR(unitOfWork);
				IList<IScenario> scenarios = scenarioRepository.FindAllSorted();

				foreach (var scenario in scenarios)
				{
					var button = createExportScenarioButton(scenario);
					flowLayoutExportToScenario.ContainerControl.Controls.Add(button);
				}
			}
			setExportVisability();
		}

		private void setExportVisability()
		{
			foreach (var control in flowLayoutExportToScenario.ContainerControl.Controls.OfType<ExportControl>())
			{
				control.Visible = !((Scenario) control.Tag).Equals(_scenario);
			}
		}

		private void clearExistingExportScenarioButtons()
		{
			flowLayoutExportToScenario.ContainerControl.Controls.OfType<ExportControl>().ForEach(c =>
			{
				c.Click -= scenarioMenuItemClick;
			});
		}

		private ExportControl createExportScenarioButton(IScenario scenario)
		{
			var button = new ExportControl(scenario);
			button.Click += scenarioMenuItemClick;
			return button;
		}

		private void scenarioMenuItemClick(object sender, EventArgs e)
		{
			var scenario = (IScenario)((ExportControl)sender).Tag;
			saveForecastToScenario(scenario);
		}

		private void saveForecastToScenario(IScenario scenario)
		{
			Cursor = Cursors.WaitCursor;

			EntityEventAggregator.EntitiesNeedsRefresh -= mainScreenEntitiesNeedsRefresh;

			initializeProgressBarBeforeSaveForecastToScenario();

			_dirtyForecastDayContainer.Clear();

			if (!backgroundWorkerSave.IsBusy)
				backgroundWorkerSave.RunWorkerAsync(scenario);
		}

		private void saveForecastToScenarioCommandProgressReporter(object sender, CustomEventArgs<int> e)
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

			disableAllControlsExceptCancelLoadButton();
		}

		private void chartControlChartRegionClick(object sender, ChartRegionMouseEventArgs e)
		{
			int column = (int)Math.Round(GridChartManager.GetIntervalValueForChartPoint(_chartControl, e.Point));

			_currentLocalDate = _gridChartManager.GetDateByColumn(column, _currentLocalDate);
			_timeNavigationControl.SetSelectedDate(_currentLocalDate);
		}

		private void mainScreenEntitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
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
				BeginInvoke(new Action<object, EntitiesUpdatedEventArgs>(mainScreenEntitiesNeedsRefresh), sender, e);
				return;
			}
			if (typeof(IWorkload).IsAssignableFrom(e.EntityType))
			{
				createTemplateGalleryRibbonBar(TemplateTarget.Workload, true, true);
			}
			else if (typeof(IMultisiteSkill).IsAssignableFrom(e.EntityType) &&
					e.UpdatedIds.Contains(_skill.Id.Value))
			{
				createTemplateGalleryRibbonBar(TemplateTarget.Multisite, true, true);
				createTemplateGalleryRibbonBar(TemplateTarget.Skill, true, true);
			}
			else if (typeof(ISkill).IsAssignableFrom(e.EntityType) ||
					 typeof(IChildSkill).IsAssignableFrom(e.EntityType))
			{
				createTemplateGalleryRibbonBar(TemplateTarget.Skill, true, true);
			}
			else if (typeof(ISkillDay).IsAssignableFrom(e.EntityType))
			{
				refreshSkillDays(e.UpdatedIds);
			}
		}

		private void refreshSkillDays(IEnumerable<Guid> guidList)
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

			EntityEventAggregator.EntitiesNeedsRefresh -= mainScreenEntitiesNeedsRefresh;
			_chartControl.ChartRegionMouseEnter -= chartControlChartRegionMouseEnter;
			_chartControl.ChartRegionClick -= chartControlChartRegionClick;

			loadSkill(_skill);
			if (backgroundWorker1.IsBusy)
			{
				backgroundWorker1.CancelAsync();
			}
			backgroundWorker1.RunWorkerAsync();
		}

		private void detailViewValuesChanged(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler(detailViewValuesChanged), sender, e);
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

		protected Forecaster(IStatisticHelper statisticHelper, IBusinessRuleConfigProvider businessRuleConfigProvider)
		{
			_statisticHelper = statisticHelper;
			_businessRuleConfigProvider = businessRuleConfigProvider;
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				ColorHelper.SetRibbonQuickAccessTexts(ribbonControlAdv1);
			}

			setColors();
			ribbonTemplatePanelsClose();
			ribbonControlAdv1.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
			Application.DoEvents();

			WindowState = FormWindowState.Maximized;
			toolStripStatusLabelInfo.Text = UserTexts.Resources.Initializing;
			toolStripProgressBarMain.Value = 0;
			toolStripProgressBarMain.Step++;
		}

		private void setGridOpeningGridViews()
		{
			setGridZoomLevel(TemplateTarget.Workload, _currentForecasterSettings.WorkingInterval);
			setGridZoomLevel(TemplateTarget.Skill, _currentForecasterSettings.WorkingIntervalSkill);
		}

		public Forecaster(ISkill skill, DateOnlyPeriod dateTimePeriod, IScenario scenario, bool longterm,
			IToggleManager toggleManager, Form mainWindow, IStatisticHelper statisticHelper,
			IBusinessRuleConfigProvider businessRuleConfigProvider,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade, IConfigReader configReader)
			: this(statisticHelper, businessRuleConfigProvider)
		{
			_toggleManager = toggleManager;
			_dateTimePeriod = dateTimePeriod;
			_mainWindow = mainWindow;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			_configReader = configReader;

			_zoomButtons = new ZoomButtons();
			_zoomButtons.ZoomChanged += buttonsZoomChanged;
			var host3 = new ToolStripControlHost(_zoomButtons);
			toolStripExZoomBtns.Items.Add(host3);

			_zoomButtonsChart = new GridViewInChartButtons();
			_zoomButtonsChart.ZoomChanged += zoomButtonsChartZoomChanged;
			var host4 = new ToolStripControlHost(_zoomButtonsChart);
			toolStripExChartViews.Items.Add(host4);

			_timeNavigationControl = new DateNavigateControl();
			_timeNavigationControl.SetAvailableTimeSpan(_dateTimePeriod);
			_timeNavigationControl.SelectedDateChanged += timeNavigationControlSelectedDateChanged;
			var hostDatepicker = new ToolStripControlHost(_timeNavigationControl);
			toolStripExDatePicker.Items.Add(hostDatepicker);

			setUpClipboard();

			_gridrowInChartSetting = new GridRowInChartSettingButtons();
			var chartsetteinghost = new ToolStripControlHost(_gridrowInChartSetting);
			toolStripExGridRowInChartButtons.Items.Add(chartsetteinghost);
			_gridrowInChartSetting.SetButtons();

			_gridrowInChartSetting.LineInChartSettingsChanged += gridlinesInChartSettingsLineInChartSettingsChanged;
			_gridrowInChartSetting.LineInChartEnabledChanged += gridrowInChartSettingLineInChartEnabledChanged;

			_skill = skill;
			_scenario = scenario;
			_longterm = longterm;
			_currentLocalDate = dateTimePeriod.StartDate;
			_currentLocalDateTime = _currentLocalDate.Date;

			ribbonControlAdv1.TabGroups[0].Name = UserTexts.Resources.Templates;
			toolStripTabItemMultisite.Visible = false;
			toolStripTabItemSkill.Visible = true;
			toolStripTabItemWorkload.Visible = true;
			ribbonControlAdv1.TabGroups[0].Visible = true;

			toolStripStatusLabelInfo.Text = UserTexts.Resources.SkillLoaded;
			toolStripProgressBarMain.Step++;
		}

		private void loadDisplayOptionsFromSetting()
		{
			splitterManager.ShowGraph = _currentForecasterSettings.ShowGraph;
			toolStripButtonShowGraph.Checked = _currentForecasterSettings.ShowGraph;
			_showGraph = _currentForecasterSettings.ShowGraph;
			splitterManager.ShowSkillView = _currentForecasterSettings.ShowSkillView;
			toolStripButtonShowSkillView.Checked = _currentForecasterSettings.ShowSkillView;
			_showSkillView = _currentForecasterSettings.ShowSkillView;
		}

		private void loadSkill(ISkill skill)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var skillRepository = SkillRepository.DONT_USE_CTOR(uow);
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

		private void setUpClipboard()
		{
			_clipboardControl = new ClipboardControl();
			var hostClipboardControl = new ToolStripControlHost(_clipboardControl);
			toolStripEx1.Items.Add(hostClipboardControl);
			var copySpecialButton = new ToolStripButton { Text = UserTexts.Resources.CopySpecial, Tag = "special" };
			copySpecialButton.Click += (s, e) => operateOnActiveGridControl(GridHelper.CopySelectedValuesAndHeadersToPublicClipboard);
			_clipboardControl.CopySpecialItems.Add(copySpecialButton);
			_clipboardControl.CutClicked += (s, e) => operateOnActiveGridControl(x => x.CutPaste.Cut());
			_clipboardControl.CopyClicked += (s, e) => operateOnActiveGridControl(x => x.CutPaste.Copy());
			_clipboardControl.PasteClicked += (s, e) => operateOnActiveGridControl(x => x.CutPaste.Paste());
		}

		private void operateOnActiveGridControl(Action<GridControl> operation)
		{
			var theGrid = new ColorHelper().GetActiveControl(ActiveControl) as GridControl;
			if (theGrid != null)
				operation.Invoke(theGrid);
		}

		private void gridrowInChartSettingLineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGrid, _currentSelectedGridRow, _gridrowInChartSetting, e.Enabled);
			if (_gridChartManager.CurrentGrid != _currentSelectedGrid)
			{
				_gridChartManager.ReloadChart();
			}
		}

		private void gridlinesInChartSettingsLineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_gridChartManager.UpdateChartSettings(_currentSelectedGridRow, e.Enabled, e.ChartSeriesStyle, e.GridToChartAxis, e.LineColor);
		}

		private void buttonsZoomChanged(object sender, ZoomButtonsEventArgs e)
		{
			setGridZoomLevel(e.Target, e.Interval);
			if (e.Target == TemplateTarget.Workload)
			{
				_currentForecasterSettings.WorkingInterval = e.Interval;
			}
			else
			{
				_currentForecasterSettings.WorkingIntervalSkill = e.Interval;
			}
		}

		private void setGridZoomLevel(TemplateTarget target, WorkingInterval workingInterval)
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

		private void zoomButtonsChartZoomChanged(object sender, ZoomButtonsEventArgs e)
		{
			e.GridKey = string.Empty;
			setChartZoomLevel(e);
			_zoomButtonsEventArgs = e;
			_currentForecasterSettings.ChartInterval = _zoomButtonsEventArgs.Interval;
			_currentForecasterSettings.TemplateTarget = _zoomButtonsEventArgs.Target;
		}

		private void setChartZoomLevel(ZoomButtonsEventArgs e)
		{
			string name;
			string type;
			Guid id;
			if (e.Target == TemplateTarget.Workload)
			{
				type = UserTexts.Resources.Workload;
				name = ((WorkloadDetailView)getCurrentWorkloadDetailView()).Workload.Name;
				id = ((WorkloadDetailView)getCurrentWorkloadDetailView()).Workload.Id.GetValueOrDefault();
			}
			else //multisite how do i work it?
			{
				type = UserTexts.Resources.Skill;
				ISkill skill = null;
				AbstractDetailView undeterminedDetailView = getCurrentSkillDetailView();
				var skillDetailView = undeterminedDetailView as SkillDetailView;
				var multisiteSkillDetailView = undeterminedDetailView as MultisiteSkillDetailView;
				var childSkillDetailView = undeterminedDetailView as ChildSkillDetailView;
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

		private void setToolStripsToPreferredSize()
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
			setChartZoomLevel(_zoomButtonsEventArgs);
		}

		private bool isMultisiteSkill
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
				var workloadDetailView = new WorkloadDetailView(
						(SkillDayCalculator)_skillDayCalculator, workload, _workloadChartSetting, _statisticHelper) {Name = "Workload"};
				initializeDetailView(workloadDetailView);
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
			var multisiteCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
			tabControlAdvMultisiteSkill.TabPages.Clear();
			TabPageAdv tabPage = ColorHelper.CreateTabPage(_multisiteSkill.Name, _multisiteSkill.Description);
			var multisiteSkillDetailView = new MultisiteSkillDetailView(multisiteCalculator, _skillChartSetting, _statisticHelper)
			{
				Name = "MultiSkill"
			};
			initializeDetailView(multisiteSkillDetailView);
			tabControlAdvMultisiteSkill.TabPages.Add(tabPage);
			tabPage.Controls.Add(multisiteSkillDetailView);

			foreach (IChildSkill childSkill in _multisiteSkill.ChildSkills)
			{
				tabPage = ColorHelper.CreateTabPage(childSkill.Name, childSkill.Description);

				var childSkillDetailView = new ChildSkillDetailView(multisiteCalculator, childSkill, _skillChartSetting, _statisticHelper)
				{
					Name = "MultiSkill"
				};
				initializeDetailView(childSkillDetailView);
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

		private void detailViewWorkingIntervalChanged(object sender, WorkingIntervalChangedEventArgs e)
		{
			if (_noChangesRightNow) return;
			_noChangesRightNow = true;

			var myabstractDetailView = sender as AbstractDetailView;
			showTabGroup(myabstractDetailView.TargetType, myabstractDetailView.CurrentWorkingInterval);
			if (myabstractDetailView is MultisiteSkillDetailView)
				showTabGroup(TemplateTarget.Multisite, myabstractDetailView.CurrentWorkingInterval); //Both skill and multisite should be shown
			if (myabstractDetailView is ChildSkillDetailView)
				showTabGroup(TemplateTarget.Multisite, WorkingInterval.Custom); //Never show multisite template tab for child skill detail views

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

			_currentLocalDate = e.NewStartDate;
			_currentLocalDateTime = e.NewStartDate.Date.Add(e.NewTimeOfDay);
			_timeNavigationControl.SetSelectedDate(_currentLocalDate);
			_noChangesRightNow = false;

			reloadChart();
		}

		private IForecastTemplateOwner getRefreshedAggregateRoot(TemplateTarget templateTarget, bool refresh)
		{
			IForecastTemplateOwner root = null;
			switch (templateTarget)
			{
				case TemplateTarget.Skill:
					AbstractDetailView detailView = getCurrentSkillDetailView();
					root = (ISkill)detailView.GetType().GetProperty("Skill")
							.GetValue(detailView, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
					break;
				case TemplateTarget.Multisite:
					AbstractDetailView multisiteDetailView = getCurrentSkillDetailView() as MultisiteSkillDetailView;
					if (multisiteDetailView == null)
					{
						multisiteDetailView = getCurrentSkillDetailView() as ChildSkillDetailView;
					}
					if (multisiteDetailView != null)
					{
						root = (ISkill)multisiteDetailView.GetType().GetProperty("Skill")
						.GetValue(multisiteDetailView, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
					}
					break;
				case TemplateTarget.Workload:
					var workloadDetailView = getCurrentWorkloadDetailView() as WorkloadDetailView;
					if (workloadDetailView != null) root = workloadDetailView.Workload;
					break;
			}
			if (root != null && refresh) ForecastingTemplateRefresher.RefreshRoot(root);

			return root;
		}

		private TeleoptiToolStripGallery getTemplateToolStripGallery(TemplateTarget templateTarget)
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

		private void showTabGroup(TemplateTarget templateTarget, WorkingInterval workingInterval)
		{
			bool visible = (workingInterval == WorkingInterval.Day);
			var toolStrip = getTemplateToolStripGallery(templateTarget);
			if (toolStrip == null) return;

			toolStrip.ParentRibbonTab.Visible = visible;
		}

		private AbstractDetailView getCurrentWorkloadDetailView()
		{
			return tabControlWorkloads.SelectedTab.Controls[0] as AbstractDetailView;
		}

		private AbstractDetailView getCurrentSkillDetailView()
		{
			return tabControlAdvMultisiteSkill.SelectedTab.Controls[0] as AbstractDetailView;
		}

		private void ribbonTemplatePanelsClose()
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
			var skillDetailView = new SkillDetailView((SkillDayCalculator)_skillDayCalculator, _skill, _skillChartSetting, _statisticHelper)
			{
				Name = "Skill"
			};
			initializeDetailView(skillDetailView);
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

		private void initializeDetailView(AbstractDetailView detailView)
		{
			detailView.Dock = DockStyle.Fill;
			detailView.WorkingIntervalChanged += detailViewWorkingIntervalChanged;
			detailView.TemplateSelected += detailViewTemplateSelected;
			detailView.ValuesChanged += detailViewValuesChanged;
			detailView.CellClicked += detailViewCellClicked;
			_detailViews.Add(detailView);
		}

		private void detailViewTemplateSelected(object sender, TemplateEventArgs e)
		{
			selectTemplateInToolstrip(getTemplateToolStripGallery(e.TemplateTarget), e.TemplateName);
		}

		private void timeNavigationControlSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
		{
			_currentLocalDate = e.Value;
			_currentLocalDateTime = e.Value.Date;
			foreach (AbstractDetailView item in _detailViews)
			{
				item.CurrentDay = _currentLocalDate;
				item.CurrentTimeOfDay = _currentLocalDateTime.TimeOfDay;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;

			if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
			{
				switch (keyData)
				{
					case Keys.Control | Keys.S:
						btnSaveClick(this, EventArgs.Empty);
						break;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void btnSaveClick(object sender, EventArgs e)
		{
			if (validateForm())
				save(null);
		}

		private void toolStripButtonForecastWorkflowClick(object sender, EventArgs e)
		{
			IWorkload workload = getRefreshedAggregateRoot(TemplateTarget.Workload, false) as Workload;
			if (workload == null) return;
			if (checkToClose())
			{
				workload = getWorkload(workload);
				using (var workflow = new ForecastWorkflow(workload, _scenario, _dateTimePeriod,
					_skillDayCalculator.SkillDays.ToList(), this, _statisticHelper))
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

		private void toolStripButtonIncreaseDecimalsClick(object sender, EventArgs e)
		{
			var before = _currentForecasterSettings.NumericCellVariableDecimals;
			_currentForecasterSettings.NumericCellVariableDecimals = before + 1;
			var after = _currentForecasterSettings.NumericCellVariableDecimals;
			if (before==after) return;

			foreach (KeyValuePair<string, TeleoptiGridControl> keyValuePair in _gridCollection)
			{
				keyValuePair.Value.ChangeNumberOfDecimals(1);
			}
		}

		private void toolStripButtonDecreaseDecimalsClick(object sender, EventArgs e)
		{
			var before = _currentForecasterSettings.NumericCellVariableDecimals;
			_currentForecasterSettings.NumericCellVariableDecimals = before-1;
			var after = _currentForecasterSettings.NumericCellVariableDecimals;
			if (before == after) return;
			
			foreach (KeyValuePair<string, TeleoptiGridControl> keyValuePair in _gridCollection)
			{
				keyValuePair.Value.ChangeNumberOfDecimals(-1);
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (_forceClose) return;

			if (backgroundWorker1.IsBusy)
			{
				//Since it doesn't seem to be possible to do cancel onm this we just need to make the user wait for the complete load
				e.Cancel = true;
				_userWantsToCloseForecaster = true;
				backgroundWorker1.ReportProgress(1, UserTexts.Resources.CancelLoading);
				return;

			}

			if (backgroundWorkerSave.IsBusy)
			{
				e.Cancel = true;
				return;
			}

			base.OnFormClosing(e);
			if (checkToClose())
			{
				setDisplaySettings();

				try
				{
					saveSettings();
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

		private void saveSettings()
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

		private void saveDisplaySettings()
		{
			setDisplaySettings();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PersonalSettingDataRepository(uow);
				repository.PersistSettingValue(_currentForecasterSettings);
				uow.PersistAll();
			}
		}

		private void setDisplaySettings()
		{
			_currentForecasterSettings.ShowGraph = _showGraph;
			_currentForecasterSettings.ShowSkillView = _showSkillView;
		}

		private void toolStripButtonExitClick(object sender, EventArgs e)
		{
			saveDisplaySettings();
			if (!CloseAllOtherForms(this))
				return; // a form was canceled

			Close();
			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

		void detailViewCellClicked(object sender, EventArgs e)
		{
			var abstractDetailView = sender as AbstractDetailView;

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

		private void toolStripButtonPrintClick(object sender, EventArgs e)
		{
			if (_chartControl == null) return;

			using (var pPrintDialog = new PrintDialog { Document = _chartControl.PrintDocument })
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

		private void toolStripButtonPrintPreviewClick(object sender, EventArgs e)
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

		private void tabControlWorkloadsSelectedIndexChanged(object sender, EventArgs e)
		{
			TabPageAdv tabPage = ((TabControlAdv)sender).SelectedTab;
			if (tabPage == null) return;

			createTemplateGalleryRibbonBar(TemplateTarget.Workload, false, false);

			var detailView = tabPage.Controls[0] as AbstractDetailView;
			triggerDetailViewWorkingIntervalChanged(detailView);

			_zoomButtonsEventArgs.GridKey = string.Empty;
			reloadChart();
		}

		private void triggerDetailViewWorkingIntervalChanged(AbstractDetailView detailView)
		{
			if (detailView == null) return;

			detailViewWorkingIntervalChanged(detailView, new WorkingIntervalChangedEventArgs
			{
				NewStartDate = detailView.CurrentDay,
				NewTimeOfDay = detailView.CurrentTimeOfDay,
				NewWorkingInterval = detailView.CurrentWorkingInterval
			});
		}

		private void tabControlAdvMultisiteSkillSelectedIndexChanged(object sender, EventArgs e)
		{
			TabPageAdv tabPage = ((TabControlAdv)sender).SelectedTab;
			if (tabPage == null) return;

			//Recreate template list
			createTemplateGalleryRibbonBar(TemplateTarget.Multisite, false, false);
			createTemplateGalleryRibbonBar(TemplateTarget.Skill, false, false);

			var detailView = tabPage.Controls[0] as AbstractDetailView;
			triggerDetailViewWorkingIntervalChanged(detailView);

			_zoomButtonsEventArgs.GridKey = string.Empty;
			reloadChart();
		}

		private void chartControlChartRegionMouseEnter(object sender, ChartRegionMouseEventArgs e)
		{
			GridChartManager.SetChartToolTip(e.Region, _chartControl);
		}

		private void teleoptiToolStripGallerySkillItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == null) return;
			if (e.ClickedItem.Tag as bool? == true) return;
			setupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Skill);
		}

		private void teleoptiToolStripGalleryWorkloadItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == null) return;
			if (e.ClickedItem.Tag as bool? == true) return;
			setupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Workload);
		}

		private void teleoptiToolStripGalleryMultisiteSkillItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == null) return;
			if (e.ClickedItem.Tag as bool? == true) return;
			setupGalleryItem(e.ContextMenuStrip, e.ClickedItem.Text, TemplateTarget.Multisite);
		}

		private void setupGalleryItem(ContextMenuStrip contextMenu, string templateName, TemplateTarget templateTarget)
		{
			var templateInfo = new TemplateEventArgs {TemplateTarget = templateTarget, TemplateName = templateName};

			var itemEdit = new ToolStripMenuItem {Text = UserTexts.Resources.Edit, Tag = templateInfo};
			itemEdit.Click += itemEditClick;
			contextMenu.Items.Add(itemEdit);

			var itemRemove = new ToolStripMenuItem {Text = UserTexts.Resources.Delete, Tag = templateInfo};
			itemRemove.Click += itemRemoveClick;
			contextMenu.Items.Add(itemRemove);

			var itemRename = new ToolStripMenuItem {Text = UserTexts.Resources.Rename, Tag = templateInfo};
			itemRename.Click += itemRenameClick;
			contextMenu.Items.Add(itemRename);
		}

		private void itemRemoveClick(object sender, EventArgs e)
		{
			var selectedItem = sender as ToolStripItem;
			if (selectedItem == null) return;

			var templateInfo = selectedItem.Tag as TemplateEventArgs;
			if (templateInfo == null) return;

			attemptDatabaseConnectionDependentAction(() =>
					deleteTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
				);
		}

		private bool attemptDatabaseConnectionDependentAction(System.Action action)
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

		private void itemEditClick(object sender, EventArgs e)
		{
			var selectedItem = sender as ToolStripItem;
			if (selectedItem == null) return;

			var templateInfo = selectedItem.Tag as TemplateEventArgs;
			if (templateInfo == null) return;

			attemptDatabaseConnectionDependentAction(() =>
				editTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
				);
		}


		private void itemRenameClick(object sender, EventArgs e)
		{
			var selectedItem = sender as ToolStripItem;
			if (selectedItem == null) return;

			var templateInfo = selectedItem.Tag as TemplateEventArgs;
			if (templateInfo == null) return;

			attemptDatabaseConnectionDependentAction(() =>
				renameTemplate(templateInfo.TemplateTarget, templateInfo.TemplateName)
				);
		}

		private void teleoptiToolStripGallerySkillGalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
		{
			applyTemplate(TemplateTarget.Skill, e.GalleryItem.Text);
		}

		private void teleoptiToolStripGalleryWorkloadGalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
		{
			applyTemplate(TemplateTarget.Workload, e.GalleryItem.Text);
		}

		private void teleoptiToolStripGalleryMultisiteSkillGalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
		{
			applyTemplate(TemplateTarget.Multisite, e.GalleryItem.Text);
		}

		private void applyTemplate(TemplateTarget templateTarget, string templateName)
		{
			switch (templateTarget)
			{
				case TemplateTarget.Workload:
					getCurrentWorkloadDetailView().SetTemplate(templateName, templateTarget);
					break;
				case TemplateTarget.Skill:
				case TemplateTarget.Multisite:
					getCurrentSkillDetailView().SetTemplate(templateName, templateTarget);
					break;
			}
		}

		private void resetTemplate(TemplateTarget templateTarget)
		{
			switch (templateTarget)
			{
				case TemplateTarget.Workload:
					getCurrentWorkloadDetailView().ResetTemplates(templateTarget);
					break;
				case TemplateTarget.Skill:
				case TemplateTarget.Multisite:
					getCurrentSkillDetailView().ResetTemplates(templateTarget);
					break;
			}
		}
		private void resetLongterm()
		{
			getCurrentWorkloadDetailView().ResetLongterm();
		}

		private static void selectTemplateInToolstrip(TeleoptiToolStripGallery toolStrip, string templateName)
		{
			if (toolStrip == null) return;
			toolStrip.SetCheckedItem(toolStrip.Items.OfType<ToolStripGalleryItem>().FirstOrDefault(t => t.Text == templateName));
		}

		private void toolStripButtonCreateNewMultisiteTemplateClick(object sender, EventArgs e)
		{
			EditMultisiteDayTemplate editTemplate = null;
			var success = attemptDatabaseConnectionDependentAction(() =>
			{
				AbstractDetailView detailView = getCurrentSkillDetailView();
				var multisiteSkill =
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
					applyTemplate(TemplateTarget.Multisite, editTemplate.TemplateName);
			}
			editTemplate.Dispose();
		}

		private void toolStripBtnCreateSkillTemplateClick(object sender, EventArgs e)
		{
			EditSkillDayTemplate editTemplate = null;
			var success = attemptDatabaseConnectionDependentAction(() =>
			{
				AbstractDetailView detailView = getCurrentSkillDetailView();
				editTemplate = new EditSkillDayTemplate(
					(Skill) detailView.GetType().GetProperty("Skill")
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
					applyTemplate(TemplateTarget.Skill, editTemplate.TemplateName);
			}
			editTemplate.Dispose();
		}

		private void toolStripButtonCreateNewTemplateClick(object sender, EventArgs e)
		{
			//TODO! Need a way to get open hours from current workload day
			//For now I'm using the default times (8-17)
			IList<TimePeriod> openHours = new List<TimePeriod>();
			EditWorkloadDayTemplate editTemplate = null;
			var success =
				attemptDatabaseConnectionDependentAction(() =>
					editTemplate =
						new EditWorkloadDayTemplate(
							((WorkloadDetailView) getCurrentWorkloadDetailView()).Workload, openHours, _statisticHelper));

			if (!success)
				return;
			if (DialogResult.OK == editTemplate.ShowDialog(this))
			{
				//If this works, ask the user if she would like to apply the template right away!
				if (
					ShowConfirmationMessage(UserTexts.Resources.ApplyNewTemplateQuestion, UserTexts.Resources.Template) ==
					DialogResult.Yes)
					applyTemplate(TemplateTarget.Workload, editTemplate.TemplateName);
			}
			editTemplate.Dispose();
		}

		private void createTemplateGalleryRibbonBar(TemplateTarget templateTarget, bool refresh, bool force)
		{
			TeleoptiToolStripGallery galleryControl = getTemplateToolStripGallery(templateTarget);
			if (galleryControl == null) return;

			if (tabControlWorkloads.SelectedTab == null || tabControlAdvMultisiteSkill.SelectedTab == null) return;

			IForecastTemplateOwner rootEntity;
			var templateList = getTemplates(templateTarget, refresh, out rootEntity);

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
						IForecastDayTemplate template = templateList[(int)dayOfWeek];
						galleryControl.Items.Add(getGalleryItem(template));
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
						galleryControl.Items.Add(getGalleryItem(template));
					}
				}
			}

			var sortedTemplates =
					from t in templateList
					orderby t.Value.Name
					select t;

			foreach (var template in sortedTemplates.Where(i => !i.Value.DayOfWeek.HasValue))
			{
				galleryControl.Items.Add(getGalleryItem(template.Value));
			}

			galleryControl.Tag = rootEntity;
		}

		private ToolStripGalleryItem getGalleryItem(IForecastDayTemplate template)
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

		private void removeDayTemplate(TemplateTarget target, string templateName)
		{
			IForecastTemplateOwner rootEntity = getRefreshedAggregateRoot(target, false);

			IEnumerable<IRootChangeInfo> changedRoots;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IForecastTemplateOwner templateOwnerOriginal = ForecastingTemplateRefresher.LoadNewInstance(rootEntity, uow);
				templateOwnerOriginal.RemoveTemplate(target, templateName);

				changedRoots = uow.PersistAll();
			}
			EntityEventAggregator.TriggerEntitiesNeedRefresh(null, changedRoots);
		}

		private void editTemplate(TemplateTarget target, string templateName)
		{
			IForecastTemplateOwner rootEntity = getRefreshedAggregateRoot(target, false);
			IForecastDayTemplate template = rootEntity.TryFindTemplateByName(target, templateName);
			if (template == null) return;

			switch (target)
			{
				case TemplateTarget.Multisite:
					var editMultisiteTemplate = new EditMultisiteDayTemplate((MultisiteDayTemplate)template);
					editMultisiteTemplate.Show(this);
					break;
				case TemplateTarget.Workload:
					var editWorkloadTemplate = new EditWorkloadDayTemplate((WorkloadDayTemplate)template, _statisticHelper);
					editWorkloadTemplate.Show(this);
					break;
				case TemplateTarget.Skill:
					var editSkillTemplate = new EditSkillDayTemplate((ISkillDayTemplate)template);
					editSkillTemplate.Show(this);
					break;
			}
		}

		// get this from metadata? asks KlasM. RogerK and RobinK replies that it is ok to hardcode for now 
		private const int templateNameMaxLength = 50;

		private void renameTemplate(TemplateTarget target, string originalTemplateName)
		{
			var ctrl = new PromptTextBox(new renameTemplateTag { Target = target, OriginalTemplateName = originalTemplateName },
					originalTemplateName, UserTexts.Resources.Template, templateNameMaxLength, validateWorkloadRenameName);
			ctrl.SetHelpId("NameForecastTemplate");
			ctrl.NameThisView += ctrlRenameTemplate;
			ctrl.ShowDialog(this);
		}

		private bool validateWorkloadRenameName(string newName)
		{
			return
				((WorkloadDetailView) getCurrentWorkloadDetailView()).Workload.TemplateWeekCollection.All(
					t => !t.Value.Name.Equals(newName,StringComparison.InvariantCultureIgnoreCase));
		}

		private class renameTemplateTag
		{
			public TemplateTarget Target { get; set; }
			public string OriginalTemplateName { get; set; }
		}

		private void ctrlRenameTemplate(object sender, CustomEventArgs<TupleItem> e)
		{
			var templateTag = (renameTemplateTag)e.Value.ValueMember;
			string newName = e.Value.Text;
			if (newName != templateTag.OriginalTemplateName)
			{
				try
				{
					saveTemplateWithNewName(newName, templateTag);
				}
				catch (DataSourceException dataSourceException)
				{
					datasourceExceptionOccurred(dataSourceException);
				}
			}
		}

		private void saveTemplateWithNewName(string newName, renameTemplateTag templateTag)
		{
			IForecastTemplateOwner rootEntity = getRefreshedAggregateRoot(templateTag.Target, false);

			IEnumerable<IRootChangeInfo> changedRoots;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IForecastTemplateOwner templateOwnerOriginal =
						ForecastingTemplateRefresher.LoadNewInstance(rootEntity, uow);
				IForecastDayTemplate template =
						templateOwnerOriginal.TryFindTemplateByName(templateTag.Target, templateTag.OriginalTemplateName);
				if (template == null) return;
				template.Name = newName;

				changedRoots = uow.PersistAll();
			}
			EntityEventAggregator.TriggerEntitiesNeedRefresh(null, changedRoots);
			RefreshTabs();
		}

		private void deleteTemplate(TemplateTarget target, string templateName)
		{
			removeDayTemplate(target, templateName);
			RefreshTabs();
		}

		private IDictionary<int, IForecastDayTemplate> getTemplates(TemplateTarget templateTarget, bool refresh, out IForecastTemplateOwner rootEntity)
		{
			rootEntity = getRefreshedAggregateRoot(templateTarget, refresh);
			IDictionary<int, IForecastDayTemplate> result = null;
			if (rootEntity != null)
			{
				result = rootEntity.GetTemplates(templateTarget).OrderBy(i => i.Key).ToDictionary(k => k.Key, v => v.Value);
			}
			return result;
		}

		private void toolStripButtonResetMultisiteSkillTemplatesClick(object sender, EventArgs e)
		{
			disableAllControlsExceptCancelLoadButton();

			backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Multisite);
		}

		private void toolStripButtonResetWorkloadTemplatesClick(object sender, EventArgs e)
		{
			disableAllControlsExceptCancelLoadButton();

			backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Workload);
		}

		private void toolStripButtonResetSkillTemplatesClick(object sender, EventArgs e)
		{
			disableAllControlsExceptCancelLoadButton();

			backgroundWorkerApplyStandardTemplates.RunWorkerAsync(TemplateTarget.Skill);
		}

		private void toolStripButtonCloseClick(object sender, EventArgs e)
		{
			saveDisplaySettings();
			Close();
		}

		private void toolStripButtonHelpClick(object sender, EventArgs e)
		{
			ViewBase.ShowHelp(this, false);
		}

		private bool checkToClose()
		{
			if (!isDirtyListEmpty())
			{
				switch (askToCommitChanges())
				{
					case DialogResult.Yes:
						if (!validateForm()) //Validation is only done if the user would like to save and something is dirty
							return false;
						showProgressBar();
						try
						{
							choppedSave();
						}
						catch (DataSourceException ex)
						{
							//transaction time out, t.ex, table locked by another.
							datasourceExceptionOccurred(ex);
							return false;
						}
						informUserOfUnsavedData();
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

		private bool isDirtyListEmpty()
		{
			return _dirtyForecastDayContainer.IsEmpty();
		}

		private void toolStripButtonSystemOptionsClick(object sender, EventArgs e)
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager, _businessRuleConfigProvider, _configReader)));
				settings.Show();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void toolStripButtonShowGraphClick(object sender, EventArgs e)
		{
			toolStripButtonShowGraph.Checked = !toolStripButtonShowGraph.Checked;
			splitterManager.ShowGraph = toolStripButtonShowGraph.Checked;
			_showGraph = toolStripButtonShowGraph.Checked;
			splitContainer2.Panel1.Controls.Remove(_chartControl);
			addChart();
		}

		private void toolStripButtonShowSkillViewClick(object sender, EventArgs e)
		{
			toolStripButtonShowSkillView.Checked = !toolStripButtonShowSkillView.Checked;
			splitterManager.ShowSkillView = toolStripButtonShowSkillView.Checked;
			_showSkillView = toolStripButtonShowSkillView.Checked;
		}

		private SplitterManager splitterManager
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

		private void toolStripTextBoxNewScenarioTextChanged(object sender, EventArgs e)
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

		private void toolStripTextBoxNewScenarioKeyPress(object sender, KeyPressEventArgs e)
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
						IScenarioRepository scenarioRepository = ScenarioRepository.DONT_USE_CTOR(unitOfWork);
						scenarioRepository.Add(newScenario);
						unitOfWork.PersistAll();
					}
					saveForecastToScenario(newScenario);
				}
			}
		}

		void toolStripTextBoxNewScenarioLostFocus(object sender, EventArgs e)
		{
			var italicFont = new Font(toolStripTextBoxNewScenario.Font.FontFamily, toolStripTextBoxNewScenario.Font.SizeInPoints, FontStyle.Italic);
			toolStripTextBoxNewScenario.Font = italicFont;
			toolStripTextBoxNewScenario.Text = "(" + UserTexts.Resources.NewScenario + ")";
		}

		void toolStripTextBoxNewScenarioGotFocus(object sender, EventArgs e)
		{
			var scenarioName = (string)toolStripTextBoxNewScenario.Tag;
			if (string.IsNullOrEmpty(scenarioName))
			{
				toolStripTextBoxNewScenario.Text = "";
			}
			else
			{
				toolStripTextBoxNewScenario.Text = (string)toolStripTextBoxNewScenario.Tag;
			}
			var regularFont = new Font(toolStripTextBoxNewScenario.Font.FontFamily, toolStripTextBoxNewScenario.Font.SizeInPoints, FontStyle.Regular);
			toolStripTextBoxNewScenario.Font = regularFont;
			//toolStripTextBoxNewScenario.Font.ChangeToRegular(); // Doesn't work... /Henry
		}
		
		private void unhookEvents()
		{
			_zoomButtons.ZoomChanged -= buttonsZoomChanged;
			_zoomButtonsChart.ZoomChanged -= zoomButtonsChartZoomChanged;
			_timeNavigationControl.SelectedDateChanged -= timeNavigationControlSelectedDateChanged;
			_gridrowInChartSetting.LineInChartSettingsChanged -= gridlinesInChartSettingsLineInChartSettingsChanged;
			_gridrowInChartSetting.LineInChartEnabledChanged -= gridrowInChartSettingLineInChartEnabledChanged;
			if (_chartControl != null)
			{
				_chartControl.ChartRegionClick -= chartControlChartRegionClick;
				_chartControl.ChartRegionMouseEnter -= chartControlChartRegionMouseEnter;
			}
			backgroundWorker1.RunWorkerCompleted -= backgroundWorker1RunWorkerCompleted;
			backgroundWorker1.DoWork -= backgroundWorker1DoWork;
			backgroundWorker1.ProgressChanged -= backgroundWorker1ProgressChanged;

			EntityEventAggregator.EntitiesNeedsRefresh -= mainScreenEntitiesNeedsRefresh;
		}

		private void releaseManagedResources()
		{
			clearExistingExportScenarioButtons();
			foreach (AbstractDetailView detailView in _detailViews)
			{
				detailView.WorkingIntervalChanged -= detailViewWorkingIntervalChanged;
				detailView.TemplateSelected -= detailViewTemplateSelected;
				detailView.ValuesChanged -= detailViewValuesChanged;
				detailView.CellClicked -= detailViewCellClicked;
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

		private void toolStripButtonLongtermWorkloadTemplatesClick(object sender, EventArgs e)
		{
			resetLongterm();
		}

		private void backgroundWorkerSaveDoWork(object sender, DoWorkEventArgs e)
		{
			var scenario = e.Argument as IScenario;
			if (scenario != null)
			{
				var command = new SaveForecastToScenarioCommand(_skill, _skillDayCalculator, _dateTimePeriod);
				command.ProgressReporter += saveForecastToScenarioCommandProgressReporter;
				e.Result = command.Execute(scenario);
				command.ProgressReporter -= saveForecastToScenarioCommandProgressReporter;
			}
			else choppedSave();
		}

		private void backgroundWorkerSaveProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			var progressBarIncrement = toolStripProgressBarMain.Value;
			if (progressBarIncrement + 1 >= toolStripProgressBarMain.Minimum && progressBarIncrement + 1 <= toolStripProgressBarMain.Maximum)
				toolStripProgressBarMain.Value++;
		}

		private void backgroundWorkerSaveRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

					EntityEventAggregator.EntitiesNeedsRefresh += mainScreenEntitiesNeedsRefresh;

					reloadDetailViews();
					setExportVisability();

					Cursor = Cursors.Default;
				}
			}

			toolStripProgressBarMain.Visible = false;
			toolStripStatusLabelInfo.Visible = false;

			if (!datasourceExceptionOccurred(e.Error))
			{
				throwIfExceptionOccurred(e);
			}

			enableAllControlsExceptCancelLoadButton();

			informUserOfUnsavedData();
		}

		private void backgroundWorkerApplyStandardTemplatesDoWork(object sender, DoWorkEventArgs e)
		{
			resetTemplate((TemplateTarget)e.Argument);
		}

		private void backgroundWorkerApplyStandardTemplatesRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;

			toolStripProgressBarMain.Visible = false;
			toolStripStatusLabelInfo.Visible = false;

			throwIfExceptionOccurred(e);

			enableAllControlsExceptCancelLoadButton();

			toolStripProgressBarMain.Visible = false;
		}

		public void ReloadForecaster()
		{
			reloadCurrentData();
			//throw new NotImplementedException();
		}

		private void backStageButtonSaveClick(object sender, EventArgs e)
		{
			if (validateForm())
				save(null);
		}

		private void backStageButtonCloseClick(object sender, EventArgs e)
		{
			saveDisplaySettings();
			Close();
		}

		private void backStageButtonOptionsClick(object sender, EventArgs e)
		{
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(_toggleManager, _businessRuleConfigProvider, _configReader)));
				settings.Show();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void backStageButton4Click(object sender, EventArgs e)
		{
			saveDisplaySettings();
			if (!CloseAllOtherForms(this))
				return; // a form was canceled

			Close();
			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

		private void Forecaster_FormClosed(object sender, FormClosedEventArgs e)
		{
			_mainWindow.Activate();
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
