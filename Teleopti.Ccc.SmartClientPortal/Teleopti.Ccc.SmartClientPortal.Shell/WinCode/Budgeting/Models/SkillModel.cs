using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class SkillModel : IBudgetModel
    {
        public SkillModel(IEntity skill)
        {
            var s = ((ISkill)skill); //dunno, sucks...
            DisplayName = s.Name;
            ContainedEntity = skill;
        }

        public SkillModel()
        {}

        public string DisplayName { get; set; }
        public int ImageIndex { get { return 0; } }
        public IEntity ContainedEntity { get; set; }
    }
}