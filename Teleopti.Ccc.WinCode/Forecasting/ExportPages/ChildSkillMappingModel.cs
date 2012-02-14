using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class ChildSkillMappingModel
    {
        public ChildSkillMappingModel(Guid sourceSkillId, Guid targetSkillId, string targetBuName, string targetSkillName)
        {
            SourceSkill = new SkillDto { Id = sourceSkillId };
            TargetSkill = new SkillDto { Id = targetSkillId };
            TargetBuName = targetBuName;
            TargetSkillName = targetSkillName;
        }

        public string TargetSkillName { get; set; }
        public string TargetBuName { get; set; }

        public SkillDto TargetSkill { get; set; }
        public SkillDto SourceSkill { get; set; }
    }
}