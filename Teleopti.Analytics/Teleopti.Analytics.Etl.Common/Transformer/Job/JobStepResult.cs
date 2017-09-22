using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobStepResult : IJobStepResult
	{
		private readonly IList<IJobResult> _jobResultCollection;

		private JobStepResult() { }

		public JobStepResult(String name, int rowsAffected, double duration, IBusinessUnit currentBusinessUnit, IList<IJobResult> jobResultCollection)
			: this()
		{
			Name = name;
			Duration = duration;
			RowsAffected = rowsAffected;
			CurrentBusinessUnit = currentBusinessUnit;
			_jobResultCollection = jobResultCollection;
		}

		public JobStepResult(String name, double duration, Exception jobStepException, IBusinessUnit currentBusinessUnit, IList<IJobResult> jobResultCollection)
			: this()
		{
			Name = name;
			Duration = duration;
			JobStepException = jobStepException;
			CurrentBusinessUnit = currentBusinessUnit;
			_jobResultCollection = jobResultCollection;
		}


		private double? _duration;
		public double? Duration
		{
			get
			{
				return _duration / 1000d;
			}
			set
			{
				_duration = value;
				FirePropertyChanged(nameof(Duration));
			}
		}


		private int? _rowsAffected;
		public int? RowsAffected
		{
			get
			{

				return _rowsAffected;
			}
			set
			{
				_rowsAffected = value;
				FirePropertyChanged(nameof(RowsAffected));
			}
		}


		private string _status;

		public string Status
		{
			get
			{
				return _status;
			}
			set
			{
				_status = value;
				FirePropertyChanged(nameof(Status));
			}
		}


		private string _name;
		public string Name
		{
			get
			{
				return _name;
			}
			private set
			{
				_name = value;
				FirePropertyChanged(nameof(Name));
			}
		}


		public Exception JobStepException { get; private set; }

		public IBusinessUnit CurrentBusinessUnit { get; private set; }

		public bool HasError
		{
			get
			{
				return BusinessUnitStatus.Length > 0;
			}
		}

		public void ClearResult()
		{
			Duration = null;
			RowsAffected = null;
			Status = string.Empty;
		}

		public string BusinessUnitStatus
		{
			get
			{
				string ret = "";
				IList<IBusinessUnit> businessUnitErrorCollection = getErrorBusinessUnits();
				if (businessUnitErrorCollection != null && businessUnitErrorCollection.Count > 0)
				{
					ret = "Error in business units: ";
					foreach (IBusinessUnit bu in businessUnitErrorCollection)
					{
						ret = string.Format(CultureInfo.CurrentCulture, "{0}'{1}', ", ret, bu.Name);
					}
					ret = ret.Substring(0, ret.Length - 2);
				}
				return ret;
			}
		}

		private IList<IBusinessUnit> getErrorBusinessUnits()
		{
			IList<IBusinessUnit> retList = new List<IBusinessUnit>();
			if (_jobResultCollection != null)
			{
				foreach (IJobResult jobResult in _jobResultCollection)
				{
					foreach (IJobStepResult jobStepResult in jobResult.JobStepResultCollection)
					{
						if (jobStepResult.JobStepException != null && jobStepResult.Name == Name)
							retList.Add(jobResult.CurrentBusinessUnit);     //Error for this step found in previous job run in other BU
					}
				}
			}

			if (JobStepException != null && !retList.Contains(CurrentBusinessUnit))
				retList.Add(CurrentBusinessUnit);

			return retList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected void FirePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public object Clone()
		{
			double duration = Duration.HasValue ? Duration.Value * 1000 : 0d;
			int rowsAffected = RowsAffected.HasValue ? RowsAffected.Value : 0;

			var clone = JobStepException != null
				? new JobStepResult(Name, duration, JobStepException, CurrentBusinessUnit, _jobResultCollection)
				: new JobStepResult(Name, rowsAffected, duration, CurrentBusinessUnit, _jobResultCollection);

			clone.Status = Status;

			return clone;
		}
	}
}
