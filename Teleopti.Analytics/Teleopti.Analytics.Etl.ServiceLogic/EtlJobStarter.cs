using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using Timer = System.Timers.Timer;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	public class EtlJobStarter : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EtlJobStarter));

		private readonly string _connectionString;
		private readonly string _cube;
		private readonly string _pmInstallation;
		private readonly IContainer _container;
		private JobHelper _jobHelper;
		private readonly Timer _timer;
		private DateTime _serviceStartTime;

		public event EventHandler NeedToStopService;

		public EtlJobStarter(EtlConfigReader configReader)
		{
			XmlConfigurator.Configure();

			try
			{
				var config = configReader.Read();
				_connectionString = config.ConnectionString;
				_cube = config.Cube;
				_pmInstallation = config.PmInstallation;
				_container = configureContainer();
				_jobHelper = new JobHelper(_container.Resolve<ILoadAllTenants>(), _container.Resolve<ITenantUnitOfWork>(),
					_container.Resolve<IAvailableBusinessUnitsProvider>());
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
			try
			{
                Log.Debug("Tick");
                _timer.Stop();
                Log.Debug("Timer stopped");

                Log.Debug("Checking configuration");
                var configHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));
				if (!configHandler.IsConfigurationValid)
				{
                    Log.Debug("Configuration not valid");
					LogInvalidConfiguration(configHandler);
					NeedToStopService(this, null);
					isStopping = true;
					return;
				}
                Log.Debug("Configuration OK");

                Log.Debug("Getting scheduled schedule");
                var rep = new Repository(_connectionString);
				IEtlJobLogCollection etlJobLogCollection = new EtlJobLogCollection(rep);
				IEtlJobScheduleCollection etlJobScheduleCollection = new EtlJobScheduleCollection(rep, etlJobLogCollection, _serviceStartTime);

                Log.Debug("Getting due schedule");
                var schedulePriority = new SchedulePriority();
                var scheduleToRun = schedulePriority.GetTopPriority(etlJobScheduleCollection, DateTime.Now, _serviceStartTime);
			    if (scheduleToRun == null)
			    {
                    Log.Debug("No due schedule");
                    return;
			    }

                Log.DebugFormat("Schedule to run {0}, Job: {1}", scheduleToRun.ScheduleName, scheduleToRun.JobName);
                CultureInfo culture = CultureInfo.CurrentCulture;
				if (configHandler.BaseConfiguration.CultureId.HasValue)
					culture = CultureInfo.GetCultureInfo(configHandler.BaseConfiguration.CultureId.Value).FixPersianCulture();
				Thread.CurrentThread.CurrentCulture = culture;
                Log.Debug("Extracting job to run from schedule");
				IJob jobToRun = JobExtractor.ExtractJobFromSchedule(
					scheduleToRun, _jobHelper, configHandler.BaseConfiguration.TimeZoneCode,
					configHandler.BaseConfiguration.IntervalLength.Value, _cube,
					_pmInstallation, _container,
					configHandler.BaseConfiguration.RunIndexMaintenance,
					culture
					);
                Log.DebugFormat("Running job {0}", jobToRun.Name);
				isStopping = !RunJob(jobToRun, scheduleToRun.ScheduleId, rep);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + '\n' + ex.StackTrace);
			}
			finally
			{
			    try
			    {
                    Log.DebugFormat("Stopping = {0}", isStopping);
			        if (!isStopping)
			        {
                        Log.Debug("Starting timer");
			            _timer.Start();
                        Log.Debug("Timer started");
                    }
			    }
			    catch (Exception ex)
			    {
			        Log.Error(ex);
			        throw;
			    }
			}
		}

		private bool RunJob(IJob jobToRun, int scheduleId, IJobLogRepository repository)
		{
			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			IRunController runController = new RunController((IRunControllerRepository)repository);
			IEtlRunningInformation etlRunningInformation;
            Log.Debug("Checking if permitted to run");
			if (runController.CanIRunAJob(out etlRunningInformation))
			{
                Log.Debug("Trying to aquire lock");
				using (var etlJobLock = new EtlJobLock(_connectionString))
				{
					Log.InfoFormat(CultureInfo.InvariantCulture, "Scheduled job '{0}' ready to start.", jobToRun.Name);
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


						IList<IJobResult> jobResultCollection = new List<IJobResult>();
						jobHelper.SelectDataSourceContainer(tenantName.DataSourceName);
						IJobRunner jobRunner = new JobRunner();
						IList<IJobResult> jobResults = jobRunner.Run(jobToRun, jobResultCollection, jobStepsNotToRun);
						if (jobResults == null)
						{
							// No license applied - stop service
							LogInvalidLicense(tenantName.DataSourceName);
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
				LogConflictingEtlRun(jobToRun, etlRunningInformation);
			}

			return true;
		}

		private static void LogConflictingEtlRun(IJob jobToRun, IEtlRunningInformation etlRunningInformation)
		{
			Log.WarnFormat(CultureInfo.InvariantCulture,
							   "Scheduled job '{0}' could not start due to another job is running at the moment. (ServerName: {1}; JobName: {2}; StartTime: {3}; IsStartByService: {4})",
							   jobToRun.Name, etlRunningInformation.ComputerName, etlRunningInformation.JobName,
							   etlRunningInformation.StartTime, etlRunningInformation.IsStartedByService);
		}

		private static void LogInvalidLicense(string tenant)
		{
			Log.WarnFormat("ETL Service could not run for tenant {0}. Please log on to that tenant and apply a license in the main client.",tenant);
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
	}

	public class EtlConfigReader
	{
		public EtlConfig Read()
		{
			var connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
			var cube = ConfigurationManager.AppSettings["cube"];
			var pmInstallation = ConfigurationManager.AppSettings["pmInstallation"];

			return new EtlConfig { ConnectionString = connectionString, Cube = cube, PmInstallation = pmInstallation };
		}
	}

	public struct EtlConfig
	{
		public string ConnectionString;
		public string Cube;
		public string PmInstallation;
	}

}