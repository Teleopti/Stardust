using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

		public int Count => _jobMultiDateDictionary.Count;

		public ReadOnlyCollection<JobCategoryType> JobCategoryCollection => new ReadOnlyCollection<JobCategoryType>(_jobMultiDateDictionary.Keys.ToArray());

		public ReadOnlyCollection<IJobMultipleDateItem> AllDatePeriodCollection => new ReadOnlyCollection<IJobMultipleDateItem>(_jobMultiDateDictionary.Values.ToArray());
	}
}