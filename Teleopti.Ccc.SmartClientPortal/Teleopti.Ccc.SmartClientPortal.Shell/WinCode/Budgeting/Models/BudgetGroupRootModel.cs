using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class BudgetGroupRootModel : IBudgetModel
    {
        private IList<BudgetGroupModel> _budgetGroups = new List<BudgetGroupModel>();

        public IList<BudgetGroupModel> BudgetGroups
        {
            get { return _budgetGroups; }
            set { _budgetGroups = value; }
        }

        public int ImageIndex { get { return 2; } }
        public string DisplayName { get { return UserTexts.Resources.Budgeting ; } }
        public IEntity ContainedEntity { get; set; }
    }
}