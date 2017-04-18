using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
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