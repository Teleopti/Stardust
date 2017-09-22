using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class JobBase : IJob
	{
		internal string _name = string.Empty;
		private readonly IList<IJobStep> _steplist = new List<IJobStep>();
		private IJobResult _jobResult;
		private bool _enabled;

		public event EventHandler<AlarmEventArgs> JobExecutionReady;
		public void NotifyJobExecutionReady()
		{
			// Notify that job exectution is ready. At least the Data Source config dialog is consumer.
			JobExecutionReady?.Invoke(this, new AlarmEventArgs(this));
		}

		private JobBase() { }

		public JobBase(IJobParameters jobParameters, IList<IJobStep> jobStep, String jobName, bool needsParameterDatePeriod, bool needsParameterDataSource)
			: this()
		{
			_name = jobName;
			_steplist = jobStep;
			JobParameters = jobParameters;
			NeedsParameterDatePeriod = needsParameterDatePeriod;
			NeedsParameterDataSource = needsParameterDataSource;
			_enabled = true;
		}

		public string Name
		{
			set
			{
				_name = value;
				FirePropertyChanged(nameof(Name));
			}
			get { return _name; }
		}

		public IList<IJobStep> StepList
		{
			get
			{
				return _steplist;
			}

		}

		public IJobResult Run(IBusinessUnit businessUnit, IList<IJobStep> jobStepsNotToRun, IList<IJobResult> jobResultCollection, bool isFirstBusinessUnitRun, bool isLastBusinessUnitRun)
		{
			clearJobStepResults(isFirstBusinessUnitRun);

			Result = new JobResult(businessUnit, jobResultCollection)
							{
								Name = Name,
								Status = string.Format(CultureInfo.CurrentCulture, "Running ({0})", businessUnit.Name),
								Success = true
							}; // Inject list of BUs
			update();

			if (JobParameters != null)
			{
				//Log onto Raptor domain
				if (!JobParameters.Helper.SetBusinessUnit(businessUnit))
				{
					Result.Status = string.Empty;
					Result.Success = true;
					update();
					return null;
				}

				JobParameters.StateHolder = new CommonStateHolder(JobParameters);
			}

			foreach (IJobStep step in StepList)
			{
				IJobStepResult jobStepResult = step.Run(jobStepsNotToRun, businessUnit, jobResultCollection, isLastBusinessUnitRun);

				Result.JobStepResultCollection.Add(jobStepResult);
				update();

				//Stop the Job if any JobStep goes wrong !!
				if (jobStepResult.JobStepException != null)
				{
					Result.Success = false;
					update();
					break;
				}
			}

			if (JobParameters != null) JobParameters.Helper.LogOffTeleoptiCccDomain();
			Result.Status = Result.Success ? "Done" : "Not completed due to error!";

			return Result;
		}

		private void update()
		{
			Result.Update();
			FirePropertyChanged(nameof(Result));
		}

		private void clearJobStepResults(bool firstRun)
		{
			foreach (IJobStep jobStep in StepList)
			{
				if (firstRun)
				{
					jobStep.SetResult(null);
				}
				else
				{
					if (jobStep.Result != null)
					{
						jobStep.Result.ClearResult();
						FirePropertyChanged(nameof(Result));
					}
				}
			}
		}


		public IJobParameters JobParameters { get; private set; }

		public bool NeedsParameterDatePeriod { get; private set; }

		public bool NeedsParameterDataSource { get; private set; }

		public IJobResult Result
		{
			get
			{
				return _jobResult;
			}
			private set
			{
				_jobResult = value;
				FirePropertyChanged(nameof(Result));
			}
		}

		public ReadOnlyCollection<JobCategoryType> JobCategoryCollection
		{
			get
			{
				IList<JobCategoryType> jobCategoryTypeCollection = new List<JobCategoryType>();

				// Get a distinct list of which job categorys that is included in the selected job
				foreach (IJobStep jobStep in StepList)
				{
					if (!jobCategoryTypeCollection.Contains(jobStep.JobCategory))
					{
						jobCategoryTypeCollection.Add(jobStep.JobCategory);
					}
				}

				return new ReadOnlyCollection<JobCategoryType>(jobCategoryTypeCollection);
			}
		}

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				FirePropertyChanged(nameof(Enabled));
			}
		}

		public override string ToString()
		{
			return Name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected void FirePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}