using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds a DayOff to a schedule. It will be created according to a specified <see cref="DayOffInfoId"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. This command will delete all activities, overtimes, absences and personal activities. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddDayOffCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory schedule day this day off should be applied to.
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the mandatory dayoff Id. Usually comes from a <see cref="DayOffInfoDto"/> loaded using the <see cref="ITeleoptiSchedulingService.GetDaysOffs"/> method.
        /// </summary>
        /// <value>The dayoff Id.</value>
        [DataMember]
        public Guid DayOffInfoId { get; set; }
		
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
