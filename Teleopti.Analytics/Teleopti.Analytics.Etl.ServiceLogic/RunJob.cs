using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Database;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using log4net;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	internal class RunJob
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(RunJob));
		private readonly string _timeZoneId;
		private readonly int _intervalLengthMinutes;
		private readonly string _cube;
		private readonly string _pmInstallation;

		public RunJob(string timeZoneId, int intervalLengthMinutes, string cube, string pmInstallation)
		{
			_timeZoneId = timeZoneId;
			_intervalLengthMinutes = intervalLengthMinutes;
			_cube = cube;
			_pmInstallation = pmInstallation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public void Run(IEtlSchedule etlScheduleToRun, ILogRepository repository, JobHelper jobHelper, string connectionString)
		{
			Log.Info("Starting schedule " + etlScheduleToRun.ScheduleName);

			IList<IJobStep> jobStepsNotToRun = new List<IJobStep>();
			IList<IJobResult> jobResultCollection = new List<IJobResult>();

			var jobParameters =
				new JobParameters(GetJobCategoryDatePeriods(etlScheduleToRun),
								  etlScheduleToRun.DataSourceId, _timeZoneId,
								  _intervalLengthMinutes,
								  _cube,
								  _pmInstallation,
								  CultureInfo.CurrentCulture)
					{
						Helper = jobHelper
					};

			var jobCollection = new JobCollection(jobParameters);

			IJob jobToRun = null;

			foreach (var job in jobCollection)
			{
				if (String.Compare(job.Name, etlScheduleToRun.JobName, StringComparison.Ordinal) == 0)
				{
					jobToRun = job;
				}
			}

			if (jobToRun == null) throw new Exception("Jobname in schedule does not exist");


			var runController = new RunController((IRunControllerRepository) repository);
			IEtlRunningInformation etlRunningInformation;
			if (runController.CanIRunAJob(out etlRunningInformation))
			{
				runController.StartEtlJobRunLock(jobToRun.Name, false, new EtlJobLock(connectionString));
				var jobRunner = new JobRunner();
				IList<IBusinessUnit> businessUnits = jobHelper.BusinessUnitCollection;
				IList<IJobResult> jobResults = jobRunner.Run(jobToRun, businessUnits, jobResultCollection, jobStepsNotToRun);
				jobRunner.SaveResult(jobResults, repository, etlScheduleToRun.ScheduleId);
				runController.Dispose();
			}
		}

		private IJobMultipleDate GetJobCategoryDatePeriods(IEtlSchedule etlSchedule)
		{
			IJobMultipleDate jobMultipleDate = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId));
			var today = DateTime.Today;

			foreach (IEtlJobRelativePeriod jobRelativePeriod in etlSchedule.RelativePeriodCollection)
			{
				jobMultipleDate.Add(today.AddDays(jobRelativePeriod.RelativePeriod.Minimum),
									today.AddDays(jobRelativePeriod.RelativePeriod.Maximum),
									jobRelativePeriod.JobCategory);
			}

			return jobMultipleDate;
		}
	}
}