using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobResult : IJobResult
	{
		private readonly IList<IJobResult> _jobResultCollection;
		private readonly IList<IJobStepResult> _jobStepResultCollection = new List<IJobStepResult>();
		private string _status;
		private string _name;

		private JobResult() { }

		public JobResult(IBusinessUnit currentBusinessUnit, IList<IJobResult> jobResultCollection)
			: this()
		{
			_jobResultCollection = jobResultCollection;
			CurrentBusinessUnit = currentBusinessUnit;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double Duration
		{
			get
			{
				double duration = 0;
				foreach (IJobStepResult runLoadStepInfo in _jobStepResultCollection)
				{
					if (runLoadStepInfo.Duration.HasValue && runLoadStepInfo.Duration.Value > 0)
					{
						duration += runLoadStepInfo.Duration.Value;
					}
				}

				return duration;
			}
		}

		public int RowsAffected
		{
			get
			{
				int rowsAffected = 0;
				foreach (IJobStepResult runLoadStepInfo in _jobStepResultCollection)
				{
					if (runLoadStepInfo.RowsAffected.HasValue)
					{
						rowsAffected += runLoadStepInfo.RowsAffected.Value;
					}
				}

				return rowsAffected;
			}
		}


		public void Update()
		{
			FirePropertyChanged(nameof(Name));
			FirePropertyChanged(nameof(Duration));
			FirePropertyChanged(nameof(RowsAffected));
			FirePropertyChanged(nameof(Status));
			FirePropertyChanged(nameof(BusinessUnitStatus));
			FirePropertyChanged(nameof(HasError));
		}

		public IList<IJobStepResult> JobStepResultCollection
		{
			get { return _jobStepResultCollection; }
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
				FirePropertyChanged(nameof(Name));
			}
		}

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

		public bool Success { get; set; }

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
					if (!jobResult.Success)
						retList.Add(jobResult.CurrentBusinessUnit);
				}
			}

			if (!Success && !retList.Contains(CurrentBusinessUnit))
				retList.Add(CurrentBusinessUnit);

			return retList;
		}

		public IBusinessUnit CurrentBusinessUnit { get; private set; }

		public bool HasError
		{
			get
			{
				return BusinessUnitStatus.Length > 0;
			}
		}

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected void FirePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}