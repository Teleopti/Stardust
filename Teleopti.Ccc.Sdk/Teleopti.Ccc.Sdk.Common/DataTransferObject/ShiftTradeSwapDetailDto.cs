using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Holds information about one swap within a shift trade request.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ShiftTradeSwapDetailDto : Dto
    {
        /// <summary>
        /// Gets or sets the person from.
        /// </summary>
        /// <value>The person from.</value>
        [DataMember]
        public PersonDto PersonFrom { get; set; }

        /// <summary>
        /// Gets or sets the person to.
        /// </summary>
        /// <value>The person to.</value>
        [DataMember]
        public PersonDto PersonTo { get; set; }

        /// <summary>
        /// Gets or sets the date from.
        /// </summary>
        /// <value>The date from.</value>
        [DataMember]
        public DateOnlyDto DateFrom { get; set; }

        /// <summary>
        /// Gets or sets the date to.
        /// </summary>
        /// <value>The date to.</value>
        [DataMember]
        public DateOnlyDto DateTo { get; set; }

        /// <summary>
        /// Gets or sets the schedule part from.
        /// This is supplied as read only information! Changes to schedule will not be saved.
        /// </summary>
        /// <value>The schedule part from.</value>
        [DataMember]
        public SchedulePartDto SchedulePartFrom { get; set; }

        /// <summary>
        /// Gets or sets the schedule part to.
        /// This is supplied as read only information! Changes to schedule will not be saved.
        /// </summary>
        /// <value>The schedule part to.</value>
        [DataMember]
        public SchedulePartDto SchedulePartTo { get; set; }

        /// <summary>
        /// Gets or sets the checksum from. The checksum is calculated at the server.
        /// If the checksum is modified outside the server the shift trade request will be referred.
        /// </summary>
        /// <value>The checksum from.</value>
        [DataMember]
        public long ChecksumFrom { get; set; }

        /// <summary>
        /// Gets or sets the checksum to. The checksum is calculated at the server.
        /// If the checksum is modified outside the server the shift trade request will be referred.
        /// </summary>
        /// <value>The checksum to.</value>
        [DataMember]
        public long ChecksumTo { get; set; }
    }
}