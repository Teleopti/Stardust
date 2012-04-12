using System;
using System.Collections.Generic;
using System.Globalization;
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
		public void Run(IEtlSchedule etlScheduleToRun, ILogRepository repository, JobHelper jobHelper)
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

			bool firstBuRun = true;
			IList<IBusinessUnit> businessUnitCollection = jobHelper.BusinessUnitCollection;
			// Iterate through bu list and run job for each bu.
			if (businessUnitCollection != null && businessUnitCollection.Count > 0)
			{
				IBusinessUnit lastBuToRun = businessUnitCollection[businessUnitCollection.Count - 1];
				foreach (IBusinessUnit businessUnit in jobHelper.BusinessUnitCollection)
				{
					DateTime startTime = DateTime.Now;
					IJobResult jobResult = jobToRun.Run(businessUnit, jobStepsNotToRun, jobResultCollection, firstBuRun, businessUnit.Id == lastBuToRun.Id);
					jobResult.EndTime = DateTime.Now;
					jobResult.StartTime = startTime;

					jobResultCollection.Add(jobResult);
					firstBuRun = false;
				}
			}

			if (jobResultCollection.Count > 0)
			{
				foreach (IJobResult jobResult in jobResultCollection)
				{
					IEtlLog etlLogItem = new EtlLog(repository);
					etlLogItem.Init(etlScheduleToRun.ScheduleId, jobResult.StartTime, jobResult.EndTime);

					foreach (var runLoadStepInfo in jobResult.JobStepResultCollection)
					{
						etlLogItem.PersistJobStep(runLoadStepInfo);
					}

					etlLogItem.Persist(jobResult);
				}
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