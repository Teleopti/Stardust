#region Imports

using System.Runtime.Serialization;

#endregion

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a LayerDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [KnownType(typeof(ActivityLayerDto))]
    [KnownType(typeof(AbsenceLayerDto))]
    [KnownType(typeof(PersonMeetingDto))]
    [KnownType(typeof(PersonalShiftActivityDto))]
    [KnownType(typeof(OvertimeLayerDto))]
    public class LayerDto : Dto
    {
        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period{ get; set; } 
    }
}