using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public class ChildSkillMappingModel
    {
        public ChildSkillMappingModel(Guid sourceSkillId, Guid targetSkillId, string targetBuName, string targetSkillName)
        {
            SourceSkill = sourceSkillId;
            TargetSkill = targetSkillId;
            TargetBuName = targetBuName;
            TargetSkillName = targetSkillName;
        }

        public string TargetSkillName { get; set; }
        public string TargetBuName { get; set; }

        public Guid TargetSkill { get; set; }
        public Guid SourceSkill { get; set; }
    }
}