using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// Mapping of a multisite skill and several child skills.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class MultisiteSkillSelectionDto : IExtensibleDataObject
    {
        public MultisiteSkillSelectionDto()
        {
            ChildSkillMapping = new Collection<ChildSkillMappingDto>();
        }

		/// <summary>
		/// Gets or sets the source multisite skill.
		/// </summary>
		[DataMember]
        public SkillDto MultisiteSkill { get; set; }

		/// <summary>
		/// Gets or sets the mapping of source child skills and export target skills.
		/// </summary>
		[DataMember]
        public ICollection<ChildSkillMappingDto> ChildSkillMapping { get;  set; }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
        public ExtensionDataObject ExtensionData { get; set; }
    }
}