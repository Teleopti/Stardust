using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a data row in the online report Schedule Audit Trail
    /// </summary>
    public interface IScheduleAuditTrailReportData
    {
        /// <summary>
        /// Gets or sets the audit datetime.
        /// </summary>
        /// <value>The audit datetime.</value>
        DateTime AuditDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the audit owner.
        /// </summary>
        /// <value>The name of the audit owner.</value>
        string AuditOwnerName { get; set; }

        /// <summary>
        /// Gets or sets the effected person.
        /// </summary>
        /// <value>The effected person.</value>
        string EffectedPerson { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the type of the audit.
        /// </summary>
        /// <value>The type of the audit.</value>
        string AuditType { get; set; }

        /// <summary>
        /// Gets or sets the detail.
        /// </summary>
        /// <value>The detail.</value>
        string Detail { get; set; }

        /// <summary>
        /// Gets or sets the schedule date minimum.
        /// </summary>
        /// <value>The schedule date minimum.</value>
        DateTime ScheduleDateMinimum { get; set; }

        /// <summary>
        /// Gets or sets the schedule date maximum.
        /// </summary>
        /// <value>The schedule date maximum.</value>
        DateTime ScheduleDateMaximum { get; set; }

    }
}