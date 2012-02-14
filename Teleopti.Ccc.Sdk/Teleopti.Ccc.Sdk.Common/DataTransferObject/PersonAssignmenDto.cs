using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an PersonAssignmentDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonAssignmentDto : Dto
    {
        private ICollection<ShiftDto> _personalShiftCollection = new List<ShiftDto>();
        private ICollection<ShiftDto> _overtimeShiftCollection = new List<ShiftDto>();

        /// <summary>
        /// Gets or sets the main shift details.
        /// </summary>
        [DataMember]
        public MainShiftDto MainShift { get; set; }

        /// <summary>
        /// Gets the collection of personal shifts.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The setter is used in serialization."), DataMember]
        public ICollection<ShiftDto> PersonalShiftCollection
        {
            get { return _personalShiftCollection; }
            private set
            {
                if (value!=null)
                {
                    _personalShiftCollection = new List<ShiftDto>(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the version used for checks of optimistic lock.
        /// </summary>
        [DataMember]
        public int Version { get; set; }


        /// <summary>
        /// Gets the collection of overtime shifts.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",Justification = "The setter is used in serialization."), DataMember]
        public ICollection<ShiftDto> OvertimeShiftCollection
        {
            get { return _overtimeShiftCollection; }
            private set
            {
                if (value!=null)
                {
                    _overtimeShiftCollection = new List<ShiftDto>(value);
                }
            }
        }
    }
}