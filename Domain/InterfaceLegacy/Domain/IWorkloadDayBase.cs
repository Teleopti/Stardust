using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Base interface for workload days and workload day templates
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-17
    /// </remarks>
    public interface IWorkloadDayBase : ITaskOwner, IAggregateEntity
    {
        /// <summary>
        /// Creates the specified workload day
        /// </summary>
        /// <param name="workloadDate">The workload date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-13
        /// </remarks>
        void Create(DateOnly workloadDate, IWorkload workload, IList<TimePeriod> openHourList);

        /// <summary>
        /// Adds an OpenHourPeriod spanning the complete date.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/28/2007
        /// </remarks>
        void MakeOpen24Hours();

        /// <summary>
        /// Closes this workload day.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/28/2007
        /// </remarks>
        void Close();

        /// <summary>
        /// Sets the open hour list.
        /// </summary>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        void ChangeOpenHours(IList<TimePeriod> openHourList);

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        IWorkload Workload { get; }

        /// <summary>
        /// Gets the open hour list.
        /// </summary>
        /// <value>The open hour list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        ReadOnlyCollection<TimePeriod> OpenHourList { get; }

        /// <summary>
        /// Gets the task period list.
        /// </summary>
        /// <value>The task period list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/27/2007
        /// </remarks>
        ReadOnlyCollection<ITemplateTaskPeriod> TaskPeriodList { get; }

        /// <summary>
        /// Gets the task period list sorted by start times.
        /// </summary>
        /// <value>The sorted task period list.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-14
        /// </remarks>
        ReadOnlyCollection<ITemplateTaskPeriod> SortedTaskPeriodList { get; }

        /// <summary>
        /// Gets the TaskPeriodList period.
        /// </summary>
        /// <value>The task period list period.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-26
        /// </remarks>
        DateTimePeriod TaskPeriodListPeriod { get; }

        /// <summary>
        /// Gets the open task period list.
        /// </summary>
        /// <value>The open task period list.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-17
        /// </remarks>
        ReadOnlyCollection<ITemplateTaskPeriod> OpenTaskPeriodList { get; }

        /// <summary>
        /// Sets the task period collection.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-22
        /// </remarks>
        void SetTaskPeriodCollection(IList<ITemplateTaskPeriod> list);


		///<summary>
		/// Sets the task period collection with Statistics assigned.
		///</summary>
		///<param name="list"></param>
		void SetTaskPeriodCollectionWithStatistics(IList<ITemplateTaskPeriod> list);

        /// <summary>
        /// Merges the template task periods.
        /// </summary>
        /// <param name="templateTaskPeriodList">The template task period list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        void MergeTemplateTaskPeriods(IList<ITemplateTaskPeriod> templateTaskPeriodList);

        /// <summary>
        /// Splits the template task periods.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-22
        /// </remarks>
        void SplitTemplateTaskPeriods(IList<ITemplateTaskPeriod> list);

        /// <summary>
        /// Determines whether [is only incoming] [the specified template task period].
        /// </summary>
        /// <param name="templateTaskPeriod">The template task period.</param>
        /// <returns>
        /// 	<c>true</c> if [is only incoming] [the specified template task period]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-28
        /// </remarks>
        bool IsOnlyIncoming(ITemplateTaskPeriod templateTaskPeriod);

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Splits SkillStaffPeriodCollection into a new collection of ISkillStaffPeriodViews.
        /// The specified period length must be shorter than on this and the split must be even.
        /// For example can not a collection of periods in 15 minutes period be split on 10. But 5.
        /// </summary>
        /// <param name="periodLength">Length of the period.</param>
        /// <returns></returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-07    
        /// </remarks>
        ReadOnlyCollection<ITemplateTaskPeriodView> TemplateTaskPeriodViewCollection(TimeSpan periodLength);

        /// <summary>
        /// Sets the queue statistics.
        /// </summary>
        /// <param name="queueStatisticsProvider">The queue statistics calculator.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-03-11
        /// </remarks>
        void SetQueueStatistics(IQueueStatisticsProvider queueStatisticsProvider);

		void DistributeTasks(IEnumerable<ITemplateTaskPeriod> sortedTemplateTaskPeriods);

	    void SetUseSkewedDistribution(bool value);
    }
}