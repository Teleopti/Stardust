using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an ActivityDto object.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ActivityDto : Dto
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description{ get; set; }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        [DataMember]
        public ColorDto DisplayColor{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether activity is [in work time].
        /// </summary>
        /// <value><c>true</c> if [in work time]; otherwise, <c>false</c>.</value>
        [DataMember(IsRequired = false, Order = 1)]
        public bool InWorkTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether activity is [in paid time].
        /// </summary>
        /// <value><c>true</c> if [in paid time]; otherwise, <c>false</c>.</value>
        [DataMember(IsRequired = false, Order = 2)]
        public bool InPaidTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether activity is [in contract time].
        /// </summary>
        /// <value><c>true</c> if [in contract time]; otherwise, <c>false</c>.</value>
        [DataMember(IsRequired = false, Order = 3)]
        public bool InContractTime { get; set; }

		/// <summary>
		/// Gets or sets the payroll code.
		/// </summary>
		/// <value>The payroll code.</value>
		[DataMember(IsRequired = false, Order = 4)]
		public string PayrollCode { get; set; }

        /// <summary>
        /// Gets or sets the IsDeleted flag.
        /// </summary>
        /// <remarks>This inidicates whether this activity is available for new usage.</remarks>
        [DataMember(IsRequired = false,Order = 5)]
        public bool IsDeleted { get; set; }
    }
}