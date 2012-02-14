using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a MultiplicatorDataDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MultiplicatorDataDto : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the date of the shift this multiplicator data belongs to.
        /// </summary>
        /// <value>The date.</value>
        [DataMember]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        [DataMember]
        public TimeSpan Amount { get; set; }

        /// <summary>
        /// Gets or sets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        [DataMember]
        public MultiplicatorDto Multiplicator { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>The person id.</value>
        [DataMember]
        public Guid? PersonId { get; set; }

        /// <summary>
        /// The actual date this multiplicator data layer is from (not the shift date)
        /// </summary>
        [DataMember(IsRequired = false,Order = 1)]
        public DateTime ActualDate { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}