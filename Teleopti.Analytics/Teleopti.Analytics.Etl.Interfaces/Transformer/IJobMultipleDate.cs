using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
	/// <summary>
	/// Stores different date periods for different job category types.
	/// </summary>
	/// <remarks>
	/// Created by: jonas n
	/// Created date: 2008-10-10
	/// </remarks>
	public interface IJobMultipleDate
	{

		/// <summary>
		/// Adds a specified date period for a job category type.
		/// </summary>
		/// <param name="startDate">The start date.</param>
		/// <param name="endDate">The end date.</param>
		/// <param name="jobCategoryType">Type of the job category.</param>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-10
		/// </remarks>
		void Add(DateTime startDate, DateTime endDate, JobCategoryType jobCategoryType);

		/// <summary>
		/// Adds the specified job multiple date item.
		/// </summary>
		/// <param name="jobMultipleDateItem">The job multiple date item.</param>
		/// <param name="jobCategoryType">Type of the job category.</param>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-20
		/// </remarks>
		void Add(IJobMultipleDateItem jobMultipleDateItem, JobCategoryType jobCategoryType);

		/// <summary>
		/// Gets the date period for the given job category type.
		/// </summary>
		/// <param name="jobCategoryType">Type of the job category.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-10
		/// </remarks>
		IJobMultipleDateItem GetJobMultipleDateItem(JobCategoryType jobCategoryType);

		/// <summary>
		/// Gets a local date period that uses earliest start and latest end from all job category periods.
		/// </summary>
		/// <value>The min max dates.</value>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-10
		/// </remarks>
		IJobMultipleDateItem MinMaxDatesLocal
		{
			get;
		}

		/// <summary>
		/// Gets a UTC date period that uses earliest start and latest end from all job category periods.
		/// </summary>
		/// <value>The min max dates UTC.</value>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-16
		/// </remarks>
		IJobMultipleDateItem MinMaxDatesUtc { get; }

		/// <summary>
		/// Clears this instance from all date periods for all job categorys.
		/// </summary>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-19
		/// </remarks>
		void Clear();


		/// <summary>
		/// Gets the number of date periods for this instance.
		/// </summary>
		/// <value>The count.</value>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-19
		/// </remarks>
		int Count { get; }

		/// <summary>
		/// Gets a read only collection of all job categorys that have a date period.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-20
		/// </remarks>
		ReadOnlyCollection<JobCategoryType> JobCategoryCollection { get; }

		/// <summary>
		/// Gets a read only collection of all date periods in all job categorys.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-10-20
		/// </remarks>
		ReadOnlyCollection<IJobMultipleDateItem> AllDatePeriodCollection { get; }

		
	}
}
