using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command updates an existing person period or adds a new person period and copy values from previous person period if it exists.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ChangePersonEmploymentCommandDto : CommandDto
    {  
        /// <summary>
        /// Gets or sets the mandatory person. Only <see cref="PersonDto.Id"/> is required on the given <see cref="PersonDto"/>.
        /// </summary>
        /// <value>The PersonDto.</value>
        [DataMember(IsRequired = true)]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the mandatory period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>Only the start date is required. End date currently has no effect.</remarks>
        [DataMember(IsRequired = true)]
        public DateOnlyPeriodDto Period { get; set; }

        /// <summary>
		/// Gets or sets the person contract. If this is set, all the properties of the object must be populated. Only <see cref="ContractDto.Id"/>, <see cref="ContractScheduleDto.Id"/> and <see cref="PartTimePercentageDto.Id"/> are required. These can be loaded using the <see cref="ITeleoptiOrganizationService.GetContracts"/>, <see cref="ITeleoptiOrganizationService.GetContractSchedules"/> and <see cref="ITeleoptiOrganizationService.GetPartTimePercentages"/>.
        /// </summary>
        /// <value>The person contract.</value>
        /// <remarks>If a new period should be created without any existing previous period for the persion, this is mandatory.</remarks>
        [DataMember]
        public PersonContractDto PersonContract { get; set; }

        /// <summary>
        /// The team for the person period. Only the <see cref="TeamDto.Id"/> property is mandatory if this is set.
        /// </summary>
		/// <remarks>If a new period should be created without any existing previous period for the persion, this is mandatory.</remarks>
        [DataMember]
        public TeamDto Team { get; set; }
        
        /// <summary>
        /// Gets or sets active skills for a person during a period. Only the <see cref="PersonSkillPeriodDto.SkillCollection"/> is used.
        /// </summary>
        [DataMember]
        public IList<PersonSkillPeriodDto> PersonSkillPeriodCollection { get; set; }

        /// <summary>
        /// Gets or sets external logon list. The <see cref="ExternalLogOnDto"/> must match an existing external log on available.
        /// </summary>
        [DataMember]
        public IList<ExternalLogOnDto> ExternalLogOn { get; set; }
        
        /// <summary>
        /// Gets or sets an optional note for the person period.
        /// </summary>
        [DataMember]
        public string Note { get; set; }
    }
}
