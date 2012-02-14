using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details of an account for one person.
    /// </summary>
    /// <remarks>Account in this case could for example be Holiday days.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonAccountDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is in minutes.
        /// Otherwise it is in days.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in minutes; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInMinutes
        { get; set; }

        /// <summary>
        /// Gets or sets the latest calculated balance. I.e. Used.
        /// </summary>
        /// <value>The latest calculated balance.</value>
        [DataMember]
        public long LatestCalculatedBalance
        { get; set; }

        /// <summary>
        /// Gets or sets the balance in.
        /// </summary>
        /// <value>The balance in.</value>
        [DataMember]
        public long BalanceIn
        { get; set; }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        [DataMember]
        public long Extra
        { get; set; }

        /// <summary>
        /// Gets or sets the accrued.
        /// </summary>
        /// <value>The accrued.</value>
        [DataMember]
        public long Accrued
        { get; set; }

        /// <summary>
        /// Gets or sets the tracking description.
        /// </summary>
        /// <value>The tracking description.</value>
        /// <remarks>  
        /// </remarks>
        [DataMember]
        public string TrackingDescription
        { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// </remarks>
        [DataMember]
        public DateOnlyPeriodDto Period
        { get; set; }

        /// <summary>
        /// Gets or sets the balance out.
        /// </summary>
        /// <value>The balance out.</value>
        /// <remarks>
        /// </remarks>
        [DataMember]
        public long BalanceOut { get; set; }

        /// <summary>
        /// Gets or sets the remaining.
        /// </summary>
        /// <value>The remaining.</value>
        [DataMember(IsRequired = false, Order = 1)] 
        public long Remaining { get; set; }
    }
}
