using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Notes;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Intraday;
using Teleopti.Wfm.Adherence.Configuration;
using Cursors = System.Windows.Forms.Cursors;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;
using ToolStripItemClickedEventArgs = Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class IntradayView : BaseRibbonForm, IIntradayView
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private readonly IResourceOptimizationHelperExtended _resourceOptimizationHelperExtended;
	    private readonly IEventPublisher _publisher;
	    private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly IComponentContext _container;

		private DateNavigateControl _timeNavigationControl;
		private GridRowInChartSettingButtons _gridrowInChartSetting;
		private IntradayViewContent _intradayViewContent;
		private ToolStripGalleryItem _previousClickedGalleryItem;
		private DateTime _lastUpdateUtc;
		private bool _forceClose;
		private readonly IntradaySettingManager _settingManager;

		public IntradayView(IEventAggregator eventAggregator, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
			IResourceOptimizationHelperExtended resourceOptimizationHelperExtended, IEventPublisher publisher,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IScheduleDayChangeCallback scheduleDayChangeCallback, ISkillPriorityProvider skillPriorityProvider, IComponentContext container)
		{
			_eventAggregator = eventAggregator;
			_overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_publisher = publisher;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_skillPriorityProvider = skillPriorityProvider;
			_container = container;

			var authorization = PrincipalAuthorization.Current();

			InitializeComponent();
			if (DesignMode) return;

			ribbonControlAdv1.Enabled = false;
			toolStripExDatePicker.Visible = false;
			SetTexts();
			ControlBox = false;
			StartProgress();
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.LoadingFormThreeDots);

			_settingManager = new IntradaySettingManager();

			if (authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.IntradayReForecasting))
				toolStripExChangeForecast.Visible = true;

			toolStripStatusLabelLastUpdate.Click += ToolStripStatusLabelLastUpdateOnClick;
		}

		public IntradayPresenter Presenter { get; set; }

		public void StartProgress()
		{
			_eventAggregator.GetEvent<IntradayLoadProgress>().Subscribe(reportProgress);
		}

		private void reportProgress(string obj)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(reportProgress), obj);
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
			_eventAggregator.GetEvent<IntradayLoadProgress>().Unsubscribe(reportProgress);
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

			_gridrowInChartSetting.LineInChartSettingsChanged += gridlinesInChartSettingsLineInChartSettingsChanged;
			_gridrowInChartSetting.LineInChartEnabledChanged += gridrowInChartSettingLineInChartEnabledChanged;
		}

		private void initializeNavigationControl()
		{
			_timeNavigationControl = new DateNavigateControl();
			_timeNavigationControl.SelectedDateChanged += timeNavigationControlSelectedDateChanged;
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

		private void authorizeFunctions()
		{
			toolStripTabItemChart.Visible = Presenter.EarlyWarningEnabled;
			toolStripTabItemChart.Enabled = Presenter.EarlyWarningEnabled;
			toolStripStatusLabelLastUpdate.Visible = Presenter.RealTimeAdherenceEnabled;
		}

		private void setupIntradayDate()
		{
			_timeNavigationControl.SetAvailableTimeSpan(Presenter.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod);
		}

		private void initializeGallery()
		{
			teleoptiToolStripGalleryViews.ParentRibbonTab.CheckState = CheckState.Indeterminate;
			teleoptiToolStripGalleryViews.ParentRibbonTab.Checked = teleoptiToolStripGalleryViews.ParentRibbonTab.Visible;
			teleoptiToolStripGalleryViews.Items.Clear();
		}

		private void finalizeGallery()
		{
			_settingManager.Remove("xxDefaultView");

			fillGallery();

			teleoptiToolStripGalleryViews.ItemClicked += teleoptiToolStripGalleryViewsItemClicked;
			teleoptiToolStripGalleryViews.GalleryItemClicked += teleoptiToolStripGalleryViewsGalleryItemClicked;
		}

		private void fillGallery()
		{
			teleoptiToolStripGalleryViews.Items.Clear();
			//Place the default first, needs refactoring?!?
			addDefaultItem();

			foreach (IntradaySetting setting in _settingManager.IntradaySettings)
			{
				var item = new ToolStripGalleryItem {Text = setting.Name, Tag = setting, Image = imageListLayouts.Images[1]};
				if (setting.Name == "xxDefaultIntradaySetting")//Default
					continue;

				teleoptiToolStripGalleryViews.Items.Add(item);
				if (setting.Name.Equals(_settingManager.CurrentIntradaySetting.Name))
					teleoptiToolStripGalleryViews.SetCheckedItem(item);
			}

		}

		private void addDefaultItem()
		{
			var defaultSetting = _settingManager.DefaultSetting();
			var defaultItem = new ToolStripGalleryItem
			{
				Text = LanguageResourceHelper.Translate(defaultSetting.Name),
				Tag = defaultSetting,
				Image = imageListLayouts.Images[0]
			};

			teleoptiToolStripGalleryViews.Items.Add(defaultItem);
		}

		private Exception ExternalAgentStateReceivedEventArgsException;

		private void presenterExternalAgentStateReceived(object sender, IntradayPresenter.ExternalAgentStateReceivedEventArgs e)
		{
			if (InvokeRequired)
				BeginInvoke(new EventHandler<IntradayPresenter.ExternalAgentStateReceivedEventArgs>(presenterExternalAgentStateReceived), sender, e);
			else
			{
				if ((int)_lastUpdateUtc.TimeOfDay.TotalSeconds == (int)DateTime.UtcNow.TimeOfDay.TotalSeconds)
					return; //To avoid updates that doesn't change the content

				_lastUpdateUtc = DateTime.UtcNow;

				ExternalAgentStateReceivedEventArgsException = e.Exception;
				if (e.Exception != null)
				{
					toolStripStatusLabelLastUpdate.Text = Resources.Error;
				}
				else
				{
					toolStripStatusLabelLastUpdate.Text = string.Format(CultureInfo.CurrentCulture,
						Resources.LastUpdateColonParameter0,
						TimeZoneHelper.ConvertFromUtc(DateTime.UtcNow, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone).
							ToLongTimeString());
				}
			}
		}

		private void ToolStripStatusLabelLastUpdateOnClick(object o, EventArgs e)
		{
			if (ExternalAgentStateReceivedEventArgsException == null)
				return;
			using (var view = new SimpleExceptionHandlerView(ExternalAgentStateReceivedEventArgsException, Resources.Error, Resources.UnableToConnectRemoteService))
			{
				view.ShowDialog();
			}
		}

		private void timeNavigationControlSelectedDateChanged(object sender, CustomEventArgs<DateOnly> e)
		{
			Presenter.IntradayDate = e.Value;
		}

		private void toolStripButtonNewViewClick(object sender, EventArgs e)
		{
			_settingManager.UpdatePreviousDockingState();
			var ctrl = new PromptTextBox(new object(), "", Resources.View, 50);
			ctrl.NameThisView += ctrlNameThisView;
			ctrl.ShowDialog(this);
		}

		private void teleoptiToolStripGalleryViewsGalleryItemClicked(object sender, ToolStripGalleryItemEventArgs e)
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

		private void teleoptiToolStripGalleryViewsItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == null) return;
			if (e.ClickedItem.Tag as bool? == true) return;
			setupGalleryItem(e);
			e.ContextMenuStrip.Items[0].Enabled = !isDefaultView();

			_intradayViewContent.SelectChartView(((IntradaySetting)e.ClickedItem.Tag).Name);
		}

		private void setupGalleryItem(ToolStripItemClickedEventArgs e)
		{
			var item = new TupleItem(e.ClickedItem.Text, e.ClickedItem.Tag);

			ToolStripItem itemRemove = new ToolStripMenuItem();
			itemRemove.Text = Resources.Delete;
			itemRemove.Tag = item;
			itemRemove.Click += itemRemoveClick;
			e.ContextMenuStrip.Items.Add(itemRemove);
		}

		private void itemRemoveClick(object sender, EventArgs e)
		{
			var toolstripItem = ((ToolStripItem)sender);
			toolstripItem.Click -= itemRemoveClick;

			var item = (TupleItem)toolstripItem.Tag;
			var deletedItemIndex = teleoptiToolStripGalleryViews.Items.IndexOf(findGalleryItemByName(item.Text));

			_settingManager.Remove(item.Text);

			fillGallery();
			toolStripExLayouts.Refresh();

			var previousOne = deletedItemIndex < 1 ? defaultView() : teleoptiToolStripGalleryViews.Items[deletedItemIndex - 1];

			var name = ((IntradaySetting)previousOne.Tag).Name;
			_settingManager.LoadDockingState(name);

			teleoptiToolStripGalleryViews.SetCheckedItem(previousOne);
			_intradayViewContent.SelectChartView(name);
			_previousClickedGalleryItem = previousOne;
			_intradayViewContent.Refresh();
		}

		private void ctrlNameThisView(object sender, CustomEventArgs<TupleItem> e)
		{
			var prompt = sender as PromptTextBox;
			if (prompt != null)
			{
				prompt.NameThisView -= ctrlNameThisView;
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
			fillGallery();
			var item = findGalleryItemByName(e.Value.Text);
			_previousClickedGalleryItem = item;
			toolStripExLayouts.Refresh();
		}

		private ToolStripGalleryItem findGalleryItemByName(string name)
		{
			foreach (var item in teleoptiToolStripGalleryViews.Items.Cast<ToolStripGalleryItem>().Where(item => ((IntradaySetting)item.Tag).Name.Equals(name)))
				return item;
			return teleoptiToolStripGalleryViews.Items[0];
		}

		private void gridlinesInChartSettingsLineInChartSettingsChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_intradayViewContent.LineInChartSettingsChanged(sender, e);
		}

		private void gridrowInChartSettingLineInChartEnabledChanged(object sender, GridlineInChartButtonEventArgs e)
		{
			_intradayViewContent.LineInChartEnabledChanged(e, _gridrowInChartSetting);
		}

		private void intradayViewFormClosing(object sender, FormClosingEventArgs e)
		{
			if (_forceClose) return;
			if (!ControlBox)
			{
				e.Cancel = true;
				return;
			}

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
			var view = ((IntradaySetting)teleoptiToolStripGalleryViews.CheckedItem.Tag).Name;

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

		private void toolStripButtonMainSaveClick(object sender, EventArgs e)
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

		public void DisableSave()
		{
			toolStripButtonMainSave.Enabled = false;
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
			if (_intradayViewContent != null)
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
			toolStripButtonMainSave.Enabled = true;
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

		private void backgroundWorkerLoadControlsDoWork(object sender, DoWorkEventArgs e)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
			Presenter.Initialize();
		}

		private void backgroundWorkerLoadControlsRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (Disposing || IsDisposed) return;
			if (InvokeRequired)
			{
				MethodInvoker updateGui = () => backgroundWorkerLoadControlsRunWorkerCompleted(sender, e);
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

			_intradayViewContent = new IntradayViewContent(Presenter, this, _eventAggregator, Presenter.SchedulerStateHolder,
				_settingManager, _overriddenBusinessRulesHolder, _resourceOptimizationHelperExtended,
				_resourceCalculationContextFactory, _scheduleDayChangeCallback, _skillPriorityProvider);
			_intradayViewContent.RightToLeft = RightToLeft; //To solve error with wrong dock labels running RTL
			_timeNavigationControl.SetSelectedDate(Presenter.IntradayDate);
			Presenter.ExternalAgentStateReceived += presenterExternalAgentStateReceived;
			_eventAggregator.GetEvent<IntradayLoadProgress>().Publish(Resources.LoadingLayoutsThreeDots);
			initializeGallery();
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

			finalizeGallery();
			selectDefaultView();

			Presenter.LoadAgentStates();
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

			setupIntradayDate();
			authorizeFunctions();
		}

		private void toolStripButtonResetLayoutClick(object sender, EventArgs e)
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

		private void statusStripButtonServerUnavailableClick(object sender, EventArgs e)
		{
			statusStripButtonServerUnavailable.Enabled = false;
			Presenter.RetryHandlingMessages();
			statusStripButtonServerUnavailable.Enabled = true;
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
				using (var wizard = new WizardNoRoot<ReforecastModelCollection>(pages, _container))
				{
					if (wizard.ShowDialog(this) != DialogResult.OK) return;

					var principal = TeleoptiPrincipal.CurrentPrincipal;
					var person = ((ITeleoptiPrincipalForLegacy)principal).UnsafePerson();
					var @event = new RecalculateForecastOnSkillCollectionEvent
					{
						SkillCollection = new Collection<RecalculateForecastOnSkill>(),
						ScenarioId = Presenter.RequestedScenario.Id.GetValueOrDefault(),
						OwnerPersonId = person.Id.GetValueOrDefault()
					};
					foreach (var model in models.ReforecastModels)
					{
                        @event.SkillCollection.Add(
							new RecalculateForecastOnSkill
							{
								SkillId = model.Skill.Id.GetValueOrDefault(),
								WorkloadIds = new Collection<Guid>(model.Workload.Select(w => w.Id.GetValueOrDefault()).ToList())
							});

					}
                    _eventInfrastructureInfoPopulator.PopulateEventContext(@event);
                    _publisher.Publish(@event);
                }
			}
		}

		private void ribbonControlAdv1BeforeContextMenuOpen(object sender, ContextMenuEventArgs e)
		{
			e.Cancel = true;
		}

	}
}
