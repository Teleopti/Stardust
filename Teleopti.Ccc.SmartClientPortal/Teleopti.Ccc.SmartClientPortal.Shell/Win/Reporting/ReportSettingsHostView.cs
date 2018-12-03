using System;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	public partial class ReportSettingsHostView : BaseUserControl, IReportSettingsHostView
	{
		private readonly ReportSettingsHostPresenter _presenter;
		private IEventAggregator _eventAggregator;
		private IComponentContext _componentContext;
		private IApplicationFunction _applicationFunction;

		public ReportSettingsHostView()
		{
			InitializeComponent();

			if (!StateHolderReader.IsInitialized || DesignMode) return;

			_presenter = new ReportSettingsHostPresenter(this);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!StateHolderReader.IsInitialized || DesignMode) return;

			SetTexts();

			reportHeader1.ShowSettings += reportHeader1ShowSettings;
			reportHeader1.HideSettings += reportHeader1HideSettings;
		}

		void reportHeader1HideSettings(object sender, EventArgs e)
		{
			Fold();
			_eventAggregator.GetEvent<ViewerFoldingChangedEvent>().Publish(true);
		}

		void reportHeader1ShowSettings(object sender, EventArgs e)
		{
			Unfold();
			_eventAggregator.GetEvent<ViewerFoldingChangedEvent>().Publish(false);
		}

		//used when call from tree
		public void ShowSettings(ReportDetail reportDetail)
		{
			_presenter.ShowSettings(reportDetail);   
		}

		//used when call from scheduler
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetupFromScheduler(ReportDetail reportDetail)
		{
			_presenter.HideSettingsAndSetReportHeader(reportDetail);
		}

		public void Unfold()
		{
			Height = 700;
		}

		public void Fold()
		{
			Height = 63;
		}

		public bool Unfolded()
		{
			return Height > 63;
		}

		public void ShowSpinningProgress(bool show)
		{
			reportHeader1.ShowSpinningProgress(show);
		}

		public void SetHeaderText(string text)
		{
			reportHeader1.HeaderText = text;
		}

		public void ReportHeaderCheckRightToLeft()
		{
			reportHeader1.CheckRightToLeft();
		}

		public void DisableShowSettings()
		{
			reportHeader1.DisableShowSettings();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IReportSettingsScheduleTimeVersusTargetTimeView GetSettingsForScheduleTimeVersusTargetTimeReport
		{
			get { return new ReportSettingsScheduleTimeVersusTargetTimeView(_eventAggregator,_componentContext); }
		}

		public  ReportSettingsScheduleTimeVersusTargetTimeModel ScheduleTimeVersusTargetSettingsModel
		{
			get { return _presenter.GetModelForScheduleTimeVersusTargetTimeReport; }
		}

		public void AddSettingsForScheduleTimeVersusTargetTimeReport(IReportSettingsScheduleTimeVersusTargetTimeView settingsScheduleTimeVersusTargetTimeView)
		{
			panelSettingsContainer.Controls.Add((ReportSettingsScheduleTimeVersusTargetTimeView)settingsScheduleTimeVersusTargetTimeView);	
		}

		public void SetReportFunctionCode(string functionCode)
		{
			reportHeader1.ReportFunctionCode = functionCode;
		}

		public void Init(IEventAggregator eventAggregator, IComponentContext componentContext, IApplicationFunction applicationFunction)
		{
			_eventAggregator = eventAggregator;
			_componentContext = componentContext;
			_applicationFunction = applicationFunction;
			eventSubscriptions();
		}

		private void eventSubscriptions()
		{
			_eventAggregator.GetEvent<LoadReport>().Subscribe(onLoadReport);
		}

		private void onLoadReport(bool obj)
		{
			Fold();
			reportHeader1.ToggleShowHideBoxes();
		}
	}
}
