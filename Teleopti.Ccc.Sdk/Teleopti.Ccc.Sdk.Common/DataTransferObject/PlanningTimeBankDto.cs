using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details for planning time bank.
    /// </summary>
     [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PlanningTimeBankDto
    {
         /// <summary>
         /// Gets or sets the balance in minutes.
         /// </summary>
         [DataMember]
         public int BalanceInMinutes { get; set; }

         /// <summary>
         /// Gets or sets the balance out minutes.
         /// </summary>
         [DataMember]
         public int BalanceOutMinutes { get; set; }

         /// <summary>
         /// Gets or sets the upper limit of balance out in minutes.
         /// </summary>
         [DataMember]
         public int BalanceOutMaxMinutes { get; set; }

         /// <summary>
         /// Gets or sets the lower limit of balance out in minutes.
         /// </summary>
         [DataMember]
         public int BalanceOutMinMinutes { get; set; }

         /// <summary>
         /// Gets or sets and indication whether the planning time bank values are editable.
         /// </summary>
         [DataMember]
         public bool IsEditable { get; set; }

    }
}