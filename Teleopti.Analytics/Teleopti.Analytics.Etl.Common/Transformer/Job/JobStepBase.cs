using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public abstract class JobStepBase : IJobStep, INotifyPropertyChanged, IDisposable
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(JobStepBase));
		private string _name = String.Empty;
		private DateTime _startDateTime;
		private IJobStepResult _jobStepResult;
		private JobCategoryType _jobCategory;
		private DataTable _bulkInsertDataTable1;
		private DataTable _bulkInsertDataTable2;
		internal IJobParameters _jobParameters;

		protected JobStepBase(IJobParameters jobParameters)
		{
			_name = string.Empty;
			_jobParameters = jobParameters;
			_jobCategory = JobCategoryType.DoNotNeedDatePeriod;
			ClearDataTablesAfterRun = true;
		}

		public bool ClearDataTablesAfterRun { get; set; }

		public string Name
		{
			get { return _name; }
			protected set { _name = value; }
		}

		public IJobParameters JobParameters
		{
			get { return _jobParameters; }
		}

		public JobCategoryType JobCategory
		{
			get { return _jobCategory; }
			protected set { _jobCategory = value; }
		}

		public void SetResult(IJobStepResult jobStepResult)
		{
			Result = jobStepResult;
		}

		public bool IsBusinessUnitIndependent { get; protected set; }

		public IJobMultipleDateItem JobCategoryDatePeriod
		{
			get
			{
				return _jobParameters.JobCategoryDates.GetJobMultipleDateItem(_jobCategory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IJobStepResult Run(IList<IJobStep> jobStepsNotToRun, IBusinessUnit currentBusinessUnit, IList<IJobResult> jobResultCollection, bool isLastBusinessUnitRun)
		{
			Result = new JobStepResult(Name, 0, 0, currentBusinessUnit, jobResultCollection);
			if (jobStepsNotToRun.Contains(this))
			{
				// job step already run by a prevous business unit
				Result.Status = "No need to run";
				return (IJobStepResult)Result.Clone();
			}

			StartTimer();
			Result.Status = "Running...";

			try
			{
				// need to add current bussiness unit here. 
				Result.RowsAffected = RunStep(jobResultCollection, isLastBusinessUnitRun);
				Result.Duration = StopTimer();
				Result.Status = "Done";
				if (IsBusinessUnitIndependent)
				{
					// Mark that this step don´t need to run for other business units
					jobStepsNotToRun.Add(this);
				}
			}
			catch (Exception ex)
			{
				double duration = StopTimer();
				_log.Warn(ex);
				Result = new JobStepResult(Name, duration, ex, currentBusinessUnit, jobResultCollection);
				Result.Status = "Error";
			}
			finally
			{
				ClearDataTables();
			}

			return (IJobStepResult)Result.Clone();
		}


		private void ClearDataTables()
		{
			if (!ClearDataTablesAfterRun)
				return;
			_bulkInsertDataTable1?.Clear();
			_bulkInsertDataTable2?.Clear();
		}

		public IJobStepResult Result
		{
			get
			{
				return _jobStepResult;
			}
			private set
			{
				_jobStepResult = value;
				FirePropertyChanged(nameof(Result));
			}
		}

		public DataTable BulkInsertDataTable1
		{
			get
			{
				if (_bulkInsertDataTable1 == null)
				{
					_bulkInsertDataTable1 = new DataTable();
					_bulkInsertDataTable1.Locale = Thread.CurrentThread.CurrentCulture;
				}

				return _bulkInsertDataTable1;
			}
			set { _bulkInsertDataTable1 = value; }
		}

		public DataTable BulkInsertDataTable2
		{
			get
			{
				if (_bulkInsertDataTable2 == null)
				{
					_bulkInsertDataTable2 = new DataTable();
					_bulkInsertDataTable2.Locale = Thread.CurrentThread.CurrentCulture;
				}

				return _bulkInsertDataTable2;
			}
			set { _bulkInsertDataTable2 = value; }
		}

		protected abstract int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit);

		protected void StartTimer()
		{
			_startDateTime = DateTime.Now;
		}

		protected Double StopTimer()
		{
			double duration = DateTime.Now.Subtract(_startDateTime).TotalMilliseconds;

			return duration;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected void FirePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_bulkInsertDataTable1 != null)
					_bulkInsertDataTable1.Dispose();
				if (_bulkInsertDataTable2 != null)
					_bulkInsertDataTable2.Dispose();
			}
		}
	}
}
