using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Data Transfer Object class for Myreport relevant Queues 
    /// </summary>
    /// <remarks>
    /// Created by: Madhuranga Pinnagoda
    /// Created date: 2008-10-13
    /// </remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class QueueDto : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name{ get; set; }
       
        /// <summary>
        /// Gets or sets the answered contracts.
        /// </summary>
        /// <value>The answered contracts.</value>
        [DataMember]
        public int AnsweredContracts{ get; set; }

        /// <summary>
        /// Gets or sets the average talk time.
        /// </summary>
        /// <value>The average talk time.</value>
        [DataMember]
        public long AverageTalkTime{ get; set; }

        /// <summary>
        /// Gets or sets the after contact work time.
        /// </summary>
        /// <value>The after contact work.</value>
        [DataMember]
        public long AfterContactWork{ get; set; } //todo: time?

        /// <summary>
        /// Gets or sets the total handling time.
        /// </summary>
        /// <value>The total handling time.</value>
        [DataMember]
        public long TotalHandlingTime{ get; set; }

        /// <summary>
        /// Gets or sets the available time.
        /// </summary>
        /// <value>The available time.</value>
        [DataMember]
        public long AvailableTime{ get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [to be bold].
        /// </summary>
        /// <value><c>true</c> if [to be bold]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool ToBeBold
        {
            get;
            set;
        }

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }
    }
}