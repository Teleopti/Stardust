using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an Absence object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AbsenceDto : Dto
    {
        /// <summary>
        /// Gets or sets the payroll code.
        /// </summary>
        /// <value>The payroll code.</value>
        [DataMember]
        public string PayrollCode { get; set; }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        [DataMember]
        public ColorDto DisplayColor{ get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>The short name.</value>
        [DataMember]
        public string ShortName{ get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// If absences ar stacked "above" each other.
        /// </summary>
        /// <value>The priority.</value>
        [DataMember]
        public byte Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [in contract time].
        /// </summary>
        /// <value><c>true</c> if [in contract time]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool InContractTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is trackable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is trackable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTrackable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        [DataMember(IsRequired = false,Order = 1)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Absence is [in work time].
        /// </summary>
        /// <value><c>true</c> if [in work time]; otherwise, <c>false</c>.</value>
        [DataMember(IsRequired = false, Order = 2)]
        public bool InWorkTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Absence is [in paid time].
        /// </summary>
        /// <value><c>true</c> if [in paid time]; otherwise, <c>false</c>.</value>
        [DataMember(IsRequired = false, Order = 3)]
        public bool InPaidTime { get; set; }
    }
}