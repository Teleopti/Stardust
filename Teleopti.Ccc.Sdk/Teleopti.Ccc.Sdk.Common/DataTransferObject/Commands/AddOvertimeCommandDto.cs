using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds an overtime layer to a schedule. The overtime layer will be created according to a specified <see cref="ActivityId"/>, <see cref="Period"/> and <see cref="OvertimeDefinitionSetId"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. Note that, the total assignment length after the overtime is added should be less than 36 hours. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddOvertimeCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the target date.
        /// </summary>
        /// <value>The target date.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the activity Id.
        /// </summary>
        /// <value>The activity Id.</value>
        [DataMember]
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the id of the overtime definition set.
        /// </summary>
        /// <remarks>This can be matched with the results from <see cref="ITeleoptiOrganizationService.GetOvertimeDefinitions"/>.</remarks>
        [DataMember]
		public Guid OvertimeDefinitionSetId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
		public Guid? ScenarioId { get; set; }
    }
}
