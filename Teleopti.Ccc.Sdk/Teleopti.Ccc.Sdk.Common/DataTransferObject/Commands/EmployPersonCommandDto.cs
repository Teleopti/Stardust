using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds a new person period to an existing person <see cref="Person"/>. The person period is created according to the specified <see cref="Period"/>, <see cref="PersonContract"/> and <see cref="Team"/>. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class EmployPersonCommandDto : CommandDto
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
        [DataMember(IsRequired = true)]
        public PersonContractDto PersonContract { get; set; }

        /// <summary>
        /// The team for the person period.
        /// </summary>
        /// <remarks>This property is not populated when loading data from the SDK. Only used for creation of new person periods!</remarks>
        [DataMember(IsRequired = true)]
        public TeamDto Team { get; set; }
    }
}
