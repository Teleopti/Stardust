using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command exports multisite skills to several skills accoring to the specified collection <see cref="MultisiteSkillSelection"/>. The collection contains several mappings (<see cref="MultisiteSkillSelectionDto"/>) between multisite skill and skills.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ExportMultisiteSkillToSkillCommandDto : CommandDto
    {
        public ExportMultisiteSkillToSkillCommandDto()
        {
            MultisiteSkillSelection = new Collection<MultisiteSkillSelectionDto>();
        }

		/// <summary>
		/// Gets or sets the mandatory export period.
		/// </summary>
		[DataMember]
        public DateOnlyPeriodDto Period { get; set; }

		/// <summary>
		/// Gets or sets the mandatory multisite skill selection and mapping.
		/// </summary>
		[DataMember]
        public ICollection<MultisiteSkillSelectionDto> MultisiteSkillSelection { get;  set; }
    }
}