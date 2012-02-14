using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// Represents a mapping between a source skill and target skill.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class ChildSkillMappingDto : IExtensibleDataObject
    {
		[DataMember]
        public SkillDto SourceSkill { get; set; }

		[DataMember]
        public SkillDto TargetSkill { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}