﻿using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command cancels all overtimes within a specified <see cref="Period"/>. All overtimes intersect with the specified <see cref="Period"/> will be hollowed out. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class CancelOvertimeCommandDto : CommandDto
    {
        /// <summary>
        /// Gets or sets the mandatory person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the mandatory target date which the overtime activity belongs to.
        /// </summary>
        /// <value>The target date.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
		public DateTimePeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
		public Guid? ScenarioId { get; set; }
    }
}
