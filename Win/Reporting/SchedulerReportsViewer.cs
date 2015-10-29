using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Linq;

namespace Teleopti.Ccc.Win.Reporting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class SchedulerReportsViewer : BaseDialogForm
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IComponentContext _componentContext;
		private readonly IApplicationFunction _applicationFunction;
		private ReportDetail _reportDetail;
		private readonly BackgroundWorker _backgroundWorkerLoadReport = new BackgroundWorker();
		private ScheduleViewBase _scheduleViewBase;
		private IScenario _scenario;
		private bool _openFromScheduler;
		private readonly CultureInfo _currentCulture;
		private const int maximumRows = 1000;

		public SchedulerReportsViewer(IEventAggregator eventAggregator, IComponentContext componentContext, IApplicationFunction applicationFunction)
		{
			_eventAggregator = eventAggregator;
			_componentContext = componentContext;
			_applicationFunction = applicationFunction;
			InitializeComponent();
			KeyPreview = true;
			_currentCulture = Thread.CurrentThread.CurrentCulture;

			if (!DesignMode)
				SetTexts();

			_backgroundWorkerLoadReport.DoWork += backgroundWorkerLoadReportDoWork;
			_backgroundWorkerLoadReport.RunWorkerCompleted += backgroundWorkerLoadReportRunWorkerCompleted;
			reportViewerControl1.ReportViewerControl1.ReportRefresh += reportViewerControlReportRefresh;
			Height = 900;
			Width = 1200;
			
			eventSubscriptions();
		}

		private void eventSubscriptions()
		{
			reportSettings1.Init(_eventAggregator, _componentContext,_applicationFunction);
			_eventAggregator.GetEvent<LoadReport>().Subscribe(onLoadReport);
			_eventAggregator.GetEvent<ViewerFoldingChangedEvent>().Subscribe(SetSize);
		}

		private void onLoadReport(bool obj)
		{
			Cursor = Cursors.WaitCursor;
			reportSettings1.ShowSpinningProgress(true);
			_backgroundWorkerLoadReport.RunWorkerAsync(getReportSettingsModel());
		}

		void reportViewerControlReportRefresh(object sender, CancelEventArgs e)
		{

			try
			{
				switch (_reportDetail.FunctionPath)
				{
					case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
						refreshScheduledTimePerActivity();
						break;
					case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:
						refreshScheduleAuditing();
						break;
					case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
						refreshScheduleTimeVersusTarget();
						break;
				}
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
			}
		}
		
		void backgroundWorkerLoadReportDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			switch (_reportDetail.FunctionPath)
			{
				case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
					e.Result = getReportDataForScheduleTimePerActivityReport(e.Argument as ReportSettingsScheduledTimePerActivityModel);
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:
					//NEW_AUDIT
					e.Result = getReportDataForScheduleAuditingReport(e.Argument as ReportSettingsScheduleAuditingModel);   
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
						e.Result = getReportDataForScheduledTimeVersusTarget(e.Argument as ReportSettingsScheduleTimeVersusTargetTimeModel);
					break;
			}
			
			Application.DoEvents();
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}

		void backgroundWorkerLoadReportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
			Cursor = Cursors.Default;
			reportSettings1.ShowSpinningProgress(false);
			if (rethrowBackgroundException(e)) return;

			switch (_reportDetail.FunctionPath)
			{
				case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
					var reportDataPackage1 = e.Result as ReportDataPackage<IReportData>;
					if (reportDataPackage1 != null)
						reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage1);
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:  
					var reportDataPackage2 = e.Result as ReportDataPackage<ScheduleAuditingReportData>;
					if (reportDataPackage2 != null)
					{
						reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage2);
						if (reportDataPackage2.LimitReached)
							MessageBox.Show(this, Resources.MaximumNumberOfReportRowsReached,
								Resources.NarrowToSeeAll, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
					var reportDataPackage3 = e.Result as ReportDataPackage<IScheduledTimeVersusTargetTimeReportData>;
					if(reportDataPackage3 != null)
						reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage3);
					break;
			}

			
			_eventAggregator.GetEvent<LoadReportDone>().Publish(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private bool rethrowBackgroundException(RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var dataSourceException = e.Error as DataSourceException;
				if (dataSourceException == null)
				{
					var ex = new Exception("Background thread exception", e.Error);
					throw ex;
				}
			
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
				{
					view.ShowDialog();
					_eventAggregator.GetEvent<LoadReportDone>().Publish(true);
					return true;
				}
			}

			return false;
		}

		private void refreshScheduleAuditing()
		{
			//NEW_AUDIT
			var reportDataPackage = getReportDataForScheduleAuditingReport(getReportSettingsModel() as ReportSettingsScheduleAuditingModel);
			reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage);
		}

		private void refreshScheduleTimeVersusTarget()
		{
			ReportDataPackage<IScheduledTimeVersusTargetTimeReportData> reportDataPackage = getReportDataForScheduledTimeVersusTarget(getReportSettingsModel() as ReportSettingsScheduleTimeVersusTargetTimeModel);
			reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage);
		}

		private void refreshScheduledTimePerActivity()
		{
			if (_openFromScheduler)
				ReportHandler.RefreshScheduleTimePerActivity(this, _reportDetail, _scheduleViewBase, _scenario, _currentCulture);
			else
			{
				ReportDataPackage<IReportData> reportDataPackage =
					getReportDataForScheduleTimePerActivityReport(getReportSettingsModel() as ReportSettingsScheduledTimePerActivityModel);
				reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage);
			}
		}


		//from scheduler
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void LoadScheduledTimePerActivityReport(ReportDetail reportDetail, IDictionary<string, IList<IReportData>> reportData, IList<IReportDataParameter> reportDataParameters, ScheduleViewBase scheduleViewBase, IScenario scenario)
		{
			var reportDataPackage = new ReportDataPackage<IReportData>(reportData, reportDataParameters, false);
			reportViewerControl1.LoadReport(reportDetail.File, reportDataPackage);
			setupSettings(reportDetail);
			_scheduleViewBase = scheduleViewBase;
			_scenario = scenario;
			_reportDetail = reportDetail;
			_openFromScheduler = true;
			reportSettings1.Fold();
			
		}

		//from scheduler
		private void setupSettings(ReportDetail reportDetail)
		{
			reportSettings1.SetupFromScheduler(reportDetail);
		}

		//from tree
		public void ShowSettings(ReportDetail reportDetail)
		{
			reportSettings1.ShowSettings(reportDetail);
			reportSettings1.Width = ClientRectangle.Width;
			_reportDetail = reportDetail;
		}

		private ReportDataPackage<IScheduledTimeVersusTargetTimeReportData> getReportDataForScheduledTimeVersusTarget(ReportSettingsScheduleTimeVersusTargetTimeModel model)
		{
			
			var data = new Dictionary<string, IList<IScheduledTimeVersusTargetTimeReportData>>
			{
				{"DataSet1", new List<IScheduledTimeVersusTargetTimeReportData>()}
			};
			var parameters = ReportHandler.CreateScheduledTimeVersusTargetParameters(model,_currentCulture);
			if (model.Persons.Any())
			{
				using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var settingDataRepository = new RepositoryFactory().CreateGlobalSettingDataRepository(unitOfWork);
					var commonNameDescriptionSetting = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());

					ReportHandler.CreateScheduledTimeVersusTargetData(unitOfWork, model, data, StateHolderReader.Instance.StateReader.UserTimeZone, commonNameDescriptionSetting);
				}
			}
			
			return new ReportDataPackage<IScheduledTimeVersusTargetTimeReportData>(data, parameters, false);
		}

		private ReportDataPackage<ScheduleAuditingReportData> getReportDataForScheduleAuditingReport(ReportSettingsScheduleAuditingModel model)
		{
			IList<ScheduleAuditingReportData> reportData;

			IScheduleHistoryReport rep;
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				 rep = new ScheduleHistoryReport(UnitOfWorkFactory.Current, TeleoptiPrincipal.CurrentPrincipal.Regional);

				if (model.ModifiedBy.Count > 1 || model.ModifiedBy.Count == 0)
				{
					reportData = rep.Report(model.ChangePeriod, model.SchedulePeriod, model.Agents, maximumRows).ToList();
				}
				else
				{
					reportData = rep.Report(model.ModifiedBy[0], model.ChangePeriod, model.SchedulePeriod, model.Agents, maximumRows).ToList();
				}
			}
			var data = new Dictionary<string, IList<ScheduleAuditingReportData>> { { "DataSet2", reportData } };
			var parameters = ReportHandler.CreateScheduleAuditingParameters(model, _currentCulture);

			return new ReportDataPackage<ScheduleAuditingReportData>(data, parameters, rep.LimitReached);
		}

		//from tree
		private ReportDataPackage<IReportData> getReportDataForScheduleTimePerActivityReport(ReportSettingsScheduledTimePerActivityModel model)
		{
			IScheduleDateTimePeriod period = new ScheduleDateTimePeriod(model.Period.ToDateTimePeriod(model.TimeZone));
			IPersonProvider personsProvider = new PersonsInOrganizationProvider(model.Persons)
												  {
													  DoLoadByPerson = true
												  };
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,
																											 false);
			var rep = new RepositoryFactory();
			var data = new Dictionary<string, IList<IReportData>>();

			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					//get these first so they are loaded
					rep.CreateActivityRepository(unitOfWork).LoadAll();
					unitOfWork.Reassociate(model.Persons);
					IScheduleDictionary dic = rep.CreateScheduleRepository(unitOfWork).FindSchedulesForPersons(
							period,
							model.Scenario,
							personsProvider,
							scheduleDictionaryLoadOptions,
							model.Persons);

					IList<IReportDataParameter> parameters = ReportHandler.CreateScheduleTimePerActivityParameters(model, _currentCulture);
					ReportHandler.CreateScheduleTimePerActivityData(dic, model, data);

					return new ReportDataPackage<IReportData>(data, parameters, false);
				}
				
			}
		}

		private Object getReportSettingsModel()
		{
			switch (_reportDetail.FunctionPath)
			{
				case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
					return reportSettings1.ScheduleTimePerActivitySettingsModel;
				case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:
					//NEW_AUDIT
					return reportSettings1.ScheduleAuditingModel;
				case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
					return reportSettings1.ScheduleTimeVersusTargetSettingsModel;
			}

			return null;
		}

		private void releaseManagedResources()
		{
			if (reportViewerControl1!=null && reportViewerControl1.ReportViewerControl1!=null)
			reportViewerControl1.ReportViewerControl1.ReportRefresh -= reportViewerControlReportRefresh;
		}

		private void schedulerReportsViewerResize(object sender, EventArgs e)
		{
			SetSize(true);
		}

		public void SetSize(bool arg)
		{
			if (reportSettings1.Unfolded()) reportSettings1.Unfold();
			var height = Height - 40;
			if (reportSettings1.Height > height && height > 0)
				reportSettings1.Height = height;
		}
	}
}
