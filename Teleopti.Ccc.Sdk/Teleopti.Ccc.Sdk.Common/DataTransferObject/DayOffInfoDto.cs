using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DayOffDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class DayOffInfoDto :Dto
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name{ get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>The short name.</value>
        [DataMember]
        public string ShortName{ get; set; }

        /// <summary>
        /// Indicates whether this item is deleted or not
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public bool IsDeleted { get; set; }

		/// <summary>
		/// Gets or sets the payroll code.
		/// </summary>
		/// <value>The payroll code.</value>
		[DataMember(IsRequired = false, Order = 2)]
		public string PayrollCode { get; set; }
    }
}