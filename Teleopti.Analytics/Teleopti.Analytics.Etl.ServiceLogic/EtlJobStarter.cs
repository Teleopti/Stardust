using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Transformer.Job;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	public class EtlJobStarter : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EtlJobStarter));

		private readonly string _connectionString;
		private readonly string _cube;
		private readonly string _pmInstallation;

		private JobHelper _jobHelper;
		private readonly Timer _timer;
		private DateTime _serviceStartTime;

		public event EventHandler NeedToStopService;

		public EtlJobStarter(string connectionString, string cube, string pmInstallation)
		{
			XmlConfigurator.Configure();

			try
			{
				_connectionString = connectionString;
				_cube = cube;
				_pmInstallation = pmInstallation;
				_jobHelper = new JobHelper();
				_timer = new Timer(10000);
				_timer.Elapsed += Tick;
			}
			catch (Exception ex)
			{
				Log.Error("The service could not be started due to the following error:" + '\n' + ex.Message + '\n' + ex.StackTrace);
				throw;
			}
		}

		public void Start()
		{
			Log.Info("The service is starting.");
			_timer.Start();
			_serviceStartTime = DateTime.Now;
			Log.Info("The service start time is:" + _serviceStartTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void Tick(object sender, ElapsedEventArgs e)
		{
			bool isStopping = false;
			_timer.Stop();
			try
			{
				var configHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));
				if (!configHandler.IsConfigurationValid)
				{
					LogInvalidConfiguration(configHandler);
					NeedToStopService(this, null);
					isStopping = true;
					return;
				}

				var rep = new Repository(_connectionString);
				IEtlJobLogCollection etlJobLogCollection = new EtlJobLogCollection(rep);
				IEtlJobScheduleCollection etlJobScheduleCollection = new EtlJobScheduleCollection(rep, etlJobLogCollection, _serviceStartTime);
				var schedulePriority = new SchedulePriority();
				var scheduleToRun = schedulePriority.GetTopPriority(etlJobScheduleCollection, DateTime.Now, _serviceStartTime);
				if (scheduleToRun != null)
				{
					IJob jobToRun = JobExtractor.ExtractJobFromSchedule(scheduleToRun, _jobHelper,
																		configHandler.BaseConfiguration.TimeZoneCode,
																		configHandler.BaseConfiguration.IntervalLength.Value, _cube,
																		_pmInstallation);
					RunJob(jobToRun, scheduleToRun.ScheduleId, rep, _jobHelper.BusinessUnitCollection);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + '\n' + ex.StackTrace);
			}
			finally
			{
				if (!isStopping) 
					_timer.Start();
			}
		}

		private void RunJob(IJob jobToRun, int scheduleId, IJobLogRepository repository, IList<IBusinessUnit> businessUnitCollection)
		{
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			IList<IJobResult> jobResultCollection = new List<IJobResult>();
			IRunController runController = new RunController((IRunControllerRepository)repository);
			IEtlRunningInformation etlRunningInformation;
			if (runController.CanIRunAJob(out etlRunningInformation))
			{
				using (var etlJobLock = new EtlJobLock(_connectionString))
				{
					Log.InfoFormat(CultureInfo.InvariantCulture, "Scheduled job '{0}' ready to start.", jobToRun.Name);
					runController.StartEtlJobRunLock(jobToRun.Name, true, etlJobLock);
					IJobRunner jobRunner = new JobRunner();
					IList<IBusinessUnit> businessUnits = businessUnitCollection;
					IList<IJobResult> jobResults = jobRunner.Run(jobToRun, businessUnits, jobResultCollection, jobStepsNotToRun);
					jobRunner.SaveResult(jobResults, repository, scheduleId);	
				}
			}
			else
			{
				Log.WarnFormat(CultureInfo.InvariantCulture,
							   "Scheduled job '{0}' could not start due to another job is running at the moment. (ServerName: {1}; JobName: {2}; StartTime: {3}; IsStartByService: {4})",
							   jobToRun.Name, etlRunningInformation.ComputerName, etlRunningInformation.JobName,
							   etlRunningInformation.StartTime, etlRunningInformation.IsStartedByService);
			}
		}

		private static void LogInvalidConfiguration(ConfigurationHandler configHandler)
		{
			var culture = configHandler.BaseConfiguration.CultureId.HasValue
							? configHandler.BaseConfiguration.CultureId.Value.ToString(CultureInfo.InvariantCulture)
							: "null";
			var intervalLength = configHandler.BaseConfiguration.IntervalLength.HasValue
									? configHandler.BaseConfiguration.IntervalLength.Value.ToString(CultureInfo.InvariantCulture)
									: "null";
			var timeZone = configHandler.BaseConfiguration.TimeZoneCode ?? "null";
			Log.WarnFormat(CultureInfo.InvariantCulture,
						   "ETL Service was stopped due to invalid base configuration (Culture: '{0}'; IntervalLengthMinutes: '{1}; TimeZoneCode: '{2}'). Please start the manual ETL Tool and configure. Then start the service again.",
						   culture, intervalLength, timeZone);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Log.Info("The service is stopping.");
				if (_timer != null)
				{
					_timer.Stop();
					_timer.Dispose();
				}
				if (_jobHelper != null)
				{
					_jobHelper.Dispose();
					_jobHelper = null;
				}
			}
		}
	}
}