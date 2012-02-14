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

		[DataMember]
        public SkillDto MultisiteSkill { get; set; }

		[DataMember]
        public ICollection<ChildSkillMappingDto> ChildSkillMapping { get;  set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}