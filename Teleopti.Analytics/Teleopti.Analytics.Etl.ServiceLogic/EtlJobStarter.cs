using System;
using System.Globalization;
using System.Timers;
using Teleopti.Analytics.Etl.Transformer;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Etl.Common.Database;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Common.Database.EtlSchedules;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Transformer.Job;

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
			_timer.Stop();
			try
			{
				var configHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));
				if (!configHandler.IsConfigurationValid)
				{
					LogInvalidConfiguration(configHandler);
					return;
				}

				var rep = new Repository(_connectionString);
				IEtlLogCollection etlLogCollection = new EtlLogCollection(rep);
				IEtlScheduleCollection etlScheduleCollection = new EtlScheduleCollection(rep, etlLogCollection, _serviceStartTime);
				var schedulePriority = new SchedulePriority();
				var scheduleToRun = schedulePriority.GetTopPriority(etlScheduleCollection, DateTime.Now, _serviceStartTime);
				if (scheduleToRun != null)
				{
					var runJob = new RunJob(configHandler.BaseConfiguration.TimeZoneCode,
					                        configHandler.BaseConfiguration.IntervalLength.Value, _cube, _pmInstallation);
					runJob.Run(scheduleToRun, rep, _jobHelper);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + '\n' + ex.StackTrace);
			}
			finally
			{
				_timer.Start();
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
			               "ETL Service could not run any jobs due to invalid base configuration. Please start the manual ETL Tool and configure. (Culture: '{0}'; IntervalLengthMinutes: '{1}; TimeZoneCode: '{2}'.)",
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
				if (_jobHelper!=null)
				{
					_jobHelper.Dispose();
					_jobHelper = null;
				}
			}
		}
	}
}