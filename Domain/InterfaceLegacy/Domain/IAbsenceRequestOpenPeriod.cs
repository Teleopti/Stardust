using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Definition of open Absence Request period.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-15
    /// </remarks>
    public interface IAbsenceRequestOpenPeriod : IAggregateEntity, ICloneableEntity<IAbsenceRequestOpenPeriod>
    {
        /// <summary>
        /// Gets or sets the absence.
        /// </summary>
        /// <value>The absence.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-15
        /// </remarks>
        IAbsence Absence { get; set; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <param name="viewpointDateOnly">The viewpoint date only.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-20
        /// </remarks>
        DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);

        /// <summary>
        /// Gets or sets the open for requests period.
        /// </summary>
        /// <value>The open for requests period.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-15
        /// </remarks>
        DateOnlyPeriod OpenForRequestsPeriod { get; set; }

        /// <summary>
        /// Gets the person account validator list.
        /// </summary>
        /// <value>The person account validator list.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-19
        /// </remarks>
        IList<IAbsenceRequestValidator> PersonAccountValidatorList { get; }

        /// <summary>
        /// Gets or sets the person account validator.
        /// </summary>
        /// <value>The person account validator.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-19
        /// </remarks>
        IAbsenceRequestValidator PersonAccountValidator { get; set; }

        /// <summary>
        /// Gets the staffing threshold validator list.
        /// </summary>
        /// <value>The staffing threshold validator list.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-20
        /// </remarks>
        IList<IAbsenceRequestValidator> StaffingThresholdValidatorList { get; }

        /// <summary>
        /// Gets or sets the staffing threshold validator.
        /// </summary>
        /// <value>The staffing threshold validator.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-20
        /// </remarks>
        IAbsenceRequestValidator StaffingThresholdValidator { get; set; }

        /// <summary>
        /// Gets the index of the order.
        /// </summary>
        /// <value>The index of the order, greater value means higher priority.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-20
        /// </remarks>
        int OrderIndex { get; }

        /// <summary>
        /// Gets the absence request process list.
        /// </summary>
        /// <value>The absence request process list.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        IList<IProcessAbsenceRequest> AbsenceRequestProcessList { get; }

        /// <summary>
        /// Gets or sets the absence request process. (Grant or Pending)
        /// </summary>
        /// <value>The absence request process.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        IProcessAbsenceRequest AbsenceRequestProcess { get; set; }

        /// <summary>
        /// Gets the selected validator list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-04-22
        /// </remarks>
        IEnumerable<IAbsenceRequestValidator> GetSelectedValidatorList();
    }
}