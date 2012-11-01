using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a MultiplicatorDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MultiplicatorDto : Dto
    {
        
		/// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember]
        public ColorDto Color { get; set; }

        /// <summary>
        /// Gets or sets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        [DataMember]
        public double Multiplicator { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        [DataMember]
        public MultiplicatorTypeDto MultiplicatorType { get;  set; }

        /// <summary>
        /// Gets or sets the payroll code.
        /// </summary>
        /// <value>The payroll code.</value>
        [DataMember]
        public string PayrollCode { get; set; }

		/// <summary>
		/// Gets or sets the short name.
		/// </summary>
		/// <value>The short name.</value>
		[DataMember(IsRequired = false, Order = 1)]
		public string ShortName { get; set; }

		/// <summary>
		/// Gets or sets is deleted
		/// </summary>
		/// <value>The short name.</value>
		[DataMember(IsRequired = false, Order = 1)]
		public bool IsDeleted { get; set; }
    }
}