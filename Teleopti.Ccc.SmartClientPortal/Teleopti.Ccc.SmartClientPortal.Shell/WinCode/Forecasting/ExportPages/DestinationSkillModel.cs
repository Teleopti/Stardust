using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages
{
    public class DestinationSkillModel
    {
        public DestinationSkillModel(IChildSkill childSkill, ICollection<ChildSkillMappingModel> childSkillMapping)
        {
            Skill = childSkill;
            ChildSkill = Skill.Name;
            ParentSkill = Skill.ParentSkill.Name;
            ChildSkillMapping = childSkillMapping;

            var v = ChildSkillMapping.Where(m => m.SourceSkill.Equals(Skill.Id)).ToList();

            if (v.IsEmpty()) return;
            TargetBu = v.First().TargetBuName;
            TargetSkill = v.First().TargetSkillName;
        }

        public ICollection<ChildSkillMappingModel> ChildSkillMapping { get; private set; }

        public IChildSkill Skill { get; private set; }

        public string ParentSkill { get; set; }
        public string ChildSkill { get; set; }
        public string TargetBu { get; set; }
        public string TargetSkill { get; set; }
        public string Map { get; set; }
    }
}