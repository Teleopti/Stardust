using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command updates an existing person period or adds a new person period and copy values from previous person period if it exists.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ChangePersonEmploymentCommandDto : CommandDto
    {  
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The PersonDto.</value>
        [DataMember(IsRequired = true)]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        [DataMember(IsRequired = true)]
        public DateOnlyPeriodDto Period { get; set; }

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
        [DataMember]
        public TeamDto Team { get; set; }
        
        /// <summary>
        /// Gets or sets active skills for a person during a period.
        /// </summary>
        [DataMember]
        public IList<PersonSkillPeriodDto> PersonSkillPeriodCollection { get; set; }

        /// <summary>
        /// Gets or sets external logon list
        /// </summary>
        [DataMember]
        public IList<ExternalLogOnDto> ExternalLogOn { get; set; }
        
        /// <summary>
        /// Gets or sets the note
        /// </summary>
        [DataMember]
        public string Note { get; set; }
    }
}
