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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class EtlJobStarter
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlJobStarter));
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private readonly PmInfoProvider _pmInfoProvider;

		private readonly string _connectionString;
		private readonly JobExtractor _jobExtractor;
		private readonly JobHelper _jobHelper;

		private readonly ITenants _tenants;
		private DateTime _serviceStartTime;
		private Action _stopService;

		public EtlJobStarter(
			JobHelper jobHelper,
			JobExtractor jobExtractor,
			ITenants tenants,
			IBaseConfigurationRepository baseConfigurationRepository,
			PmInfoProvider pmInfoProvider)
		{
			_jobHelper = jobHelper;
			_jobExtractor = jobExtractor;
			_tenants = tenants;
			_baseConfigurationRepository = baseConfigurationRepository;
			_pmInfoProvider = pmInfoProvider;
			_connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
			
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
			var configHandler = new ConfigurationHandler(new GeneralFunctions(new GeneralInfrastructure(_baseConfigurationRepository)));
			configHandler.SetConnectionString(_connectionString);
			if (!configHandler.IsConfigurationValid)
			{
				log.Debug("Configuration not valid");
				logInvalidConfiguration(configHandler);
				_stopService();
				return false;
			}

			log.Debug("Configuration OK");

			log.Debug("Getting scheduled schedule");
			var jobScheduleRepository =  new JobScheduleRepository();
			jobScheduleRepository.SetDataMartConnectionString(_connectionString);
			var repository = new Repository(_connectionString);
			var etlJobScheduleCollection =
				 new EtlJobScheduleCollection(
					 jobScheduleRepository,
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
				culture = CultureInfo.GetCultureInfo(configHandler.BaseConfiguration.CultureId.Value).FixPersianCulture();
			Thread.CurrentThread.CurrentCulture = culture;

			var cube = _pmInfoProvider.Cube();
			var pmInstallation = _pmInfoProvider.PmInstallation();
			log.Debug("Extracting job to run from schedule");
			var jobToRun = _jobExtractor.ExtractJobFromSchedule(
				 scheduleToRun,
				 _jobHelper,
				 configHandler.BaseConfiguration.TimeZoneCode,
				 configHandler.BaseConfiguration.IntervalLength.GetValueOrDefault(),
				 cube,
				 pmInstallation,
				 configHandler.BaseConfiguration.RunIndexMaintenance,
				 culture
				 );

			log.DebugFormat("Running job {0}", jobToRun.Name);
			var success = runEtlJob(jobToRun, scheduleToRun, repository, jobScheduleRepository);
			return success;
		}

		private bool runEtlJob(IJob jobToRun, IEtlJobSchedule scheduleJob, IJobLogRepository repository, IJobScheduleRepository jobScheduleRepository)
		{
			var runController = new RunController((IRunControllerRepository)repository);

			log.Debug("Checking if permitted to run");
			if (runController.CanIRunAJob(out var etlRunningInformation))
			{
				try
				{
					log.Debug("Trying to acquire lock");
					using (new EtlJobLock(_connectionString, jobToRun.Name, true))
					{
						log.InfoFormat(CultureInfo.InvariantCulture, "Scheduled job '{0}' ready to start.", jobToRun.Name);

						var etlTenantName = scheduleJob.TenantName;
						var etlTenants = Tenants.IsAllTenants(etlTenantName)
							? _tenants.EtlTenants().ToList()
							: new List<TenantInfo> {_tenants.Tenant(etlTenantName)};

						foreach (var tenant in etlTenants)
						{
							if (tenant == null) continue;

							var jobStepsNotToRun = new List<IJobStep>();
							jobToRun.StepList[0].JobParameters.SetTenantBaseConfigValues(tenant.EtlConfiguration);

							var jobResultCollection = new List<IJobResult>();
							_jobHelper.SelectDataSourceContainer(tenant.Name);

							var jobRunner = new JobRunner();
							var jobResults = jobRunner.Run(jobToRun, jobResultCollection, jobStepsNotToRun);
							if (jobResults != null && jobResults.Any())
							{
								var exception = jobResults.First().JobStepResultCollection.First().JobStepException;
								if (exception != null && exception.Message.Contains("license"))
									logInvalidLicense(tenant.Name);
							}
							jobRunner.SaveResult(jobResults, repository, scheduleJob.ScheduleId);
						}

						if (scheduleJob.ScheduleType == JobScheduleType.Manual)
						{
							jobScheduleRepository.DisableScheduleJob(scheduleJob.ScheduleId);
						}
					}
				}
				catch (DistributedLockException)
				{
					if (!runController.CanIRunAJob(out var runningEtlJob))
					{
						logConflictingEtlRun(jobToRun, runningEtlJob);
					}
					else
					{
						log.InfoFormat("Distributed lock could not created. Job '{0}' could not be started.", jobToRun.Name);
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
			log.InfoFormat(CultureInfo.InvariantCulture,
				 "Scheduled job '{0}' could not start due to another job is running at the moment. (ServerName: {1}; JobName: {2}; StartTime: {3}; IsStartByService: {4})",
				 jobToRun.Name,
				 etlRunningInformation.ComputerName,
				 etlRunningInformation.JobName,
				 etlRunningInformation.StartTime,
				 etlRunningInformation.IsStartedByService);
		}

		private static void logInvalidLicense(string tenant)
		{
			log.WarnFormat(
				"ETL Service could not run for tenant {0}. Please log on to that tenant and apply a license in the main client.",
				tenant);
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