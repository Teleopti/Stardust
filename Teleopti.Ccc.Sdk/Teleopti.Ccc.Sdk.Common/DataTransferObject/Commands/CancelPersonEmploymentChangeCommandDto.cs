using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command removes person period with the given date and all future person periods.
    /// </summary>
	/// <remarks>This command requires the user to have permissions to open the People module for the given Person.</remarks>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class CancelPersonEmploymentChangeCommandDto : CommandDto
    {  
        /// <summary>
        /// Gets or sets the mandatory Id of the Person.
        /// </summary>
        /// <value>The guid of the person</value>
        [DataMember(IsRequired = true)]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the mandatory date.
        /// </summary>
        /// <value>The date for when the person periods will be removed.</value>
        [DataMember(IsRequired = true)]
        public DateOnlyDto Date { get; set; }
    }
}
