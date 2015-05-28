using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate
{
	public class JobMultipleDate : IJobMultipleDate
	{
		private readonly IDictionary<JobCategoryType, IJobMultipleDateItem> _jobMultiDateDictionary = new Dictionary<JobCategoryType, IJobMultipleDateItem>();
		private readonly TimeZoneInfo _timeZone;

		private JobMultipleDate() { }

		public JobMultipleDate(TimeZoneInfo timeZone)
			: this()
		{
			_timeZone = timeZone;
		}

		public void Add(DateTime startDate, DateTime endDate, JobCategoryType jobCategoryType)
		{
			IJobMultipleDateItem jobMultipleDateItem = new JobMultipleDateItem(DateTimeKind.Local, startDate, endDate,
																									 _timeZone);
			_jobMultiDateDictionary.Add(jobCategoryType, jobMultipleDateItem);
		}

		public void Add(IJobMultipleDateItem jobMultipleDateItem, JobCategoryType jobCategoryType)
		{
			_jobMultiDateDictionary.Add(jobCategoryType, jobMultipleDateItem);
		}

		public IJobMultipleDateItem GetJobMultipleDateItem(JobCategoryType jobCategoryType)
		{
			return _jobMultiDateDictionary[jobCategoryType];
		}

		public IJobMultipleDateItem MinMaxDatesLocal
		{
			get
			{
				DateTime startDate = DateTime.MaxValue;
				DateTime endDate = DateTime.MinValue;

				foreach (IJobMultipleDateItem jobMultiDateItem in _jobMultiDateDictionary.Values)
				{
					if (jobMultiDateItem.StartDateLocal < startDate)
					{
						startDate = jobMultiDateItem.StartDateLocal;
					}
					if (jobMultiDateItem.EndDateLocal > endDate)
					{
						endDate = jobMultiDateItem.EndDateLocal;
					}

				}
				return new JobMultipleDateItem(DateTimeKind.Local, startDate, endDate, _timeZone);
			}
		}

		public IJobMultipleDateItem MinMaxDatesUtc
		{
			get
			{
				DateTime startDate = DateTime.MaxValue;
				DateTime endDate = DateTime.MinValue;

				foreach (IJobMultipleDateItem jobMultiDateItem in _jobMultiDateDictionary.Values)
				{
					if (jobMultiDateItem.StartDateUtc < startDate)
					{
						startDate = jobMultiDateItem.StartDateUtc;
					}
					if (jobMultiDateItem.EndDateUtc > endDate)
					{
						endDate = jobMultiDateItem.EndDateUtc;
					}

				}
				return new JobMultipleDateItem(DateTimeKind.Utc, startDate, endDate, _timeZone);
			}
		}

		public void Clear()
		{
			_jobMultiDateDictionary.Clear();
		}

		public int Count
		{
			get
			{
				return _jobMultiDateDictionary.Count;
			}
		}

		public ReadOnlyCollection<JobCategoryType> JobCategoryCollection
		{
			get { return new ReadOnlyCollection<JobCategoryType>(new List<JobCategoryType>(_jobMultiDateDictionary.Keys)); }
		}

		public ReadOnlyCollection<IJobMultipleDateItem> AllDatePeriodCollection
		{
			get
			{
				return
					 new ReadOnlyCollection<IJobMultipleDateItem>(
						  new List<IJobMultipleDateItem>(_jobMultiDateDictionary.Values));
			}
		}
	}
}