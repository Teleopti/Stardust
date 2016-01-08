using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using Autofac;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using Timer = System.Timers.Timer;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	public class EtlJobStarter : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EtlJobStarter));

		private readonly string _connectionString;
		private readonly string _cube;
		private readonly string _pmInstallation;
		private readonly IContainer _container;
		private JobHelper _jobHelper;
		private readonly Timer _timer;
		private DateTime _serviceStartTime;

		public event EventHandler NeedToStopService;
		
		public EtlJobStarter()
		{
			XmlConfigurator.Configure();

			try
			{
				_connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
				_cube = ConfigurationManager.AppSettings["cube"];
				_pmInstallation = ConfigurationManager.AppSettings["pmInstallation"];
				_container = configureContainer();
				_jobHelper = new JobHelper(
					_container.Resolve<ILoadAllTenants>(),
					_container.Resolve<ITenantUnitOfWork>(),
					_container.Resolve<IAvailableBusinessUnitsProvider>());
				_timer = new Timer(10000);
				_timer.Elapsed += tick;
			}
			catch (Exception ex)
			{
				log.Error("The service could not be started", ex);
				throw;
			}
		}

		public void Start()
		{
			log.Info("The service is starting.");
			_timer.Start();
			_serviceStartTime = DateTime.Now;
			log.Info("The service started at " + _serviceStartTime);
		}

		void tick(object sender, ElapsedEventArgs e)
		{
			var isStopping = false;
			try
			{
                log.Debug("Tick");
                _timer.Stop();
                log.Debug("Timer stopped");

				var success = checkForEtlJob();

				isStopping = !success;
			}
			catch (Exception ex)
			{
				log.Error("Exception occurred in tick", ex);
			}
			finally
			{
			    try
			    {
                    log.DebugFormat("Stopping: {0}", isStopping);
			        if (!isStopping)
			        {
                        log.Debug("Starting timer");
			            _timer.Start();
                        log.Debug("Timer started");
                    }
			    }
			    catch (Exception ex)
			    {
			        log.Error(ex);
			        throw;
			    }
			}
		}

		private bool checkForEtlJob()
		{
			log.Debug("Checking configuration");
			var configHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));
			if (!configHandler.IsConfigurationValid)
			{
				log.Debug("Configuration not valid");
				logInvalidConfiguration(configHandler);
				NeedToStopService(this, null);
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
				culture = CultureInfo.GetCultureInfo(configHandler.BaseConfiguration.CultureId.Value).FixPersianCulture();
			Thread.CurrentThread.CurrentCulture = culture;

			log.Debug("Extracting job to run from schedule");
			var jobToRun = JobExtractor.ExtractJobFromSchedule(
				scheduleToRun,
				_jobHelper,
				configHandler.BaseConfiguration.TimeZoneCode,
				configHandler.BaseConfiguration.IntervalLength.Value,
				_cube,
				_pmInstallation,
				_container,
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

					var jobHelper = jobToRun.JobParameters.Helper;
					jobHelper.RefreshTenantList();
					var tenantNames = jobHelper.TenantCollection;

					foreach (var tenantName in tenantNames)
					{
						var tenantBaseConfig = TenantHolder.Instance.TenantBaseConfigs.SingleOrDefault(x => x.Tenant.Name.Equals(tenantName.DataSourceName));
						if (tenantBaseConfig == null || tenantBaseConfig.BaseConfiguration == null)
							return false;
						jobToRun.StepList[0].JobParameters.SetTenantBaseConfigValues(tenantBaseConfig.BaseConfiguration);

						var jobResultCollection = new List<IJobResult>();
						jobHelper.SelectDataSourceContainer(tenantName.DataSourceName);

						var jobRunner = new JobRunner();
						var jobResults = jobRunner.Run(jobToRun, jobResultCollection, jobStepsNotToRun);
						if (jobResults == null)
						{
							// No license applied - stop service
							logInvalidLicense(tenantName.DataSourceName);
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
			log.WarnFormat("ETL Service could not run for tenant {0}. Please log on to that tenant and apply a license in the main client.",tenant);
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

		private static IContainer configureContainer()
		{
			var builder = new ContainerBuilder();
			var iocArgs = new IocArgs(new ConfigReader());
			var configuration = new IocConfiguration(
						  iocArgs,
						  CommonModule.ToggleManagerForIoc(iocArgs));

			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new EtlModule(configuration));
			return builder.Build();
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
				log.Info("The service is stopping.");
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