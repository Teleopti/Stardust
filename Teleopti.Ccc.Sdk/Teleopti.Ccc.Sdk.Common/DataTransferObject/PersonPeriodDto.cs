using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PersonPeriodDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/"), Serializable]
    public class PersonPeriodDto : IExtensibleDataObject
    {
		[NonSerialized]
		private ExtensionDataObject _extensionData;
    	
    	/// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateOnlyPeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the schedule published to date.
        /// </summary>
        /// <value>The schedule published to date.</value>
		[DataMember, Obsolete("This information is available for the workflow control set on PersonDto instead.")]
        public DateTime? SchedulePublishedToDate { get; set; }

        /// <summary>
        /// Gets or sets the schedule preference date.
        /// </summary>
        /// <value>The schedule preference date.</value>
        [DataMember, Obsolete("This information is available for the workflow control set on PersonDto instead.")]
        public DateTime? SchedulePreferenceDate { get; set; }

        /// <summary>
        /// Gets or sets the person contract.
        /// </summary>
        /// <value>The person contract.</value>
        [DataMember]
        public PersonContractDto PersonContract { get; set; }

        /// <summary>
        /// The team for the person period.
        /// </summary>
        /// <remarks>This property is not populated when loading data from the SDK. Only used for creation of new person periods!</remarks>
        [DataMember(IsRequired = false, Order = 1)]
        public TeamDto Team { get; set; }

    	public ExtensionDataObject ExtensionData
    	{
    		get { return _extensionData; }
    		set { _extensionData = value; }
    	}
    }
}