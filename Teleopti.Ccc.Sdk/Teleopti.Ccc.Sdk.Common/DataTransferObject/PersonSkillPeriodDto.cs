using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details about skills for a person during a period.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonSkillPeriodDto : Dto
    {
        public PersonSkillPeriodDto()
        {
            SkillCollection = new List<Guid>();
            PersonSkillCollection = new List<PersonSkillDto>();
        }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the from date.
        /// </summary>
        [DataMember]
        public DateOnlyDto DateFrom { get; set; }

        /// <summary>
        /// Gets or sets the to date.
        /// </summary>
        [DataMember]
        public DateOnlyDto DateTo { get; set; }

        /// <summary>
        /// Gets the list of available skill id's.
        /// </summary>
        /// <remarks>Skill id's can be matched to skills fetched from <see cref="ITeleoptiForecastingService.GetSkills"/> using Id for more details.</remarks>
        [DataMember]
        public IList<Guid> SkillCollection { get; private set; }

        /// <summary>
        /// Gets the list of skills for this person period with proficiency and active information.
        /// </summary>
        [DataMember(IsRequired = false,Order = 1)]
        public IList<PersonSkillDto> PersonSkillCollection { get; private set; } 
    }
}
