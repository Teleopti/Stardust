using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class BudgetGroupModel : IBudgetModel
    {
        private IList<SkillModel> _skillModels = new List<SkillModel>();

        public BudgetGroupModel(IEntity budgetGroup)
        {
            var group = ((IBudgetGroup) budgetGroup); //dunno, sucks... 
            DisplayName = group.Name;
            ContainedEntity = budgetGroup;
        }

        public BudgetGroupModel()
        {}

        public string DisplayName { get; set; }
        public int ImageIndex { get { return 1; } }
        public IList<SkillModel> SkillModels
        {
            get { return _skillModels; }
            set { _skillModels = value; }
        }

        public IEntity ContainedEntity { get; set; }
    }
}