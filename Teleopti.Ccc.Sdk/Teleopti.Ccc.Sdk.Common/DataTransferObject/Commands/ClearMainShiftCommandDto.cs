using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command clears the mainshift from a schedule. To specify the schedule, you can declare it by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule.
    /// </summary>
    /// <remarks>To remove overtime layers you need to execute the separate command <see cref="CancelOvertimeCommandDto"/>, and to remove personal activity layers execute the <see cref="CancelPersonalActivityCommandDto"/> command.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class ClearMainShiftCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory target shift start date.
        /// </summary>
        /// <value>The target date.</value>
        [DataMember]
		public DateOnlyDto Date { get; set; }

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
    }
}
