using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
    /// This command adds an absence layer to a schedule. The absence layer will be created according to a specified <see cref="AbsenceId"/> and <see cref="Period"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddAbsenceCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory date of schedule.
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period of the absence layer.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the mandatory absence id. The id usually comes from <see cref="AbsenceDto"/> loaded using the method <see cref="ITeleoptiSchedulingService.GetAbsences"/>.
        /// </summary>
        /// <value>The absence Id.</value>
        [DataMember]
        public Guid AbsenceId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1,IsRequired = false)]
    	public Guid? ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the schedule tag id. If omitted then Null schedule tag will be used.
        /// </summary>
        [DataMember(Order = 2, IsRequired = false)]
        public Guid? ScheduleTagId { get; set; }
    }
}
