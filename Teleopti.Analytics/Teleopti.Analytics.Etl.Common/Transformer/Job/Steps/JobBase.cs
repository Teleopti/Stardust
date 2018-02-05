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
		private string _name;
		private IJobResult _jobResult;
		private bool _enabled;

		public event EventHandler<AlarmEventArgs> JobExecutionReady;
		public void NotifyJobExecutionReady()
		{
			// Notify that job exectution is ready. At least the Data Source config dialog is consumer.
			JobExecutionReady?.Invoke(this, new AlarmEventArgs(this));
		}

		private JobBase() { }

		public JobBase(IJobParameters jobParameters, IList<IJobStep> jobStep, string jobName, bool needsParameterDatePeriod, bool needsParameterDataSource)
			: this()
		{
			_name = jobName;
			StepList = jobStep;
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
				firePropertyChanged(nameof(Name));
			}
			get => _name;
		}

		public IList<IJobStep> StepList { get; }

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
					Result.Status = "";
					Result.Success = false;
					Result.JobStepResultCollection.Add(
						new JobStepResult("",0,
							new Exception(getInvalidLicenseErrorMessage(JobParameters.Helper.SelectedDataSource.DataSourceName)),businessUnit,jobResultCollection));
					update();
					return Result;
				}

				JobParameters.StateHolder = new CommonStateHolder(JobParameters);
			}

			foreach (var step in StepList)
			{
				var jobStepResult = step.Run(jobStepsNotToRun, businessUnit, jobResultCollection, isLastBusinessUnitRun);

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

			JobParameters?.Helper.LogOffTeleoptiCccDomain();
			Result.Status = Result.Success ? "Done" : "Not completed due to error!";

			return Result;
		}
		
		private static string getInvalidLicenseErrorMessage(string tenant)
		{
			return $"ETL Service could not run for tenant {tenant}. Please log on to that tenant and apply a license in the main client.";
		}

		private void update()
		{
			Result.Update();
			firePropertyChanged(nameof(Result));
		}

		private void clearJobStepResults(bool firstRun)
		{
			foreach (var jobStep in StepList)
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
						firePropertyChanged(nameof(Result));
					}
				}
			}
		}


		public IJobParameters JobParameters { get; private set; }

		public bool NeedsParameterDatePeriod { get; private set; }

		public bool NeedsParameterDataSource { get; private set; }

		public IJobResult Result
		{
			get => _jobResult;
			private set
			{
				_jobResult = value;
				firePropertyChanged(nameof(Result));
			}
		}

		public ReadOnlyCollection<JobCategoryType> JobCategoryCollection
		{
			get
			{
				IList<JobCategoryType> jobCategoryTypeCollection = new List<JobCategoryType>();

				// Get a distinct list of which job categorys that is included in the selected job
				foreach (var jobStep in StepList)
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
			get => _enabled;
			set
			{
				_enabled = value;
				firePropertyChanged(nameof(Enabled));
			}
		}

		public override string ToString()
		{
			return Name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		private void firePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}