using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class SchedulerReportsViewer : BaseDialogForm
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IComponentContext _componentContext;
		private readonly IApplicationFunction _applicationFunction;
		private ReportDetail _reportDetail;
		private readonly BackgroundWorker _backgroundWorkerLoadReport = new BackgroundWorker();
		private readonly CultureInfo _currentCulture;

		public SchedulerReportsViewer(IEventAggregator eventAggregator, IComponentContext componentContext,
			IApplicationFunction applicationFunction)
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
			reportSettings1.Init(_eventAggregator, _componentContext, _applicationFunction);
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
				refreshScheduleTimeVersusTarget();
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view =
					new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
			}
		}

		void backgroundWorkerLoadReportDoWork(object sender, DoWorkEventArgs e)
		{
			setThreadCulture();
			e.Result = getReportDataForScheduledTimeVersusTarget(
				e.Argument as ReportSettingsScheduleTimeVersusTargetTimeModel);
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

			if (e.Result is ReportDataPackage<IScheduledTimeVersusTargetTimeReportData> reportDataPackage3)
			{
				reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage3);
			}

			_eventAggregator.GetEvent<LoadReportDone>().Publish(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private bool rethrowBackgroundException(RunWorkerCompletedEventArgs e)
		{
			if (e.Error == null) return false;

			var dataSourceException = e.Error as DataSourceException;
			if (dataSourceException == null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}

			using (var view =
				new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
			{
				view.ShowDialog();
				_eventAggregator.GetEvent<LoadReportDone>().Publish(true);
				return true;
			}
		}

		private void refreshScheduleTimeVersusTarget()
		{
			var reportDataPackage =
				getReportDataForScheduledTimeVersusTarget(
					getReportSettingsModel() as ReportSettingsScheduleTimeVersusTargetTimeModel);
			reportViewerControl1.LoadReport(_reportDetail.File, reportDataPackage);
		}

		//from tree
		public void ShowSettings(ReportDetail reportDetail)
		{
			reportSettings1.ShowSettings(reportDetail);
			reportSettings1.Width = ClientRectangle.Width;
			_reportDetail = reportDetail;
		}

		private ReportDataPackage<IScheduledTimeVersusTargetTimeReportData> getReportDataForScheduledTimeVersusTarget(
			ReportSettingsScheduleTimeVersusTargetTimeModel model)
		{
			var data = new Dictionary<string, IList<IScheduledTimeVersusTargetTimeReportData>>
			{
				{"DataSet1", new List<IScheduledTimeVersusTargetTimeReportData>()}
			};

			var parameters = ReportHandler.CreateScheduledTimeVersusTargetParameters(model, _currentCulture);
			if (!model.Persons.Any())
			{
				return new ReportDataPackage<IScheduledTimeVersusTargetTimeReportData>(data, parameters, false);
			}

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var settingDataRepository = new RepositoryFactory().CreateGlobalSettingDataRepository(unitOfWork);
				var commonNameDescriptionSetting =
					settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());

				ReportHandler.CreateScheduledTimeVersusTargetData(unitOfWork, model, data,
					TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, commonNameDescriptionSetting);
			}

			return new ReportDataPackage<IScheduledTimeVersusTargetTimeReportData>(data, parameters, false);
		}
		
		private Object getReportSettingsModel()
		{
			return reportSettings1.ScheduleTimeVersusTargetSettingsModel;
		}

		private void releaseManagedResources()
		{
			if (reportViewerControl1?.ReportViewerControl1 != null)
			{
				reportViewerControl1.ReportViewerControl1.ReportRefresh -= reportViewerControlReportRefresh;
			}
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
