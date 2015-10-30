using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command cancels all absences within a specified <see cref="Period"/>. All absences intersect with the specified <see cref="Period"/> will be hollowed out. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class CancelAbsenceCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
		public DateTimePeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
		public Guid? ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the schedule tag id. If omitted then Null schedule tag will be used.
		/// </summary>
		[DataMember(Order = 2, IsRequired = false)]
		public Guid? ScheduleTagId { get; set; }

		[DataMember(Order = 3, IsRequired = false)]
	    public Guid? AbsenceId { get; set; }
    }
}
