using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds a DayOff to a schedule. It will be created according to a specified <see cref="DayOffInfoId"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule. This command will delete all activities, overtimes, absences and personal activities. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class AddDayOffCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the date of schedule.
        /// </summary>
        /// <value>The date of schedule.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the dayoff Id.
        /// </summary>
        /// <value>The dayoff Id.</value>
        [DataMember]
        public Guid DayOffInfoId { get; set; }
		
		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1,IsRequired = false)]
		public Guid? ScenarioId { get; set; }
    }
}
