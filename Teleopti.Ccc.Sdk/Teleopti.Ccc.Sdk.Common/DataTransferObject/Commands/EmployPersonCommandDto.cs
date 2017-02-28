using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
	/// This command adds a new person period to an existing person and updates the person with new details or creates a new person if the Id of <see cref="PersonDto"/> is set to null. The person period is created according to the specified <see cref="Period"/>, <see cref="PersonContract"/> and <see cref="Team"/>. 
    /// </summary>
	/// <remarks>This command requires the user to have permissions to open the People module.</remarks>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class EmployPersonCommandDto : CommandDto
    {
		/// <summary>
		/// Gets or sets the mandatory person. The entire <see cref="PersonDto"/> is required as the details are used to update or create a person.
		/// </summary>
		/// <value>The PersonDto.</value>
		[DataMember(IsRequired = true)]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>Currently only the start date is used, as it will be the start date of the personperiod.</remarks>
        [DataMember(IsRequired = true)]
        public DateOnlyPeriodDto Period { get; set; }

        /// <summary>
		/// Gets or sets the mandatory person contract. Only id for <see cref="ContractDto"/>, <see cref="ContractScheduleDto"/> and <see cref="PartTimePercentageDto"/> is required. These can be loaded using the <see cref="ITeleoptiOrganizationService.GetContracts"/>, <see cref="ITeleoptiOrganizationService.GetContractSchedules"/> and <see cref="ITeleoptiOrganizationService.GetPartTimePercentages"/>.
        /// </summary>
        /// <value>The person contract.</value>
        [DataMember(IsRequired = true)]
        public PersonContractDto PersonContract { get; set; }

        /// <summary>
        /// The mandatory team for the person period. Only the Id of <see cref="TeamDto"/> is required.
        /// </summary>
        [DataMember(IsRequired = true)]
        public TeamDto Team { get; set; }
    }
}
