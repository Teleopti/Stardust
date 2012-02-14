using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class for holding one day of workload data
    /// </summary>
    public interface IWorkloadDay : ITemplateDay, IWorkloadDayBase, IAnnotatable, ICloneableEntity<IWorkloadDay>
    {

        /// <summary>
        /// Creates the worklad day.
        /// </summary>
        /// <param name="workloadDate">The date.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="workloadDayTemplate">The workload day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created WorkloadDate: 2008-01-30
        /// </remarks>
        void CreateFromTemplate(DateOnly workloadDate, IWorkload workload, IWorkloadDayTemplate workloadDayTemplate);

        /// <summary>
        /// Applies the template.
        /// </summary>
        /// <param name="workloadDayTemplate">The workload day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-03
        /// </remarks>
        void ApplyTemplate(IWorkloadDayTemplate workloadDayTemplate);
    }
}