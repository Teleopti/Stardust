using System;
using System.Collections.ObjectModel;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobMultipleDate
	{
		void Add(DateTime startDate, DateTime endDate, JobCategoryType jobCategoryType);

		void Add(IJobMultipleDateItem jobMultipleDateItem, JobCategoryType jobCategoryType);
		
		IJobMultipleDateItem GetJobMultipleDateItem(JobCategoryType jobCategoryType);

		IJobMultipleDateItem MinMaxDatesUtc { get; }

		void Clear();

		int Count { get; }

		ReadOnlyCollection<JobCategoryType> JobCategoryCollection { get; }

		ReadOnlyCollection<IJobMultipleDateItem> AllDatePeriodCollection { get; }
	}
}
