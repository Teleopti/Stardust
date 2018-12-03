using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Template for WorkloadDays
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-01-23
    /// </remarks>
    public interface IWorkloadDayTemplate : IForecastDayTemplate, IWorkloadDayBase, ICloneableEntity<IWorkloadDayTemplate>
    {
        /// <summary>
        /// Creates the specified workload day template.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createdDate">The created date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="openHourList">The open hour list.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        void Create(string name, DateTime createdDate, IWorkload workload, IList<TimePeriod> openHourList);

        /// <summary>
        /// Gets the workload date.
        /// </summary>
        /// <value>The workload date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-23
        /// </remarks>
        DateTime CreatedDate { get; }

        /// <summary>
        /// Gets the sorted original task period list.
        /// </summary>
        /// <value>The sorted original task period list.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        ReadOnlyCollection<ITemplateTaskPeriod> SortedSnapshotTaskPeriodList { get; }

        /// <summary>
        /// Does the running smoothning.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-27
        /// </remarks>
        void DoRunningSmoothing(int periods, TaskPeriodType type);

        /// <summary>
        /// Snapshots the template task period list.
        /// Copies from SortedTaskPeriodList to a snapshotlist
        /// It only copies the selected type Tasks, AverageTasksTime, AverageAfterTaskTime 
        /// Or All.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-27
        /// </remarks>
        void SnapshotTemplateTaskPeriodList(TaskPeriodType type);

        ///<summary>
        /// 
        ///</summary>
        ///<param name="workload"></param>
        void SetWorkloadInstance(IWorkload workload);
    }
}
