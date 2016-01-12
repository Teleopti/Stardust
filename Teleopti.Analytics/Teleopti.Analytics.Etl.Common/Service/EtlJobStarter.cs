using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using log4net;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlJobStarter
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlJobStarter));

		private readonly string _connectionString;
		private readonly string _cube;
		private readonly string _pmInstallation;
		private readonly JobHelper _jobHelper;
		private readonly JobExtractor _jobExtractor;
		private readonly Tenants _tenants;
		private DateTime _serviceStartTime;
		private Action _stopService;

		public EtlJobStarter(
			JobHelper jobHelper, 
			JobExtractor jobExtractor,
			Tenants tenants)
		{
			_jobHelper = jobHelper;
			_jobExtractor = jobExtractor;
			_tenants = tenants;
			_connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
			_cube = ConfigurationManager.AppSettings["cube"];
			_pmInstallation = ConfigurationManager.AppSettings["pmInstallation"];
		}

		public void Initialize(DateTime serviceStartTime, Action stopService)
		{
			_serviceStartTime = serviceStartTime;
			_stopService = stopService;
		}

		public bool Tick()
		{
			return checkForEtlJob();
		}

		private bool checkForEtlJob()
		{
			log.Debug("Checking configuration");
			var configHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));
			if (!configHandler.IsConfigurationValid)
			{
				log.Debug("Configuration not valid");
				logInvalidConfiguration(configHandler);
				_stopService();
				return false;
			}

			log.Debug("Configuration OK");

			log.Debug("Getting scheduled schedule");
			var repository = new Repository(_connectionString);
			var etlJobScheduleCollection =
				new EtlJobScheduleCollection(
					repository,
					new EtlJobLogCollection(repository),
					_serviceStartTime);

			log.Debug("Getting due schedule");
			var scheduleToRun = new SchedulePriority().GetTopPriority(etlJobScheduleCollection, DateTime.Now, _serviceStartTime);
			if (scheduleToRun == null)
			{
				log.Debug("No due schedule");
				return true;
			}

			log.DebugFormat("Schedule to run {0}, Job: {1}", scheduleToRun.ScheduleName, scheduleToRun.JobName);

			var culture = CultureInfo.CurrentCulture;
			if (configHandler.BaseConfiguration.CultureId.HasValue)
				culture = PersianCultureHelper.FixPersianCulture(CultureInfo.GetCultureInfo(configHandler.BaseConfiguration.CultureId.Value));
			Thread.CurrentThread.CurrentCulture = culture;

			log.Debug("Extracting job to run from schedule");
			var jobToRun = _jobExtractor.ExtractJobFromSchedule(
				scheduleToRun,
				_jobHelper,
				configHandler.BaseConfiguration.TimeZoneCode,
				configHandler.BaseConfiguration.IntervalLength.Value,
				_cube,
				_pmInstallation,
				configHandler.BaseConfiguration.RunIndexMaintenance,
				culture
				);

			log.DebugFormat("Running job {0}", jobToRun.Name);
			var success = runEtlJob(jobToRun, scheduleToRun.ScheduleId, repository);
			return success;
		}

		private bool runEtlJob(IJob jobToRun, int scheduleId, IJobLogRepository repository)
		{
			var jobStepsNotToRun = new List<IJobStep>();
			var runController = new RunController((IRunControllerRepository)repository);

			log.Debug("Checking if permitted to run");
			IEtlRunningInformation etlRunningInformation;
			if (runController.CanIRunAJob(out etlRunningInformation))
			{
				log.Debug("Trying to aquire lock");
				using (var etlJobLock = new EtlJobLock(_connectionString))
				{
					log.InfoFormat(CultureInfo.InvariantCulture, "Scheduled job '{0}' ready to start.", jobToRun.Name);
					runController.StartEtlJobRunLock(jobToRun.Name, true, etlJobLock);

					foreach (var tenantName in _tenants.CurrentTenants())
					{
						if (tenantName.BaseConfiguration == null)
						{
							//we can't stop service now beacuse one tenant isnt configured correctly, just try next
							//return false;
							continue;
						}
						jobToRun.StepList[0].JobParameters.SetTenantBaseConfigValues(tenantName.BaseConfiguration);

						var jobResultCollection = new List<IJobResult>();
						_jobHelper.SelectDataSourceContainer(tenantName.Name);

						var jobRunner = new JobRunner();
						var jobResults = jobRunner.Run(jobToRun, jobResultCollection, jobStepsNotToRun);
						if (jobResults == null)
						{
							// No license applied - stop service
							logInvalidLicense(tenantName.Name);
							//we can't stop service now beacuse one tenant don't have a License, just try next
							//NeedToStopService(this, null);
							//return false;
							continue;
						}
						jobRunner.SaveResult(jobResults, repository, scheduleId);
					}
				}
			}
			else
			{
				logConflictingEtlRun(jobToRun, etlRunningInformation);
			}

			return true;
		}

		private static void logConflictingEtlRun(IJob jobToRun, IEtlRunningInformation etlRunningInformation)
		{
			log.WarnFormat(CultureInfo.InvariantCulture,
				"Scheduled job '{0}' could not start due to another job is running at the moment. (ServerName: {1}; JobName: {2}; StartTime: {3}; IsStartByService: {4})",
				jobToRun.Name,
				etlRunningInformation.ComputerName,
				etlRunningInformation.JobName,
				etlRunningInformation.StartTime,
				etlRunningInformation.IsStartedByService);
		}

		private static void logInvalidLicense(string tenant)
		{
			log.WarnFormat("ETL Service could not run for tenant {0}. Please log on to that tenant and apply a license in the main client.", tenant);
		}

		private static void logInvalidConfiguration(ConfigurationHandler configHandler)
		{
			var culture = configHandler.BaseConfiguration.CultureId.HasValue
				? configHandler.BaseConfiguration.CultureId.Value.ToString(CultureInfo.InvariantCulture)
				: "null";
			var intervalLength = configHandler.BaseConfiguration.IntervalLength.HasValue
				? configHandler.BaseConfiguration.IntervalLength.Value.ToString(CultureInfo.InvariantCulture)
				: "null";
			var timeZone = configHandler.BaseConfiguration.TimeZoneCode ?? "null";
			log.WarnFormat(CultureInfo.InvariantCulture,
				"ETL Service was stopped due to invalid base configuration (Culture: '{0}'; IntervalLengthMinutes: '{1}; TimeZoneCode: '{2}'). Please start the manual ETL Tool and configure. Then start the service again.",
				culture, intervalLength, timeZone);
		}
	}
}